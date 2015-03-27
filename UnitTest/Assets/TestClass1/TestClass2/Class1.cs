using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace TestNestedDel
{
    public class Class1<TKey, TValue>
    {
        public delegate TValue InnerDelegate(TKey tkey);
        public void M(TKey a, InnerDelegate d)
        {
        }
    }

    public class Class2<TKey, TValue>
    {
        public class Class3<TKey2, TValue2>
        {
            public delegate TValue InnerDelegate(TKey tkey);
            public void M(TKey a, InnerDelegate d)
            {
            }
        }
    }

    public class Class3<TKey, TValue>
    {
        public class Class4<TKey2, TValue2>
        {
            public delegate TValue2 InnerDelegate(TKey2 tkey);
            public void M(TKey2 a, InnerDelegate d)
            {
            }
        }
    }
}

namespace Microsoft.Special.Types
{
    public class typeClass
    {
        // 1. ArrayTypeDecorator
        public int[,,] funcArray1()
        {
            return new int[2, 3, 4];
        }
        // 2. SZArrayTypeDecorator
        public int[][] funcArray2()
        {
            return new int[5][];
        }
        // 3. ByRefTypeDecorator
        public void funcRef(ref string str)
        {
        }
        // 4. PointerTypeDecorator
        public unsafe void funcPointer(int* p)
        {
        }
        // 5. CModType
        public volatile int iVolatile;
        // 6. ClassTypeDecorator or ValueTypeTypeDecorator
        public void funcClassTypeDecorator(int a, string b) { }
        // 7. GenericParameterRef
        public void funcParamRef<T>(T a) { }
        // 8. CLRGenericInstantiation
        public void funcGenericInstantiation(IEnumerable<string> s) { }
        // 9. CLRType, types defined in current module
        public void funcCLRType(typeClass tc) { }
        // 10. CLRTypeReference, types defined out of current module
        public void funcCLRTypeRef(DateTime s) { }
        // 11. SimpleType, when type is String or Object ref is true; otherwise, ref is false
        public void funcSimpleType(string s) { }
        // 12. TypedByRefType
        public void funcTypedByRefType(TypedReference tr) { }
    }
}

namespace TestLibraryC
{
    // Multi Inherit
    public class TestLibraryC : Microsoft.DevDiv.Build.TestLibraryB.TestLibraryB,
                                Microsoft.DevDiv.Build.TestLibraryB.TestLibraryBInterfaceBase
    {
        private int c = 0;

        public void MultiplyMethod()
        {

        }

        public int MinusMethod()
        {
            return 0;
        }
    }
}

namespace TestParameters
{
    // Parameters with In/Out attribute and out keyword
    public class ParametersWithInOutAttribute
    {
        // out keyword
        public void FooOutKeyword(out int i)
        {
            i = default(int);
        }

        //Only Out attribute
        public void FooOutAttribute([Out] string s)
        {

        }

        //Only In attribute
        public void FooInAttribute([In] int i)
        {

        }

        public void FooInOutAttribute([In, Out] double d)
        {

        }
    }
}

namespace InvisibleNamespace
{
    internal class InvisibleClass
    {
    }

    internal delegate void InvisibleDelegate();

    internal struct InvisibleStruct
    {
    }

    internal interface InvisibleInterface
    {
    }

    internal enum InvisibleEnum
    {
        First,
        Second,
    }
}

namespace Microsoft.Special.EmptyClass
{
    public class EmptyClass
    {
    }
}

namespace Microsoft.Special.Operators
{
    public class OperatorClass
    {
        public static int operator +(OperatorClass lsr, int rsr) { return 1; }
        public static int operator -(OperatorClass lsr, int rsr) { return 1; }
        public static int operator -(OperatorClass lsr, long rsr) { return 1; }
        public static int operator -(OperatorClass lsr, double rsr) { return 1; }
        public static int operator %(OperatorClass lsr, int a) { return a; }
        public static double operator *(OperatorClass lsr, double a) { return a; }
        public static double operator /(OperatorClass lsr, double a) { return a; }
        public static OperatorClass operator ++(OperatorClass lsr) { return lsr; }
        public static OperatorClass operator --(OperatorClass lsr) { return lsr; }
        public static bool operator ==(OperatorClass lsr, OperatorClass rsr) { return lsr == rsr; }
        public static bool operator !=(OperatorClass lsr, OperatorClass rsr) { return lsr != rsr; }
        public static bool operator >(OperatorClass lsr, OperatorClass rsr) { return lsr > rsr; }
        public static bool operator >=(OperatorClass lsr, OperatorClass rsr) { return lsr >= rsr; }
        public static bool operator <(OperatorClass lsr, OperatorClass rsr) { return lsr < rsr; }
        public static bool operator <=(OperatorClass lsr, OperatorClass rsr) { return lsr <= rsr; }
        public string this[string index]
        {
            get { return ""; }
            set { }
        }
        public override bool Equals(object operatorClassObj) { return true; }
        public override int GetHashCode() { return 1; }
        public static implicit operator OperatorClass(byte value) { return new OperatorClass(); }
        public static implicit operator OperatorClass(int value) { return new OperatorClass(); }
        public static explicit operator long (OperatorClass value) { return 1; }
        public static explicit operator int (OperatorClass value) { return 1; }
    }

