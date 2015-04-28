namespace UnitTest
{
    using System.Linq;
    using System.Threading.Tasks;

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

        [TestMethod]
        [DeploymentItem("Assets", "Assets")]
        public async Task TestGenereateMetadataAsync_CSharp_Generic()
        {
            string code = @"
using System.Collections.Generic
namespace Test1
{
    public class Class1<T> where T : struct
    {
        public TResult? Func1<TResult>(T? x) where T : struct
        {
            return null;
        }
        public IEnumerable<T> Items { get; set; }
        public event EventHandler Event1;
        public static bool operator ==(Class1<T> x, Class1<T> y) { return false; }
    }
}
";
            MetadataItem output = BuildMetaHelper.GenerateYamlMetadata(CreateCompilationFromCsharpCode(code));
            Assert.AreEqual(1, output.Items.Count);

            {
                var function = output.Items[0].Items[0].Items[0];
                Assert.IsNotNull(function);
                Assert.AreEqual("Func1<TResult>(T?)", function.DisplayNames.First().Value);
                Assert.AreEqual("Test1.Class1<T>.Func1<TResult>(T?)", function.DisplayQualifiedNames.First().Value);
                Assert.AreEqual("Test1.Class1`1.Func1``1(System.Nullable{`0})", function.Name);
                var parameter = function.Syntax.Parameters[0];
                Assert.AreEqual("x", parameter.Name);
                Assert.AreEqual("System.Nullable{`0}", parameter.Type.Name);
                Assert.IsTrue(parameter.Type.IsExternalPath);
                Assert.IsNull(parameter.Type.Href);
                var returnValue = function.Syntax.Return;
                Assert.IsNotNull(returnValue);
                Assert.IsNotNull(returnValue.Type);
                Assert.AreEqual("System.Nullable{``0}", returnValue.Type.Name);
                Assert.AreEqual("TResult?", returnValue.Type.DisplayName);
                Assert.AreEqual(@"public TResult? Func1<TResult>(T? x)where T : struct", function.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var proptery = output.Items[0].Items[0].Items[1];
                Assert.IsNotNull(proptery);
                Assert.AreEqual("Items", proptery.DisplayNames.First().Value);
                Assert.AreEqual("Test1.Class1<T>.Items", proptery.DisplayQualifiedNames.First().Value);
                Assert.AreEqual("Test1.Class1`1.Items", proptery.Name);
                Assert.AreEqual(1, proptery.Syntax.Parameters.Count);
                Assert.IsNull(proptery.Syntax.Return);
                //var returnValue = proptery.Syntax.Return;
                //Assert.IsNotNull(returnValue.Type);
                //Assert.AreEqual("System.Collection.Generic.IEnumerable{`0}", returnValue.Type.Name);
                //Assert.AreEqual("IEnumerable<T>", returnValue.Type.DisplayName);
                Assert.AreEqual(@"public IEnumerable<T> Items
{
    get;
    set;
}", proptery.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var event1 = output.Items[0].Items[0].Items[2];
                Assert.IsNotNull(event1);
                Assert.AreEqual("Event1", event1.DisplayNames.First().Value);
                Assert.AreEqual("Test1.Class1<T>.Event1", event1.DisplayQualifiedNames.First().Value);
                Assert.AreEqual("Test1.Class1`1.Event1", event1.Name);
                Assert.IsNull(event1.Syntax.Parameters);
                Assert.IsNull(event1.Syntax.Return);
                Assert.AreEqual("public event EventHandler Event1;", event1.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var operator1 = output.Items[0].Items[0].Items[3];
                Assert.IsNotNull(operator1);
                Assert.AreEqual("operator ==(Class1<T>, Class1<T>)", operator1.DisplayNames.First().Value);
                Assert.AreEqual("Test1.Class1<T>.operator ==(Test1.Class1<T>, Test1.Class1<T>)", operator1.DisplayQualifiedNames.First().Value);
                Assert.AreEqual("Test1.Class1`1.op_Equality(Test1.Class1{`0},Test1.Class1{`0})", operator1.Name);
                Assert.IsNotNull(operator1.Syntax.Parameters);

                var parameterX = operator1.Syntax.Parameters[0];
                Assert.AreEqual("x", parameterX.Name);
                Assert.AreEqual("Test1.Class1`1", parameterX.Type.Name);
                Assert.IsFalse(parameterX.Type.IsExternalPath);
                Assert.AreEqual("", parameterX.Type.Href);

                var parameterY = operator1.Syntax.Parameters[1];
                Assert.AreEqual("y", parameterY.Name);
                Assert.AreEqual("Test1.Class1`1", parameterY.Type.Name);
                Assert.IsFalse(parameterY.Type.IsExternalPath);
                Assert.AreEqual("", parameterY.Type.Href);

                Assert.IsNotNull(operator1.Syntax.Return);
                Assert.AreEqual("System.Boolean", operator1.Syntax.Return.Type.Name);
                Assert.IsTrue(operator1.Syntax.Return.Type.IsExternalPath);
                Assert.IsNull(operator1.Syntax.Return.Type.Href);

                Assert.AreEqual("public static bool operator ==(Class1<T> x, Class1<T> y)", operator1.Syntax.Content[SyntaxLanguage.CSharp]);
            }
        }

        [TestMethod]
        [DeploymentItem("Assets", "Assets")]
        public async Task TestGenereateMetadataAsync_CSharp_Interface()
        {
            string code = @"
namespace Test1
{
    public interface IFoo
    {
        string Bar(int x);
        int Count { get; }
        event EventHandler FooBar;
    }
}
";
            MetadataItem output = BuildMetaHelper.GenerateYamlMetadata(CreateCompilationFromCsharpCode(code));
            Assert.AreEqual(1, output.Items.Count);
            {
                var method = output.Items[0].Items[0].Items[0];
                Assert.IsNotNull(method);
                Assert.AreEqual("Bar(int)", method.DisplayNames.First().Value);
                Assert.AreEqual("Test1.IFoo.Bar(int)", method.DisplayQualifiedNames.First().Value);
                Assert.AreEqual("Test1.IFoo.Bar(System.Int32)", method.Name);
                var parameter = method.Syntax.Parameters[0];
                Assert.AreEqual("x", parameter.Name);
                Assert.AreEqual("System.Int32", parameter.Type.Name);
                Assert.IsTrue(parameter.Type.IsExternalPath);
                Assert.IsNull(parameter.Type.Href);
                var returnValue = method.Syntax.Return;
                Assert.IsNotNull(returnValue);
                Assert.IsNotNull(returnValue.Type);
                Assert.AreEqual("System.String", returnValue.Type.Name);
                Assert.AreEqual("string", returnValue.Type.DisplayName);
            }
            {
                var property = output.Items[0].Items[0].Items[1];
                Assert.IsNotNull(property);
                Assert.AreEqual("Count", property.DisplayNames.First().Value);
                Assert.AreEqual("Test1.IFoo.Count", property.DisplayQualifiedNames.First().Value);
                Assert.AreEqual("Test1.IFoo.Count", property.Name);
                Assert.AreEqual(1, property.Syntax.Parameters.Count);
                var returnValue = property.Syntax.Return;
                Assert.IsNull(returnValue);
                //Assert.IsNotNull(returnValue);
                //Assert.IsNotNull(returnValue.Type);
                //Assert.AreEqual("System.Int32", returnValue.Type.Name);
                //Assert.AreEqual("int", returnValue.Type.DisplayName);
            }
            {
                var @event = output.Items[0].Items[0].Items[2];
                Assert.IsNotNull(@event);
                Assert.AreEqual("FooBar", @event.DisplayNames.First().Value);
                Assert.AreEqual("Test1.IFoo.FooBar", @event.DisplayQualifiedNames.First().Value);
                Assert.AreEqual("Test1.IFoo.FooBar", @event.Name);
                Assert.AreEqual("event EventHandler FooBar;", @event.Syntax.Content[SyntaxLanguage.CSharp]);
                Assert.IsNull(@event.Syntax.Parameters);
                Assert.IsNull(@event.Syntax.Return);
                //Assert.IsNotNull(returnValue);
                //Assert.IsNotNull(returnValue.Type);
                //Assert.AreEqual("System.Int32", returnValue.Type.Name);
                //Assert.AreEqual("int", returnValue.Type.DisplayName);
            }
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
