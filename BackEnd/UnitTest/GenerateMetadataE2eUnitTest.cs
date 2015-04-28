using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.IO;
using System.Threading.Tasks;
using DocAsCode.Utility;
using DocAsCode.EntityModel;

namespace UnitTest
{
    /// <summary>
    /// MEF is used for workspace host service provider, need to copy dll manually
    /// </summary>
    [TestClass]
    [DeploymentItem("NativeBinaries", "NativeBinaries")]
    public class GenerateMetadataE2eUnitTest
    {
        [TestMethod]
        [DeploymentItem("Assets", "Assets")]
        [DeploymentItem("Microsoft.CodeAnalysis.CSharp.Workspaces.dll")]
        [DeploymentItem("Microsoft.CodeAnalysis.CSharp.Workspaces.Desktop.dll")]
        public async Task TestGenereateMetadataAsync_SimpleProject()
        {
            string slnPath = "Assets/TestClass1/BaseClassForTestClass1/BaseClassForTestClass1.csproj";
            string fileList = "filelist.list";
            File.WriteAllText(fileList, slnPath);
            string outputList = "output.list";
            var result = await BuildMetaHelper.GenerateMetadataFromProjectListAsync(fileList, outputList);
            Assert.AreEqual(ResultLevel.Success, result.ResultLevel);
        }

        [TestMethod]
        [DeploymentItem("Assets", "Assets")]
        [DeploymentItem("Microsoft.CodeAnalysis.CSharp.Workspaces.dll")]
        [DeploymentItem("Microsoft.CodeAnalysis.CSharp.Workspaces.Desktop.dll")]
        [DeploymentItem("Microsoft.CodeAnalysis.VisualBasic.Workspaces.dll")]
        [DeploymentItem("Microsoft.CodeAnalysis.VisualBasic.Workspaces.Desktop.dll")]
        public async Task TestGenereateMetadataAsync_Solution_Overall()
        {
            var projectList = @"Assets/TestClass1/TestClass1.sln, Assets\TestClass1\CatLibrary\CatLibrary.csproj";
            string outputList = "obj/inter.list";
            var md = "Assets/Markdown/About.md";
            var output = "output";
            FileExtensions.CopyFile(md, Path.Combine(output, md));
            await BuildMetaHelper.GenerateMetadataFromProjectListAsync(projectList, outputList);
            await BuildMetaHelper.MergeMetadataFromMetadataListAsync(outputList, "output", "index.yml", "toc.yml", "api", BuildMetaHelper.MetadataType.Yaml);
            await BuildMetaHelper.GenerateIndexForMarkdownListAsync("output/index.yml", Path.Combine(output, md), string.Empty, string.Empty, string.Empty);
            Assert.IsTrue(Directory.Exists(output));
            Assert.Fail();
        }

        [TestMethod]
        [DeploymentItem("Assets", "Assets")]
        [DeploymentItem("Microsoft.CodeAnalysis.CSharp.Workspaces.dll")]
        [DeploymentItem("Microsoft.CodeAnalysis.CSharp.Workspaces.Desktop.dll")]
        [DeploymentItem("Microsoft.CodeAnalysis.VisualBasic.Workspaces.dll")]
        [DeploymentItem("Microsoft.CodeAnalysis.VisualBasic.Workspaces.Desktop.dll")]
        public async Task TestGenereateMetadataAsync_CatProject_Overall()
        {
            var projectList = @"Assets\TestClass1\CatLibrary\CatLibrary.csproj";
            string outputList = "obj/inter.list";
            string mdList = "Assets/Markdown/AboutCodeSnippet.md";
            await BuildMetaHelper.GenerateMetadataFromProjectListAsync(projectList, outputList);
            await BuildMetaHelper.MergeMetadataFromMetadataListAsync(outputList, "output", "index.yml", "toc.yml", "api", BuildMetaHelper.MetadataType.Yaml);
            await BuildMetaHelper.GenerateIndexForMarkdownListAsync("output/index.yml", mdList, "map", "map", "reference");
            Assert.Fail();
        }
    }
}
