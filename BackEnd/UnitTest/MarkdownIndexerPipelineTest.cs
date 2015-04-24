namespace Docascode.UnitTest
{
    using System.Collections.Generic;

    using DocAsCode.EntityModel;
    using DocAsCode.EntityModel.MarkdownIndexer;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class MarkdownIndexerPipelineTest
    {
        [TestMethod]
        public void TestResolveYamlHeader()
        {
            MapFileItemViewModel item = new MapFileItemViewModel();
            IndexerContext context = new IndexerContext();
            context.ExternalApiIndex = new Dictionary<string, MetadataItem>
                                           {
                                               { "api1", new MetadataItem() { Name = "api1", Href = "api1.yml" } },
                                               { "api2", new MetadataItem() { Name = "api2", Href = "api2.yml" } }
                                           };
            ResolveYamlHeader resolver = new ResolveYamlHeader();
            var result = resolver.Run(item, context);
            Assert.AreEqual(result.ResultLevel, ResultLevel.Success);
        }
    }
}