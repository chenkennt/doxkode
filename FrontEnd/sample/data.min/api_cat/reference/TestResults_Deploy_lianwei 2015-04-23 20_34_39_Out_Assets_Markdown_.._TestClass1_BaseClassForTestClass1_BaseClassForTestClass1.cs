using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
[assembly: CLSCompliant(false)]

namespace Microsoft.DevDev.Build.PropertyOverload
{
    /// <summary>
    /// Class to test property in *BaseClass* to be inherited by: @Microsoft.DevDiv.Build.TestLibraryA.TestLibraryA .
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
    /// <summary>
    /// **Where** should `extension` method go? It is the extension method for <see cref="String"/>.
    /// </summary>
    // Extension methods must be defined in a static class
    public static class StringExtension
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