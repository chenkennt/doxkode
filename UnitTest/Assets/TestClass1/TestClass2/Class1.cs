using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
[assembly: CLSCompliant(false)]

namespace Microsoft.DevDev.Build.PropertyOverload
{
    /// <summary>
    /// Class to test property
    /// </summary>
    public class TestProperty
    {
        public int this[int index]
        {
            get { return 0; }
            set { }
        }

        public string this[string index]
        {
            get { return "aa"; }
            set { }
        }
    }
}

namespace Microsoft.DevDiv.Build.TestLibraryA
{
    using CustomExtensionMethods;

    #region Non-static classes
    /// <summary>
    /// Partial class is not currently *supported*...
    /// </summary>
    public partial class TestLibraryA
    {
        /// <summary>
        /// This is an **ENUM** <see cref="Tuple"/>
        /// </summary>
        public enum ColorType { Red, Blue, Yellow }
        private enum PrivateColorType { Purple, White }
        protected enum ProtectedColorType { Black }
        internal enum InternalColorType { Grey }
        [Serializable]
        protected internal enum ProtectedInternalColorType { Green }

        /// <summary>
        /// This is a Tuple
        /// </summary>
        /// <typeparam name="T">Tuple type</typeparam>
        /// <returns>A</returns>
        public Tuple<string, T> GetTuple<T>(Tuple<string, Tuple<T, T>> a) where T : class
        {
            return Tuple.Create(string.Empty, default(T));
        }

        /// <summary>
        /// Get k
        /// </summary>
        /// <param name="a">a</param>
        /// <returns>a</returns>
        public Tuple<string, int> GetInt(Tuple<string, int> a)
        {
            return null;
        }
    }

    #region Basic Class
    // Abstract Base Class
    /// <summary>
    /// test
    /// </summary>
    /// <typeparam name="T">generic type</typeparam>
    public abstract class Stack<T>
    {
        public int StackIntField;

        public static int StackStaticField;

        public static int StackStaticProperty { get; set; }

        public int StackIntProperty { get; set; }

        public abstract void AddAbstract();

        public abstract void AddAbstract(string name, int num);

        /// <summary>
        /// Overload method a 
        /// </summary>
        /// <param name="A">The input</param>
        public void OverLoadMethod(int A)
        {

        }

        public void OverLoadMethod(int A, int B)
        {

        }
    }

    public abstract class AbstractClass
    {
        [ComVisible(true)]
        public abstract int AbstractProperty { get; set; }

        public abstract Dictionary<List<string>, List<long>> AbstractProperty2 { get; set; }

        public virtual List<double> VirtualProperty { get; set; }

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
    }

    public class DerivedClass : AbstractClass
    {
        private int field = 0;

        private static readonly long f2 = 3;

        /// <summary>
        /// <see cref="Microsoft.DevDiv.Build.TestLibraryA.AbstractClass"/>
        /// <see cref="System.Type"/>
        /// </summary>
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

        public static double StaticProperty { get; set; }
    }

    // Interface Base
    /// <summary>
    /// base interface ISurface
    /// </summary>
    public interface ISurface
    {
        ///<summary>
        ///property ParamC
        /// </summary>
        // Properties
        long ParamC { get; }

        ///<summary>
        ///AddMethod
        /// </summary>
        // Methods
        void AddMethod();

        ///<summary>
        ///event
        /// </summary>
        // Events
        event EventHandler<EventInfoArgs> eventHandler;
    }

    // Templates Interface Base
    public interface LinkedListInterface<K, T, L>
        where K : class, IComparable
        where T : struct
        where L : TestLibraryA, IEnumerable<long>
    {

    }

    public interface LinkedListInterface2<T>
        where T : struct
    {
    }

    public interface LinkedListInterface3
    {
    }

    // Basic Delegate Base
    public delegate string StackDelegate<T>(string name, out string outName);
    // public delegate int StackIntDelegate<T>(long num, [In]string name, params object[] scores);
    public delegate int StackIntDelegate<T>(long num, string name, params object[] scores);