    public class OperatorClass<T, K>
    {
        public static int operator +(OperatorClass<T, K> lsr, int rsr) { return 1; }
        public static int operator -(OperatorClass<T, K> lsr, int rsr) { return 1; }
        public static int operator -(OperatorClass<T, K> lsr, long rsr) { return 1; }
        public static int operator -(OperatorClass<T, K> lsr, double rsr) { return 1; }
        public static int operator %(OperatorClass<T, K> lsr, int a) { return a; }
        public static double operator *(OperatorClass<T, K> lsr, double a) { return a; }
        public static double operator /(OperatorClass<T, K> lsr, double a) { return a; }
        public static OperatorClass<T, K> operator ++(OperatorClass<T, K> lsr) { return lsr; }
        public static OperatorClass<T, K> operator --(OperatorClass<T, K> lsr) { return lsr; }
        public static bool operator ==(OperatorClass<T, K> lsr, OperatorClass<T, K> rsr) { return lsr == rsr; }
        public static bool operator !=(OperatorClass<T, K> lsr, OperatorClass<T, K> rsr) { return lsr != rsr; }
        public static bool operator >(OperatorClass<T, K> lsr, OperatorClass<T, K> rsr) { return lsr > rsr; }
        public static bool operator >=(OperatorClass<T, K> lsr, OperatorClass<T, K> rsr) { return lsr >= rsr; }
        public static bool operator <(OperatorClass<T, K> lsr, OperatorClass<T, K> rsr) { return lsr < rsr; }
        public static bool operator <=(OperatorClass<T, K> lsr, OperatorClass<T, K> rsr) { return lsr <= rsr; }
        public string this[string index]
        {
            get { return ""; }
            set { }
        }
        public override bool Equals(object OperatorClassObj) { return true; }
        public override int GetHashCode() { return 1; }
    }

    public struct OperatorStruct
    {
        public static int operator +(OperatorStruct lsr, int rsr) { return 1; }
        public static int operator -(OperatorStruct lsr, int rsr) { return 1; }
        public static int operator -(OperatorStruct lsr, long rsr) { return 1; }
        public static int operator -(OperatorStruct lsr, double rsr) { return 1; }
        public static int operator %(OperatorStruct lsr, int a) { return a; }
        public static double operator *(OperatorStruct lsr, double a) { return a; }
        public static double operator /(OperatorStruct lsr, double a) { return a; }
        public static OperatorStruct operator ++(OperatorStruct lsr) { return lsr; }
        public static OperatorStruct operator --(OperatorStruct lsr) { return lsr; }
        public static bool operator ==(OperatorStruct lsr, OperatorStruct rsr) { return lsr == rsr; }
        public static bool operator !=(OperatorStruct lsr, OperatorStruct rsr) { return lsr != rsr; }
        public static bool operator >(OperatorStruct lsr, OperatorStruct rsr) { return lsr > rsr; }
        public static bool operator >=(OperatorStruct lsr, OperatorStruct rsr) { return lsr >= rsr; }
        public static bool operator <(OperatorStruct lsr, OperatorStruct rsr) { return lsr < rsr; }
        public static bool operator <=(OperatorStruct lsr, OperatorStruct rsr) { return lsr <= rsr; }
        public string this[string index]
        {
            get { return ""; }
            set { }
        }
        public override bool Equals(object operatorClassObj) { return true; }
        public override int GetHashCode() { return 1; }
    }

