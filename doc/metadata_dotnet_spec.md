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

### 2.1 Namespaces

For a namespace, *ID* is its name and *UID* is defined by concatenating its parent namespace's *UID* and its own *ID*, separated by dot. 

> Example 2.1 *ID* and *UID* for namespace
>
> C#:
> ```csharp
> namespace System.Xml
> {
> }
> ```
> YAML:
> ```yaml
> uid: System.Xml
> id: Xml
> ```

### 2.2 Types
Types include classes, structs, interfaces, enums, and delegates.
The definition of *ID* and *UID* of type is same as namespace.

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

Nested types are a bit different, the *ID* of a nested class if defined by concatenating the *ID* of all its containing types, separated by dot.
The definition of *UID* is same as normal types. 

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


#### 2.3.1 Explicit Interface Implementation
#### 2.3.2 Operator Overloads