    // Template Delegate Base
    public delegate void StackTemplateDelegate<K, T, L>()
        where K : class, IComparable
        where T : struct
        where L : TestLibraryA, IEnumerable<long>;
    #endregion

    // Abstract Inherit and Generics
    public class StackChild<T> : Stack<T>
    {
        // Fields : Public, Private, Protected, Internal and Protected Internal
        public static string publicStaticField;
        private int privateField = 0;
        [System.Runtime.CompilerServices.SpecialName]
        protected int protectedField;
        internal int internalField = 0;
        [NonSerialized]
        protected internal volatile int protectedInternalField;

        /// <summary>
        /// public field <see cref = "Microsoft.DevDiv.Build.TestLibraryA.Stack{T}"/>
        /// </summary>
        /// <see cref = "Microsoft.DevDiv.Build.TestLibraryA.Stack(T)"/>
        public const string publicConstField = "publicConstField";

        [ComVisible(true)]
        public readonly long PublicReadonlyField = 100;

        public static readonly double PublicStaticReadonlyField = 2.0;

        //different complex type
        public List<string> publicListString = new List<string>();
        public Dictionary<int, List<long>> publicDic_IntList = new Dictionary<int, List<long>>();
        public List<Dictionary<List<int>, Array>> publicList_Dic_ListArray = new List<Dictionary<List<int>, Array>>();

        // Properties : Public, Private, Protected, Internal and Protected Internal
        [ComVisible(true)]
        public static string PublicStaticProperty
        {
            get
            {
                return publicStaticField;
            }
            set
            {
                publicStaticField = value;
            }
        }

        private int PrivateProperty
        {
            get
            {
                return privateField;
            }
        }

        [System.Runtime.CompilerServices.SpecialName]
        protected int ProtectedProperty
        {
            get
            {
                return privateField;
            }
        }

        internal int InternalProperty { get; set; }

        protected internal int ProtectedInternalProperty
        {
            set
            {
                protectedInternalField = value;
            }
        }

        /// <summary>
        /// ProtectedStaticProperty
        /// </summary>
        /// <seealso cref="System.Object.ToString()"/>
        //not support this release
        protected static long ProtectedStaticProperty { get; set; }

        private static long PrivateStaticProperty { get; set; }

        // public string this[string name, int num, [In]string tech, [Out]string level, params object[] scores]
        public string this[string name, int num, string tech, string level, params object[] scores]
        {
            get
            {
                return publicStaticField;
            }
            protected set
            {
                publicStaticField = value;
            }
        }

        public static List<string> PublicStaticListStringProperty { get; set; }

        public Dictionary<int, List<long>> Dic_IntListProperty
        {
            get
            {
                return publicDic_IntList;
            }
        }

        public List<Dictionary<List<int>, Array>> List_Dic_ListArray
        {
            set
            {
                publicList_Dic_ListArray = value;
            }
        }

        // Methods: Private, Protected, Internal and Protected Internal
        public StackChild()
        {

        }

        public Dictionary<int[], double[]> IntRefProperty { get; set; }

        // Methods: Private, Protected, Internal and Protected Internal
        internal void AddInternal() { }

        private void AddPrivate() { }

        /// <summary>
        /// <see cref="System.Object.ToString()"/>
        /// </summary>
        protected internal void AddProtectedInternal() { }

        [System.Runtime.CompilerServices.SpecialName]
        protected string AddProtected()
        {
            return null;
        }

        /// <summary>
        /// Protected method
        /// </summary>
        /// <param name="name">Used to indicate name</param>
        /// <param name="num">Used to indicate number</param>
        /// <param name="level">Used to indicate level</param>
        /// <see cref="System.Type"/>
        /// <returns></returns>
        // no return 
        [ComVisible(true)]
        [Obsolete]
        protected string AddProtected(string name, int num, string level)
        {
            // int wordCount = name.WordCount();
            return "asdf";
        }


        public sealed override void AddAbstract()
        {
        }

        public override void AddAbstract(string name, int num)
        {
        }

        /// <permission cref="System.Object">Everyone can access this method.</permission>
        public void AddAbstract(string name, int num, string level)
        {

        }