    public struct OperatorStruct<T, K>
    {
        public static int operator +(OperatorStruct<T, K> lsr, int rsr) { return 1; }
        public static int operator -(OperatorStruct<T, K> lsr, int rsr) { return 1; }
        public static int operator -(OperatorStruct<T, K> lsr, long rsr) { return 1; }
        public static int operator -(OperatorStruct<T, K> lsr, double rsr) { return 1; }
        public static int operator %(OperatorStruct<T, K> lsr, int a) { return a; }
        public static double operator *(OperatorStruct<T, K> lsr, double a) { return a; }
        public static double operator /(OperatorStruct<T, K> lsr, double a) { return a; }
        public static OperatorStruct<T, K> operator ++(OperatorStruct<T, K> lsr) { return lsr; }
        public static OperatorStruct<T, K> operator --(OperatorStruct<T, K> lsr) { return lsr; }
        public static bool operator ==(OperatorStruct<T, K> lsr, OperatorStruct<T, K> rsr) { return lsr == rsr; }
        public static bool operator !=(OperatorStruct<T, K> lsr, OperatorStruct<T, K> rsr) { return lsr != rsr; }
        public static bool operator >(OperatorStruct<T, K> lsr, OperatorStruct<T, K> rsr) { return lsr > rsr; }
        public static bool operator >=(OperatorStruct<T, K> lsr, OperatorStruct<T, K> rsr) { return lsr >= rsr; }
        public static bool operator <(OperatorStruct<T, K> lsr, OperatorStruct<T, K> rsr) { return lsr < rsr; }
        public static bool operator <=(OperatorStruct<T, K> lsr, OperatorStruct<T, K> rsr) { return lsr <= rsr; }
        public string this[string index]
        {
            get { return ""; }
            set { }
        }
        public override bool Equals(object OperatorStructObj) { return true; }
        public override int GetHashCode() { return 1; }
    }
}

namespace Microsoft.Template
{
    public interface TemplateInterface<T, in K, out L>
    {
        void Add(T a, out int b);
        void Add(T a, K b);
    }
}

namespace TestLibraryC.Sprint66.BasicMethod
{
    public interface INormalAdvance<T>
    {

    }
    public class NormalAdvance<T> : INormalAdvance<T>
    {

    }
    public interface INormalBasic
    {

    }
    public class NormalBasic : INormalBasic
    {

    }
    public interface INormalTemplate<T>
    {

    }
    public class NormalTemplate<T> : INormalTemplate<T>
    {

    }
    public interface IBasic<in T, out K>
    {

    }
    public interface IInBasic<in T>
    {

    }
    public interface IOutBasic<out T>
    {

    }
    public interface IStrictInBasic<T> : IInBasic<T>
    {

    }
    public interface IStrictOutBasic<T> : IOutBasic<T>
    {

    }
    public class OutBasic<T> : IOutBasic<T>
    {

    }
    public class UnsafePointer
    {
        public unsafe UnsafePointer(int* a) { }

        public unsafe int* IntPointerProperty { get { return null; } set { } }
        public unsafe int this[long* a] { get { return 1; } }

        public unsafe void IntPointerMethod(int* a) { }
        public unsafe void* VoidPointerMethod(int a) { return null; }
        public unsafe long* LongPointerMethod(long* a) { return null; }

        public unsafe double* DoubleFiled;

        public unsafe delegate string IntPointerParaDelegate(int* a);
        public unsafe delegate long* LongPointerDelegate(int a);

        public static unsafe string operator +(UnsafePointer up, int* r) { return "1"; }
        public static unsafe string operator -(UnsafePointer up, int* r) { return "1"; }
        public static unsafe implicit operator UnsafePointer(int* target) { return null; }
        public static unsafe explicit operator string (UnsafePointer source) { return null; }
    }
}

namespace TestLibraryC.Sprint66.FakeAttribute
{
    public enum Language
    {
        CSharp,
        VisualBasic,
    }

    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public class ExtensionHelperAttribute : Attribute
    {
        public readonly string methodId;

        public ExtensionHelperAttribute(string methodId)
        {
            this.methodId = methodId;
        }
    }
}

namespace TestLibraryC.Sprint66.ExtensionMethods
{
    using BasicMethod;
    using FakeAttribute;
    public static class stringExtensions
    {
        /// <summary>
        /// basic class extension scenario
        /// </summary>
        [ExtensionHelper("e190c30b-8ae7-42ff-a848-3e096473ae18")]
        public static int MethodA(this string str) { return 1; }

        /// <summary>
        /// overload extension scenario
        /// </summary>
        [ExtensionHelper("cb0adee8-5231-41b1-b556-bc0a33b95231")]
        public static DateTime MethodA(this string str, DateTime time) { return time; }

