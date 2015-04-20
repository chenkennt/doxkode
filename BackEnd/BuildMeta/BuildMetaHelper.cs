using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using System.IO;
using Microsoft.CodeAnalysis.MSBuild;
using DocAsCode.Utility;
using System.Threading.Tasks;
using DocAsCode.EntityModel;
using System.Diagnostics;

namespace DocAsCode.BuildMeta
{
    public static class BuildMetaHelper
    {
        private static readonly MSBuildWorkspace Workspace = MSBuildWorkspace.Create();

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
                if (!Directory.Exists(directory)) return files;
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
        /// <param name="outputListFile"></param>
        /// <returns></returns>
        public static async Task<ParseResult> GenerateMetadataFromProjectListAsync(string projectListFile, string outputListFile)
        {
            var projectList = GetFileList(projectListFile);

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

            return await Task.Run(() => MergeYamlMetadataFromMetadataListCore(metadataList, outputFolder, indexFileName, tocFileName, apiFolder));
        }

        public static Task<ParseResult> GenerateIndexForMarkdownListAsync(string workingDirectory, string metadataFileName, string markdownListFile, string outputFileName, string mdFolderName, string referenceFolderName)
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

                string indexFilePath = TryGenerateMarkdownIndexFileCore(workingDirectory, outputFileName, metadataFileName, markdownList, mdFolderName, referenceFolderName);
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
                    var solution = await Workspace.OpenSolutionAsync(projectFile);
                    projects = solution.Projects.ToList();
                }
                else if (fileExtension == ".csproj" || fileExtension == ".vbproj")
                {
                    var project = await Workspace.OpenProjectAsync(projectFile);
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

        private static ParseResult MergeYamlMetadataFromMetadataListCore(List<string> metadataFiles, string outputFolder, string indexFileName, string tocFileName, string apiFolder)
        {
            if (metadataFiles == null || metadataFiles.Count == 0)
            {
                return null;
            }

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

                        ns.Items?.ForEach(s =>
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

                                s.Items?.ForEach(s1 =>
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
                            });
                    }
                }
            }

            return allMembers;
        }


        private static async Task<YamlItemViewModel> GenerateYamlMetadataAsync(Project project)
        {
            if (project == null)
            {
                return null;
            }

            var compilation = await project.GetCompilationAsync();
            object visitorContext = new object();
            YamlModelGeneratorVisitor visitor;
            if (project.Language == "Visual Basic")
            {
                visitor = new VBYamlModelGeneratorVisitor(visitorContext);
            }
            else if (project.Language == "C#")
            {
                visitor = new CSYamlModelGeneratorVisitor(visitorContext);
            }
            else
            {
                Debug.Assert(false, "Language not supported: " + project.Language);
                ParseResult.WriteToConsole(ResultLevel.Error, "Language not supported: " + project.Language);
                return null;
            }

            YamlItemViewModel item = compilation.Assembly.Accept(visitor);
            return item;
        }

        private static ParseResult TryParseYamlMetadataFile(string metadataFileName, out YamlItemViewModel projectMetadata)
        {
            projectMetadata = null;
            try
            {
                using (StreamReader reader = new StreamReader(metadataFileName))
                {
                    projectMetadata = YamlUtility.Deserialize<YamlItemViewModel>(reader);
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
                    YamlUtility.Serialize(writer, doc);
                    return new ParseResult(ResultLevel.Success, "Successfully generated metadata {0} for {1}", filePath, doc.Name);
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
            viewModel.TocYamlViewModel.Href = tocFileName;
            viewModel.TocYamlListViewModel = viewModel.TocYamlViewModel.Items;
            string tocFilePath = Path.Combine(folder, viewModel.TocYamlViewModel.Href);
            using (StreamWriter sw = new StreamWriter(tocFilePath))
            {
                YamlUtility.Serialize(sw, viewModel.TocYamlListViewModel);
            }

            // 2. generate api.yaml
            string indexFilePath = Path.Combine(folder, indexFileName);
            using (StreamWriter sw = new StreamWriter(indexFilePath))
            {
                YamlUtility.Serialize(sw, viewModel.IndexYamlViewModel);
            }

            // 3. generate each item's yaml
            foreach (var item in viewModel.MemberYamlViewModelList)
            {
                string itemFilepath = Path.Combine(folder, apiFolder, item.Href);
                Directory.CreateDirectory(Path.GetDirectoryName(itemFilepath));
                using (StreamWriter sw = new StreamWriter(itemFilepath))
                {
                    YamlUtility.Serialize(sw, item);
                    ParseResult.WriteToConsole(ResultLevel.Success, "Metadata file for {0} is saved to {1}", item.Name, itemFilepath);
                }
            }

            return new ParseResult(ResultLevel.Success);
        }

        private static string TryGenerateMarkdownIndexFileCore(string workingDirectory, string markdownIndexFileName, string indexFileName, List<string> mdFiles, string mdFolderName, string referenceFolderName)
        {
            Dictionary<string, IndexYamlItemViewModel> indexViewModel;

            // Read index
            string indexFilePath = Path.Combine(workingDirectory, indexFileName);
            if (!File.Exists(indexFilePath)) return null;
            using (StreamReader sr = new StreamReader(indexFilePath))
            {
                indexViewModel = YamlUtility.Deserialize<Dictionary<string, IndexYamlItemViewModel>>(sr);
            }

            // Generate markdown index
            if (mdFiles != null && mdFiles.Count > 0)
            {
                Dictionary<string, MarkdownIndex> mdresult = BuildMarkdownIndexHelper.MergeMarkdownResults(mdFiles, indexViewModel, workingDirectory, mdFolderName, referenceFolderName);
                if (mdresult.Any())
                {
                    string markdownIndexFilePath = Path.Combine(workingDirectory, markdownIndexFileName);
                    using (StreamWriter sw = new StreamWriter(markdownIndexFilePath))
                    {
                        YamlUtility.Serialize(sw, mdresult);
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