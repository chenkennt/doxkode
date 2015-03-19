using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.IO;
using Microsoft.CodeAnalysis.MSBuild;
using Newtonsoft.Json;
using System.Collections;
using DocAsCode.Utility;
using System.ComponentModel;
using EntityModel;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Diagnostics;
using EntityModel.ViewModel;
using YamlDotNet.Serialization;

namespace DocAsCode.BuildMeta
{
    public static class BuildMetaHelper
    {
        /// <summary>
        /// Support file with .list files and search pattern delimited by comma(,)
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static List<string> GetFileList(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }

            List<string> files = new List<string>();
            var fileList = input.ToArray(StringSplitOptions.RemoveEmptyEntries, ',');
            if (fileList.Length == 0) return null;
            foreach(var f in fileList)
            {
                var directory = Path.GetDirectoryName(f);
                if (string.IsNullOrWhiteSpace(directory)) directory = Directory.GetCurrentDirectory();
                var searchPattern = Path.GetFileName(f);
                var fileNames = Directory.GetFiles(directory, searchPattern, SearchOption.TopDirectoryOnly);
                foreach(var item in fileNames)
                {
                    if (Path.GetExtension(item) == FileExtensions.ListFileExtension)
                    {
                        var listFiles = FileExtensions.GetFileListFromFile(item);
                        files.AddRange(listFiles);
                    }
                    else
                    {
                        files.Add(item);
                    }
                }
            }

            return files;
        }

        /// <summary>
        /// Could be absolute path
        /// </summary>
        /// <param name="projectListFile"></param>
        /// <param name="outputFolder"></param>
        /// <param name="outputListFile"></param>
        /// <returns></returns>
        public static async Task<ParseResult> GenerateMetadataFromProjectListAsync(string projectListFile, string outputListFile)
        {
            List<string> projectList;
            projectList = GetFileList(projectListFile);

            if (projectList == null || projectList.Count == 0)
            {
                return new ParseResult(ResultLevel.Error, "No project file listed in {0}, Exiting", projectListFile);
            }

            string outputFolder = Path.GetDirectoryName(outputListFile);
            if (string.IsNullOrEmpty(outputFolder))
            {
                outputFolder = Path.GetRandomFileName();
            }

            List<string> outputFileList = await GenerateMetadataFromProjectListCoreAsync(projectList, outputFolder);
            if (outputFileList != null && outputFileList.Count > 0)
            {
                FileExtensions.SaveFileListToFile(outputFileList, outputListFile);
                return new ParseResult(ResultLevel.Success);
            }
            else
            {
                return new ParseResult(ResultLevel.Warn, "No metadata file generated for {0}", projectList.ToDelimitedString());
            }
        }

        public enum MetadataType
        {
            Yaml,
            Json
        }

        public static async Task<ParseResult> MergeMetadataFromMetadataListAsync(string metadataListFile, string outputFolder, string indexFileName, string tocFileName, string apiFolder, MetadataType metadataType)
        {
            if (string.IsNullOrWhiteSpace(outputFolder))
            {
                throw new ArgumentException("output folder is required when merging metadata from metadata list");
            }

            var metadataList = GetFileList(metadataListFile);
            if (metadataList == null || metadataList.Count == 0)
            {
                return new ParseResult(ResultLevel.Error, "No metadata file listed in {0}, Exiting", metadataListFile);
            }

            return await MergeYamlMetadataFromMetadataListCoreAsync(metadataList, outputFolder, indexFileName, tocFileName, apiFolder);
        }

        public static Task<ParseResult> GenerateIndexForMarkdownListAsync(string workingDirectory, string metadataFileName, string markdownListFile, string outputFileName, string mdFolderName)
        {
            if (string.IsNullOrWhiteSpace(workingDirectory))
            {
                throw new ArgumentException("output folder is required when merging metadata from metadata list");
            }

            return Task.Run(() => {
                var markdownList = GetFileList(markdownListFile);
                if (markdownList == null || markdownList.Count == 0)
                {
                    return new ParseResult(ResultLevel.Error, "No markdown file listed in {0}, Exiting", markdownListFile);
                }

                string indexFilePath = TryGenerateMarkdownIndexFileCore(workingDirectory, outputFileName, metadataFileName, markdownList, mdFolderName);
                if (!string.IsNullOrEmpty(indexFilePath))
                {
                    return new ParseResult(ResultLevel.Success);
                }
                else
                {
                    return new ParseResult(ResultLevel.Warn, "No markdown file is generated for {0}", markdownListFile);
                }
            });
        }