        /// <summary>
        /// complex type parameter extension scenario
        /// </summary>
        [ExtensionHelper("df070ea0-702c-4a70-b388-549af733b16e")]
        public static void MethodB(this string str, List<string> a) { }
    }

    public static class intExtensions
    {
        /// <summary>
        /// struct extension method scenario with out parameter
        /// </summary>
        [ExtensionHelper("b13726b0-dd21-47dc-a458-99ad1b9e7ac0")]
        public static bool TryParse(this int i, string s, out int result)
        {
            result = 1; return true;
        }
    }

    public static class DictionaryExtensions
    {
        /// <summary>
        /// template class extension method
        /// </summary>
        [ExtensionHelper("085c6325-f39d-4ab2-949a-ed21c633e93e")]
        public static bool All<TKey, TValue>(this Dictionary<TKey, TValue> dic, int condition) { return false; }

        /// <summary>
        /// should also be regard as Dictionary<TKey, TValue> 's extension method, and will be overloaded method with the above one
        /// </summary>
        [ExtensionHelper("9925c3b2-1cd7-4bce-9af4-c284c4b1cf84")]
        public static bool All<TK, TV>(this Dictionary<TK, TV> dic, List<string> condition) { return true; }

        /// <summary>
        /// should also be regard as Dictionary<TKey, TValue> 's extension method
        /// </summary>
        [ExtensionHelper("71bd7f59-7c49-4cfe-a84a-d9badb316e18")]
        public static void AsParallel<T, V>(this Dictionary<T, V> dic) { }

        /// <summary>
        /// special func for Dictionary<int, int> only, Dictionary<TKey, TValue> won't have this.
        /// </summary>
        [ExtensionHelper("382c1ce3-6a3b-4fa4-9143-7f335d1fe15b")]
        public static void AsParallelSpecial(this Dictionary<int, int> dic) { }
    }

    public static class IListExtensions
    {
        /// <summary>
        /// interface extension method, should be inheritant by class List<T>
        /// </summary>
        [ExtensionHelper("55b74131-0080-4406-ac11-37b1b75bcd04")]
        public static bool AsParallel<T>(this IList<T> iList) { return false; }
    }

    public static class ListExtensions
    {
        /// <summary>
        /// template class extension method, which should be overloaded with the one in class IListExtensions
        /// </summary>
        [ExtensionHelper("b1b23cdd-d441-4099-9a65-1ca46fa30cf5")]
        public static bool AsParallel<TK>(this List<TK> list, bool result) { return result; }

        /// <summary>
        /// List<T> 's extension method
        /// </summary>
        [ExtensionHelper("3e958a3a-ca54-43d5-a450-bdc27fa6e14b")]
        public static bool Find<T, V>(this List<T> list, T a, V b) { return true; }

        /// <summary>
        /// List<T> 's extension method, same with above one
        /// </summary>
        [ExtensionHelper("906ebaeb-68f9-41b3-b9d1-d4eb4927c113")]
        public static bool Find<T, V>(this List<T> list, V a, T b) { return true; }

        /// <summary>
        /// List<T> 's extension method with constrains
        /// </summary>
        [ExtensionHelper("9bb77d78-4039-40a0-821a-a7dcd90e1839")]
        public static bool Find<T, K, L>(this List<T> list, T a, K b, L c)
            //where T: struct
            where K : struct
        {
            return false;
        }

        /// <summary>
        /// List<T> 's extension method with constrains, different with above one
        /// </summary>
        [ExtensionHelper("fe6b5ed2-fd3c-422d-ad19-aae6f9bb876e")]
        public static bool Find<T, K, L>(this List<T> list, L a, K b, T c)
            //where T: class
            where K : class
        {
            return false;
        }

        [ExtensionHelper("71d17bdb-ec13-42ce-824a-9499670520c9")]
        public static void ExHiddenMethod<T>(this List<List<T>> list) { }

        [ExtensionHelper("53345562-37a7-47ab-b6cb-613d05acccf0")]
        public static bool ExOverloadMethod<T>(this List<List<T>> list) { return false; }
    }

    public static class IEnumerableExtensions
    {
        [ExtensionHelper("eede242f-d8fd-46ca-9d94-673df38082e3")]
        public static void ExHiddenMethod<T>(this IEnumerable<IEnumerable<T>> IEnum) { }

