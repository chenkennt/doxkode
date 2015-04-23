namespace DocAsCode.EntityModel.MarkdownIndexer
{
    using System.IO;

    public class Save : IIndexerPipeline
    {
        public ParseResult Run(MapFileItemViewModel item, IndexerContext context)
        {
            var filePath = context.MarkdownFilePath;
            var markdownMapFileOutputFolder = context.MarkdownMapFileOutputFolder;
            string markdownMapFileName = Path.GetFileName(filePath) + Constants.MapFileExtension;
            string markdownFolder = Path.GetDirectoryName(filePath);

            // Use the same folder as api.yaml if the output folder is not set
            string markdownMapFileFolder = (string.IsNullOrEmpty(markdownMapFileOutputFolder) ? markdownFolder : markdownMapFileOutputFolder)
                                           ?? string.Empty;
            string markdownMapFileFullPath = Path.Combine(markdownMapFileFolder, markdownMapFileName);

            MapFileViewModel mapFile = new MapFileViewModel();
            mapFile.Add(filePath, item);
            YamlUtility.Serialize(markdownMapFileFullPath, mapFile);

            return new ParseResult(ResultLevel.Success);
        }
    }
}