        private static MSBuildWorkspace workspace = MSBuildWorkspace.Create();

        private static async Task<List<string>> GenerateMetadataFromProjectListCoreAsync(List<string> projectFiles, string outputFolder)
        {
            if (projectFiles == null || projectFiles.Count == 0)
            {
                return null;
            }

            List<string> outputFilePathList = new List<string>();
            if (!string.IsNullOrEmpty(outputFolder))
            {
                if (Directory.Exists(outputFolder))
                {
                    ParseResult.WriteToConsole(ResultLevel.Warn, "{0} directory already exists.", outputFolder);
                }
                else
                {
                    Directory.CreateDirectory(outputFolder);
                }
            }

            HashSet<ProjectId> tempMapping = new HashSet<ProjectId>();

            foreach (var projectFile in projectFiles)
            {
                List<Project> projects = new List<Project>();
                var fileExtension = Path.GetExtension(projectFile);
                ParseResult.WriteToConsole(ResultLevel.Info, "Generating documentation metadata from {0} ...", projectFile);
                if (fileExtension == ".sln")
                {
                    var solution = await workspace.OpenSolutionAsync(projectFile);
                    projects = solution.Projects.ToList();
                }
                else if (fileExtension == ".csproj")
                {
                    var project = await workspace.OpenProjectAsync(projectFile);
                    projects.Add(project);
                }
                else
                {
                    ParseResult.WriteToConsole(ResultLevel.Warn, "Project type {0} is currently not supported, ignored", fileExtension);
                    continue;
                }

                if (projects.Count == 0)
                {
                    ParseResult.WriteToConsole(ResultLevel.Warn, "No project is found under {0}, ignored", projectFile);
                    continue;
                }

                foreach (var project in projects)
                {
                    if (tempMapping.Contains(project.Id))
                    {
                        ParseResult.WriteToConsole(ResultLevel.Info, "Metadata for project {0} already generated.", project.Name);
                        continue;
                    }
                    else
                    {
                        tempMapping.Add(project.Id);
                    }

                    string projectFilePath;
                    var projectMetadata = await GenerateYamlMetadataAsync(project);
                    var result = TryExportYamlMetadataFile(projectMetadata, outputFolder, UriKind.Absolute, out projectFilePath);
                    if (result.ResultLevel == ResultLevel.Success)
                    {
                        outputFilePathList.Add(projectFilePath);
                    }

                    result.WriteToConsole();
                }
            }

            return outputFilePathList;
        }

        private static async Task<ParseResult> MergeYamlMetadataFromMetadataListCoreAsync(List<string> metadataFiles, string outputFolder, string indexFileName, string tocFileName, string apiFolder)
        {
            if (metadataFiles == null || metadataFiles.Count == 0)
            {
                return null;
            }

            List<string> outputFilePathList = new List<string>();
            if (Directory.Exists(outputFolder))
            {
                ParseResult.WriteToConsole(ResultLevel.Warn, "{0} directory already exists.", outputFolder);
            }
            else
            {
                Directory.CreateDirectory(outputFolder);
            }

            List<YamlItemViewModel> projectMetadataList = new List<YamlItemViewModel>();
            foreach (var metadataFile in metadataFiles)
            {
                string projectFilePath;
                YamlItemViewModel projectMetadata;
                var result = TryParseYamlMetadataFile(metadataFile, out projectMetadata);
                if (result.ResultLevel == ResultLevel.Success)
                {
                    projectMetadataList.Add(projectMetadata);
                }
                else
                {
                    result.WriteToConsole();
                }
            }

            var allMemebers = MergeYamlProjectMetadata(projectMetadataList);

            if (allMemebers == null || allMemebers.Count == 0)
            {
                ParseResult.WriteToConsole(ResultLevel.Error, "No metadata is generated for {0}.", metadataFiles.ToDelimitedString());
            }
            else
            {
                string indexFilePath;
                ParseResult result = ResolveAndExportYamlMetadata(allMemebers, outputFolder, indexFileName, tocFileName, apiFolder);
                if (result.ResultLevel == ResultLevel.Success)
                {
                    return result;
                }
                else
                {
                    result.WriteToConsole();
                }
            }

            return null;
        }


