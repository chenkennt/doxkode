﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using DocAsCode.EntityModel;

namespace UnitTest
{
    using System.Linq;

    using YamlDotNet.Core;

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
        [DeploymentItem("Assets/index.yml/catlibrary_index.yml", "Assets/index.yml")]
        [DeploymentItem("Assets/Markdown/AboutCodeSnippet.md", "Assets/Markdown")]
        public async Task TestGenerateIndexForMarkdownListAsync()
        {
            // Prepare file
            var outputFolder = "output";

            var indexFile = "Assets/index.yml/catlibrary_index.yml";
            // Index file should be @output folder... TODO: change
            var outputIndexFile = Path.Combine(outputFolder, indexFile);
            var outputDirectory = Path.GetDirectoryName(outputIndexFile);
            if (!string.IsNullOrEmpty(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            File.Copy(indexFile, outputIndexFile);

            var inputMarkdownList = "Assets/Markdown/AboutCodeSnippet.md";
            var outputMarkdownFolder = "output/md";
            var outputMarkdownIndexFolder = "output/map";
            var outputReferenceFolder = "output/reference";

            // Call GenerateIndexForMarkdownListAsync
            var result =
                await
                BuildMetaHelper.GenerateIndexForMarkdownListAsync(
                    outputFolder,
                    indexFile,
                    inputMarkdownList,
                    outputMarkdownIndexFolder,
                    outputMarkdownFolder,
                    outputReferenceFolder);
            Assert.AreEqual(ResultLevel.Success, result.ResultLevel, result.Message);

            // TODO: check if output/md folder & output/reference folder get created?
            // TODO: Copy files in Program or in Targets?

            var mapFiles = Directory.GetFiles(outputMarkdownIndexFolder);

            Assert.AreEqual(4, mapFiles.Length);

            // Check CatLibrary.IAnimal.yml.map
            var interfaceYamlMapFileName = "CatLibrary.IAnimal.yml.map";
            AssertFileExists(interfaceYamlMapFileName, mapFiles);

            var interfaceYamlMapViewModel = LoadMapFile(Path.Combine(outputMarkdownIndexFolder, interfaceYamlMapFileName));
            Assert.AreEqual(2, interfaceYamlMapViewModel.Count);
            var interface1 = interfaceYamlMapViewModel["CatLibrary.IAnimal"];
            Assert.IsNotNull(interface1);
            Assert.AreEqual(5, interface1.Startline);
            Assert.AreEqual(20, interface1.Endline);
            var interfaceReferences = interface1.References;
            Assert.IsNotNull(interfaceReferences);

            Assert.AreEqual(1, interfaceReferences.Count);
            Assert.AreEqual(1, interfaceReferences["'../testclass1/catlibrary/class1.cs'[20-46]"].Keys.Count);
            Assert.IsNotNull(interface1.CustomProperties);
            Assert.AreEqual("This is a *TEST* for override summary @CatLibrary.FakeDelegate`1", interface1.CustomProperties["summary"]);

            var catYamlMapFileName = "CatLibrary.Cat`2.yml.map";
            AssertFileExists(catYamlMapFileName, mapFiles);
            // Check CatLibrary.Cat`2.yml.map
            var catYamlMapViewModel = LoadMapFile(Path.Combine(outputMarkdownIndexFolder, catYamlMapFileName));
            Assert.AreEqual(1, catYamlMapViewModel.Count);
            var cat1 = catYamlMapViewModel["CatLibrary.Cat`2.CalculateFood(System.DateTime)"];
            Assert.IsNotNull(cat1);
            Assert.AreEqual(24, cat1.Startline);
            Assert.AreEqual(41, cat1.Endline);
            Assert.IsNull(cat1.References);
            Assert.IsNull(cat1.CustomProperties);

            var delegateYamlMapFileName = "CatLibrary.FakeDelegate`1.yml.map";
            AssertFileExists(delegateYamlMapFileName, mapFiles);
            // Check CatLibrary.FakeDelegate`1.yml.map
            var delegateYamlMapViewModel = LoadMapFile(Path.Combine(outputMarkdownIndexFolder, delegateYamlMapFileName));
            Assert.AreEqual(1, delegateYamlMapViewModel.Count);
            var delegate1 = catYamlMapViewModel["CatLibrary.FakeDelegate`1"];
            Assert.IsNotNull(delegate1);
            Assert.AreEqual(45, delegate1.Startline);
            Assert.AreEqual(56, delegate1.Endline);
            var delegateReferences = delegate1.References;
            Assert.IsNotNull(delegateReferences);

            Assert.AreEqual(1, delegateReferences.Count);
            Assert.AreEqual(2, delegateReferences["../testclass1/testclass1/class1.cs[0-]"].Keys.Count);
            Assert.IsNull(cat1.CustomProperties);

            // Check .md.map
            var mdMapFileName = "AboutCodeSnippet.md.map";
            AssertFileExists(mdMapFileName, mapFiles);
            var mdMapFileViewModel = LoadMapFile(Path.Combine(outputMarkdownIndexFolder, mdMapFileName));

            Assert.AreEqual(1, mdMapFileViewModel.Count);
            var references = mdMapFileViewModel["assets/markdown/aboutcodesnippet.md"].References;
            Assert.AreEqual(3, references.Count);
            var reference1 = references["../testclass1/catlibrary/class1.cs[20-46]"];
            Assert.IsNotNull(reference1);
            Assert.AreEqual(20, reference1.Startline);
            Assert.AreEqual(46, reference1.Endline);
            Assert.AreEqual("../TestClass1/CatLibrary/Class1.cs", reference1.Href);
            Assert.AreEqual(1, reference1.Keys.Count);
            Assert.AreEqual(@"{{'../TestClass1/CatLibrary/Class1.cs'[20-46]}}", reference1.Keys[0]);

            var reference2 = references["../testclass1/testclass1/class1.cs[0-]"];
            Assert.IsNotNull(reference2);
            Assert.AreEqual(0, reference2.Startline);
            Assert.AreEqual(-1, reference2.Endline);
            Assert.AreEqual("../TestClass1/TestClass1/Class1.cs", reference2.Href);
            Assert.AreEqual(2, reference2.Keys.Count);
            Assert.AreEqual(@"{{""../TestClass1/TestClass1/Class1.cs""}}", reference2.Keys[0]);
            Assert.AreEqual(@"{{../TestClass1\testClass1/Class1.cs[0-]}}", reference2.Keys[1]);

            var reference3 = references["CatLibrary.FakeDelegate`1"];
            Assert.IsNotNull(reference3);
            Assert.AreEqual(1, reference3.Keys.Count);
            Assert.AreEqual("@CatLibrary.FakeDelegate`1", reference3.Keys[0]);
        }

        private static void AssertFileExists(string expectedFileName, IEnumerable<string> fileNames)
        {
            Assert.IsTrue(
                fileNames.Any(s => s.EndsWith(expectedFileName, StringComparison.OrdinalIgnoreCase)),
                "map files should contain" + expectedFileName);
        }

        private static MapFileViewModel LoadMapFile(string path)
        {
            return YamlUtility.Deserialize<MapFileViewModel>(path);
        }
    }
}