        new public void OverLoadMethod(int A, int B)
        {

        }

        /// <summary>
        /// check overload
        /// </summary>
        /// <exception cref="System.Exception">Thrown exception when error</exception>
        /// <exception cref="System.Exception">another exception</exception>
        /// <param name="A">Used to indicate number</param>
        /// <param name="B">Used to indicate number</param>
        /// <param name="C">Used to indicate number</param>
        public void OverLoadMethod(int A, int B, int C)
        {

        }

        public static int operator +(StackChild<T> lsc, int rsc)
        {
            return (lsc.privateField + rsc);
        }

        /// <exception cref="System.Exception">Thrown exception when error</exception>
        public static int operator +(StackChild<T> lsc, double rsc)
        {
            return (lsc.privateField + (int)rsc);
        }


        // public static int AddPublic<K, TA, L>(string name, int num, [In] string tech, out string level, params object[] scores)
        /// <summary>
        /// fist text under summary <c>codeinline <i>italic word</i> int</c>
        /// Sprint67's <b>development</b> comment test
        /// <para>Template <i>italic method</i></para>
        /// text between para with no para tag
        /// <para>para <b>with</b> <i>one</i> line description</para>
        /// <i>italic between para with no para tag</i>
        /// <para>two line of description. One typeparamref <typeparamref name="TA"/>
        /// parameter. here's languageKeyword <languageKeyword>in</languageKeyword>.</para>
        /// last text in summary with no para tag
        /// </summary>
        /// <example>
        /// <code language="c#">
        ///        int main()
        ///        {
        ///                    return 0;
        ///        }
        /// </code>
        /// <list type="ordered">
        /// <item><description>item in example</description></item>
        /// <item><description><code language="c#">string c# code</code></description></item>
        /// </list>
        /// </example>
        /// <example>
        /// <code language="c#">
        ///     long a = 1;
        /// </code>
        /// <list type="nobullet">
        /// <item>
        /// <description>second code example</description>
        /// </item>
        /// </list>
        /// </example>
        /// <value><para>get an <c>int <i>italic word</i></c> value</para>
        /// <list type="bullet">
        /// <item>
        /// <description>list in returns</description>
        /// </item>
        /// </list>
        /// </value>
        /// <remarks>
        /// <para>
        /// here is remarks
        /// </para>
        /// <list type="bullet">
        /// <item>
        /// <description>
        /// <code language="c#">
        /// public class XmlElement 
        ///     : XmlLinkedNode
        /// </code>
        /// <list type="ordered">
        /// <item>
        /// <description>
        /// word inside list->listItem->list->listItem->para.
        /// </description>
        /// </item>
        /// </list>
        /// </description>
        /// </item>
        /// </list>
        /// </remarks>
        /// <seealso cref="T:System.Object"/>
        public static int AddPublic<K, TA, L>(string name, int num, string tech, out string level, params object[] scores)
            where K : class, IComparable, ICollection<List<string>>, IComparable<L>
            where TA : struct
            where L : TestLibraryA, IEnumerable<long>
        {
            level = "";
            return 0;
        }

        /// <summary>
        /// overload method AddPublic
        /// </summary>
        /// <typeparam name="K">the element type</typeparam>
        /// <typeparam name="TA">the element type</typeparam>
        /// <typeparam name="L">the element type</typeparam>
        /// <param name="a">used to indicate number</param>
        /// <returns>static int</returns>
        public static int AddPublic<K, TA, L>(long a)
        {
            return 0;
        }

        // Interface: Private, Protected, Internal and Protected Internal
        private interface StackChildPrivateInterface<K>
        {

        }

        internal interface StackChildInternalInterface<K>
        {

        }

        protected interface StackChildProtectedInterface<K>
        {

        }

        protected internal interface StackChildProtectedInternalInterface<K>
        {

        }

        // Classes: Private, Protected, Internal and Protected Internal
        private class StackChildPrivate<K> : StackChildPrivateInterface<K>
        {

        }

        internal class StackChildInternal<K> : StackChildInternalInterface<K>
        {

        }

