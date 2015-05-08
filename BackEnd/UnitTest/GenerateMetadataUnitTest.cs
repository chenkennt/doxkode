namespace UnitTest
{
    using System.Linq;
    using System.Threading.Tasks;

    using DocAsCode.EntityModel;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.MSBuild;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using CS = Microsoft.CodeAnalysis.CSharp;
    using VB = Microsoft.CodeAnalysis.VisualBasic;

    /// <summary>
    /// MEF is used for workspace host service provider, need to copy dll manually
    /// </summary>
    [TestClass]
    [DeploymentItem("Microsoft.CodeAnalysis.CSharp.Workspaces.dll")]
    [DeploymentItem("Microsoft.CodeAnalysis.CSharp.Workspaces.Desktop.dll")]
    [DeploymentItem("Microsoft.CodeAnalysis.VisualBasic.Workspaces.dll")]
    [DeploymentItem("Microsoft.CodeAnalysis.VisualBasic.Workspaces.Desktop.dll")]
    public class GenerateMetadataUnitTest
    {
        private static readonly MSBuildWorkspace Workspace = MSBuildWorkspace.Create();

        #region CSharp

        [TestMethod]
        [DeploymentItem("Assets", "Assets")]
        public void TestGenereateMetadataAsync_Csharp_FuncVoidReturn()
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
        public void TestGenereateMetadata_CSharp_GenericClass()
        {
            string code = @"
using System.Collections.Generic
namespace Test1
{
    public sealed class Class1<T> where T : struct, IEnumerable<T>
    {
        public TResult? Func1<TResult>(T? x, IEnumerable<T> y) where TResult : struct
        {
            return null;
        }
        public IEnumerable<T> Items { get; set; }
        public event EventHandler Event1;
        public static bool operator ==(Class1<T> x, Class1<T> y) { return false; }
        public IEnumerable<T> Items2 { get; private set; }
    }
}
";
            MetadataItem output = BuildMetaHelper.GenerateYamlMetadata(CreateCompilationFromCsharpCode(code));
            Assert.AreEqual(1, output.Items.Count);
            {
                var type = output.Items[0].Items[0];
                Assert.IsNotNull(type);
                Assert.AreEqual("Class1<T>", type.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Class1<T>", type.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Class1`1", type.Name);
                Assert.AreEqual(@"public sealed class Class1<T>
    where T : struct, IEnumerable<T>", type.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var function = output.Items[0].Items[0].Items[0];
                Assert.IsNotNull(function);
                Assert.AreEqual("Func1<TResult>(T?, IEnumerable<T>)", function.DisplayNames.First().Value);
                Assert.AreEqual("Test1.Class1<T>.Func1<TResult>(T?, System.Collections.Generic.IEnumerable<T>)", function.DisplayQualifiedNames.First().Value);
                Assert.AreEqual("Test1.Class1`1.Func1``1(System.Nullable{`0},System.Collections.Generic.IEnumerable{`0})", function.Name);

                var parameterX = function.Syntax.Parameters[0];
                Assert.AreEqual("x", parameterX.Name);
                Assert.AreEqual("System.Nullable{`0}", parameterX.Type.Name);
                Assert.AreEqual("T?", parameterX.Type.DisplayName);
                Assert.IsTrue(parameterX.Type.IsExternalPath);
                Assert.IsNull(parameterX.Type.Href);

                var parameterY = function.Syntax.Parameters[1];
                Assert.AreEqual("y", parameterY.Name);
                Assert.AreEqual("System.Collections.Generic.IEnumerable{`0}", parameterY.Type.Name);
                Assert.AreEqual("IEnumerable<T>", parameterY.Type.DisplayName);
                Assert.IsTrue(parameterY.Type.IsExternalPath);
                Assert.IsNull(parameterY.Type.Href);

                var returnValue = function.Syntax.Return;
                Assert.IsNotNull(returnValue);
                Assert.IsNotNull(returnValue.Type);
                Assert.AreEqual("System.Nullable{``0}", returnValue.Type.Name);
                Assert.AreEqual("TResult?", returnValue.Type.DisplayName);
                Assert.AreEqual(@"public TResult? Func1<TResult>(T? x, IEnumerable<T> y)where TResult : struct", function.Syntax.Content[SyntaxLanguage.CSharp]);
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
                Assert.AreEqual("public event EventHandler Event1", event1.Syntax.Content[SyntaxLanguage.CSharp]);
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
                Assert.AreEqual("Class1<T>", parameterX.Type.DisplayName);
                Assert.IsFalse(parameterX.Type.IsExternalPath);
                Assert.AreEqual("", parameterX.Type.Href);

                var parameterY = operator1.Syntax.Parameters[1];
                Assert.AreEqual("y", parameterY.Name);
                Assert.AreEqual("Test1.Class1`1", parameterY.Type.Name);
                Assert.AreEqual("Class1<T>", parameterY.Type.DisplayName);
                Assert.IsFalse(parameterY.Type.IsExternalPath);
                Assert.AreEqual("", parameterY.Type.Href);

                Assert.IsNotNull(operator1.Syntax.Return);
                Assert.AreEqual("System.Boolean", operator1.Syntax.Return.Type.Name);
                Assert.IsTrue(operator1.Syntax.Return.Type.IsExternalPath);
                Assert.IsNull(operator1.Syntax.Return.Type.Href);

                Assert.AreEqual("public static bool operator ==(Class1<T> x, Class1<T> y)", operator1.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var proptery = output.Items[0].Items[0].Items[4];
                Assert.IsNotNull(proptery);
                Assert.AreEqual("Items2", proptery.DisplayNames.First().Value);
                Assert.AreEqual("Test1.Class1<T>.Items2", proptery.DisplayQualifiedNames.First().Value);
                Assert.AreEqual("Test1.Class1`1.Items2", proptery.Name);
                Assert.AreEqual(1, proptery.Syntax.Parameters.Count);
                Assert.IsNull(proptery.Syntax.Return);
                //var returnValue = proptery.Syntax.Return;
                //Assert.IsNotNull(returnValue.Type);
                //Assert.AreEqual("System.Collection.Generic.IEnumerable{`0}", returnValue.Type.Name);
                //Assert.AreEqual("IEnumerable<T>", returnValue.Type.DisplayName);
                Assert.AreEqual(@"public IEnumerable<T> Items2
{
    get;
}", proptery.Syntax.Content[SyntaxLanguage.CSharp]);
            }
        }

        [TestMethod]
        [DeploymentItem("Assets", "Assets")]
        public void TestGenereateMetadata_CSharp_Interface()
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
                Assert.AreEqual("event EventHandler FooBar", @event.Syntax.Content[SyntaxLanguage.CSharp]);
                Assert.IsNull(@event.Syntax.Parameters);
                Assert.IsNull(@event.Syntax.Return);
                //Assert.IsNotNull(returnValue);
                //Assert.IsNotNull(returnValue.Type);
                //Assert.AreEqual("System.Int32", returnValue.Type.Name);
                //Assert.AreEqual("int", returnValue.Type.DisplayName);
            }
        }

        [TestMethod]
        [DeploymentItem("Assets", "Assets")]
        public void TestGenereateMetadata_CSharp_Interface_Inherits()
        {
            string code = @"
namespace Test1
{
    public interface IFoo { }
    public interface IBar : IFoo { }
    public interface IFooBar : IBar { }
}
";
            MetadataItem output = BuildMetaHelper.GenerateYamlMetadata(CreateCompilationFromCsharpCode(code));
            Assert.AreEqual(1, output.Items.Count);

            var ifoo = output.Items[0].Items[0];
            Assert.IsNotNull(ifoo);
            Assert.AreEqual("IFoo", ifoo.DisplayNames[SyntaxLanguage.CSharp]);
            Assert.AreEqual("Test1.IFoo", ifoo.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
            Assert.AreEqual("public interface IFoo", ifoo.Syntax.Content[SyntaxLanguage.CSharp]);

            var ibar = output.Items[0].Items[1];
            Assert.IsNotNull(ibar);
            Assert.AreEqual("IBar", ibar.DisplayNames[SyntaxLanguage.CSharp]);
            Assert.AreEqual("Test1.IBar", ibar.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
            Assert.AreEqual("public interface IBar : IFoo", ibar.Syntax.Content[SyntaxLanguage.CSharp]);

            var ifoobar = output.Items[0].Items[2];
            Assert.IsNotNull(ifoobar);
            Assert.AreEqual("IFooBar", ifoobar.DisplayNames[SyntaxLanguage.CSharp]);
            Assert.AreEqual("Test1.IFooBar", ifoobar.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
            Assert.AreEqual("public interface IFooBar : IBar, IFoo", ifoobar.Syntax.Content[SyntaxLanguage.CSharp]);
        }

        [TestMethod]
        [DeploymentItem("Assets", "Assets")]
        public void TestGenereateMetadata_CSharp_Class_Inherits()
        {
            string code = @"
namespace Test1
{
    public class Foo : IFoo { }
    public class Bar : Foo, IBar { }
    public class FooBar : Bar, IFooBar { }
    public interface IFoo { }
    public interface IBar { }
    public interface IFooBar : IFoo, IBar { }
}
";
            MetadataItem output = BuildMetaHelper.GenerateYamlMetadata(CreateCompilationFromCsharpCode(code));
            Assert.AreEqual(1, output.Items.Count);

            var foo = output.Items[0].Items[0];
            Assert.IsNotNull(foo);
            Assert.AreEqual("Foo", foo.DisplayNames[SyntaxLanguage.CSharp]);
            Assert.AreEqual("Test1.Foo", foo.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
            Assert.AreEqual("public class Foo : IFoo", foo.Syntax.Content[SyntaxLanguage.CSharp]);

            var bar = output.Items[0].Items[1];
            Assert.IsNotNull(bar);
            Assert.AreEqual("Bar", bar.DisplayNames[SyntaxLanguage.CSharp]);
            Assert.AreEqual("Test1.Bar", bar.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
            Assert.AreEqual("public class Bar : Foo, IFoo, IBar", bar.Syntax.Content[SyntaxLanguage.CSharp]);

            var foobar = output.Items[0].Items[2];
            Assert.IsNotNull(foobar);
            Assert.AreEqual("FooBar", foobar.DisplayNames[SyntaxLanguage.CSharp]);
            Assert.AreEqual("Test1.FooBar", foobar.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
            Assert.AreEqual("public class FooBar : Bar, IFooBar, IFoo, IBar", foobar.Syntax.Content[SyntaxLanguage.CSharp]);
        }

        [TestMethod]
        [DeploymentItem("Assets", "Assets")]
        public void TestGenereateMetadata_CSharp_Enum()
        {
            string code = @"
namespace Test1
{
    public enum ABC{A,B,C}
    public enum YN : byte {Y=1, N=0}
    public enum XYZ:int{X,Y,Z}
}
";
            MetadataItem output = BuildMetaHelper.GenerateYamlMetadata(CreateCompilationFromCsharpCode(code));
            Assert.AreEqual(1, output.Items.Count);
            {
                var type = output.Items[0].Items[0];
                Assert.IsNotNull(type);
                Assert.AreEqual("ABC", type.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.ABC", type.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.ABC", type.Name);
                Assert.AreEqual("public enum ABC", type.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var type = output.Items[0].Items[1];
                Assert.IsNotNull(type);
                Assert.AreEqual("YN", type.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.YN", type.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.YN", type.Name);
                Assert.AreEqual("public enum YN : byte", type.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var type = output.Items[0].Items[2];
                Assert.IsNotNull(type);
                Assert.AreEqual("XYZ", type.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.XYZ", type.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.XYZ", type.Name);
                Assert.AreEqual("public enum XYZ", type.Syntax.Content[SyntaxLanguage.CSharp]);
            }
        }

        [TestMethod]
        [DeploymentItem("Assets", "Assets")]
        public void TestGenereateMetadata_CSharp_Struct()
        {
            string code = @"
using System.Collections
using System.Collections.Generic
namespace Test1
{
    public struct Foo{}
    public struct Bar<T> : IEnumerable<T>
    {
        public IEnumerator<T> GetEnumerator() => null;
        IEnumerator IEnumerable.GetEnumerator() => null;
    }
}
";
            MetadataItem output = BuildMetaHelper.GenerateYamlMetadata(CreateCompilationFromCsharpCode(code));
            Assert.AreEqual(1, output.Items.Count);
            {
                var type = output.Items[0].Items[0];
                Assert.IsNotNull(type);
                Assert.AreEqual("Foo", type.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo", type.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo", type.Name);
                Assert.AreEqual("public struct Foo", type.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var type = output.Items[0].Items[1];
                Assert.IsNotNull(type);
                Assert.AreEqual("Bar<T>", type.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Bar<T>", type.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Bar`1", type.Name);
                Assert.AreEqual("public struct Bar<T> : IEnumerable<T>, IEnumerable", type.Syntax.Content[SyntaxLanguage.CSharp]);
            }
        }

        [TestMethod]
        [DeploymentItem("Assets", "Assets")]
        public void TestGenereateMetadata_CSharp_Delegate()
        {
            string code = @"
using System.Collections.Generic
namespace Test1
{
    public delegate void Foo();
    public delegate T Bar<T>(IEnumerable<T> x = null) where T : class;
    public delegate void FooBar(ref int x, out string y, params byte[] z);
}
";
            MetadataItem output = BuildMetaHelper.GenerateYamlMetadata(CreateCompilationFromCsharpCode(code));
            Assert.AreEqual(1, output.Items.Count);
            {
                var type = output.Items[0].Items[0];
                Assert.IsNotNull(type);
                Assert.AreEqual("Foo", type.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo", type.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo", type.Name);
                Assert.AreEqual("public delegate void Foo();", type.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var type = output.Items[0].Items[1];
                Assert.IsNotNull(type);
                Assert.AreEqual("Bar<T>", type.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Bar<T>", type.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Bar`1", type.Name);
                Assert.AreEqual(@"public delegate T Bar<T>(IEnumerable<T> x = null)where T : class;", type.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var type = output.Items[0].Items[2];
                Assert.IsNotNull(type);
                Assert.AreEqual("FooBar", type.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.FooBar", type.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.FooBar", type.Name);
                Assert.AreEqual(@"public delegate void FooBar(ref int x, out string y, params byte[] z);", type.Syntax.Content[SyntaxLanguage.CSharp]);
            }
        }

        [TestMethod]
        [DeploymentItem("Assets", "Assets")]
        public void TestGenereateMetadata_CSharp_Method()
        {
            string code = @"
using System.Threading.Tasks
namespace Test1
{
    public abstract class Foo<T>
    {
        public abstract void M1();
        protected virtual Foo<T> M2<TArg>(TArg arg) where TArg : Foo<T> => this;
        public static TResult M3<TResult>(string x) where TResult : class => null;
        public void M4(int x){}
    }
    public class Bar : Foo<string>, IFooBar
    {
        public override void M1(){}
        protected override sealed Foo<T> M2<TArg>(TArg arg) where TArg : Foo<string> => this;
        public int M5<TArg>(TArg arg) where TArg : struct, new() => 2;
    }
    public interface IFooBar
    {
        void M1();
        Foo<T> M2<TArg>(TArg arg) where TArg : Foo<string>;
        int M5<TArg>(TArg arg) where TArg : struct, new();
    }
}
";
            MetadataItem output = BuildMetaHelper.GenerateYamlMetadata(CreateCompilationFromCsharpCode(code));
            Assert.AreEqual(1, output.Items.Count);
            {
                var method = output.Items[0].Items[0].Items[0];
                Assert.IsNotNull(method);
                Assert.AreEqual("M1()", method.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo<T>.M1()", method.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo`1.M1", method.Name);
                Assert.AreEqual("public abstract void M1()", method.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var method = output.Items[0].Items[0].Items[1];
                Assert.IsNotNull(method);
                Assert.AreEqual("M2<TArg>(TArg)", method.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo<T>.M2<TArg>(TArg)", method.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo`1.M2``1(``0)", method.Name);
                Assert.AreEqual("protected virtual Foo<T> M2<TArg>(TArg arg)where TArg : Foo<T>", method.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var method = output.Items[0].Items[0].Items[2];
                Assert.IsNotNull(method);
                Assert.AreEqual("M3<TResult>(string)", method.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo<T>.M3<TResult>(string)", method.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo`1.M3``1(System.String)", method.Name);
                Assert.AreEqual("public static TResult M3<TResult>(string x)where TResult : class", method.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var method = output.Items[0].Items[0].Items[3];
                Assert.IsNotNull(method);
                Assert.AreEqual("M4(int)", method.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo<T>.M4(int)", method.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo`1.M4(System.Int32)", method.Name);
                Assert.AreEqual("public void M4(int x)", method.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var method = output.Items[0].Items[1].Items[0];
                Assert.IsNotNull(method);
                Assert.AreEqual("M1()", method.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Bar.M1()", method.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Bar.M1", method.Name);
                Assert.AreEqual("public override void M1()", method.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var method = output.Items[0].Items[1].Items[1];
                Assert.IsNotNull(method);
                Assert.AreEqual("M2<TArg>(TArg)", method.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Bar.M2<TArg>(TArg)", method.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Bar.M2``1(``0)", method.Name);
                Assert.AreEqual("protected override sealed Foo<T> M2<TArg>(TArg arg)where TArg : Foo<string>", method.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var method = output.Items[0].Items[1].Items[2];
                Assert.IsNotNull(method);
                Assert.AreEqual("M5<TArg>(TArg)", method.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Bar.M5<TArg>(TArg)", method.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Bar.M5``1(``0)", method.Name);
                Assert.AreEqual("public int M5<TArg>(TArg arg)where TArg : struct, new ()", method.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var method = output.Items[0].Items[2].Items[0];
                Assert.IsNotNull(method);
                Assert.AreEqual("M1()", method.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.IFooBar.M1()", method.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.IFooBar.M1", method.Name);
                Assert.AreEqual("void M1()", method.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var method = output.Items[0].Items[2].Items[1];
                Assert.IsNotNull(method);
                Assert.AreEqual("M2<TArg>(TArg)", method.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.IFooBar.M2<TArg>(TArg)", method.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.IFooBar.M2``1(``0)", method.Name);
                Assert.AreEqual("Foo<T> M2<TArg>(TArg arg)where TArg : Foo<string>", method.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var method = output.Items[0].Items[2].Items[2];
                Assert.IsNotNull(method);
                Assert.AreEqual("M5<TArg>(TArg)", method.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.IFooBar.M5<TArg>(TArg)", method.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.IFooBar.M5``1(``0)", method.Name);
                Assert.AreEqual("int M5<TArg>(TArg arg)where TArg : struct, new ()", method.Syntax.Content[SyntaxLanguage.CSharp]);
            }
        }

        [TestMethod]
        [DeploymentItem("Assets", "Assets")]
        public void TestGenereateMetadata_CSharp_Eii()
        {
            string code = @"
using System.Collections.Generic
namespace Test1
{
    public class Foo<T> : IFoo, IFoo<string>, IFoo<T> where T : class
    {
        object IFoo.Bar(ref int x) => null;
        string IFoo<string>.Bar<TArg>(TArg[] x) => "";
        T IFoo<T>.Bar<TArg>(TArg[] x) => null;
        string IFoo<string>.P { get; set; }
        T IFoo<T>.P { get; set; }
        int IFoo<string>.this[string x] { get { return 1; } }
        int IFoo<T>.this[T x] { get { return 1; } }
        event EventHandler IFoo.E { add { } remove { } }
    }
    public interface IFoo
    {
        object Bar(ref int x);
        event EventHandler E;
    }
    public interface IFoo<out T>
    {
        T Bar<TArg>(TArg[] x)
        T P { get; set; }
        int this[T x] { get; }
    }
}
";
            MetadataItem output = BuildMetaHelper.GenerateYamlMetadata(CreateCompilationFromCsharpCode(code));
            Assert.AreEqual(1, output.Items.Count);
            {
                var type = output.Items[0].Items[0];
                Assert.IsNotNull(type);
                Assert.AreEqual("Foo<T>", type.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo<T>", type.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo`1", type.Name);
                Assert.AreEqual(@"public class Foo<T> : IFoo, IFoo<string>, IFoo<T> where T : class", type.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var method = output.Items[0].Items[0].Items[0];
                Assert.IsNotNull(method);
                Assert.AreEqual("IFoo.Bar(ref int)", method.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo<T>.Test1.IFoo.Bar(ref int)", method.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo`1.Test1#IFoo#Bar(System.Int32@)", method.Name);
                Assert.AreEqual(@"object IFoo.Bar(ref int x)", method.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var method = output.Items[0].Items[0].Items[1];
                Assert.IsNotNull(method);
                Assert.AreEqual("IFoo<string>.Bar<TArg>(TArg[])", method.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo<T>.Test1.IFoo<string>.Bar<TArg>(TArg[])", method.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo`1.Test1#IFoo{System#String}#Bar``1(``0[])", method.Name);
                Assert.AreEqual(@"string IFoo<string>.Bar<TArg>(TArg[] x)", method.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var method = output.Items[0].Items[0].Items[2];
                Assert.IsNotNull(method);
                Assert.AreEqual("IFoo<T>.Bar<TArg>(TArg[])", method.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo<T>.Test1.IFoo<T>.Bar<TArg>(TArg[])", method.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo`1.Test1#IFoo{T}#Bar``1(``0[])", method.Name);
                Assert.AreEqual(@"T IFoo<T>.Bar<TArg>(TArg[] x)", method.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var p = output.Items[0].Items[0].Items[3];
                Assert.IsNotNull(p);
                Assert.AreEqual("IFoo<string>.P", p.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo<T>.Test1.IFoo<string>.P", p.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo`1.Test1#IFoo{System#String}#P", p.Name);
                Assert.AreEqual(@"string IFoo<string>.P
{
    get;
    set;
}", p.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var p = output.Items[0].Items[0].Items[4];
                Assert.IsNotNull(p);
                Assert.AreEqual("IFoo<T>.P", p.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo<T>.Test1.IFoo<T>.P", p.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo`1.Test1#IFoo{T}#P", p.Name);
                Assert.AreEqual(@"T IFoo<T>.P
{
    get;
    set;
}", p.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var p = output.Items[0].Items[0].Items[5];
                Assert.IsNotNull(p);
                Assert.AreEqual("IFoo<string>.this[string]", p.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo<T>.Test1.IFoo<string>.this[string]", p.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo`1.Test1#IFoo{System#String}#Item(System.String)", p.Name);
                Assert.AreEqual(@"int IFoo<string>.this[string x]
{
    get;
}", p.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var p = output.Items[0].Items[0].Items[6];
                Assert.IsNotNull(p);
                Assert.AreEqual("IFoo<T>.this[T]", p.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo<T>.Test1.IFoo<T>.this[T]", p.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo`1.Test1#IFoo{T}#Item(`0)", p.Name);
                Assert.AreEqual(@"int IFoo<T>.this[T x]
{
    get;
}", p.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var e = output.Items[0].Items[0].Items[7];
                Assert.IsNotNull(e);
                Assert.AreEqual("IFoo.E", e.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo<T>.Test1.IFoo.E", e.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo`1.Test1#IFoo#E", e.Name);
                Assert.AreEqual(@"event EventHandler IFoo.E", e.Syntax.Content[SyntaxLanguage.CSharp]);
            }
        }

        [TestMethod]
        [DeploymentItem("Assets", "Assets")]
        public void TestGenereateMetadata_CSharp_Operator()
        {
            string code = @"
using System.Collections.Generic
namespace Test1
{
    public class Foo
    {
        // unary
        public static Foo operator +(Foo x) => x;
        public static Foo operator -(Foo x) => x;
        public static Foo operator !(Foo x) => x;
        public static Foo operator ~(Foo x) => x;
        public static Foo operator ++(Foo x) => x;
        public static Foo operator --(Foo x) => x;
        public static Foo operator true(Foo x) => x;
        public static Foo operator false(Foo x) => x;
        // binary
        public static Foo operator +(Foo x, int y) => x;
        public static Foo operator -(Foo x, int y) => x;
        public static Foo operator *(Foo x, int y) => x;
        public static Foo operator /(Foo x, int y) => x;
        public static Foo operator %(Foo x, int y) => x;
        public static Foo operator &(Foo x, int y) => x;
        public static Foo operator |(Foo x, int y) => x;
        public static Foo operator ^(Foo x, int y) => x;
        public static Foo operator >>(Foo x, int y) => x;
        public static Foo operator <<(Foo x, int y) => x;
        // comparison
        public static bool operator ==(Foo x, int y) => false;
        public static bool operator !=(Foo x, int y) => false;
        public static bool operator >(Foo x, int y) => false;
        public static bool operator <(Foo x, int y) => false;
        public static bool operator >=(Foo x, int y) => false;
        public static bool operator <=(Foo x, int y) => false;
        // convertion
        public static implicit operator Foo (int x) => null;
        public static explicit operator int (Foo x) => 0;
    }
}
";
            MetadataItem output = BuildMetaHelper.GenerateYamlMetadata(CreateCompilationFromCsharpCode(code));
            Assert.AreEqual(1, output.Items.Count);
            // unary
            {
                var method = output.Items[0].Items[0].Items[0];
                Assert.IsNotNull(method);
                Assert.AreEqual("operator +(Foo)", method.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo.operator +(Test1.Foo)", method.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo.op_UnaryPlus(Test1.Foo)", method.Name);
                Assert.AreEqual(@"public static Foo operator +(Foo x)", method.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var method = output.Items[0].Items[0].Items[1];
                Assert.IsNotNull(method);
                Assert.AreEqual("operator -(Foo)", method.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo.operator -(Test1.Foo)", method.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo.op_UnaryNegation(Test1.Foo)", method.Name);
                Assert.AreEqual(@"public static Foo operator -(Foo x)", method.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var method = output.Items[0].Items[0].Items[2];
                Assert.IsNotNull(method);
                Assert.AreEqual("operator !(Foo)", method.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo.operator !(Test1.Foo)", method.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo.op_LogicalNot(Test1.Foo)", method.Name);
                Assert.AreEqual(@"public static Foo operator !(Foo x)", method.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var method = output.Items[0].Items[0].Items[3];
                Assert.IsNotNull(method);
                Assert.AreEqual("operator ~(Foo)", method.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo.operator ~(Test1.Foo)", method.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo.op_OnesComplement(Test1.Foo)", method.Name);
                Assert.AreEqual(@"public static Foo operator ~(Foo x)", method.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var method = output.Items[0].Items[0].Items[4];
                Assert.IsNotNull(method);
                Assert.AreEqual("operator ++(Foo)", method.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo.operator ++(Test1.Foo)", method.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo.op_Increment(Test1.Foo)", method.Name);
                Assert.AreEqual(@"public static Foo operator ++(Foo x)", method.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var method = output.Items[0].Items[0].Items[5];
                Assert.IsNotNull(method);
                Assert.AreEqual("operator --(Foo)", method.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo.operator --(Test1.Foo)", method.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo.op_Decrement(Test1.Foo)", method.Name);
                Assert.AreEqual(@"public static Foo operator --(Foo x)", method.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var method = output.Items[0].Items[0].Items[6];
                Assert.IsNotNull(method);
                Assert.AreEqual("operator true(Foo)", method.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo.operator true(Test1.Foo)", method.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo.op_True(Test1.Foo)", method.Name);
                Assert.AreEqual(@"public static Foo operator true (Foo x)", method.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var method = output.Items[0].Items[0].Items[7];
                Assert.IsNotNull(method);
                Assert.AreEqual("operator false(Foo)", method.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo.operator false(Test1.Foo)", method.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo.op_False(Test1.Foo)", method.Name);
                Assert.AreEqual(@"public static Foo operator false (Foo x)", method.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            // binary
            {
                var method = output.Items[0].Items[0].Items[8];
                Assert.IsNotNull(method);
                Assert.AreEqual("operator +(Foo, int)", method.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo.operator +(Test1.Foo, int)", method.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo.op_Addition(Test1.Foo,System.Int32)", method.Name);
                Assert.AreEqual(@"public static Foo operator +(Foo x, int y)", method.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var method = output.Items[0].Items[0].Items[9];
                Assert.IsNotNull(method);
                Assert.AreEqual("operator -(Foo, int)", method.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo.operator -(Test1.Foo, int)", method.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo.op_Subtraction(Test1.Foo,System.Int32)", method.Name);
                Assert.AreEqual(@"public static Foo operator -(Foo x, int y)", method.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var method = output.Items[0].Items[0].Items[10];
                Assert.IsNotNull(method);
                Assert.AreEqual("operator *(Foo, int)", method.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo.operator *(Test1.Foo, int)", method.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo.op_Multiply(Test1.Foo,System.Int32)", method.Name);
                Assert.AreEqual(@"public static Foo operator *(Foo x, int y)", method.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var method = output.Items[0].Items[0].Items[11];
                Assert.IsNotNull(method);
                Assert.AreEqual("operator /(Foo, int)", method.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo.operator /(Test1.Foo, int)", method.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo.op_Division(Test1.Foo,System.Int32)", method.Name);
                Assert.AreEqual(@"public static Foo operator /(Foo x, int y)", method.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var method = output.Items[0].Items[0].Items[12];
                Assert.IsNotNull(method);
                Assert.AreEqual("operator %(Foo, int)", method.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo.operator %(Test1.Foo, int)", method.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo.op_Modulus(Test1.Foo,System.Int32)", method.Name);
                Assert.AreEqual(@"public static Foo operator %(Foo x, int y)", method.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var method = output.Items[0].Items[0].Items[13];
                Assert.IsNotNull(method);
                Assert.AreEqual("operator &(Foo, int)", method.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo.operator &(Test1.Foo, int)", method.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo.op_BitwiseAnd(Test1.Foo,System.Int32)", method.Name);
                Assert.AreEqual(@"public static Foo operator &(Foo x, int y)", method.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var method = output.Items[0].Items[0].Items[14];
                Assert.IsNotNull(method);
                Assert.AreEqual("operator |(Foo, int)", method.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo.operator |(Test1.Foo, int)", method.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo.op_BitwiseOr(Test1.Foo,System.Int32)", method.Name);
                Assert.AreEqual(@"public static Foo operator |(Foo x, int y)", method.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var method = output.Items[0].Items[0].Items[15];
                Assert.IsNotNull(method);
                Assert.AreEqual("operator ^(Foo, int)", method.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo.operator ^(Test1.Foo, int)", method.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo.op_ExclusiveOr(Test1.Foo,System.Int32)", method.Name);
                Assert.AreEqual(@"public static Foo operator ^(Foo x, int y)", method.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var method = output.Items[0].Items[0].Items[16];
                Assert.IsNotNull(method);
                Assert.AreEqual("operator >>(Foo, int)", method.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo.operator >>(Test1.Foo, int)", method.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo.op_RightShift(Test1.Foo,System.Int32)", method.Name);
                Assert.AreEqual(@"public static Foo operator >>(Foo x, int y)", method.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var method = output.Items[0].Items[0].Items[17];
                Assert.IsNotNull(method);
                Assert.AreEqual("operator <<(Foo, int)", method.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo.operator <<(Test1.Foo, int)", method.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo.op_LeftShift(Test1.Foo,System.Int32)", method.Name);
                Assert.AreEqual(@"public static Foo operator <<(Foo x, int y)", method.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            // comparison
            {
                var method = output.Items[0].Items[0].Items[18];
                Assert.IsNotNull(method);
                Assert.AreEqual("operator ==(Foo, int)", method.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo.operator ==(Test1.Foo, int)", method.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo.op_Equality(Test1.Foo,System.Int32)", method.Name);
                Assert.AreEqual(@"public static bool operator ==(Foo x, int y)", method.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var method = output.Items[0].Items[0].Items[19];
                Assert.IsNotNull(method);
                Assert.AreEqual("operator !=(Foo, int)", method.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo.operator !=(Test1.Foo, int)", method.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo.op_Inequality(Test1.Foo,System.Int32)", method.Name);
                Assert.AreEqual(@"public static bool operator !=(Foo x, int y)", method.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var method = output.Items[0].Items[0].Items[20];
                Assert.IsNotNull(method);
                Assert.AreEqual("operator >(Foo, int)", method.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo.operator >(Test1.Foo, int)", method.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo.op_GreaterThan(Test1.Foo,System.Int32)", method.Name);
                Assert.AreEqual(@"public static bool operator>(Foo x, int y)", method.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var method = output.Items[0].Items[0].Items[21];
                Assert.IsNotNull(method);
                Assert.AreEqual("operator <(Foo, int)", method.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo.operator <(Test1.Foo, int)", method.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo.op_LessThan(Test1.Foo,System.Int32)", method.Name);
                Assert.AreEqual(@"public static bool operator <(Foo x, int y)", method.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var method = output.Items[0].Items[0].Items[22];
                Assert.IsNotNull(method);
                Assert.AreEqual("operator >=(Foo, int)", method.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo.operator >=(Test1.Foo, int)", method.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo.op_GreaterThanOrEqual(Test1.Foo,System.Int32)", method.Name);
                Assert.AreEqual(@"public static bool operator >=(Foo x, int y)", method.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var method = output.Items[0].Items[0].Items[23];
                Assert.IsNotNull(method);
                Assert.AreEqual("operator <=(Foo, int)", method.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo.operator <=(Test1.Foo, int)", method.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo.op_LessThanOrEqual(Test1.Foo,System.Int32)", method.Name);
                Assert.AreEqual(@"public static bool operator <=(Foo x, int y)", method.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            // conversion
            {
                var method = output.Items[0].Items[0].Items[24];
                Assert.IsNotNull(method);
                Assert.AreEqual("implicit operator Foo(int)", method.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo.implicit operator Test1.Foo(int)", method.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo.op_Implicit(System.Int32)~Test1.Foo", method.Name);
                Assert.AreEqual(@"public static implicit operator Foo(int x)", method.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var method = output.Items[0].Items[0].Items[25];
                Assert.IsNotNull(method);
                Assert.AreEqual("explicit operator int(Foo)", method.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo.explicit operator int(Test1.Foo)", method.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo.op_Explicit(Test1.Foo)~System.Int32", method.Name);
                Assert.AreEqual(@"public static explicit operator int (Foo x)", method.Syntax.Content[SyntaxLanguage.CSharp]);
            }
        }

        [TestMethod]
        [DeploymentItem("Assets", "Assets")]
        public void TestGenereateMetadata_CSharp_Constructor()
        {
            string code = @"
namespace Test1
{
    public class Foo<T>
    {
        static Foo(){}
        public Foo(){}
        public Foo(int x) : base(x){}
        protected internal Foo(string x) : base(0){}
    }
    public class Bar
    {
        public Bar(){}
        protected Bar(int x){}
    }
}
";
            MetadataItem output = BuildMetaHelper.GenerateYamlMetadata(CreateCompilationFromCsharpCode(code));
            Assert.AreEqual(1, output.Items.Count);
            {
                var constructor = output.Items[0].Items[0].Items[0];
                Assert.IsNotNull(constructor);
                Assert.AreEqual("Foo()", constructor.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo<T>.Foo()", constructor.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo`1.#ctor", constructor.Name);
                Assert.AreEqual("public Foo()", constructor.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var constructor = output.Items[0].Items[0].Items[1];
                Assert.IsNotNull(constructor);
                Assert.AreEqual("Foo(int)", constructor.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo<T>.Foo(int)", constructor.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo`1.#ctor(System.Int32)", constructor.Name);
                Assert.AreEqual("public Foo(int x)", constructor.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var constructor = output.Items[0].Items[0].Items[2];
                Assert.IsNotNull(constructor);
                Assert.AreEqual("Foo(string)", constructor.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo<T>.Foo(string)", constructor.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo`1.#ctor(System.String)", constructor.Name);
                Assert.AreEqual("protected Foo(string x)", constructor.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var constructor = output.Items[0].Items[1].Items[0];
                Assert.IsNotNull(constructor);
                Assert.AreEqual("Bar()", constructor.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Bar.Bar()", constructor.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Bar.#ctor", constructor.Name);
                Assert.AreEqual("public Bar()", constructor.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var constructor = output.Items[0].Items[1].Items[1];
                Assert.IsNotNull(constructor);
                Assert.AreEqual("Bar(int)", constructor.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Bar.Bar(int)", constructor.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Bar.#ctor(System.Int32)", constructor.Name);
                Assert.AreEqual("protected Bar(int x)", constructor.Syntax.Content[SyntaxLanguage.CSharp]);
            }
        }

        [TestMethod]
        [DeploymentItem("Assets", "Assets")]
        public void TestGenereateMetadata_CSharp_Field()
        {
            string code = @"
namespace Test1
{
    public class Foo<T>
    {
        public volatile int X;
        protected static readonly Foo<T> Y = null;
        protected internal const string Z = "";
    }
}
";
            MetadataItem output = BuildMetaHelper.GenerateYamlMetadata(CreateCompilationFromCsharpCode(code));
            Assert.AreEqual(1, output.Items.Count);
            {
                var constructor = output.Items[0].Items[0].Items[0];
                Assert.IsNotNull(constructor);
                Assert.AreEqual("X", constructor.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo<T>.X", constructor.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo`1.X", constructor.Name);
                Assert.AreEqual("public volatile int X", constructor.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var constructor = output.Items[0].Items[0].Items[1];
                Assert.IsNotNull(constructor);
                Assert.AreEqual("Y", constructor.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo<T>.Y", constructor.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo`1.Y", constructor.Name);
                Assert.AreEqual("protected static readonly Foo<T> Y", constructor.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var constructor = output.Items[0].Items[0].Items[2];
                Assert.IsNotNull(constructor);
                Assert.AreEqual("Z", constructor.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo<T>.Z", constructor.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo`1.Z", constructor.Name);
                Assert.AreEqual("protected const string Z", constructor.Syntax.Content[SyntaxLanguage.CSharp]);
            }
        }

        [TestMethod]
        [DeploymentItem("Assets", "Assets")]
        public void TestGenereateMetadata_CSharp_Event()
        {
            string code = @"
using System;
namespace Test1
{
    public abstract class Foo<T> where T : EventArgs
    {
        public event EventHandler A;
        protected static event EventHandler B { add {} remove {}}
        protected internal abstract event EventHandler<T> C;
        public virtual event EventHandler<T> D { add {} remove {}}
    }
    public class Bar<T> : Foo<T> where T : EventArgs
    {
        public new event EventHandler A;
        protected internal override sealed event EventHandler<T> C;
        public override event EventHandler<T> D;
    }
    public interface IFooBar<T> where T : EventArgs
    {
        event EventHandler A;
        event EventHandler<T> D;
    }
}
";
            MetadataItem output = BuildMetaHelper.GenerateYamlMetadata(CreateCompilationFromCsharpCode(code));
            Assert.AreEqual(1, output.Items.Count);
            {
                var a = output.Items[0].Items[0].Items[0];
                Assert.IsNotNull(a);
                Assert.AreEqual("A", a.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo<T>.A", a.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo`1.A", a.Name);
                Assert.AreEqual("public event EventHandler A", a.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var b = output.Items[0].Items[0].Items[1];
                Assert.IsNotNull(b);
                Assert.AreEqual("B", b.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo<T>.B", b.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo`1.B", b.Name);
                Assert.AreEqual("protected static event EventHandler B", b.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var c = output.Items[0].Items[0].Items[2];
                Assert.IsNotNull(c);
                Assert.AreEqual("C", c.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo<T>.C", c.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo`1.C", c.Name);
                Assert.AreEqual("protected abstract event EventHandler<T> C", c.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var d = output.Items[0].Items[0].Items[3];
                Assert.IsNotNull(d);
                Assert.AreEqual("D", d.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo<T>.D", d.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo`1.D", d.Name);
                Assert.AreEqual("public virtual event EventHandler<T> D", d.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var a = output.Items[0].Items[1].Items[0];
                Assert.IsNotNull(a);
                Assert.AreEqual("A", a.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Bar<T>.A", a.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Bar`1.A", a.Name);
                Assert.AreEqual("public event EventHandler A", a.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var c = output.Items[0].Items[1].Items[1];
                Assert.IsNotNull(c);
                Assert.AreEqual("C", c.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Bar<T>.C", c.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Bar`1.C", c.Name);
                Assert.AreEqual("protected override sealed event EventHandler<T> C", c.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var d = output.Items[0].Items[1].Items[2];
                Assert.IsNotNull(d);
                Assert.AreEqual("D", d.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Bar<T>.D", d.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Bar`1.D", d.Name);
                Assert.AreEqual("public override event EventHandler<T> D", d.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var a = output.Items[0].Items[2].Items[0];
                Assert.IsNotNull(a);
                Assert.AreEqual("A", a.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.IFooBar<T>.A", a.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.IFooBar`1.A", a.Name);
                Assert.AreEqual("event EventHandler A", a.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var d = output.Items[0].Items[2].Items[1];
                Assert.IsNotNull(d);
                Assert.AreEqual("D", d.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.IFooBar<T>.D", d.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.IFooBar`1.D", d.Name);
                Assert.AreEqual("event EventHandler<T> D", d.Syntax.Content[SyntaxLanguage.CSharp]);
            }
        }

        [TestMethod]
        [DeploymentItem("Assets", "Assets")]
        public void TestGenereateMetadata_CSharp_Property()
        {
            string code = @"
namespace Test1
{
    public abstract class Foo<T> where T : class
    {
        public int A { get; set; }
        public virtual int B { get { return 1; } }
        public abstract int C { set; }
        protected int D { get; private set; }
        public T E { get; protected set; }
        protected internal static int F { get; protected set; }
    }
    public class Bar : Foo<string>, IFooBar
    {
        public new virtual int A { get; set; }
        public override int B { get { return 2; } }
        public override sealed int C { set; }
    }
    public interface IFooBar
    {
        int A { get; set; }
        int B { get; }
        int C { set; }
    }
}
";
            MetadataItem output = BuildMetaHelper.GenerateYamlMetadata(CreateCompilationFromCsharpCode(code));
            Assert.AreEqual(1, output.Items.Count);
            {
                var a = output.Items[0].Items[0].Items[0];
                Assert.IsNotNull(a);
                Assert.AreEqual("A", a.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo<T>.A", a.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo`1.A", a.Name);
                Assert.AreEqual(@"public int A
{
    get;
    set;
}", a.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var b = output.Items[0].Items[0].Items[1];
                Assert.IsNotNull(b);
                Assert.AreEqual("B", b.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo<T>.B", b.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo`1.B", b.Name);
                Assert.AreEqual(@"public virtual int B
{
    get;
}", b.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var c = output.Items[0].Items[0].Items[2];
                Assert.IsNotNull(c);
                Assert.AreEqual("C", c.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo<T>.C", c.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo`1.C", c.Name);
                Assert.AreEqual(@"public abstract int C
{
    set;
}", c.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var d = output.Items[0].Items[0].Items[3];
                Assert.IsNotNull(d);
                Assert.AreEqual("D", d.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo<T>.D", d.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo`1.D", d.Name);
                Assert.AreEqual(@"protected int D
{
    get;
}", d.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var e = output.Items[0].Items[0].Items[4];
                Assert.IsNotNull(e);
                Assert.AreEqual("E", e.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo<T>.E", e.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo`1.E", e.Name);
                Assert.AreEqual(@"public T E
{
    get;
    protected set;
}", e.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var f = output.Items[0].Items[0].Items[5];
                Assert.IsNotNull(f);
                Assert.AreEqual("F", f.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo<T>.F", f.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo`1.F", f.Name);
                Assert.AreEqual(@"protected static int F
{
    get;
    set;
}", f.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var a = output.Items[0].Items[1].Items[0];
                Assert.IsNotNull(a);
                Assert.AreEqual("A", a.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Bar.A", a.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Bar.A", a.Name);
                Assert.AreEqual(@"public virtual int A
{
    get;
    set;
}", a.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var b = output.Items[0].Items[1].Items[1];
                Assert.IsNotNull(b);
                Assert.AreEqual("B", b.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Bar.B", b.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Bar.B", b.Name);
                Assert.AreEqual(@"public override int B
{
    get;
}", b.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var c = output.Items[0].Items[1].Items[2];
                Assert.IsNotNull(c);
                Assert.AreEqual("C", c.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Bar.C", c.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Bar.C", c.Name);
                Assert.AreEqual(@"public override sealed int C
{
    set;
}", c.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var a = output.Items[0].Items[2].Items[0];
                Assert.IsNotNull(a);
                Assert.AreEqual("A", a.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.IFooBar.A", a.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.IFooBar.A", a.Name);
                Assert.AreEqual(@"int A
{
    get;
    set;
}", a.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var b = output.Items[0].Items[2].Items[1];
                Assert.IsNotNull(b);
                Assert.AreEqual("B", b.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.IFooBar.B", b.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.IFooBar.B", b.Name);
                Assert.AreEqual(@"int B
{
    get;
}", b.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var c = output.Items[0].Items[2].Items[2];
                Assert.IsNotNull(c);
                Assert.AreEqual("C", c.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.IFooBar.C", c.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.IFooBar.C", c.Name);
                Assert.AreEqual(@"int C
{
    set;
}", c.Syntax.Content[SyntaxLanguage.CSharp]);
            }
        }

        [TestMethod]
        [DeploymentItem("Assets", "Assets")]
        public void TestGenereateMetadata_CSharp_Indexer()
        {
            string code = @"
using System;
namespace Test1
{
    public abstract class Foo<T> where T : class
    {
        public int this[int x] { get { return 0; } set { } }
        public virtual int this[string x] { get { return 1; } }
        public abstract int this[object x] { set; }
        protected int this[DateTime x] { get { return 0; } private set { } }
        public int this[T t] { get { return 0; } protected set { } }
        protected internal int this[int x, T t] { get; protected set; }
    }
    public class Bar : Foo<string>, IFooBar
    {
        public new virtual int this[int x] { get { return 0; } set { } }
        public override int this[string x] { get { return 2; } }
        public override sealed int this[object x] { set; }
    }
    public interface IFooBar
    {
        int this[int x] { get; set; }
        int this[string x] { get; }
        int this[object x] { set; }
    }
}
";
            MetadataItem output = BuildMetaHelper.GenerateYamlMetadata(CreateCompilationFromCsharpCode(code));
            Assert.AreEqual(1, output.Items.Count);
            {
                var a = output.Items[0].Items[0].Items[0];
                Assert.IsNotNull(a);
                Assert.AreEqual("this[int]", a.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo<T>.this[int]", a.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo`1.Item(System.Int32)", a.Name);
                Assert.AreEqual(@"public int this[int x]
{
    get;
    set;
}", a.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var b = output.Items[0].Items[0].Items[1];
                Assert.IsNotNull(b);
                Assert.AreEqual("this[string]", b.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo<T>.this[string]", b.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo`1.Item(System.String)", b.Name);
                Assert.AreEqual(@"public virtual int this[string x]
{
    get;
}", b.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var c = output.Items[0].Items[0].Items[2];
                Assert.IsNotNull(c);
                Assert.AreEqual("this[object]", c.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo<T>.this[object]", c.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo`1.Item(System.Object)", c.Name);
                Assert.AreEqual(@"public abstract int this[object x]
{
    set;
}", c.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var d = output.Items[0].Items[0].Items[3];
                Assert.IsNotNull(d);
                Assert.AreEqual("this[DateTime]", d.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo<T>.this[System.DateTime]", d.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo`1.Item(System.DateTime)", d.Name);
                Assert.AreEqual(@"protected int this[DateTime x]
{
    get;
}", d.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var e = output.Items[0].Items[0].Items[4];
                Assert.IsNotNull(e);
                Assert.AreEqual("this[T]", e.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo<T>.this[T]", e.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo`1.Item(`0)", e.Name);
                Assert.AreEqual(@"public int this[T t]
{
    get;
    protected set;
}", e.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var f = output.Items[0].Items[0].Items[5];
                Assert.IsNotNull(f);
                Assert.AreEqual("this[int, T]", f.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo<T>.this[int, T]", f.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Foo`1.Item(System.Int32,`0)", f.Name);
                Assert.AreEqual(@"protected int this[int x, T t]
{
    get;
    set;
}", f.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var a = output.Items[0].Items[1].Items[0];
                Assert.IsNotNull(a);
                Assert.AreEqual("this[int]", a.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Bar.this[int]", a.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Bar.Item(System.Int32)", a.Name);
                Assert.AreEqual(@"public virtual int this[int x]
{
    get;
    set;
}", a.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var b = output.Items[0].Items[1].Items[1];
                Assert.IsNotNull(b);
                Assert.AreEqual("this[string]", b.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Bar.this[string]", b.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Bar.Item(System.String)", b.Name);
                Assert.AreEqual(@"public override int this[string x]
{
    get;
}", b.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var c = output.Items[0].Items[1].Items[2];
                Assert.IsNotNull(c);
                Assert.AreEqual("this[object]", c.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Bar.this[object]", c.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.Bar.Item(System.Object)", c.Name);
                Assert.AreEqual(@"public override sealed int this[object x]
{
    set;
}", c.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var a = output.Items[0].Items[2].Items[0];
                Assert.IsNotNull(a);
                Assert.AreEqual("this[int]", a.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.IFooBar.this[int]", a.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.IFooBar.Item(System.Int32)", a.Name);
                Assert.AreEqual(@"int this[int x]
{
    get;
    set;
}", a.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var b = output.Items[0].Items[2].Items[1];
                Assert.IsNotNull(b);
                Assert.AreEqual("this[string]", b.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.IFooBar.this[string]", b.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.IFooBar.Item(System.String)", b.Name);
                Assert.AreEqual(@"int this[string x]
{
    get;
}", b.Syntax.Content[SyntaxLanguage.CSharp]);
            }
            {
                var c = output.Items[0].Items[2].Items[2];
                Assert.IsNotNull(c);
                Assert.AreEqual("this[object]", c.DisplayNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.IFooBar.this[object]", c.DisplayQualifiedNames[SyntaxLanguage.CSharp]);
                Assert.AreEqual("Test1.IFooBar.Item(System.Object)", c.Name);
                Assert.AreEqual(@"int this[object x]
{
    set;
}", c.Syntax.Content[SyntaxLanguage.CSharp]);
            }
        }

        [TestMethod]
        [DeploymentItem("Assets", "Assets")]
        public void TestGenereateMetadata_CSharp_Method_DefaultValue()
        {
            string code = @"
namespace Test1
{
    public class Foo
    {
        public void Test(
            int a = 1, uint b = 1,
            short c = 1, ushort d = 1,
            long e = 1, ulong f= 1,
            byte g = 1, sbyte h = 1,
            char i = '1', string j = ""1"",
            bool k = true, object l = null)
        {
        }
    }
}
";
            MetadataItem output = BuildMetaHelper.GenerateYamlMetadata(CreateCompilationFromCsharpCode(code));
            Assert.AreEqual(1, output.Items.Count);
            {
                var a = output.Items[0].Items[0].Items[0];
                Assert.IsNotNull(a);
                Assert.AreEqual(@"public void Test(int a = 1, uint b = 1U, short c = 1, ushort d = 1, long e = 1L, ulong f = 1UL, byte g = 1, sbyte h = 1, char i = '1', string j = ""1"", bool k = true, object l = null)", a.Syntax.Content[SyntaxLanguage.CSharp]);
            }
        }

        private static Compilation CreateCompilationFromCsharpCode(string code)
        {
            var tree = CS.SyntaxFactory.ParseSyntaxTree(code);
            var compilation = CS.CSharpCompilation.Create(
                "test.dll",
                options: new CS.CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                syntaxTrees: new[] { tree },
                references: new[] { MetadataReference.CreateFromAssembly(typeof(object).Assembly) });
            return compilation;
        }

        #endregion

        #region VB

        [TestMethod]
        [DeploymentItem("Assets", "Assets")]
        public void TestGenereateMetadata_VB_Class()
        {
            string code = @"
Imports System.Collections.Generic
Namespace Test1
{
    Public Class Class1
    End Class
    Public Class Class2(Of T)
        Inherits List(Of T)
    End Class
    Public Class Class3(Of T1, T2 As T1)
    End Class
    Public Class Class4(Of T1 As { Structure, IEnumerable(Of T2) }, T2 As { Class, New })
    End Class
}
";
            MetadataItem output = BuildMetaHelper.GenerateYamlMetadata(CreateCompilationFromVBCode(code));
            Assert.AreEqual(1, output.Items.Count);
            {
                var type = output.Items[0].Items[0];
                Assert.IsNotNull(type);
                Assert.AreEqual("Class1", type.DisplayNames[SyntaxLanguage.VB]);
                Assert.AreEqual("Test1.Class1", type.DisplayQualifiedNames[SyntaxLanguage.VB]);
                Assert.AreEqual("Test1.Class1", type.Name);
                Assert.AreEqual(@"Public Class Class1", type.Syntax.Content[SyntaxLanguage.VB]);
            }
            {
                var type = output.Items[0].Items[1];
                Assert.IsNotNull(type);
                Assert.AreEqual("Class2(Of T)", type.DisplayNames[SyntaxLanguage.VB]);
                Assert.AreEqual("Test1.Class2(Of T)", type.DisplayQualifiedNames[SyntaxLanguage.VB]);
                Assert.AreEqual("Test1.Class2`1", type.Name);
                Assert.AreEqual(@"Public Class Class2(Of T)
    Inherits List(Of T)
    Implements IList(Of T), ICollection(Of T), IList, ICollection, IReadOnlyList(Of T), IReadOnlyCollection(Of T), IEnumerable(Of T), IEnumerable", type.Syntax.Content[SyntaxLanguage.VB]);
            }
            {
                var type = output.Items[0].Items[2];
                Assert.IsNotNull(type);
                Assert.AreEqual("Class3(Of T1, T2)", type.DisplayNames[SyntaxLanguage.VB]);
                Assert.AreEqual("Test1.Class3(Of T1, T2)", type.DisplayQualifiedNames[SyntaxLanguage.VB]);
                Assert.AreEqual("Test1.Class3`2", type.Name);
                Assert.AreEqual(@"Public Class Class3(Of T1, T2 As T1)", type.Syntax.Content[SyntaxLanguage.VB]);
            }
            {
                var type = output.Items[0].Items[3];
                Assert.IsNotNull(type);
                Assert.AreEqual("Class4(Of T1, T2)", type.DisplayNames[SyntaxLanguage.VB]);
                Assert.AreEqual("Test1.Class4(Of T1, T2)", type.DisplayQualifiedNames[SyntaxLanguage.VB]);
                Assert.AreEqual("Test1.Class4`2", type.Name);
                Assert.AreEqual(@"Public Class Class4(Of T1 As {Structure, IEnumerable(Of T2)}, T2 As {Class, New})", type.Syntax.Content[SyntaxLanguage.VB]);
            }
        }

        private static Compilation CreateCompilationFromVBCode(string code)
        {
            var tree = VB.SyntaxFactory.ParseSyntaxTree(code);
            var compilation = VB.VisualBasicCompilation.Create(
                "test.dll",
                options: new VB.VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                syntaxTrees: new[] { tree },
                references: new[] { MetadataReference.CreateFromAssembly(typeof(object).Assembly) });
            return compilation;
        }

        #endregion
    }
}
