# Datafilters [![Build status](https://candoumbe.visualstudio.com/DataFilters/_apis/build/status/Filters-CI%20(master))](https://candoumbe.visualstudio.com/DataFilters/_build/latest?definitionId=16)


DataFilters is a small library that allow to convert a string to a generic `IFilter`object.
Highly inspired by the elastic syntax, it offers a powerful way to query data

1. [Introduction](#intro)
2. [Filtering](#filtering)
   i. [Starts with](#starts-with-expression)
   ii. [Ends with](#ends-with-expression)
   iii. [Contains](#contains-expression)     
   iv. [Greater than or equal](#gte-expression)
   v. [Less than or equal](#lte-expression)
   vi. [Between](#btw-expression)
3. [Sorting](#sorting)
4. [How to install](#how-to-install)


## <a href='#' id='intro'>Introduction</a>
The idea came to me when working on a set of REST apis and trying to build `/search` endpoints.
I wanted to have a uniform way to query a collection of resources whilst abstracting away underlying datasources.



Let's say your api manage `vigilante` resources :
```csharp
/* C# object */
class Vigilante
{
    public string Firstname { get; set; }
    public string Lastname { get; set; }
    public string Nickname {get; set; }
    public int Age { get; set; }
}
```

```JSon
/* Json representation */
{
    firstname : string,
    lastname : string,
    nickname : string,
    age : number
}
```

and the base url of your api is `https://my-beautiful/api`.

`vigilante` resources could then be located at `https://my-beautiful/api/vigilantes/`

Wouldn't it be nice to be able to search any resource like so
`https://my-beautiful/api/vigilantes/search?nickname=Bat*|Super*` ?

This is exactly what this project is about : giving you an uniform syntax to query resources without having to thing about the underlying datasource.


## <a href='#' id='filtering'>Filtering</a>

The main classes to deal with are :

- [IFilter][class-ifilter] is the interface that describes the shape of filters. There are two kind of filter
- [Filter][class-filter] is an implementation of a filter on a single property
- [CompositeFilter][class-complex-filter] : combines several [Filter][class-ifilter]s using a logical operator that can be `AND` or `OR`. 


The library supports a custom syntax that can be used to specified one or more criteria resources must fullfill. 
The currently supported syntax mimic the query string syntax : a key-value pair separated by _ampersand_ (`&` character) where :
- `field` is the name of a property of the resource to filter
- `value` is an expression which syntax is highly inspired by the [Lucene syntax](http://www.lucenetutorial.com/lucene-query-syntax.html)

Several expressions are supported to filter elements

**<a href='#' id='starts-with-expression'>Starts with</a>**

Search for any vigilante resource that starts with "bat" in the `nickname` property
```csharp
"nickname=bat*"
```

Search for any vigilante resource that starts with "bat" in the 

will result in a [IFilter][class-ifilter] instance equivalent to
```csharp
IFilter filter = new Filter("nickname", StartsWith, "bat");
```


**<a href='#' id='ends-with-expression'>Ends with</a>**

Search for `vigilante` resource that ends with `man` in the `nickname` property.

```csharp
"nickname=*man"
```

**<a href='#' id='contains-expression'>Contains</a>**

Search for `vigilante` resources that contains `bat` in the `nickname` property.

```csharp
"nickname=*bat*"
```

##### Regular expressions
The library offers a limited support for regular expressions


#### Ranges

Search for `vigilante`s that are 18 years old at least
```csharp
"age=[18 TO *]"
```

which will be turned into the following [IFilter][class-ifilter] instance.
```csharp
IFilter filter = new Filter("age", GreaterThanOrEqualTo, 18),
```

**<a href='#' id='gte-expression'>Greater than or equal</a>**

Search for `vigilante` resources where the value of `age` property is greater than or equal to `18`

```csharp
"age=[* TO 18]"
```
will result in a [IFilter][class-ifilter] instance equivalent to
```csharp
x => x.Age >= 20
```

**<a href='#' id='lte-expression'>Less than or equal</a>**

Search for `vigilante` resource where the value of `age` property is lower than `30`
```csharp
"age=[* TO 30]"
```
will be parsed into a [IFilter][class-filter]  equivalent to
```csharp
IFilter filter = new Filter("age", LessThanOrEqualTo, 30);
```

##### <a href='#' id='btw-expression'>Between</a>

Search for vigilante resources where `age` property is between `20` and `35`

```csharp
"age=[20 TO 35]" 
```
will result in a [IFilter][class-ifilter] instance equivalent to
```csharp
IFilter filter = new CompositeFilter
{
    Logic = Or,
    Filters = new IFilter[]
    {
        new Filter("age", GreaterThanOrEqualTo, 20),
        new Filter("age", LessThanOrEqualTo, 35)
    }
}
```

#### Logical operators

Logicial operators helps combine several instances of [IFilter][class-ifilter]

**<a href='#' id='and-expression'>And</a>**

The `,` to combine multiple expressions 
```csharp
"nickname=Bat*,*man"
```
will result in a [IFilter][class-ifilter] instance equivalent to
```csharp
x => x.Nickname.StartsWith("Bat") && x.Nickname.EndsWith("man")
```
##### <a href='#' id='or-expression'>Or</a>

Search for `vigilante` resources where the value of the `nickname` property either starts with `"Bat"` or 
ends with `"man"`
```csharp
"nickname=Bat*|*man"
```
will result in 
```csharp
Ifilter filter = new CompositeFilter 
{
    Logic = Or,
    Filters = new IFilter[]
    {
        new Filter("nickname", StartsWith, "Bat"),
        new Filter("nickname", EndsWith, "man")
    }
}
```
#### <a href='#' id='not-expression'>Not</a>
To invert a filter, simply put a `!` before the expression to negate

Search for `vigilante` resources where the value of `nickname` property does not starts with `"B"`
```URL
"nickname=!B*"
```
will be parsed into a [IFilter][class-ifilter] instance equivalent to
```csharp
IFilter filter = new Filter("nickname", DoesNotStartWith, "B");
```

Expressions can be arbitrarily complex.


`"Nickname=(Bat*|Sup*)|(*man|*er)"`

will be parsed into a 
```csharp
IFilter filter = new CompositeFilter
{
    Logic = Or,
    Filters = new IFilter[]
    {
        new CompositeFilter
        {
            Logic = Or,
            Filters = new IFilter[]
            {
                new Filter("Firstname", StartsWith, "Bat"),
                new Filter("Firstname", StartsWith, "Sup"),
            }
        },
        new CompositeFilter
        {
            Logic = Or,
            Filters = new IFilter[]
            {
                new Filter("Firstname", EndsWith, "man"),
                new Filter("Firstname", EndsWith, "er"),
            }
        },
    }
}

```
## <a href='#' id='sorting'>Sorting</a>
This library also supports a custom syntax to sort elements.

`sort=nickname` or `sort=+nickname` sort items by their `nickname` properties in ascending 
order.

You can sort by several properties at once by separating them with a `,`.

For example `sort=+nickname,-age` allows to sort by `nickname` ascending, then by `age` property descending. 

# <a href='#' id='how-to-install'>How to install</a>

1. run `install DataFilters` : you can already start to build [IFilter][class-ifilter] instances
2. You can then add `Datafilters.Expressions` package to build `Expression<Func<T, bool>>`


[class-ifilter]: /src/DataFilters/IFilter.cs
[class-filter]: /src/DataFilters/Filter.cs
[class-complex-filter]: /src/DataFilters/CompositeFilter.cs
[dir-lnk-filtering]: #filtering