        [ExtensionHelper("0ef23997-97c1-45cd-afe4-b65e3f5956e4")]
        public static bool ExOverloadMethod<T>(this IEnumerable<IEnumerable<T>> IEnum, T t) { return false; }

        [ExtensionHelper("b3cace09-34ac-4980-ab33-10c166124a81")]
        public static bool IEnumerableMethodA(this IEnumerable<string> IEnum) { return true; }
    }

    public static class INormalAdvanceExtensions
    {
        [ExtensionHelper("971BE40A-6E57-4ED4-BADC-EFC1B7317BD0")]
        public static bool AddOverloadMethod(this INormalAdvance<string> ina, int a) { return false; }

        [ExtensionHelper("E7CDCABF-F408-4C1F-AE90-DF498C613B82")]
        public static double IndependentMethod(this INormalAdvance<int> ina, double a) { return 1.0; }

        [ExtensionHelper("BC4F8484-F03F-4DF3-9A37-968B9A38220F")]
        public static long AddOverloadMethod<T>(this INormalAdvance<T> ina, T b, long c) { return 1; }

        [ExtensionHelper("6DB9B8CD-56FF-4870-86AB-2629189C0CA7")]
        public static long AddOverloadMethod<U>(this INormalAdvance<U> ina, U b, U c) { return 1; }
    }

    public static class NormalAdvanceExtensions
    {
        [ExtensionHelper("88BB42AF-8A46-4D57-9EA7-94313908082F")]
        public static string AddOverloadMethod<T>(this NormalAdvance<T> na, int a) { return ""; }
    }

    public static class INormalBasicExtensions
    {
        [ExtensionHelper("AB8ECFE6-589D-4030-8F11-B4A3F7179CF4")]
        public static void AddOverloadMethod<T>(this INormalBasic inb, T t) { }

        [ExtensionHelper("CD3C22FF-C73E-456E-8604-8225381271CB")]
        public static bool HiddenMethod(this INormalBasic inb, int a) { return false; }
    }

    public static class NormalBasicExtensions
    {
        [ExtensionHelper("050D1E62-088B-400A-B8DE-11C711253E80")]
        public static void AddOverloadMethod(this NormalBasic nb, int a) { }

        [ExtensionHelper("21749650-BC10-43C4-9394-2F40077A4274")]
        public static void HiddenMethod(this NormalBasic nb, int a) { }
    }

    public static class INormalTemplateExtensions
    {
        [ExtensionHelper("E1158F9A-042B-47D3-8501-924C76F72508")]
        public static void AddOverloadMethod<T>(this INormalTemplate<T> inb, T t) { }

        [ExtensionHelper("09FD1748-F34A-4731-BA40-818498B13EC1")]
        public static bool HiddenMethod<T>(this INormalTemplate<T> inb, int a) { return false; }
    }

    public static class NormalTemplateExtensions
    {
        [ExtensionHelper("D65A49FF-A120-4CE6-8EBA-2C5164D99017")]
        public static void AddOverloadMethod<T>(this NormalTemplate<T> nb, int a) { }

        [ExtensionHelper("1DB5AEB1-C2E5-4E8A-8DBC-34E000C2E999")]
        public static void HiddenMethod<T>(this NormalTemplate<T> nb, int a) { }
    }

    public static class IBasicExtensions
    {
        [ExtensionHelper("ff9945ae-b010-4910-88e1-615e2b8a1d31")]
        public static void BasicA<T, K>(this IBasic<T, K> ib) { }

        [ExtensionHelper("5eb53587-70da-4fd5-9bd5-092d69633e3e")]
        public static void BasicB(this IBasic<string, object> ib) { }

        [ExtensionHelper("3d27178a-e990-4811-a9f1-8287bbf1664b")]
        public static void BasicC<T>(this IBasic<T, object> ib) { }

        [ExtensionHelper("6ac63edb-ef08-41cc-948f-bbf4e045dbaf")]
        public static void BasicD<T>(this IBasic<object, string> ib) { }
    }

    public static class IInBasicExtensions
    {
        [ExtensionHelper("026c86f0-e95a-4034-8326-5a88a90dc934")]
        public static void InBasicA<T>(this IInBasic<IInBasic<T>> inb) { }

        [ExtensionHelper("a5cf6682-41c9-47e3-9289-553988973186")]
        public static void InBasicB(this IInBasic<string> in1b) { }

        [ExtensionHelper("ed64bdb4-ffd8-436b-8c1a-3986ab217a3f")]
        public static void InBasicC(this IInBasic<IInBasic<object>> in2b) { }

