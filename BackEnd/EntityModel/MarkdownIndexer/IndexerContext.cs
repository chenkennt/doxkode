namespace DocAsCode.EntityModel.MarkdownIndexer
{
    using System.Collections.Generic;

    public class IndexerContext
    {
        public Dictionary<string, MetadataItem> ExternalApiIndex { get; set; } 

        public string MarkdownFilePath { get; set; }

        public string MarkdownContent { get; set; }

        public string MarkdownMapFileOutputFolder { get; set; }

        public string ApiMapFileOutputFolder { get; set; }

    }
}
