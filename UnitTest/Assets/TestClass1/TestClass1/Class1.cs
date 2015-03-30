using Microsoft.DevDiv.Build.TestLibraryA;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

// partial class will override one other randomly
// namespace Microsoft.DevDiv.Build.TestLibraryA
// {
//    /// <summary>
//    /// This is *partial* libraryA
//    /// </summary>
//    public partial class TestLibraryA
//    {
//        public enum ColorType { Red, Blue }
//    }
// }

namespace Microsoft.DevDiv.Build.TestLibraryB
{
    public class TestLibraryB : ISurface
    {
        // Fields
        private long c = 0;

        // Properties
        public long ParamC
        {
            get
            {
                return c;
            }
        }

        // Method
        public void AddMethod()
        {
            c = 1 + 1;
        }

        // Events
        public event EventHandler<EventInfoArgs> eventHandler
        {
            add { }
            remove { }
        }
    }

    public interface TestLibraryBInterfaceBase
    {
        void MultiplyMethod();

        int MinusMethod();
    }

    public class TestLibraryB2
    {
        protected internal void cal()
        {
        }
    }

    /// <summary>
    /// This is the struct: visibility public & abstract false
    /// </summary>
    public struct TestPublicStruct
    {
    }


    /// <summary>
    /// This is the struct: visibility assembly (internal)
    /// </summary>
    internal struct TestInternalStruct
    {
    }

    /// <summary>
    /// This is a struct with serializable true
    /// </summary>
    [Serializable]
    public struct TestSerializableStruct
    {
    }

    // layout auto 
    [StructLayout(LayoutKind.Auto)]
    public struct TestAutoStruct
    {
    }

    // layout sequential
    [StructLayout(LayoutKind.Sequential)]
    public struct TestSequentialStruct
    {
    }

    // layout explicit
    [StructLayout(LayoutKind.Explicit)]
    public struct TestExplicitStruct
    {
    }

    // templates & implements
    public struct TestTemplatesImplementsList<K, T, L, M>
        where K : class, IComparable
        where T : struct
        where L : TestLibraryB, IEnumerable<long>
        where M : new()
    {
    }

    // elements
    public struct TestElementsStruct
    {
        // <element api="M
        public int TestAdd(int a, int b)
        {
            return a + b;
        }

        // fields <element api="F
        public string testStr;
        private double seconds;

        // property  <element api="P
        public double Hours
        {
            get { return seconds / 3600; }
            set { seconds = value * 3600; }
        }

        enum WeeksInClass { Monday = 1, Tue, Thr };

        //Applied to a return value.
        [return: MarshalAs(UnmanagedType.LPWStr)]
        public String GetMessage()
        {
            return "Hello World";
        }

        public int val
        {
            get { return 10; }
            set { }
        }

        // <element api="E:
        public delegate void ChangedEventHandler(object sender, EventArgs e);
        public event ChangedEventHandler Changed;

        public void setEvent()
        {
            // use Changed, disable warning
            ChangedEventHandler t = Changed;
        }
    }

    // container
    public struct TestContainerStruct
    {
        public struct TestSubStruct
        {
        }
    }
}

namespace Microsoft.DevDiv.Build.TestLibraryB2
{
    public interface ISurfaceA : ISurface
    {
        int Foo();
        int FooA();

        int PropertyA { get; }
        int Property { get; }

        event EventHandler<EventInfoArgs> otherEventHandler;
    }

    public interface ISurfaceB : ISurface
    {
        long Foo();

        long FooB();

        long Fun(long a);

        long OverrideFun();

        long SameNumParaFunc(long a, string b);

        long PropertyB { set; }

        long Property { set; }

        long this[string a, long b] { set; }

        long this[string a] { set; }

        long this[string a, params object[] b] { set; }
    }

    public interface ISurfaceC : ISurfaceB
    {
        new double Foo();

        double FooC();

        double Fun(double a);
        double Fun(long a, double b);

        new double OverrideFun();

        long SameNumParaFunc(string b, long a);

        double PropertyC { get; set; }

        new double Property { get; set; }

        double this[string a, double b] { get; set; }

        new double this[string a] { get; set; }

        double this[string a, object b, params object[] c] { get; set; }
    }

    public interface ISurfaceD : ISurfaceC, ISurfaceA
    {
        string FooD();

        new string PropertyB { get; set; }