        [System.Runtime.CompilerServices.SpecialName]
        protected class StackChildProtected<K> : StackChildProtectedInterface<K>
        {

        }

        /// <summary>
        /// <para>This <c>class</c> is <i>inherit</i> from <c>interface</c> <b>StackChildProtectedInternalInterface(</b> <typeparamref name="K"/>)</para>
        /// It's an internal class.
        /// The base interface is also internal
        /// <b>class</b> doesn't contain any methods
        /// <para><b>interface</b> also doesn't contain any api need to be <b>implemented</b>.</para>
        /// you can add one method return <languageKeyword>int</languageKeyword> or <languageKeyword>long</languageKeyword> or any type you need inside this class.
        /// </summary>
        /// <example>
        /// This sample shows how to create <languageKeyword>internal</languageKeyword> class
        /// <code>
        /// class internal class TestClass
        /// {
        ///     void DonotForgetInternalKeyword()
        ///     {
        ///     }
        /// }
        /// </code>
        /// This is a <b>simple</b> internal class definition sample.
        /// </example>
        /// <remarks>
        /// <list type="bullet">
        /// <item>
        /// <description>
        /// word inside list->listItem without para
        /// </description>
        /// </item>
        /// </list>
        /// </remarks>
        /// <seealso cref="T:System.String"/>
        /// <seealso cref="T:System.Int32"/>
        /// <seealso cref="T:System.Collections.Generic.Dictionary`2"/>
        protected internal class StackChildProtectedInternal<K> : StackChildProtectedInternalInterface<K>
        {

        }

        // Delegates: Private, Protected, Internal and Protected Internal
        private delegate void StackDelegatePrivate<K>();

        internal delegate void StackDelegateInternal<K>();

        protected delegate void StackDelegateProtected<K>();

        [Serializable]
        protected internal delegate void StackDelegateProtectedInternal<K>();

        // Events: Private, Protected, Internal and Protected Internal
        private event EventHandler privateEventHandler
        {
            add { }
            remove { }
        }

        /// <summary>
        /// ResolveEventHandler
        /// </summary>
        [System.Runtime.CompilerServices.SpecialName]
        protected event ResolveEventHandler protectedEventHandler
        {
            add { }
            remove { }
        }

        internal event EventHandler internalEventHandler
        {
            add { }
            remove { }
        }

        protected internal static event EventHandler protectedInternalEventHandler
        {
            add { }
            remove { }
        }

        public event Action<object, int> publicActionEvent
        {
            add { }
            remove { }
        }
    }

    // Interface Implements Interface
    /// <summary>
    /// see cref = "Microsoft.DevDiv.Build.TestLibraryA.ISurface" 
    /// </summary>
    //check only have summary
    public interface INestInterface : ISurface
    {
        // Properties
        int ParamB { get; set; }
        new long ParamC { set; }

        // Methods
        void MinusMethod();
        void MultiplyMethod();

        /// <summary>
        /// Multiply method
        /// </summary>
        /// <param name="a">the number will multiple 
        /// <paramref name="b"/>
        /// </param>
        /// <param name="b">the number</param>
        void MultiplyMethod(int a, int b);


        int MultiplyMethod(int a);
    }

    // Class Implements ISurface
    public sealed class ImplementISurface : ISurface
    {
        // Fields
        public long b;
        private long a, c = 100;

        // Method
        void ISurface.AddMethod()
        {

        }

        public void AddMethod()
        {
            c = a + b;
        }

        private void MinusMethod()
        {
            c = a - b;
        }

        // Constructor
        /// <summary>
        /// Class Implements ISurface
        /// </summary>
        public ImplementISurface()
        {
            a = 100;
            b = 200;
            c = 0;
        }

        public ImplementISurface(int a, int b)
        {
            this.a = a;
            this.b = b;
            c = 0;
        }

        public ImplementISurface(int a, int b, int c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }

        [ComVisible(true)]
        [CLSCompliant(false)]
        public void MultiMethod()
        {
            c = a * b;
        }

