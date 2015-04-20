using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DocAsCode.BuildMeta;
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
    public class GenerateMetadataUnitTest
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
            string[] slnPath = new string[] { "Assets/TestClass1/TestClass1.sln", @"Assets\TestClass1\CatLibrary\CatLibrary.csproj" };
            string fileList = "filelist.list";
            File.WriteAllText(fileList, slnPath.ToDelimitedString(Environment.NewLine));
            string outputList = "obj/inter.list";
            string outputDirectory = "output";
            string mdList = "md.list";
            File.WriteAllText(mdList, "Assets/Markdown/About.md");
            await BuildMetaHelper.GenerateMetadataFromProjectListAsync(fileList, outputList);
            await BuildMetaHelper.MergeMetadataFromMetadataListAsync(outputList, outputDirectory, "index.yml", "toc.yml", "api", BuildMetaHelper.MetadataType.Yaml);
            await BuildMetaHelper.GenerateIndexForMarkdownListAsync(outputDirectory, "index.yml", mdList, "md.yml", "md", "reference");
            Console.WriteLine(Path.GetFullPath(outputDirectory));
            Assert.IsTrue(Directory.Exists(outputDirectory));
            Assert.Fail();
        }

        [TestMethod]
        [DeploymentItem("Assets", "Assets")]
        [DeploymentItem("Microsoft.CodeAnalysis.VisualBasic.Workspaces.dll")]
        [DeploymentItem("Microsoft.CodeAnalysis.VisualBasic.Workspaces.Desktop.dll")]
        public async Task TestGenereateMetadataAsync_VBProject()
        {
            string[] slnPath = new string[] { @"Assets\TestClass1\VBTestClass1\VBTestClass1.vbproj" };
            string fileList = "filelist.list";
            File.WriteAllText(fileList, slnPath.ToDelimitedString(Environment.NewLine));
            string outputList = Path.GetRandomFileName() + ".list";
            string outputDirectory = "output";
            string mdList = "md.list";
            File.WriteAllText(mdList, "Assets/Markdown/About.md");
            await BuildMetaHelper.GenerateMetadataFromProjectListAsync(fileList, outputList);
            await BuildMetaHelper.MergeMetadataFromMetadataListAsync(outputList, outputDirectory, "index.yml", "toc.yml", "api", BuildMetaHelper.MetadataType.Yaml);
            await BuildMetaHelper.GenerateIndexForMarkdownListAsync(outputDirectory, "index.yml", mdList, "md.yml", "md", "reference");
            Console.WriteLine(Path.GetFullPath(outputDirectory));
            Assert.IsTrue(Directory.Exists(outputDirectory));
            //Assert.Fail();
        }

        [TestMethod]
        [DeploymentItem("Assets", "Assets")]
        [DeploymentItem("Microsoft.CodeAnalysis.CSharp.Workspaces.dll")]
        [DeploymentItem("Microsoft.CodeAnalysis.CSharp.Workspaces.Desktop.dll")]
        [DeploymentItem("Microsoft.CodeAnalysis.VisualBasic.Workspaces.dll")]
        [DeploymentItem("Microsoft.CodeAnalysis.VisualBasic.Workspaces.Desktop.dll")]
        public async Task TestGenereateMetadataAsync_CatProject_Overall()
        {
            string[] slnPath = new string[] { @"Assets\TestClass1\CatLibrary\CatLibrary.csproj" };
            string fileList = "filelist.list";
            File.WriteAllText(fileList, slnPath.ToDelimitedString(Environment.NewLine));
            string outputList = "obj/inter.list";
            string outputDirectory = "output";
            string mdList = "md.list";
            File.WriteAllText(mdList, "Assets/Markdown/AboutCodeSnippet.md");
            await BuildMetaHelper.GenerateMetadataFromProjectListAsync(fileList, outputList);
            await BuildMetaHelper.MergeMetadataFromMetadataListAsync(outputList, outputDirectory, "index.yml", "toc.yml", "api", BuildMetaHelper.MetadataType.Yaml);
            await BuildMetaHelper.GenerateIndexForMarkdownListAsync(outputDirectory, "index.yml", mdList, "md.yml", "md", "reference");
            Console.WriteLine(Path.GetFullPath(outputDirectory));
            Assert.IsTrue(Directory.Exists(outputDirectory));
            Assert.Fail();
        }
    }
}