        [ExtensionHelper("96aaa84a-9298-489c-a66e-87c9c450de35")]
        public static void InBasicD(this IInBasic<IInBasic<IInBasic<string>>> in3b) { }

        [ExtensionHelper("dbf6df2f-c219-46fd-8ca0-90572de2ba5a")]
        public static void InBasicE(this IInBasic<IStrictInBasic<IInBasic<string>>> inisinb) { }
    }

    public static class IOutBasicExtensions
    {
        [ExtensionHelper("5f6fd6a2-26dc-4ac2-a773-e2f134953592")]
        public static void OutBasicA<T>(this IOutBasic<IOutBasic<T>> outb) { }

        [ExtensionHelper("217d690f-c204-49ad-90c2-a124f48e6155")]
        public static void OutBasicB(this IOutBasic<IStrictOutBasic<IOutBasic<object>>> outisoutb) { }
    }

    public static class IStrictInBasicExtensions
    {
        [ExtensionHelper("936bd02e-58fc-4a80-9b58-417009fd0b15")]
        public static void StrictInBasicA<T>(this IStrictInBasic<IInBasic<T>> isinb) { }

        [ExtensionHelper("73c72be9-16e3-4fff-93fc-00aa53fb8364")]
        public static void StrictInBasicB(this IStrictInBasic<IInBasic<string>> isinb) { }
    }

    public static class IStrictOutBasicExtensions
    {
        [ExtensionHelper("af3bae83-3d5b-4773-839e-dcbea3f42b38")]
        public static void StrictOutBasicA<T>(this IStrictOutBasic<IOutBasic<T>> isoutb) { }

        [ExtensionHelper("c838dac4-9b02-4f42-a28d-98e529b43a03")]
        public static void StrictOutBasicB(this IStrictOutBasic<IOutBasic<object>> isoutb) { }
    }
}

namespace TestLibraryC.Sprint66.ClassWithExtensions
{
    using BasicMethod;
    using ExtensionMethods;
    using FakeAttribute;
    public class NormalTemplateDerived<T> : NormalTemplate<T>
    {
        void A()
        {
            this.AddOverloadMethod(1);
            this.AddOverloadMethod<T>(default(T));
            this.HiddenMethod(1);
            //bool a = this.AddOverloadMethod<string>(1);   //this method is invalid
        }
    }

    public class NormalBasicDerived : NormalBasic
    {
        void A()
        {
            this.AddOverloadMethod<double>(1.0);    //this means this class will have AddOverloadMethod<T> extension method
            this.AddOverloadMethod(1);
            this.AddOverloadMethod<string>("1");
            this.HiddenMethod(1);
            //bool b = this.HiddenMethod(1);    //this is error because it is hidden by NormalBasic class's extension method
        }
    }

    public class NormalAdvanceDerived : NormalAdvance<string>, INormalAdvance<int>
    {
        void A()
        {
            this.AddOverloadMethod(1);  //AddOverloadMethod<T>(this NormalAdvance<T> na, int a) //from normal
            this.AddOverloadMethod(1, 1);   //AddOverloadMethod<T>(this INormalAdvance<T> na, int a, int b)
            this.AddOverloadMethod(1, (long)2);   //AddOverloadMethod<T>(this INormalAdvance<T> na, int a, long b)
            this.IndependentMethod(1.0);
        }
    }

    public class NormalAdvanceOne : INormalAdvance<string>
    {
        void A()
        {
            this.AddOverloadMethod(1);
            this.AddOverloadMethod<string>(string.Empty, 1);
        }
    }

    public class NormalAdvanceTwo : INormalAdvance<long>
    {
        void A()
        {
            this.AddOverloadMethod<long>(1, 1);
        }
    }

    public class NormalAdvanceThree : NormalAdvance<string>
    {
        void A()
        {
            //bool b = this.AddOverloadMethod(1);   //this is invalid
            string a = this.AddOverloadMethod(1);
            this.AddOverloadMethod("", 1);
            this.AddOverloadMethod<string>("", "");
        }
    }

    public class NormalBasicOne : INormalBasic
    {
        void A()
        {
            this.AddOverloadMethod<int>(1);
            this.HiddenMethod(1);
        }
    }

    public class InBasicOneIn<T, K> : IInBasic<T>
        where T : IInBasic<K>     //T is more derived than IBasic2<K> because it derived from IBasic2<K>
    {
        void A()
        {

        }
    }

