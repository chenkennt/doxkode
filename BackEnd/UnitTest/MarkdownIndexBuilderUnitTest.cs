using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DocAsCode.BuildMeta;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using DocAsCode.EntityModel;

namespace UnitTest
{
    /// <summary>
    /// MEF is used for workspace host service provider, need to copy dll manually
    /// </summary>
    [TestClass]
    [DeploymentItem("NativeBinaries", "NativeBinaries")]
    [DeploymentItem("Microsoft.CodeAnalysis.CSharp.Workspaces.dll")]
    [DeploymentItem("Microsoft.CodeAnalysis.CSharp.Workspaces.Desktop.dll")]
    public class MarkdownIndexBuilderUnitTest
    {
        [TestMethod]
        [DeploymentItem("Assets/Markdown/About.md", "Assets/Markdown")]
        [DeploymentItem("Assets/TestClass1/TestClass1/Class1.cs", "Assets/TestClass1/TestClass1")]
        public void TestMarkdownIndexBuilder()
        {
            const string markdownFile = "Assets/Markdown/About.md";
            List<MarkdownIndex> indexes;
            var result =  BuildMarkdownIndexHelper.TryParseCustomizedMarkdown(markdownFile, null, "reference", null, out indexes);
            int itemCount = 0;
            foreach(var index in indexes)
            {
                Console.WriteLine(index);
                if (index.Items != null)
                {
                    foreach (var item in index.Items)
                    {
                        itemCount++;
                        Console.WriteLine(item);
                    }
                }
            }

            Assert.AreEqual(4, indexes.Count);
            Assert.AreEqual(2, itemCount);
            Assert.AreNotEqual(ResultLevel.Error, result.ResultLevel);
        }
    }
}
