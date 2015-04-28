namespace DocAsCode.EntityModel
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using DocAsCode.EntityModel.MarkdownIndexer;
    using DocAsCode.Utility;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.MSBuild;

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

        public static Task<ParseResult> GenerateIndexForMarkdownListAsync(string inputApiIndexFilePath, string inputMarkdownListFile, string outputApiMapFileFolder, string outputMarkdownMapFileFolder, string outputReferenceFolder)
        {
            return Task.Run(() => {
                var markdownList = GetFileList(inputMarkdownListFile);
                if (markdownList == null || markdownList.Count == 0)
                {
                    return new ParseResult(ResultLevel.Error, "No markdown file listed in {0}, Exiting", inputMarkdownListFile);
                }

                return TryGenerateMarkdownIndexFileCore(inputApiIndexFilePath, markdownList, outputMarkdownMapFileFolder, outputApiMapFileFolder, outputReferenceFolder);
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
                    var compilation = await project.GetCompilationAsync();
                    var projectMetadata = GenerateYamlMetadata(compilation);
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

            List<MetadataItem> projectMetadataList = new List<MetadataItem>();
            foreach (var metadataFile in metadataFiles)
            {
                MetadataItem projectMetadata;
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

        private static Dictionary<string, MetadataItem> MergeYamlProjectMetadata(List<MetadataItem> projectMetadataList)
        {
            if (projectMetadataList == null || projectMetadataList.Count == 0)
            {
                return null;
            }

            Dictionary<string, MetadataItem> namespaceMapping = new Dictionary<string, MetadataItem>();
            Dictionary<string, MetadataItem> allMembers = new Dictionary<string, MetadataItem>();

            foreach (var project in projectMetadataList)
            {
                if (project.Items != null)
                {
                    foreach (var ns in project.Items)
                    {
                        if (ns.Type == MemberType.Namespace)
                        {
                            MetadataItem nsOther;
                            if (namespaceMapping.TryGetValue(ns.Name, out nsOther))
                            {
                                if (ns.Items != null)
                                {
                                    if (nsOther.Items == null)
                                    {
                                        nsOther.Items = new List<MetadataItem>();
                                    }

                                    nsOther.Items.AddRange(ns.Items);
                                }
                            }
                            else
                            {
                                namespaceMapping.Add(ns.Name, ns);
                            }
                        }

                        if (!allMembers.ContainsKey(ns.Name))
                        {
                            allMembers.Add(ns.Name, ns);
                        }

                        ns.Items?.ForEach(s =>
                            {
                                MetadataItem existingMetadata;
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
                                        MetadataItem existingMetadata1;
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


        internal static MetadataItem GenerateYamlMetadata(Compilation compilation)
        {
            if (compilation == null)
            {
                return null;
            }

            object visitorContext = new object();
            YamlModelGeneratorVisitor visitor;
            if (compilation.Language == "Visual Basic")
            {
                visitor = new VBYamlModelGeneratorVisitor(visitorContext);
            }
            else if (compilation.Language == "C#")
            {
                visitor = new CSYamlModelGeneratorVisitor(visitorContext);
            }
            else
            {
                Debug.Assert(false, "Language not supported: " + compilation.Language);
                ParseResult.WriteToConsole(ResultLevel.Error, "Language not supported: " + compilation.Language);
                return null;
            }

            MetadataItem item = compilation.Assembly.Accept(visitor);
            return item;
        }

        private static ParseResult TryParseYamlMetadataFile(string metadataFileName, out MetadataItem projectMetadata)
        {
            projectMetadata = null;
            try
            {
                using (StreamReader reader = new StreamReader(metadataFileName))
                {
                    projectMetadata = YamlUtility.Deserialize<MetadataItem>(reader);
                    return new ParseResult(ResultLevel.Success);
                }
            }
            catch (Exception e)
            {
                return new ParseResult(ResultLevel.Error, e.Message);
            }
        }
        private static ParseResult TryExportYamlMetadataFile(MetadataItem doc, string folder, UriKind uriKind, out string filePath)
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

        private static ParseResult ResolveAndExportYamlMetadata(Dictionary<string, MetadataItem> allMembers, string folder, string indexFileName, string tocFileName, string apiFolder)
        {
            var model = YamlMetadataResolver.ResolveMetadata(allMembers, apiFolder);
            // 1. generate toc.yml
            model.TocYamlViewModel.Href = tocFileName;
            model.TocYamlViewModel.Type = MemberType.Toc;

            // TOC do not change
            var tocViewModel = TocViewModel.Convert(model.TocYamlViewModel);
            string tocFilePath = Path.Combine(folder, tocFileName);
            using (StreamWriter sw = new StreamWriter(tocFilePath))
            {
                YamlUtility.Serialize(sw, tocViewModel);
            }

            // 2. generate api.yml
            string indexFilePath = Path.Combine(folder, indexFileName);
            using (StreamWriter sw = new StreamWriter(indexFilePath))
            {
                YamlUtility.Serialize(sw, model.Indexer);
            }

            // 3. generate each item's yaml
            var members = model.Members;
            foreach (var memberModel in members)
            {
                string itemFilepath = Path.Combine(folder, apiFolder, memberModel.Href);
                Directory.CreateDirectory(Path.GetDirectoryName(itemFilepath));
                using (StreamWriter sw = new StreamWriter(itemFilepath))
                {
                    var memberViewModel = OnePageViewModel.Convert(memberModel);
                    YamlUtility.Serialize(sw, memberViewModel);
                    ParseResult.WriteToConsole(ResultLevel.Success, "Metadata file for {0} is saved to {1}", memberModel.Name, itemFilepath);
                }
            }

            return new ParseResult(ResultLevel.Success);
        }

        private static ParseResult TryGenerateMarkdownIndexFileCore(string inputApiIndexFilePath, List<string> mdFiles, string outputMardownMapFileFolder, string outputApiMapFileFolder, string outputReferenceFolder)
        {
           
            // Generate markdown index
            if (mdFiles != null && mdFiles.Count > 0)
            {
                // TODO: parse in, how to keep .md.map file name unique if the file is copied to a centralized folder？
                // TODO: thougth.1: copy all the md files with the folder structrue to the output folder?
                string markdownIndexOutputFolder = outputMardownMapFileFolder;
                string apiIndexOutputFolder = outputApiMapFileFolder;
                string referenceOutputFolder = outputReferenceFolder;
                foreach (var mdFile in mdFiles.Distinct())
                {
                    IndexerContext context = new IndexerContext
                                                 {
                                                     ApiIndexFilePath = inputApiIndexFilePath,
                                                     MarkdownFilePath = mdFile,
                                                     MarkdownMapFileOutputFolder = markdownIndexOutputFolder,
                                                     ApiMapFileOutputFolder = apiIndexOutputFolder,
                                                     ReferenceOutputFolder = referenceOutputFolder
                                                 };
                    var result = MarkdownIndexer.MarkdownIndexer.Exec(context);
                    result.WriteToConsole();
                }
            }
            else
            {
                return new ParseResult(ResultLevel.Info, "No Markdown file is included.");
            }

            return new ParseResult(ResultLevel.Success);
        }
    }
}