        // Event
        public event EventHandler<EventInfoArgs> eventHandler
        {
            add
            {
                eventHandler += value;
            }
            remove
            {
                eventHandler -= value;
            }
        }

        // Property
        public long ParamC
        {
            get
            {
                return c;
            }
        }

        public long ParamA
        {
            set
            {
                a = value;
            }
        }
    }

    [Serializable]
    [ComVisibleAttribute(false)]
    [StructLayout(LayoutKind.Sequential)]
    public class LinkedList<K, T, L> : StackChild<T>, LinkedListInterface<K, T, L>, LinkedListInterface2<T>, LinkedListInterface3
        where K : class, IComparable
        where T : struct
        where L : TestLibraryA, IEnumerable<long>, IComparable<K>
    {
        static LinkedList() { }
        [Obsolete]
        public LinkedList() { }

        public LinkedList(List<List<string>> a, List<Dictionary<int, List<long>>> b, Dictionary<List<string>, Dictionary<string, List<long>>> c) { }

        public LinkedList(K a) { }

        public LinkedList(int a, T b) { }

        public LinkedList(char a, Dictionary<int, string> dic) { }

        public LinkedList(int a, out int b) { b = 1; }

        public LinkedList(short a, long b, float c, ushort d, uint e, ulong f) { }

        protected LinkedList(K a, T b, L c) { }

        protected LinkedList(DateTime a, List<string> b) { }

        protected LinkedList(T a, List<int> b) { }

        private LinkedList(double a, char b, Single c) { }
    }

    // Event
    public class EventInfoArgs : EventArgs
    {
        public EventInfoArgs(string eventProperty)
        {
            this.EventProperty = eventProperty;
        }

        public string EventProperty { get; private set; }
    }

    // Others
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public sealed class StackSealedChild<T> : Stack<T>
    {
        public override void AddAbstract()
        {
        }

        public override void AddAbstract(string name, int num)
        {
        }
    }
    #endregion

    #region Static classes
    // Static Class
    public static class StaticClass
    {

    }
    #endregion
}

// Struct and Types Based on It
namespace Microsoft.DevDiv
{
    // Struct and Class
    /// <summary>
    /// Struct ContainersRefType
    /// </summary>
    public struct ContainersRefType
    {
        ///<summary>
        ///ColorCount
        /// </summary>
        // Field
        public long ColorCount;

        // Enumeration
        /// <summary>
        /// Enumeration ColorType
        /// </summary>
        public enum ColorType
        {
            /// <summary>
            /// red
            /// </summary>
            Red,
            /// <summary>
            /// blue
            /// </summary>
            Blue,
            /// <summary>
            /// yellow
            /// </summary>
            Yellow
        }

        // Delegate
        /// <summary>
        /// Delegate ContainersRefTypeDelegate
        /// </summary>
        public delegate void ContainersRefTypeDelegate();

        ///<summary>
        ///GetColorCount
        /// </summary>
        // Property
        public long GetColorCount
        {
            get
            {
                return ColorCount;
            }

            private set
            {
                ColorCount = value;
            }
        }

        /// <summary>
        /// ContainersRefTypeNonRefMethod
        /// <param name ="parmsArray">array</param>
        /// </summary>
        // Method
        public static int ContainersRefTypeNonRefMethod(params object[] parmsArray)
        {
            return 0;
        }

        // Interface
        public interface ContainersRefTypeChildInterface
        {

        }

        // Class
        public class ContainersRefTypeChild
        {

        }

        // Event
        public event EventHandler ContainersRefTypeEventHandler
        {
            add { }
            remove { }
        }
    }

    // Struct Layout Explicit
    [StructLayout(LayoutKind.Explicit)]
    public class ExplicitLayoutClass
    {

    }
}

// Extension Methods
namespace CustomExtensionMethods
{
    // Extension methods must be defined in a static class
    public static class stringExtension
    {
        // The first parameter takes the "this" modifier
        // And specifies the type for which the method is defined
        /// <summary>
        /// extension method
        /// </summary>
        /// <param name="str">this</param>
        /// <returns>int</returns>
        public static int WordCount(this String str)
        {
            return 10;
        }
    }
}