        private static async Task<string> MergeMetadataFromMetadataListCoreAsync(List<string> metadataFiles, string outputFolder, string indexFileName)
        {
            if (metadataFiles == null || metadataFiles.Count == 0)
            {
                return null;
            }

            List<string> outputFilePathList = new List<string>();
            if (Directory.Exists(outputFolder))
            {
                ParseResult.WriteToConsole(ResultLevel.Warn, "{0} directory already exists.", outputFolder);
            }
            else
            {
                Directory.CreateDirectory(outputFolder);
            }

            List<ProjectMetadata> projectMetadataList = new List<ProjectMetadata>();
            foreach (var metadataFile in metadataFiles)
            {
                string projectFilePath;
                ProjectMetadata projectMetadata;
                var result = TryParseMetadataFile(metadataFile, out projectMetadata);
                if (result.ResultLevel == ResultLevel.Success)
                {
                    projectMetadataList.Add(projectMetadata);
                }
                else
                {
                    result.WriteToConsole();
                }
            }
            DocumentationMetadata documentationMetadata = MergeProjectMetadata(projectMetadataList);

            if (documentationMetadata == null)
            {
                ParseResult.WriteToConsole(ResultLevel.Error, "No metadata is generated for {0}.", metadataFiles.ToDelimitedString());
            }
            else
            {
                string indexFilePath;
                ParseResult result = TryExportMetadataFile(documentationMetadata, outputFolder, indexFileName, out indexFilePath);
                if (result.ResultLevel == ResultLevel.Success)
                {
                    return indexFilePath;
                }
                else
                {
                    result.WriteToConsole();
                }
            }

            return null;
        }
        private static DocumentationMetadata MergeProjectMetadata(List<ProjectMetadata> projectMetadataList)
        {
            if (projectMetadataList == null || projectMetadataList.Count == 0)
            {
                return null;
            }

            DocumentationMetadata document = new DocumentationMetadata();
            document.Namespaces = new IdentityMapping<NamespaceMetadata>();
            document.AllMembers = new IdentityMapping<IMetadata>();

            foreach (var project in projectMetadataList)
            {
                if (project.Namespaces != null)
                {
                    foreach (var ns in project.Namespaces)
                    {
                        NamespaceMetadata nsOther;
                        if (document.Namespaces.TryGetValue(ns.Key, out nsOther))
                        {
                            ns.Value.Members.AddRange(nsOther.Members);
                        }
                        else
                        {
                            document.Namespaces.Add(ns.Key, ns.Value);
                        }

                        document.AllMembers.GetOrAdd(ns.Key, ns.Value);
                        if (ns.Value.Members != null)
                        {
                            ns.Value.Members.ForEach(s =>
                            {
                                IMetadata existingMetadata;
                                if (document.AllMembers.TryGetValue(s.Identity, out existingMetadata))
                                {
                                    ParseResult.WriteToConsole(ResultLevel.Error, "Duplicate member {0} is found from {1} and {2}, use the one in {1} and ignore the one from {2}", s.Identity.ToString(), existingMetadata.FilePath, s.FilePath);
                                }
                                else
                                {
                                    document.AllMembers.Add(s.Identity, s);

                                }

                                if (s.Members != null)
                                {
                                    s.Members.ForEach(s1 =>
                                    {
                                        IMetadata existingMetadata1;
                                        if (document.AllMembers.TryGetValue(s1.Identity, out existingMetadata1))
                                        {
                                            ParseResult.WriteToConsole(ResultLevel.Error, "Duplicate member {0} is found from {1} and {2}, use the one in {1} and ignore the one from {2}", s1.Identity.ToString(), existingMetadata1.FilePath, s1.FilePath);
                                        }
                                        else
                                        {
                                            document.AllMembers.Add(s1.Identity, s1);
                                        }
                                    });
                                }
                            });
                        }
                    }
                }
            }

            return document;
        }
        private static Dictionary<string, YamlItemViewModel> MergeYamlProjectMetadata(List<YamlItemViewModel> projectMetadataList)
        {
            if (projectMetadataList == null || projectMetadataList.Count == 0)
            {
                return null;
            }

            Dictionary<string, YamlItemViewModel> namespaceMapping = new Dictionary<string, YamlItemViewModel>();
            Dictionary<string, YamlItemViewModel> allMembers = new Dictionary<string, YamlItemViewModel>();

            foreach (var project in projectMetadataList)
            {
                if (project.Items != null)
                {
                    foreach (var ns in project.Items)
                    {
                        if (ns.Type == MemberType.Namespace)
                        {
                            YamlItemViewModel nsOther;
                            if (namespaceMapping.TryGetValue(ns.Name, out nsOther))
                            {
                                if (ns.Items != null)
                                {
                                    if (nsOther.Items == null)
                                    {
                                        nsOther.Items = new List<YamlItemViewModel>();
                                    }

                                    nsOther.Items.AddRange(ns.Items);
                                }
                            }
                            else
                            {
                                namespaceMapping.Add(ns.Name, ns);
                            }
                        }

                        allMembers[ns.Name] = ns;

                        if (ns.Items != null)
                        {
                            ns.Items.ForEach(s =>
                            {
                                YamlItemViewModel existingMetadata;
                                if (allMembers.TryGetValue(s.Name, out existingMetadata))
                                {
                                    ParseResult.WriteToConsole(ResultLevel.Error, "Duplicate member {0} is found from {1} and {2}, use the one in {1} and ignore the one from {2}", s.Name, existingMetadata.Source.Path, s.Source.Path);
                                }
                                else
                                {
                                    allMembers.Add(s.Name, s);
                                }

                                if (s.Items != null)
                                {
                                    s.Items.ForEach(s1 =>
                                    {
                                        YamlItemViewModel existingMetadata1;
                                        if (allMembers.TryGetValue(s1.Name, out existingMetadata1))
                                        {
                                            ParseResult.WriteToConsole(ResultLevel.Error, "Duplicate member {0} is found from {1} and {2}, use the one in {1} and ignore the one from {2}", s1.Name, existingMetadata1.Source.Path, s1.Source.Path);
                                        }
                                        else
                                        {
                                            allMembers.Add(s1.Name, s1);
                                        }
                                    });
                                }
                            });
                        }
                    }
                }
            }

            return allMembers;
        }

