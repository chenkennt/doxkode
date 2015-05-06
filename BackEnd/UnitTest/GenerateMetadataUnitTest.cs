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
    [DeploymentItem("Microsoft.CodeAnalysis.CSharp.Workspaces.dll")]
    [DeploymentItem("Microsoft.CodeAnalysis.CSharp.Workspaces.Desktop.dll")]
    [DeploymentItem("Microsoft.CodeAnalysis.VisualBasic.Workspaces.dll")]
    [DeploymentItem("Microsoft.CodeAnalysis.VisualBasic.Workspaces.Desktop.dll")]
    public class GenerateMetadataUnitTest
    {
        private static readonly MSBuildWorkspace Workspace = MSBuildWorkspace.Create();

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
                Assert.AreEqual("event EventHandler FooBar;", @event.Syntax.Content[SyntaxLanguage.CSharp]);
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
    }
    public interface IFoo
    {
        object Bar(ref int x);
    }
    public interface IFoo<out T>
    {
        T Bar<TArg>(TArg[] x)
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
using System.Collections.Generic
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
