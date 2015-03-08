﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DocAsCode.BuildMeta;
using System.IO;
using System.Threading.Tasks;
using DocAsCode.Utility;

namespace UnitTest
{
    /// <summary>
    /// MEF is used for workspace host service provider, need to copy dll manually
    /// </summary>
    [TestClass]
    [DeploymentItem("NativeBinaries", "NativeBinaries")]
    [DeploymentItem("Microsoft.CodeAnalysis.CSharp.Workspaces.dll")]
    [DeploymentItem("Microsoft.CodeAnalysis.CSharp.Workspaces.Desktop.dll")]
    public class GenerateMetadataUnitTest
    {
        [TestMethod]
        [DeploymentItem("Assets", "Assets")]
        public async Task TestGenereateMetadataAsync_SimpleProject()
        {
            string slnPath = "Assets/TestClass1/BaseClassForTestClass1/BaseClassForTestClass1.csproj";
            string fileList = "filelist.list";
            File.WriteAllText(fileList, slnPath);
            string outputList = "output.list";
            string outputDirectory = "output";
            await BuildMetaHelper.GenerateMetadataFromProjectListAsync(fileList, outputList);
            Console.WriteLine(Path.GetFullPath(outputDirectory));
            Assert.IsTrue(Directory.Exists(outputDirectory));
        }

        [TestMethod]
        [DeploymentItem("Assets", "Assets")]
        public async Task TestGenereateMetadataAsync_Solution_Overall()
        {
            string[] slnPath = new string[] { "Assets/TestClass1/TestClass1.sln", @"Assets\TestClass1\TestClass1\TestClass1.csproj" };
            string fileList = "filelist.list";
            File.WriteAllText(fileList, slnPath.ToDelimitedString(Environment.NewLine));
            string outputList = Path.GetRandomFileName();
            string outputDirectory = "output";
            string mdList = "md.list";
            File.WriteAllText(mdList, "Assets/Markdown/About.md");
            await BuildMetaHelper.GenerateMetadataFromProjectListAsync(fileList, outputList);
            await BuildMetaHelper.MergeMetadataFromMetadataListAsync(outputList, outputDirectory, "index.yaml", BuildMetaHelper.MetadataType.Yaml);
            await BuildMetaHelper.GenerateIndexForMarkdownListAsync(outputDirectory, "index.yaml", mdList, "md.yaml", "md");
            Console.WriteLine(Path.GetFullPath(outputDirectory));
            Assert.IsTrue(Directory.Exists(outputDirectory));
            Assert.Fail();
        }

        [TestMethod]
        [DeploymentItem("Assets", "Assets")]
        public async Task TestGenereateMetadataAsync_Project()
        {
            string[] slnPath = new string[] { @"Assets\TestClass1\TestClass2\TestClass2.csproj" };
            string fileList = "filelist.list";
            File.WriteAllText(fileList, slnPath.ToDelimitedString(Environment.NewLine));
            string outputList = Path.GetRandomFileName();
            string outputDirectory = "output";
            string mdList = "md.list";
            File.WriteAllText(mdList, "Assets/Markdown/About.md");
            await BuildMetaHelper.GenerateMetadataFromProjectListAsync(fileList, outputList);
            await BuildMetaHelper.MergeMetadataFromMetadataListAsync(outputList, outputDirectory, "index.yaml", BuildMetaHelper.MetadataType.Yaml);
            await BuildMetaHelper.GenerateIndexForMarkdownListAsync(outputDirectory, "index.yaml", mdList, "md.yaml", "md");
            Console.WriteLine(Path.GetFullPath(outputDirectory));
            Assert.IsTrue(Directory.Exists(outputDirectory));
            Assert.Fail();
        }
    }
}