        private class SymbolMetadataTable : ConcurrentDictionary<ISymbol, IMetadata>
        {
        }

        private static async Task<YamlItemViewModel> GenerateYamlMetadataAsync(Project project)
        {
            if (project == null)
            {
                return null;
            }

            var compilation = await project.GetCompilationAsync();
            object visitorContext = new object();

            YamlItemViewModel item = compilation.Assembly.Accept(new YamlModelGeneratorVisitor(visitorContext));
            return item;
        }

        private static async Task<ProjectMetadata> GenerateMetadataAsync(Project project)
        {
            if (project == null)
            {
                return null;
            }

            IdentityMapping<NamespaceMetadata> namespaceMapping = new IdentityMapping<NamespaceMetadata>();

            // Stores the generated IMetadata with the relationship to ISymbol
            SymbolMetadataTable symbolMetadataTable = new SymbolMetadataTable();

            ProjectMetadata projectMetadata = new ProjectMetadata
            {
                ProjectName = project.Name,
                Namespaces = namespaceMapping,
            };

            // Project.Name is unique per solution
            MetadataExtractContext context = new MetadataExtractContext
            {
                ProjectName = project.Name
            };

            var compilation = await project.GetCompilationAsync();
            object visitorContext = new object();

            YamlItemViewModel item = compilation.Assembly.Accept(new YamlModelGeneratorVisitor(visitorContext));

            var namespaceMembers = compilation.Assembly.GlobalNamespace.GetNamespaceMembers();
            var namespaceQueue = new Queue<INamespaceSymbol>();

            foreach (var ns in namespaceMembers)
            {
                await TreeIterator.PreorderAsync<ISymbol>(
                    ns
                    , null
                    , s =>
                    {
                        var namespaceOrTypeSymbol = s as INamespaceOrTypeSymbol;
                        if (namespaceOrTypeSymbol != null)
                        {
                            return namespaceOrTypeSymbol.GetMembers();
                        }
                        else
                        {
                            return null;
                        }
                    }
                    , (current, parent) => GenerateMetadataForEachSymbol(current, parent, context, namespaceMapping, symbolMetadataTable)
                    );
            }

            return projectMetadata;
        }