        new event EventHandler<EventInfoArgs> otherEventHandler;
    }

    public class OuterClass
    {
        public class InnerClass
        {
            public class InnerClassReturn
            {
                public int a;
            }

            public InnerClass() { }

            public InnerClass(int a) { }

            public InnerClassReturn AddMethod()
            {
                return new InnerClassReturn();
            }

            public void AddMethod(int a) { }

            public void AddMethod(int a, ref int b) { }

            public int filedA;

            public int fieldB;

            public int PropertyA { get; set; }

            public int PropertyB { get; set; }
        }
    }

    public abstract class Abstract
    {
        [ComVisible(true)]
        public abstract int AbstractProperty { get; set; }

        public abstract Dictionary<List<string>, List<long>> AbstractProperty2 { get; set; }

        public virtual List<double> VirtualProperty { get; set; }

        public virtual int VirtualIntProperty { get; set; }

        public virtual long VirtualLongProperty { get; set; }

        public ushort PrivateGetProperty { private get; set; }

        public ulong PrivateSetProperty { get; private set; }

        public int PublicIntProperty
        {
            get
            {
                return privateIntField;
            }
            private set
            {
                privateIntField = value;
            }
        }

        private int privateIntField = 1;

        public abstract void AbstractMethod(int a);

        public abstract void AbstractMethod(int a, int b);

        public virtual void VirtualMethod(int a, int b, int c) { }

        public virtual void VirtualMethod(int a, int b, int c, int d) { }

        public virtual void VirtualNonSealedMethod(int a) { }
    }

    public class Derived : Abstract, ISurface
    {
        public Derived()
        {
            eventHandler = Handler;
        }

        private int field = 0;

        private static readonly long f2 = 3;

        public override int AbstractProperty
        {
            get
            {
                return (int)f2;
            }
            set
            {
                field = value;
            }
        }

        public override Dictionary<List<string>, List<long>> AbstractProperty2 { get; set; } //won't consider it.

        public override sealed List<double> VirtualProperty { get; set; }

        public override int VirtualIntProperty { get; set; }

        public new virtual long VirtualLongProperty { get; set; }

        public static double StaticProperty { get; set; }

        public long ParamC
        {
            get
            {
                return f2;
            }
            set
            {
                field = 2;
            }
        }

        public override void AbstractMethod(int a) { }

        public override void AbstractMethod(int a, int b) { }

        public override void VirtualMethod(int a, int b, int c) { }

        public sealed override void VirtualMethod(int a, int b, int c, int d) { }

        public new virtual void VirtualNonSealedMethod(int a) { }

        public void AddMethod() { }

        public List<List<string>> AddMethod(Dictionary<List<int>, string> a, List<long> b, string C) { return new List<List<string>>(); }

        public virtual event EventHandler<EventInfoArgs> eventHandler;

        public void Handler(object sender, EventInfoArgs e) { }

        public static T CreateInstance<T>(T a) { return a; }
    }

    public delegate void ContainersRefTypeDelegate();

    public delegate TResult delegateFunc<T1, in T2, in T3, out TResult>(T1 arg1, T2 arg2, T3 arg3);

    public delegate void delegateFunc<T1, T2>(T1 arg1, T2 arg2, int argInt, params object[] parmsArray);

    public delegate void delegateFunc(int a, ref int b, out int c);

    internal delegate void delegateFunc<T>(long a, long b);

    internal enum InternalEnum
    {
        First,
        Second,
    }

    public class CaseSensitiveClass
    {
        public CaseSensitiveClass() { }

        public void CaseSensitiveFunc(boolClass boolArg, BoolClass BoolArg) { }

        public void CaseSensitiveFunc() { }

        public void CaseSensitivefunc() { }

        public boolClass boolProperty { get; set; }

        public BoolClass BoolProperty { get; set; }

        public boolClass boolField;

        public BoolClass BoolField;

        public class boolClass { }

        public class BoolClass { }
    }

    public sealed class PrivateCtorSealedClass  //no ctor for this class in preview
    {
        private PrivateCtorSealedClass() { }
    }

    public class PrivateCtorClass   //no ctor for this class in preview
    {
        private PrivateCtorClass() { }
    }

    public class ProtectedCtorClass //has ctor for this class in preivew
    {
        protected ProtectedCtorClass() { }
    }
}
