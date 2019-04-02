# Karambolo.Common

The main goal of this library is to gather base functionality that is frequently needed but not built into the .NET Base Class Library. It also provides backports of a few APIs for legacy target frameworks. (Target frameworks: .NET Framework 4 & 4.5, .NET Standard 1.0 & 2.0).

[![NuGet Release](https://img.shields.io/nuget/v/Karambolo.Common.svg)](https://www.nuget.org/packages/Karambolo.Common/)

Before version 3.0 the library rather served as a common base for my projects including a bunch of APIs not so relevant for the public. The codebase of version 3 went through a major revision and the APIs to include were carefully selected to keep the library as focused and lightweight as possible. This, of course, involved a lot of breaking changes, what users of the previous versions should keep in mind when considering upgrading.

### Feature highlights

[Generic ordered dictionary](#generic-ordered-dictionary)  
[Localization base types](#localization-base-types)  
[Monetary types](#monetary-types)  
[Path utilities](#path-utilities)  
[String utilities](#string-utilities)  
[Date/time utilities](#datetime-utilities)  
[Tree traversal](#tree-traversal)  
[Expression tree utilities](#expression-tree-utilities)  
[Reflection utilities](#reflection-utilities)  
[Building/parsing assembly and type names](#buildingparsing-assembly-and-type-names)  
[IEnumerable<T> counterparts of several Rx operators](#ienumerablet-counterparts-of-several-rx-operators)  
[IList<T> and IReadOnlyList<T> extension methods](#ilistt-and-ireadonlylistt-extension-methods)  

#### Generic ordered dictionary

There is [a non-generic ordered dictionary in the BCL](https://docs.microsoft.com/en-us/dotnet/api/system.collections.specialized.ordereddictionary "a non-generic ordered dictionary in the BCL") but we've never got its generic counterpart for some unknown reason (well, at least it's unknown to me). Though the BCL contains a class named `SortedDictionary`, it solves another problem. It stores items sorted (in a binary search tree) while `OrderedDictionary` has nothing to do with the order relation of the items, but retains insertion order.

You can find some more or less complete solutions on the internet but it's surprisingly tedious to implement a generic ordered dictionary if you want to cover every aspect of it (including the legacy interfaces) correctly. I put some effort in the implementation you find in this library and I think it's pretty close.

#### Localization base types

.NET Core has built-in ways to support localization, but .NET Framework offers no such functionality out-of-the-box. For that reason, the builds targeting the .NET Framework include some fundamental types for a similar localization solution. [My sample ASP.NET project](https://github.com/adams85/aspnetskeleton/tree/NetFramework/source/Web/UI/Infrastructure/Localization "My sample ASP.NET project") demonstrates how localization can be implemented on top of this foundation to make localization as painless as possible.

#### Monetary types

The `Monetary` namespace contains a few types which encapsulates base functionality for money-related applications, enabling you to write code with clear semantics.

The `Currency` struct simply stores an (ISO 4217) currency code. However, you can statically register metadata (a symbol and the default number of decimals used) for the currency code. The implementation is very economical: a `Currency` instance only takes up the size of a 32-bit integer.

The `Money` struct is a pair of a `decimal` and a `Currency` value. It defines a set of meaningful operators and functions and does the heavy-lifting of parsing and generating string representations based on culture and currency metadata information. 

And last but not least, there is a `Conversion` class, which defines a currency conversion. When instantiating, it takes an input and an output currency and an exchange rate, then it can be used to convert `Money` values.

Here is a simple console application which shows how all this works in practice:

```
static async Task Main()
{
    // registers currencies provided by the OS (on platforms other than Windows you may have to register currencies manually)
    Currency.RegisterSystemDefaults();
    // registers a custom currency
    Currency.Register("BTC", "₿", 8);

    var moneyString = Console.ReadLine();
    if (!Money.TryParse(moneyString, out var money))
    {
        Console.Error.WriteLine($"Invalid money value.");
        return;
    }
    else if (money.Currency == Currency.None)
    {
        Console.Error.WriteLine($"Currency was not specified.");
        return;
    }

    decimal rate;
    using (var httpClient = new HttpClient())
        rate = decimal.Parse(await httpClient.GetStringAsync($"https://blockchain.info/tobtc?currency={money.Currency.Code}&value=1"), CultureInfo.InvariantCulture);

    var conversion = new Conversion(money.Currency, Currency.FromCode("BTC"), rate);
    Console.WriteLine($"{money:C} = {conversion.Convert(money):C} ({conversion})");
}
```

You enter a money value (like '$100', 'USD 100', '100 usd', etc.), then the application tells how much bitcoin you could buy for that amount.

#### Path utilities

##### File paths

`PathUtils` offers two useful methods for dealing with file paths:

* `MakeValidFileName`, which, as its name suggests, can be used to produce a valid file name from an arbitrary string (it not only takes length and reserved characters into account but also reserved file names of Windows like *con*, *prn*, *prn.txt*, etc.),
* `MakeRelativePath`, which converts a path relative to the specified base path (the implementation had some bugs up to 3.1.0, was fixed in 3.1.1).

##### URL paths

`UriUtils` contains another helper method named `GetCanonicalPath`, which can be used to normalize URL paths (e.g. `/dir//subdir/./../my.file` -> `/dir/my.file`). (Version 3.1.2 contains an improved, optimized algorithm which gets edge cases right and allocates only when necessary.)

It's worth adding that [there is a built-in solution](https://stackoverflow.com/questions/3192389/whats-the-quickest-cleanest-way-to-remove-parent-pathing-from-a-url-string) that may be sufficient in some cases but it doesn't collapse multiple slashes (`/a//b`) and doesn't handle edge cases (like `/..` or `a/../../b`) well.

#### String utilities

The `StringUtils` class provides several useful extension methods for strings. Here are some of the most interesting ones:

* `BytesToHexString`/`BytesFromHexString`, which solve one of the popular issues on StackOverflow: how to convert a byte array to a hexadecimal string and vice versa (moreover, it's done in a performant way, which could be only improved further by leveraging `Span<T>` or unsafe operations).
* `Escape`/`Unescape`, which, not surprisingly, can escape/unescape special characters in a string with an arbitrary escape character. Together with `SplitEscaped` it can be very handy when you want to join strings with a separator so that it can be split later reliably:

      string[] values = { "Houston, we have a problem.", "Not at all,", "thanks to StringUtils!" };
        
      var joinedWithoutEscaping = string.Join(", ", values);
      var joinedWithEscaping = string.Join(", ", values.Select(value => value.Escape('\\', ',')));
        
      Console.WriteLine("Joined then split without escaping: " + string.Join(", ", joinedWithoutEscaping.Split(',').Select(value => $"<{value.Trim()}>")));
      Console.WriteLine("Joined then split with escaping: " + string.Join(", ", joinedWithEscaping.SplitEscaped('\\', ',').Select(value => $"<{value.Trim()}>")));

  Output:
  
      Joined then split without escaping: <Houston>, <we have a problem.>, <Not at all>, <>, <thanks to StringUtils!>
      Joined then split with escaping: <Houston, we have a problem.>, <Not at all,>, <thanks to StringUtils!>  

  For the sake of completeness it's worth mentioning that it's even possible to search in escaped string using the `IndexOfEscaped`/`LastIndexOfEscaped` overloads.

#### Date/time utilities

The `TimeSpanUtils` class has a really neat feature: the `ToTimeReference` extension methods. With these you can convert your timespans into a human-friendly format like `in 3 days 13 hours` or `3 days 13 hours 1 minute ago`. You can configure the precision and the format, and even localization is supported. For an example see [this StackOverflow post](https://stackoverflow.com/questions/51826732/is-there-a-concise-way-to-achieve-conditional-pluralization-of-timespan-format/51828526#51828526 "this StackOverflow post").

#### Tree traversal

The `TreeUtils` class provides depth-first search (pre-order, post-order) and breadth-first search (level order) traversal algorithms for generic tree data structures. The implementation is non-recursive and heap-based, so it can be safely used on huge tree graphs without the risk of stack overflow.

You can traverse the tree towards the leaves with the help of the  `Descendants` method:

```
class TreeNode
{
    public int Value { get; set; }
}

static void Main()
{
    // an ad-hoc tree data structure
    var nodes = Enumerable.Range(0, 7).Select(n => new TreeNode { Value = n }).ToArray();
    var tree = new Dictionary<TreeNode, TreeNode[]>
    {
        [nodes[0]] = new[] { nodes[1], nodes[2] },
        [nodes[1]] = new[] { nodes[3], nodes[4] },
        [nodes[4]] = new[] { nodes[5], nodes[6] },
    };

    IEnumerable<TreeNode> traversal = TreeUtils.Descendants(nodes[0], node => 
        tree.TryGetValue(node, out var children) ? children : Enumerable.Empty<TreeNode>(), TreeTraversal.PreOrder, includeSelf: true);
    
    Console.WriteLine("Pre-order traversal: " + string.Join(", ", traversal.Select(node => node.Value)));

    traversal = TreeUtils.Descendants(nodes[0], node =>
        tree.TryGetValue(node, out var children) ? children : Enumerable.Empty<TreeNode>(), TreeTraversal.PostOrder, includeSelf: true);
    
    Console.WriteLine("Post-order traversal: " + string.Join(", ", traversal.Select(node => node.Value)));

    traversal = TreeUtils.Descendants(nodes[0], node =>
        tree.TryGetValue(node, out var children) ? children : Enumerable.Empty<TreeNode>(), TreeTraversal.LevelOrder, includeSelf: true);

    Console.WriteLine("Level order traversal: " + string.Join(", ", traversal.Select(node => node.Value)));
}
```

Output:

```
Pre-order traversal: 0, 2, 1, 4, 6, 5, 3
Post-order traversal: 2, 6, 5, 4, 3, 1, 0
Level order traversal: 0, 1, 2, 3, 4, 5, 6
```

Please note that the DFS traversals (pre-order, post-order) visit children in right-to-left order for performance reasons. If you need left-to-right order, you need to change the children selector to enumerate them in reverse order.

There are further methods in the class at your service:
* `Ancestors`, which traverses the tree towards the root,
* `Root`, whichs returns the root of the tree,
* `Leaves`, which enumerates the leaves of the tree,
* `Level`, which returns a node's distance from the root,
* `EnumeratePaths`, which enumerates all possible paths in the tree.

#### Expression tree utilities

The `Lambda` class contains some useful functions for obtaining reflection information from strongly typed lambda expressions.

##### Property paths

E.g. you can get property paths with `Lambda.MemberPath`:

`Lambda.MemberPath((DateTime dt) => dt.Date.Kind)` returns `"Date.Kind"`.

##### MemberInfo from expression

You can get `FieldInfo`, `PropertyInfo`, `MethodInfo` in a typesafe way by the `Lambda.Field`, `Lambda.Property`, `Lambda.Method` functions.

For example, you want to call a generic method with type arguments known only at run-time. You will need a generic method definition for that which you can obtain simply and safely like this:

`Lambda.Method(() => Enumerable.Where(default, default(Func<object, bool>))).GetGenericMethodDefinition()`

##### Chaining expressions

The overloads of the `Lambda.Chain` method enables you to compose expression trees, which can be really useful when you need to build complex `IQueryable<T>` queries:

```
class User
{
    public string UserName { get; set; }
}

class UserRole
{
    public User User { get; set; }
    public string RoleName { get; set; }
}

// simulating a queryable db context
class DbContext
{
    public DbContext() =>
        UserRoles = new[]
        {
            new UserRole { User = new User { UserName = "Andrew" }, RoleName = "Admin" },
            new UserRole { User = new User { UserName = "Ann" }, RoleName = "User" },
            new UserRole { User = new User { UserName = "Peter" }, RoleName = "User" },
        }.AsQueryable();

    public IQueryable<UserRole> UserRoles { get; }
}

static void Main()
{
    Expression<Func<User, bool>> usersWhoseNameStartsWithA = user => user.UserName.StartsWith("A");
    Expression<Func<UserRole, User>> userFromRole = userRole => userRole.User;
    var userRolesWhereUserNameStartsWithA = userFromRole.Chain(usersWhoseNameStartsWithA);

    Console.WriteLine($"The roles of users whose name starts with 'A':");
    foreach (var userRole in new DbContext().UserRoles.Where(userRolesWhereUserNameStartsWithA))
        Console.WriteLine($"* {userRole.User.UserName}'s role is {userRole.RoleName}");
}
```

Output:

```
The roles of users whose name starts with 'A':
* Andrew's role is Admin
* Ann's role is User
```

##### Predicate builder

Another problem which occurs frequently when building dynamic `IQueryable<T>` queries is that there is no built-in way to create filter criteria from disjunct sub-criteria if they are not known at compile-time. The `PredicateBuilder` class offers a solution:

```
static void Main()
{
	var builder = PredicateBuilder<User>.False()
		.Or(user => user.UserName.StartsWith("P"))
		.Or(user => user.UserName.EndsWith("n"));

	Console.WriteLine($"The users whose name starts with 'P' or ends with 'n':");
	foreach (var user in new DbContext().UserRoles.Select(ur => ur.User).Where(builder.Build()))
		Console.WriteLine($"* {user.UserName}");
}
```

Output:

```
The users whose name starts with 'P' or ends with 'n':
* Ann
* Peter
```

However, composing LINQ queries may easily get so convoluted that you cannot get along even with these techniques. For those cases I recommend the excellent [LinqKit](https://github.com/scottksmith95/LINQKit "LinqKit library") library.

#### Reflection utilities

The `ReflectionUtils` class contains some stop-gap extension methods for `Type` like
* `AllowsNull`, which tells if an instance of the type can hold null values (is it reference type or nullable struct type),
* `IsAssignableFrom`,  which takes an object and returns if the type could hold that instance (considering null references, of course),
* `HasInterface`/`GetInterface`, which tell if the type implements the specified interface,
* `HasClosedInterface`/`GetClosedInterfaces`, which tells if the type implements the specified open interface/returns the implemented closed interfaces,
* `HasAttribute`/`GetAttributes`, which are convenience methods for querying/obtaining attributes (plus dealing with gotchas regarding inheritance),
* and some more.

##### Fast setters/getters for object properties

This class also provides you with a few helpers to build better performing applications when reflection is unavoidable: `MakeFastGetter`/`MakeFastSetter` create you type-safe, fast accessors to object properties using expression trees:

```
class MyClass
{
    public int Property { get; set; }
}

static void Main()
{
    // you usually cache these delegates
    var getter = ReflectionUtils.MakeFastGetter((MyClass o) => o.Property);
    var setter = ReflectionUtils.MakeFastSetter((MyClass o) => o.Property);

    var obj = new MyClass { Property = 1 };
    Console.WriteLine($"Original value: {getter(obj)}");
    setter(obj, 2);
    Console.WriteLine($"New value: {getter(obj)}");
}
```

Output:

```
Original value: 1
New value: 2
```

##### Object to dictionary

Sometimes you want to convert (anonymous) objects to dictionaries. For example ASP.NET MVC makes heavy use of this, but the implementation doesn't reside in the BCL and is usually buried in internal classes anyway. Using the `ReflectionUtils.ObjectToDictionary` method you can now use this feature anywhere in your code:

```
foreach (var kvp in ReflectionUtils.ObjectToDictionary(new { Id = 1, Text = "ABC" }))
    Console.WriteLine($"{kvp.Key}={kvp.Value}");
```

Output:

```
Id=1
Text=ABC
```

The function has an alternative version (`ReflectionUtils.ObjectToDictionaryCached`) for those cases when multiple instances of the same type needs to be converted. So reflection metadata will be cached instead of being collected on every call.)

#### Building/parsing assembly and type names

The `AssemblyNameBuilder` and `TypeNameBuilder` class allows you to build and parse .NET assembly and type names. These classes are especially useful when you need to rewrite type names.

Let's assume you want to strip assembly version information from generic type names:

```
var input =  typeof(Dictionary<string, int>).FullName;
var output = new TypeNameBuilder(input)
    .Transform(builder => builder.AssemblyName = builder.AssemblyName != null ? new AssemblyNameBuilder(builder.AssemblyName).Name : null)
    .ToString();

Console.WriteLine($"Input: {input}");
Console.WriteLine($"Output: {output}");
```

Output:

```
Input: System.Collections.Generic.Dictionary`2[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]
Output: System.Collections.Generic.Dictionary`2[[System.String, mscorlib],[System.Int32, mscorlib]]
```

#### `IEnumerable<T>` counterparts of several Rx operators

Currently these are: `Return`, `Repeat`, `SkipLast`, `TakeLast` and `Scan`.

`Scan` is the most interesting operator here: it is handy when you want to calculate running totals:

```
IEnumerable<int> Fibonacci()
{
    int n = 0, m = 1, sum;

    yield return n; 
    yield return m;

    for (; ; )
    {
        yield return sum = n + m;
        n = m;
        m = sum;
    }
}

var fibonacci = Fibonacci();

Console.WriteLine($"Fibonacci sequence: {string.Join(", ", fibonacci.Take(10))}");
Console.WriteLine($"Running totals of Fibonacci sequence: {string.Join(", ", fibonacci.Scan((accumulator, element) => accumulator += element).Take(10))}");
```

Output:

```
Fibonacci sequence: 0, 1, 1, 2, 3, 5, 8, 13, 21, 34
Running totals of Fibonacci sequence: 0, 1, 2, 4, 7, 12, 20, 33, 54, 88
```

#### `IList<T>` and `IReadOnlyList<T>` extension methods

Working with lists when you only have an interface reference can be annoying. It's not exactly the feeling of ease when you need to copy your list to an array or another list just to be able to perform a binary search on it. (And we've not even mentioned the read-only list interface, which is one of the few painful design flaws of .NET anyway).

This library addresses this problem by making these familiar methods of arrays and lists available for both read-only and non-readonly list interfaces (in form of extension methods): `BinarySearch`, `ConvertAll`, `Exists`, `Find`, `FindAll`, `FindIndex`, `FindLast`, `FindLastIndex`, `ForEach`, `GetRange`, `LastIndexOf` and `TrueForAll`.