        private static async Task<bool> GenerateMetadataForEachSymbol(ISymbol current, ISymbol parent, IMetadataExtractContext context, IdentityMapping<NamespaceMetadata> namespaceMapping, SymbolMetadataTable symbolMetadataTable)
        {
            if (!MetadataExtractorManager.CanExtract(current))
            {
                return false;
            }

            var namespaceSymbol = current as INamespaceSymbol;

            if (namespaceSymbol != null)
            {
                NamespaceMetadata nsMetadata = await MetadataExtractorManager.ExtractAsync(current, context) as NamespaceMetadata;
                context.OwnerNamespace = nsMetadata;

                // Namespace(N)--(N)Project is N-N mapping
                nsMetadata = namespaceMapping.GetOrAdd(nsMetadata.Identity, nsMetadata);

                Debug.Assert(!symbolMetadataTable.ContainsKey(current), "Should not contain current symbol in the first iteration");
                symbolMetadataTable.TryAdd(current, nsMetadata);
            }
            else
            {
                var currentAsTypeSymbol = current as ITypeSymbol;
                var parentAsNamespaceSymbol = parent as INamespaceSymbol;

                // Flatten TypeSymbol hierarchy
                if (currentAsTypeSymbol != null || parentAsNamespaceSymbol != null)
                {
                    IMetadata parentMetadata;
                    NamespaceMemberMetadata nsMemberMetadata = await MetadataExtractorManager.ExtractAsync(current, context) as NamespaceMemberMetadata;
                    if (nsMemberMetadata != null)
                    {
                        // Will not check if members with duplicate Identify exists
                        // Should not ignore the namespace's member even if it contains nothing because we can override the comments in markdown
                        ((NamespaceMetadata)context.OwnerNamespace).Members.Add(nsMemberMetadata);

                        Debug.Assert(!symbolMetadataTable.ContainsKey(current), "Should not contain current symbol in the first iteration");
                        symbolMetadataTable.TryAdd(current, nsMemberMetadata);
                    }
                }
                else
                {
                    IMetadata parentMetadata;
                    symbolMetadataTable.TryGetValue(parent, out parentMetadata);
                    var parentNamespaceMetadata = parentMetadata as NamespaceMemberMetadata;
                    Debug.Assert(parentNamespaceMetadata != null, "Non-INamespaceSymbol should correspond to NamespaceMemberMetadata");

                    NamespaceMembersMemberMetadata nsMembersMembersMetadata = await MetadataExtractorManager.ExtractAsync(current, context) as NamespaceMembersMemberMetadata;
                    if (nsMembersMembersMetadata != null)
                    {
                        parentNamespaceMetadata.Members.Add(nsMembersMembersMetadata);
                    }
                }
            }

            return true;
        }


        private static ParseResult TryParseYamlMetadataFile(string metadataFileName, out YamlItemViewModel projectMetadata)
        {
            projectMetadata = null;
            try
            {
                using (StreamReader reader = new StreamReader(metadataFileName))
                {
                    projectMetadata = YamlUtility.Deserializer.Deserialize<YamlItemViewModel>(reader);
                    return new ParseResult(ResultLevel.Success);
                }
            }
            catch (Exception e)
            {
                return new ParseResult(ResultLevel.Error, e.Message);
            }
        }

        private static ParseResult TryParseMetadataFile(string metadataFileName, out ProjectMetadata projectMetadata)
        {
            projectMetadata = null;
            try
            {
                using (StreamReader reader = new StreamReader(metadataFileName))
                {
                    projectMetadata = JsonUtility.Deserialize<ProjectMetadata>(reader);
                    return new ParseResult(ResultLevel.Success);
                }
            }
            catch (Exception e)
            {
                return new ParseResult(ResultLevel.Error, e.Message);
            }
        }