    public class InBasicTwoIn<T> : IInBasic<IInBasic<string>>
    {
        void A()
        {
            this.InBasicA();
            this.InBasicC();
        }
    }

    public class InBasicThreeIn : IInBasic<IInBasic<IInBasic<object>>>
    {
        void A()
        {
            this.InBasicA();
            this.InBasicC();
            this.InBasicD();
            this.InBasicE();
        }
    }

    public class OutBasicTwoOut<T> : OutBasic<OutBasic<T>>
    {
        void A()
        {
            this.OutBasicA();
        }
    }

    public class BasicOne : IBasic<string, string>
    {
        void A()
        {
            this.BasicA();
            this.BasicB();
        }
    }

    public class BasicTwo : IBasic<object, object>
    {
        void A()
        {
            this.BasicA();
            this.BasicB();
            this.BasicC();
        }
    }

    public class BasicThree<T> : IBasic<IBasic<object, object>, string>
        where T : IBasic<object, object>, new()
    {
        void A()
        {
            this.BasicA();
        }
    }

    /// <summary>
    /// This class can have BasicA and BasicC methods
    /// </summary>
    /// <typeparam name="T">T is struct</typeparam>
    public class BasicFour<T> : IBasic<T, object>
        where T : struct
    {
        void A()
        {
            this.BasicA();
            this.BasicC();
        }
    }

    public class StrictInBasicA<T> : IStrictInBasic<IInBasic<T>>
    {
        void A()
        {
            this.InBasicA();
            this.StrictInBasicA();
        }
    }

    public class StrictInBasicB : IStrictInBasic<IInBasic<object>>
    {
        void A()
        {
            this.InBasicA();
            this.InBasicC();
            this.StrictInBasicA();
        }
    }

    public class StrictInBasicC : IStrictInBasic<IInBasic<string>>
    {
        void A()
        {
            this.InBasicA();
            this.InBasicC();
            this.StrictInBasicA();
            this.StrictInBasicB();
        }
    }

    public class StrictOutBasicA<T> : IStrictOutBasic<IOutBasic<T>>
    {
        void A()
        {
            this.OutBasicA();
            this.StrictOutBasicA();
        }
    }

    public class StrictOutBasicB<T> : IStrictOutBasic<IOutBasic<string>>
    {
        void A()
        {
            this.OutBasicA();
            this.StrictOutBasicA();
        }
    }

    public class StrictOutBasicC<T> : IStrictOutBasic<IOutBasic<object>>
    {
        void A()
        {
            this.OutBasicA();
            this.StrictOutBasicA();
            this.StrictOutBasicB();
        }
    }

    public class ListListString : List<List<string>>
    {
        void A()
        {
            this.ExHiddenMethod();
            this.ExOverloadMethod();
            this.AsParallel();
            this.AsParallel(false);
            this.Find("1", new List<string>());
            this.Find(new List<string>(), "1");
            this.Find("1", "1", new List<string>());
            this.Find(new List<string>(), 1, "1");
        }
    }

    public class ListListU<U> : List<List<U>>
    {
        void A()
        {
            this.AsParallel();
            this.AsParallel(false);
            this.ExHiddenMethod();
            this.ExOverloadMethod();
            this.Find("1", new List<U>());
            this.Find(new List<U>(), "1");
            this.Find("1", "1", new List<U>());
            this.Find(new List<U>(), 1, "1");
        }
    }

    public class ListT<T> : List<T>
        where T : new()
    {
        void a()
        {
            T t = default(T);
            this.AsParallel();
            this.AsParallel(false);
            this.Find(t, "1");
            this.Find("1", t);
            this.Find(t, 1, 1);
            this.Find(t, 1, "1");
        }
    }
}

namespace TestLibraryC.Sprint66.EII
{
    public class MixedAnimal : Dog<string, int>, Cat
    {
        //Constructor
        public MixedAnimal() { }
        //Property
        public float MixedAnimalCount { get { return mixedAnimalCount; } set { value = mixedAnimalCount; } }
        //Field
        protected long mixedAnimalCount;
        //Method
        public void ForEvent()
        {
            EventHandler handler1 = event1;
            EventHandler hanlder2 = selfevent;
        }
        //event
        private event EventHandler selfevent;

        public event EventHandler event1;

        event EventHandler SelfEvent
        {
            add { selfevent += value; }
            remove { selfevent -= value; }
        }
        //explicit implement event
        event EventHandler Dog<string, int>.eatEvent
        {
            add { event1 += value; }
            remove { event1 -= value; }
        }

