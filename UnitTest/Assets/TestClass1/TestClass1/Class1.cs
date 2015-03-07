﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestClass1.Base;

namespace TestClass1
{
    /// <summary>
    /// Parital classes can not cross assemblies, ```Classes in assemblies are by definition complete.```
    /// </summary>
    public partial class Partial1
    {
        /// <summary>
        /// This is the property from *partial* class
        /// </summary>
        public string Property2 { get; set; }
    }

    /// <summary>
    /// This is a *Class1* in **TestClass1**
    /// </summary>
    public class Class1 : BaseClassForTestClass1
    {
        /// <summary>
        /// This is a *Class1*'s inner class
        /// </summary>
        public class Class1InnerClass
        {
            /// <summary>
            /// This is a *Class1*'s inner class's inner class
            /// </summary>
            public class Class1InnerClassesInnerClass
            {
            }

            /// <summary>
            /// This is the TestEnum for inner class
            /// </summary>
            public enum TestEnum
            {
                /// <summary>
                /// This is Enum A
                /// </summary>
                A,

                /// <summary>
                /// This is Enum B
                /// </summary>
                B,
            }
        }

        /// <summary>
        /// This is a Struct
        /// </summary>
        public struct TestStruct
        {

        }

        /// <summary>
        /// This is a Delegate
        /// </summary>
        public delegate void ADelegate();

        /// <summary>
        /// This is a *test* with no return
        /// </summary>
        public void Test1(int k)
        {
            Partial1 partial1 = new Partial1();
            
        }

        /// <summary>
        /// This is a *test* with return and <see cref="System.AccessViolationException"/> should fail compilation
        /// </summary>
        /// <returns></returns>
        public Tuple<string, int> Test1_3()
        {
            return null;
        }

        /// <summary>
        /// This is an async *test*
        /// </summary>
        /// <returns>Task</returns>
        public async Task<Tuple<string, int>> Test3()
        {
            return await Task.FromResult(Tuple.Create("1", 1));
        }
        /// <summary>
        /// This is a generic *test*
        /// </summary>
        /// <typeparam name="T">This is a Type param</typeparam>
        /// <param name="input">This is the input</param>
        /// <param name="output">This is the output</param>
        public void Test4<T>(T input, out T output) where T : class
        {
            output = null;
        }
    }

    namespace TestClass1.TestClassInnerNamesapce
    {
        /// <summary>
        /// This is a class in inner namespace
        /// </summary>
        public class ClassInInnerNamespace
        {
        }
    }
}
