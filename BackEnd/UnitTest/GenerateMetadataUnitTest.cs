namespace UnitTest
{
    using System.Linq;
    using System.Threading.Tasks;

    using DocAsCode.BuildMeta;
    using DocAsCode.EntityModel;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.MSBuild;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// MEF is used for workspace host service provider, need to copy dll manually
    /// </summary>
    [TestClass]
    [DeploymentItem("NativeBinaries", "NativeBinaries")]
    [DeploymentItem("Microsoft.CodeAnalysis.CSharp.Workspaces.dll")]
    [DeploymentItem("Microsoft.CodeAnalysis.CSharp.Workspaces.Desktop.dll")]
    [DeploymentItem("Microsoft.CodeAnalysis.VisualBasic.Workspaces.dll")]
    [DeploymentItem("Microsoft.CodeAnalysis.VisualBasic.Workspaces.Desktop.dll")]
    public class GenerateMetadataUnitTest
    {
        private static readonly MSBuildWorkspace Workspace = MSBuildWorkspace.Create();

        [TestMethod]
        [DeploymentItem("Assets", "Assets")]
        public async Task TestGenereateMetadataAsync_Csharp_FuncVoidReturn()
        {
            string code = @"
namespace Test1
{
    public class Class1
    {
        public void Func1(int i)
        {
            return;
        }
    }
}
";
            MetadataItem output = BuildMetaHelper.GenerateYamlMetadata(CreateCompilationFromCsharpCode(code));
            var function = output.Items[0].Items[0].Items[0];
            Assert.IsNotNull(function);
            Assert.AreEqual("Func1(int)", function.DisplayNames.First().Value);
            Assert.AreEqual("Test1.Class1.Func1(int)", function.DisplayQualifiedNames.First().Value);
            Assert.AreEqual("Test1.Class1.Func1(System.Int32)", function.Name);
            Assert.AreEqual(1, output.Items.Count);
            var parameter = function.Syntax.Parameters[0];
            Assert.AreEqual("i", parameter.Name);
            Assert.AreEqual("System.Int32", parameter.Type.Name);
            Assert.IsTrue(parameter.Type.IsExternalPath);
            Assert.IsNull(parameter.Type.Href);
            var returnValue = function.Syntax.Return;
            Assert.IsNull(returnValue);
        }

        private static Compilation CreateCompilationFromCsharpCode(string code)
        {
            var tree = SyntaxFactory.ParseSyntaxTree(code);
            var compilation = CSharpCompilation.Create(
                "test.dll",
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                syntaxTrees: new[] { tree },
                references: new[] { MetadataReference.CreateFromAssembly(typeof(object).Assembly) });
            return compilation;
        }
    }
}