        event EventHandler Cat.eatEvent
        {
            add { event1 += value; }
            remove { event1 -= value; }
        }
        //explicit implement property
        string Dog<string, int>.Name { get { return "Buf"; } }
        string Cat.Name { get { return "Tom"; } }
        bool Dog<string, int>.IsAlive { get { return true; } }
        bool Cat.IsAlive { get { return true; } }
        string Dog<string, int>.nickName { get { return "SmallDog"; } set { } }
        string Cat.NickName { get { return "SmallCat"; } set { } }
        bool Dog<string, int>.HasBone { get { return true; } }
        bool Cat.HasFish { get { return false; } }
        string Dog<string, int>.this[int a] { get { return "Dog"; } }
        string Cat.this[int a] { get { return "Cat"; } }
        //explicit implement method
        void Dog<string, int>.Bite(string tool) { }
        void Cat.Bite(string tool) { }
        int Dog<string, int>.Jump() { return 1; }
        double Cat.Jump() { return 1.0; }
        long Dog<string, int>.Sleep() { return 2; }
        long Cat.sleep() { return 1; }

        [ComVisible(true)]
        void Dog<string, int>.Swim<K>(K a) { }

        [ComVisible(false)]
        void Cat.Swim<T>(T a) { }
        //explicit implement Animal's member
        void Animal.Eat() { }
        void Animal.Eat(string t) { }
        string Animal.Name { get { return "Animal"; } }
        bool Animal.HasTail { get { return false; } }
        int Animal.FeetNumber { get { return 1; } }
        string Animal.this[int a] { get { return "Animal"; } }
        string Animal.this[int a, int b] { get { return "Animal"; } }
        long Animal.this[string a] { get { return 1; } }
    }

    public class Monster<T> : MixedAnimal
    {

    }

    public class TOMCAT : MixedAnimal, Cat
    {
        string Animal.Name { get { return "TOMCAT"; } }

        void Animal.Eat() { }

        event EventHandler Cat.eatEvent
        {
            add { eventTom += value; }
            remove { eventTom -= value; }
        }

        //not eii
        public void ForTOMCATEvent()
        {
            EventHandler handler1 = eventTom;
        }

        public event EventHandler eventTom;
    }

    public class Monkey
    {
        protected class WuKong : Monster<string>
        {

        }
    }

    public class Pig : Animal
    {
        void Animal.Eat() { }
        void Animal.Eat(string t) { }
        string Animal.Name { get { return "Pig"; } }
        public bool HasTail { get { return true; } }
        public int FeetNumber { get { return 4; } }
        string Animal.this[int a] { get { return "Animal"; } }
        string Animal.this[int a, int b] { get { return "Animal1"; } }
        long Animal.this[string a] { get { return 1; } }
        public string this[int a] { get { return "Pig"; } }
    }

    public interface Dog<K, U> : Animal
        where K : class
        where U : struct
    {
        //event
        event EventHandler eatEvent;
        //property
        new string Name { get; }
        bool IsAlive { get; }
        bool HasBone { get; }
        string nickName { get; set; }
        new string this[int a] { get; }
        //method
        void Bite(string tool);
        int Jump();
        long Sleep();
        void Swim<T>(T a)
            where T : class;
    }

    public interface Cat : Animal
    {
        //event
        event EventHandler eatEvent;
        //property
        new string Name { get; }
        bool IsAlive { get; }
        bool HasFish { get; }
        string NickName { get; set; }
        new string this[int a] { get; }
        //method
        void Bite(string tool);
        double Jump();
        long sleep();
        void Swim<T>(T a)
            where T : struct;
    }

    public interface Animal
    {
        //property
        string Name { get; }
        bool HasTail { get; }
        int FeetNumber { get; }
        string this[int a] { get; }
        string this[int a, int b] { get; }
        long this[string a] { get; }
        //method
        void Eat();
        void Eat(string tool);
    }
}

namespace TestLibraryC.Sprint66.EnumMethod
{
    public enum Grades
    {
        F = 0, D = 1, C = 2, B = 3, A = 4
    }

    public static class Extensions
    {
        public static Grades minPassing = Grades.D;
        public static bool Passing(this Grades grade)
        {
            return grade >= minPassing;
        }
        public static bool Passing(this Grades grade, Grades fakeGrade)
        {
            return fakeGrade >= minPassing;
        }
    }
}