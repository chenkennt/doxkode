Doc-as-Code: Metadata Format for .NET Languages
===============================================

0. Introduction
---------------

### 0.1 Goal and Non-goals

### 0.2 Terminology

1. Items
--------

The following .NET elements are defined as *items* in metadata:

1. Namespaces
2. Types, including class, struct, interface, enum, delegate
3. Type members, including field, property, method, event

Other elements like parameters, generic parameters are not standalone *items*, they're part of other *items*.

2. Identifiers
--------------

### 2.1 Unique Identifiers

For any *item* in .NET languages, its *UID* is defined by concatenating its parent's *UID* and its own *ID* by dot.
The *ID* for each kind of item is defined in the following sections.

### 2.1 Namespaces

For a namespace, *ID* is its name. 

> Example 2.1 *ID* and *UID* for namespace
>
> C#:
> ```csharp
> namespace System.IO
> {
> }
> ```
> YAML:
> ```yaml
> uid: System.IO
> id: IO
> ```

### 2.2 Types
Types include classes, structs, interfaces, enums, and delegates.
*ID* for a type is also its name.

> Example 2.2 *ID* and *UID* for types
> 
> C#:
> ```csharp
> namespace System
> {
>     public class String {}
>     public struct Boolean {}
>     public interface IComparable {}
>     public enum ConsoleColor {}
>     public delegate void Action();
> }
> ```
> YAML:
> ```yaml
> - uid: System.String
>   id: String
> - uid: System.Boolean
>   id: Boolean
> - uid: System.IComparable
>   id: IComparable
> - uid: System.ConsoleColor
>   id: ConsoleColor
> - uid: System.Action
>   id: Action
> ```

#### Nested Types

For nested types, *ID* is defined by concatenating the *ID* of all its containing types and the *ID* of itself, separated by dot.

The parent type of a nested type is its containing namespace, rather than its containing type.

> Example 2.3 *ID* and *UID* for nested type
>
> C#:
> ```csharp
> namespace System
> {
>     public class Environment
>     {
>         public enum SpecialFolder {}
>     }
> }
> ```
> YAML:
> ```yaml
> uid: System.Environment.SpecialFolder
> id: Environment.SpecialFolder
> ```

### 2.3 Members

Members include fields, properties, methods, events.

The *ID* of field, property or event is its name.

> Example 2.4 *ID* and *UID* for field, property and event
>
> C#:
> ```csharp
> namespace System
> {
>     public sealed class String
>     {
>         public static readonly String Empty;
>         public int Length { get; }
>     }
>
>     public static class Console
>     {
>         public static event ConsoleCancelEventHandler CancelKeyPress;
>     }
> }
> ```
> YAML:
> ```yaml
> - uid: System.String.Empty
>   id: Empty
> - uid: System.String.Length
>   id: Length
> - uid: System.Console.CancelKeyPress
>   id: CancelKeyPress
> ```

#### Methods

The *ID* of a method are defined by its name, followed by the list of the *UIDs* of its parameters:
```yaml
method_name(param1,param2,...)
```

Even a method does not have parameter, its *ID* **MUST** end with parentheses.

Constructor is treated as normal methods, i.e., its *ID* is the name of the type, followed by parameters. 

> Example 2.5 *ID* and *UID* for method
>
> C#:
> ```csharp
> namespace System
> {
>     public sealed class String
>     {
>         public String();
>         public String ToString();
>         public String ToString(IFormatProvider provider);
>     }
> }
> ```
> YAML:
> ```yaml
> - uid: System.String()
>   id: String()
> - uid: System.String.ToString()
>   id: ToString()
> - uid: System.String.ToString(System.IFormatProvider)
>   id: ToString(System.IFormatProvider)
> ```

#### Explicit Interface Implementation

The *ID* of an explicit interface implementation (EII) member **MUST** be prefixed by the *UID* of the interface it implements, enclosed by `[]`.

> Example 2.6 *ID* and *UID* for EII
>
> C#:
> ```csharp
> namespace System
> {
>     using System.Collections;
>
>     public sealed class String : IEnumerable
>     {
>         IEnumerator IEnumerable.GetEnumerator();
>     }
> }
> ```
> YAML:
> ```yaml
> - uid: "System.String.[System.Collections.IEnumerable.]GetEnumerator()"
>   id: "[System.Collections.IEnumerable.]GetEnumerator()"
> ```

#### Operator Overloads

#### Conversion Operator