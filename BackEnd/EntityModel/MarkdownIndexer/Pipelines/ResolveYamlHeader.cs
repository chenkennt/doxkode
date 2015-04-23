namespace DocAsCode.EntityModel.MarkdownIndexer
{
    using System.Collections.Generic;
    using System.IO;

    public class ResolveYamlHeader : IIndexerPipeline
    {
        public ParseResult Run(MapFileItemViewModel item, IndexerContext context)
        {
            var filePath = context.MarkdownFilePath;
            var content = context.MarkdownContent;
            var apis = context.ExternalApiIndex;
            var apiMapFileOutputFolder = context.ApiMapFileOutputFolder;
            var yamlHeaders = YamlHeaderParser.Select(content);
            if (yamlHeaders == null || yamlHeaders.Count == 0) return new ParseResult(ResultLevel.Info, "No valid yaml header reference found for {0}", filePath);

            if (item.References == null) item.References = new ReferencesViewModel();
            ReferencesViewModel references = item.References;
            List<MarkdownSection> sections = SplitToSections(content, yamlHeaders);
            Dictionary<string, MarkdownSection> validMarkdownSections = new Dictionary<string, MarkdownSection>();
            foreach (var markdownSection in sections)
            {
                if (!string.IsNullOrEmpty(markdownSection.Id))
                {
                    validMarkdownSections[markdownSection.Id] = markdownSection;
                }
            }

            foreach (var yamlHeader in yamlHeaders)
            {
                var referenceId = yamlHeader.Id;
                var apiId = yamlHeader.Id;

                MetadataItem api;
                if (apis.TryGetValue(apiId, out api))
                {
                    var reference = new MapFileItemViewModel
                                        {
                                            Id = referenceId,
                                            ReferenceKeys = yamlHeader.MatchedSections,
                                            Href = api.Href,
                                        };
                    // 1. Add references to Markdown file
                    references.AddItem(reference);

                    // 2. Write api reference to API's map file
                    MarkdownSection markdownSection;
                    if (!validMarkdownSections.TryGetValue(apiId, out markdownSection)) continue;
                    
                    var apiPath = api.Href;
                    string apiMapFileName = Path.GetFileName(apiPath) + Constants.MapFileExtension;
                    string apiFolder = Path.GetDirectoryName(apiPath);

                    // Use the same folder as api.yaml if the output folder is not set
                    string apiMapFileFolder = (string.IsNullOrEmpty(apiMapFileOutputFolder) ? apiFolder : apiMapFileOutputFolder)
                                              ?? string.Empty;
                    string apiMapFileFullPath = Path.Combine(apiMapFileFolder, apiMapFileName);
                    MapFileItemViewModel apiMapFileSection = new MapFileItemViewModel
                                                                 {
                                                                     Id = apiId,
                                                                     Href = item.Href,
                                                                     Path = item.Path,
                                                                     Remote = item.Remote,
                                                                     Startline = markdownSection.Location.StartLocation.Line,
                                                                     Endline = markdownSection.Location.EndLocation.Line,
                                                                     References = SelectReferenceSection(references, markdownSection.Location),
                                                                     CustomProperties = yamlHeader.Properties,
                                                                 };
                    MapFileViewModel apiMapFile;
                    if (File.Exists(apiMapFileFullPath))
                    {
                        using (StreamReader reader = new StreamReader(apiMapFileFullPath))
                        {
                            apiMapFile = YamlUtility.Deserialize<MapFileViewModel>(reader);
                        }
                    }
                    else
                    {
                        apiMapFile = new MapFileViewModel();
                    }

                    // Current behavior: Override existing one
                    apiMapFile[apiId] = apiMapFileSection;
                    YamlUtility.Serialize(apiMapFileFullPath, apiMapFile);
                }
            }

            // Select references to the indices where DefinedLine is between startline and endline
            return new ParseResult(ResultLevel.Success);
        }

        private static ReferencesViewModel SelectReferenceSection(ReferencesViewModel allReferences, Location range)
        {
            ReferencesViewModel referenceSection = new ReferencesViewModel();
            foreach (var reference in allReferences)
            {
                Dictionary<string, Section> sections = new Dictionary<string, Section>();
                foreach (var referenceKey in reference.Value.ReferenceKeys)
                {
                    
                    foreach (var location in referenceKey.Value.Locations)
                    {
                        if (location.IsIn(range))
                        {
                            sections.Add(referenceKey.Key, referenceKey.Value);
                            continue;
                        }
                    }
                }

                if (sections.Count > 0)
                {
                    var referenceCloned = (MapFileItemViewModel)reference.Value.Clone();
                    referenceCloned.ReferenceKeys = sections;
                    referenceSection.AddItem(referenceCloned);
                }
            }

            return referenceSection;
        }

        private static List<MarkdownSection> SplitToSections(string content, IEnumerable<MatchDetail> yamlDetails)
        {
            MarkdownSection section = new MarkdownSection
                                          {
                                              Location = new Location { EndLocation = Coordinate.GetCoordinate(content) }
                                          };
            List<MarkdownSection> sections = new List<MarkdownSection> { section };
            foreach (var splitterDetail in yamlDetails)
            {
                var matchedSections = splitterDetail.MatchedSections;
                var splitterId = splitterDetail.Id;
                foreach (var matchedSection in matchedSections)
                {
                    foreach (var location in matchedSection.Value.Locations)
                    {
                        sections = Split(sections, location, splitterId);
                    }
                }
            }

            return sections;
        }

        private static List<MarkdownSection> Split(List<MarkdownSection> sectionsInput, Location splitter, string splitterId)
        {
            var sections = new List<MarkdownSection>();
            foreach (var markdownSection in sectionsInput)
            {
                sections.AddRange(Split(markdownSection, splitter, splitterId));
            }

            return sections;
        }

        private static List<MarkdownSection> Split(MarkdownSection section, Location splitter, string splitterId)
        {
            var sections = new List<MarkdownSection>();
            var sectionStart = section.Location.StartLocation;
            var sectionEnd = section.Location.EndLocation;
            var splitterStart = splitter.StartLocation;
            var splitterEnd = splitter.EndLocation;

            var firstStart = sectionStart;

            var secondEnd = sectionEnd;

            var firstEnd = splitterStart.CompareTo(sectionStart) > 0 ? splitterStart : sectionStart;

            var secondeStart = splitterEnd.CompareTo(sectionEnd) < 0 ? splitterEnd : sectionEnd;

            if (firstStart.CompareTo(firstEnd) < 0) sections.Add(new MarkdownSection { Location = new Location { StartLocation = firstStart, EndLocation = firstEnd } });
            if (secondeStart.CompareTo(secondEnd) < 0)
                sections.Add(
                    new MarkdownSection
                        {
                            Location = new Location { StartLocation = secondeStart, EndLocation = secondEnd },
                            Id = splitterId
                        });

            return sections;
        }

        private class MarkdownSection
        {
            public Location Location { get; set; }

            /// <summary>
            /// Id from the nearest upper uid from YAML header
            /// </summary>
            public string Id { get; set; }
        }
    }
}