        private static ParseResult TryExportYamlMetadataFile(YamlItemViewModel doc, string folder, UriKind uriKind, out string filePath)
        {
            filePath = Path.Combine(folder, doc.Name).FormatPath(uriKind, folder);

            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    YamlUtility.Serializer.Serialize(writer, doc);
                    return new ParseResult(ResultLevel.Success, "Successfully generated metadata {0} for {1}", filePath, doc.Name);
                }
            }
            catch (Exception e)
            {
                return new ParseResult(ResultLevel.Error, e.Message);
            }
        }

        private static ParseResult TryExportMetadataFile(ProjectMetadata doc, string folder, UriKind uriKind, out string filePath)
        {
            filePath = Path.Combine(folder, doc.ProjectName).FormatPath(uriKind, folder);

            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    JsonUtility.Serialize(doc, writer);
                    return new ParseResult(ResultLevel.Success, "Successfully generated metadata {0} for {1}", filePath, doc.ProjectName);
                }
            }
            catch (Exception e)
            {
                return new ParseResult(ResultLevel.Error, e.Message);
            }
        }

        private static ParseResult ResolveAndExportYamlMetadata(Dictionary<string, YamlItemViewModel> allMembers, string folder, string indexFileName, string tocFileName, string apiFolder)
        {
            var viewModel = YamlMetadataResolver.ResolveMetadata(allMembers, apiFolder);
            // 1. generate toc.yaml
            viewModel.TocYamlViewModel.YamlPath = tocFileName;
            string tocFilePath = Path.Combine(folder, viewModel.TocYamlViewModel.YamlPath);
            using (StreamWriter sw = new StreamWriter(tocFilePath))
            {
                YamlUtility.Serializer.Serialize(sw, viewModel.TocYamlViewModel);
            }

            // 2. generate api.yaml
            string indexFilePath = Path.Combine(folder, indexFileName);
            using (StreamWriter sw = new StreamWriter(indexFilePath))
            {
                YamlUtility.Serializer.Serialize(sw, viewModel.IndexYamlViewModel);
            }

            // 3. generate each item's yaml
            foreach (var item in viewModel.MemberYamlViewModelList)
            {
                string itemFilepath = Path.Combine(folder, item.YamlPath);
                Directory.CreateDirectory(Path.GetDirectoryName(itemFilepath));
                using (StreamWriter sw = new StreamWriter(itemFilepath))
                {
                    YamlUtility.Serializer.Serialize(sw, item);
                    ParseResult.WriteToConsole(ResultLevel.Success, "Metadata file for {0} is saved to {1}", item.Name, itemFilepath);
                }
            }

            return new ParseResult(ResultLevel.Success);
        }

        private static ParseResult TryExportMetadataFile(DocumentationMetadata doc, string folder, string indexFileName, out string indexFilePath)
        {
            // Folder structure
            // toc.yaml # toc containing all the namespaces
            // api.yaml # id-yaml mapping
            // api/{id}.yaml # items
            const string YamlExtension = ".yaml";
            const string TocYamlFileName = "toc" + YamlExtension;
            const string ApiFolder = "api";
            indexFilePath = null;
            var viewModel = doc.ToYamlViewModel(ApiFolder);

            // 1. generate toc.yaml
            string tocFilePath = Path.Combine(folder, TocYamlFileName);
            using (StreamWriter sw = new StreamWriter(tocFilePath))
            {
                YamlUtility.Serializer.Serialize(sw, viewModel.TocYamlViewModel);
            }

            // 2. generate api.yaml
            indexFilePath = Path.Combine(folder, indexFileName);
            using (StreamWriter sw = new StreamWriter(indexFilePath))
            {
                YamlUtility.Serializer.Serialize(sw, viewModel.IndexYamlViewModel);
            }

            // 3. generate each item's yaml
            foreach (var item in viewModel.MemberYamlViewModelList)
            {
                string itemFilepath = Path.Combine(folder, item.YamlPath);
                Directory.CreateDirectory(Path.GetDirectoryName(itemFilepath));
                using (StreamWriter sw = new StreamWriter(itemFilepath))
                {
                    YamlUtility.Serializer.Serialize(sw, item);
                    ParseResult.WriteToConsole(ResultLevel.Success, "Metadata file for {0} is saved to {1}", item.Name, itemFilepath);
                }
            }

            return new ParseResult(ResultLevel.Success);
        }

        private static string TryGenerateMarkdownIndexFileCore(string workingDirectory, string markdownIndexFileName, string indexFileName, List<string> mdFiles, string mdFolderName)
        {
            Dictionary<string, IndexYamlItemViewModel> indexViewModel;

            // Read index
            string indexFilePath = Path.Combine(workingDirectory, indexFileName);
            using (StreamReader sr = new StreamReader(indexFilePath))
            {
                indexViewModel = YamlUtility.Deserializer.Deserialize<Dictionary<string, IndexYamlItemViewModel>>(sr);
            }

            // Generate markdown index
            if (mdFiles != null && mdFiles.Count > 0)
            {
                Dictionary<string, MarkdownIndex> mdresult = BuildMarkdownIndexHelper.MergeMarkdownResults(mdFiles, indexViewModel, workingDirectory, mdFolderName);
                if (mdresult.Any())
                {
                    string markdownIndexFilePath = Path.Combine(workingDirectory, markdownIndexFileName);
                    using (StreamWriter sw = new StreamWriter(markdownIndexFilePath))
                    {
                        YamlUtility.Serializer.Serialize(sw, mdresult);
                        return markdownIndexFilePath;
                    }
                }
                else
                {
                    ParseResult.WriteToConsole(ResultLevel.Warn, "No api matching the markdown file headers is found.");
                }
            }
            else
            {
                ParseResult.WriteToConsole(ResultLevel.Success, "Markdown index file {0} is successfully generated.", indexFilePath);
            }

            return null;
        }
    }
}
