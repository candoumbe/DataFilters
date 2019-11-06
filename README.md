# Datafilters [![Build status](https://candoumbe.visualstudio.com/DataFilters/_apis/build/status/Filters-CI%20(master))](https://candoumbe.visualstudio.com/DataFilters/_build/latest?definitionId=16)


DataFilters is a small library that allow to convert a string to a generic `IFilter`object.
Highly inspired by the elastic syntax, it offers a powerful way to query data

## Introduction
The idea came to me when working on a set of REST apis and trying to build `/search` endpoints.
I wanted to have a uniform way to query a collection of resources whilst abstracting away underlying datasources.



Let's say your api manage `vigilante` resources :
```csharp
/* C# object */
[JsonObject]
class Vigilante
{
    public string Firstname { get; set; }
    public string Lastname { get; set; }
    public string Nickname {get; set; }
    public int Age { get; set; }
}
```

```javascript
/* JSon */
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


## Filtering

The main classes to deal with are :

- [IFilter](/src/DataFilters/IFilter.cs) is the interface that describes the shape of filters. There are two kind of filter
- [Filter](/src/DataFilters/Filter.cs) is an implementation of a filter on a single property
- [CompositeFilter](/src/DataFilters/CompositeFilter.cs) : combines several [Filter](/src/DataFilters/Filter.cs)s using a logical operator that can be `AND` or `OR`. 


The library supports a custom syntax that can be used to specified one or more criteria resources must fullfill. 
The currently supported syntax mimic the query string syntax : a key-value pair separated by _ampersand_ (`&` character) where :
- `field` is the name of a property of the resource to filter
- `value` is an expression which syntax is highly inspired by the [Lucene syntax](http://www.lucenetutorial.com/lucene-query-syntax.html)

### Expressions

#### Starts with
Search for any vigilante resource that starts with "bat" in the `nickname` property
```
"nickname=bat*"
```

Search for any vigilante resource that starts with "bat" in the 

will result in a [IFilter](/src/DataFilters/IFilter.cs) instance equivalent to
```c#
x => x.Nickname.StartsWith("S")
```

#### Ends with

Search for `vigilante` resource that ends with `man` in the `nickname` property.

```URL
"nickname=*man"
```

#### Contains

Search for `vigilante` resources that contains `bat` in the `nickname` property.

```
nickname=*bat*
```

##### Regular expressions
The library offers a limited support for regular expressions


#### Ranges

##### Greater than or equal

```c#
Expression<Func<Vigilante, bool>> filter = "age=20-".ToFilter<Client>();
```
will result in a [IFilter](./src/DataFilters/IFilter.cs) instance equivalent to
```c#
x => x.Age >= 20
```

##### Less than or equal
```c#
Expression<Func<Vigilante, bool>> filter = "age=-35".ToFilter<Client>();
```
will result in a [IFilter](./src/DataFilters/IFilter.cs) instance equivalent to
```c#
x => x.Age <= 35
```

##### Between

Search for vigilante resources where `age` property is between `20` and `35`

```URL
"age=20-35" 
```
will result in a [IFilter](./src/DataFilters/IFilter.cs) instance equivalent to
```
x => x.Age >= 20 && x.Age <= 35
```

#### Logical operators

Logicial operators helps combine several instances of [IFilter](#1)

##### And
```c#
Expression<Func<Vigilante, bool>> filter = "nickname=Bat*,*man".ToFilter<Client>();
```
will result in a [IFilter](./src/DataFilters/IFilter.cs) instance equivalent to
```c#
x => x.Nickname.StartsWith("Bat") && x.Nickname.EndsWith("man")
```
##### Or
```c#
Expression<Func<Vigilante, bool>> filter = "nickname=Bat*|*man".ToFilter<Client>();
```
will result in 
```c#
x => x.Nickname.StartsWith("Bat") || x.Nickname.EndsWith("man")
```
#### Not
To negate a filter, simply put a `!` before the filter to negate
```URL
"nickname=!B*"
```
will 

## Ordering
This library also supports a custom syntax to sort elements.

`sort=nickname` or `sort=+nickname` sort items by their `nickname` properties in ascending 
order.

You can sort by several properties at once by separating them with a `,`.

For example `sort=+nickname,-age` allows to sort by `nickname` ascending, then by `age` property descending. 

# How to install

1. run `install DataFilters` : you can already start to build [IFilter](./src/DataFilters/IFilter.cs) instances
2. You can then add `Datafilters.Expressions` package to build `Expression<Func<T, bool>>`


