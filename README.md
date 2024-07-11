# Datafilters <!-- omit in toc -->

![GitHub Main branch Status](https://img.shields.io/github/actions/workflow/status/candoumbe/datafilters/delivery.yml?branch=main&label=main)
![GitHub Develop branch Status](https://img.shields.io/github/actions/workflow/status/candoumbe/datafilters/integration.yml?branch=develop&label=develop)
[![codecov](https://codecov.io/gh/candoumbe/DataFilters/branch/develop/graph/badge.svg?token=FHSC41A4X3)](https://codecov.io/gh/candoumbe/DataFilters)
[![GitHub raw issues](https://img.shields.io/github/issues-raw/candoumbe/datafilters)](https://github.com/candoumbe/datafilters/issues)
[![DataFilters](https://img.shields.io/nuget/vpre/datafilters?label=Datafilters)](https://nuget.org/packages/datafilters)

A small library that allow to convert a string to a generic [`IFilter`][class-ifilter] object.
Highly inspired by the elastic query syntax, it offers a powerful way to build and query data with a syntax that's not bound to a peculiar datasource.

## **Disclaimer** <!-- omit in toc -->

This project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

Major version zero (0.y.z) is for initial development. **Anything MAY change at any time**.

The public API SHOULD NOT be considered stable.

- [Parsing](#parsing)
- [Filters syntax](#filters-syntax)
- [Equals](#equals)
- [Starts with](#starts-with)
- [Ends with](#ends-with)
- [Contains](#contains)
- [Is null](#is-null)
- [Any of](#any-of)
- [Is not null](#is-not-null)
- [Interval expressions](#interval-expressions)
  - [Greater than or equal](#greater-than-or-equal)
  - [Less than or equal](#less-than-or-equal)
  - [Between](#between)
- [Regular expression](#regular-expression)
- [Logical operators](#logical-operators)
  - [And](#and)
  - [Or](#or)
  - [Not](#not)
- [Special character handling](#special-character-handling)
- [Sorting](#sorting)
- [How to install](#how-to-install)
- [How to use](#how-to-use)
  - [On the client](#on-the-client)
  - [On the backend](#on-the-backend)
    - [Building expression trees to filtering data from any datasource](#building-expression-trees-to-filtering-data-from-any-datasource)
    - [Extending `IFIlter`s](#extending-ifilters)


The idea came to me when working on a set of REST APIs and trying to build `/search` endpoints.
I wanted to have a uniform way to query a collection of resources whilst abstracting away underlying datasources.

Let's say your API handles `vigilante` resources :

```csharp
public class Vigilante
{
    public string Firstname { get; set; }
    public string Lastname { get; set; }
    public string Nickname {get; set; }
    public int Age { get; set; }
    public string Description {get; set;}
    public IEnumerable<string> Powers {get; set;}
    public IEnumerable<Vigilante> Acolytes {get; set;} 
}
```

JSON Schema

```json
{
  "id": "vigilante_root",
  "title": "Vigilante",
  "type": "object",
  "properties": {
    "firstname": {
      "required": true,
      "type": "string"
    },
    "lastname": {
      "required": true,
      "type": "string"
    },
    "nickname": {
      "required": true,
      "type": "string"
    },
    "age": {
      "required": true,
      "type": "integer"
    },
    "description": {
      "required": true,
      "type": "string"
    },
    "powers": {
      "required": true,
      "type": "array",
      "items": {
        "type": "string"
      }
    },
    "acolytes": {
      "required": true,
      "type": "array",
      "items": {
        "$ref": "vigilante_root"
      }
    }
  }
}
```

and the base URL of your API is `https://my-beautiful/api`.

`vigilante` resources could then be located at `https://my-beautiful/api/vigilantes/`

Wouldn't it be nice to be able to search any resource like so
`https://my-beautiful/api/vigilantes/search?nickname=Bat*|Super*` ?

This is exactly what this project is about : giving you an uniform syntax to query resources
without having to think about the underlying datasource.

## Parsing

This is the first step on filtering data. Thanks to [SuperPower](https://github.com/datalust/superpower/),
the library supports a custom syntax that can be used to specified one or more criteria resources must fullfill.
The currently supported syntax mimic the query string syntax : a key-value pair separated by _ampersand_ (`&` character) where :

- `field` is the name of a property of the resource to filter
- `value` is an expression which syntax is highly inspired by the [Lucene syntax](http://www.lucenetutorial.com/lucene-query-syntax.html)

To parse an expression, simply call  `ToFilter<T>` extension method
(see unit tests for more details on the syntax)

## Filters syntax

Several expressions are supported and here's how you can start using them in your search queries.

|                                                | `string` | numeric types (`int`, `short`, ...) | Date and time types (`DateTime`, `DateTimeOffset`, ...) |
| ---------------------------------------------- | -------- | ----------------------------------- | ------------------------------------------------------- |
| [EqualTo](#equals)                             | ✅        | ✅                                   | ✅                                                       |
| [StartsWith](#starts-with)                     | ✅        | N/A                                 | N/A                                                     |
| [Ends with](#ends-with)                        | ✅        | N/A                                 | N/A                                                     |
| [Contains](#contains)                          | ✅        | N/A                                 | N/A                                                     |
| [IsNull](#is-null)                             | ✅        | N/A                                 | N/A                                                     |
| [IsNotNull](#is-not-null)                      | ✅        | N/A                                 | N/A                                                     |
| [LessThanOrEqualTo](#less-than-or-equal)       | N/A      | ✅                                   | ✅                                                       |
| [GreaterThanOrEqualTo](#greater-than-or-equal) | N/A      | ✅                                   | ✅                                                       |
| [bracket expression](#regular-expression)      | N/A      | ✅                                   | ✅                                                       |

## Equals

Search for any `vigilante` resources where the value of the property `nickname` is `manbat`

| Query string      | JSON                                                  | C#                                                                                     |
| ----------------- | ----------------------------------------------------- | -------------------------------------------------------------------------------------- |
| `nickname=manbat` | `{ "field":"nickname", "op":"eq", "value":"manbat" }` | `new Filter(field: "nickname", @operator : FilterOperator.EqualsTo, value : "manbat")` |

## Starts with

Search for any `vigilante` resources where the value of the property `nickname` starts with `"bat"`

| Query string    | JSON                                                       | C#                                                                                    |
| --------------- | ---------------------------------------------------------- | ------------------------------------------------------------------------------------- |
| `nickname=bat*` | `{ "field":"nickname", "op":"startswith", "value":"bat" }` | `new Filter(field: "nickname", @operator : FilterOperator.StartsWith, value : "bat")` |

## Ends with

Search for `vigilante` resources where the value of the property `nickname` ends with `man`.

| Query string    | JSON                                                     | C#                                                                                  |
| --------------- | -------------------------------------------------------- | ----------------------------------------------------------------------------------- |
| `nickname=*man` | `{ "field":"nickname", "op":"endswith", "value":"man" }` | `new Filter(field: "nickname", @operator : FilterOperator.EndsWith, value : "man")` |

## Contains

Search for any `vigilante` resources where the value of the property `nickname` contains `"bat"`.

| Query string     | JSON                                                     | C#                                                                                  |
| ---------------- | -------------------------------------------------------- | ----------------------------------------------------------------------------------- |
| `nickname=*bat*` | `{ "field":"nickname", "op":"contains", "value":"bat" }` | `new Filter(field: "nickname", @operator : FilterOperator.Contains, value : "bat")` |

💡 `contains` also work on arrays. `powers=*strength*` will search for `vigilante`s who have `strength` related powers.

Search for `vigilante` resources that have no powers.

| Query string | JSON                                   | C#                                                                |
| ------------ | -------------------------------------- | ----------------------------------------------------------------- |
| `powers=!*`  | `{ "field":"powers", "op":"isempty" }` | `new Filter(field: "powers", @operator : FilterOperator.IsEmpty)` |

## Is null

Search for `vigilante` resources that have no powers.

| Query string | JSON                                  | C#                                                                                                                                                  |
| ------------ | ------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------- |
| `N/A`        | `{ "field":"powers", "op":"isnull" }` | `new Filter(field: "powers", @operator : FilterOperator.IsNull)` or `new Filter(field: "powers", @operator : FilterOperator.EqualsTo, value: null)` |

## Any of

Search for `vigilante` resources that have at least one of the specified powers.

| Query string                     | JSON |
| -------------------------------- | ---- |
| `powers={strength\|speed\|size}` | N/A  |

will result in a [IFilter][class-ifilter] instance equivalent to

```csharp
IFilter filter = new MultiFilter
{
     Logic = Or,
     Filters = new IFilter[]
     {
         new Filter("powers", EqualTo, "strength"),
         new Filter("powers", EqualTo, "speed"),
         new Filter("powers", EqualTo, "size")
     }
};
```

## Is not null

Search for `vigilante` resources that have no powers.

| Query string | JSON                                     | C#                                                                                                                                                               |
| ------------ | ---------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `N/A`        | `{ "field":"powers", "op":"isnotnull" }` | `(new Filter(field: "powers", @operator : FilterOperator.IsNull)).Negate()` or `new Filter(field: "powers", @operator : FilterOperator.NotEqualTo, value: null)` |


## Interval expressions

Interval expressions are delimited by upper and a lower bound. The generic syntax is

`<field>=<min> TO <max>`

where

- `field` is the name of the property current interval expression will be apply to
- `min` is the lowest bound of the interval
- `max` is the highest bound of the interval

### Greater than or equal

Search for `vigilante` resources where the value of `age` property is greater than or equal to `18`

| Query string    | JSON                                      | C#                                                                                      |
| --------------- | ----------------------------------------- | --------------------------------------------------------------------------------------- |
| `age=[18 TO *[` | `{"field":"age", "op":"gte", "value":18}` | `new Filter(field: "age", @operator : FilterOperator.GreaterThanOrEqualTo, value : 18)` |

### Less than or equal

Search for `vigilante` resource where the value of `age` property is lower than `30`

| Query string    | JSON                                      | C#                                                                                   |
| --------------- | ----------------------------------------- | ------------------------------------------------------------------------------------ |
| `age=]* TO 30]` | `{"field":"age", "op":"lte", "value":30}` | `new Filter(field: "age", @operator : FilterOperator.LessThanOrEqualTo, value : 30)` |

### Between

Search for vigilante resources where `age` property is between `20` and `35`

| Query string     | JSON                                                                                                          | C#                                                                                                                                                    |
| ---------------- | ------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------- |
| `age=[20 TO 35]` | `{"logic": "and", filters[{"field":"age", "op":"gte", "value":20}, {"field":"age", "op":"lte", "value":35}]}` | `new MultiFilter { Logic = And, Filters = new IFilter[] { new Filter ("age", GreaterThanOrEqualTo, 20), new Filter("age", LessThanOrEqualTo, 35) } }` |

---
💡 You can exclude the lower (resp. upper) bound by using `]` (resp. `[`).

- `age=]20 TO 35[` means `age` strictly greater than `20` and strictly less than`35`
- `age=[20 TO 35[` means `age` greater than or equal to `20` and strictly less than`35`
- `age=]20 TO 35]` means `age` greater than `20` and less than or equal to `35`
  
💡 Dates, times and durations must be specified in [ISO 8601 format](https://en.wikipedia.org/wiki/ISO_8601)

Examples :

- `]1998-10-26 TO 2000-12-10[`
- `my/beautiful/api/search?date=]1998-10-26 10:00 TO 1998-10-26 10:00[`
- `]1998-10-12T12:20:00 TO 13:30[` is equivalent to `]1998-10-12T12:20:00 TO 1998-10-12T13:30:00[`

💡 You can apply filters to any sub-property of a given collection

Example :
`acolytes["name"]='robin'` will filter any `vigilante` resource where at least one item in `acolytes` array with `name` equals to `robin`.

The generic syntax for filtering on in a hierarchical tree
`property["subproperty"]...["subproperty-n"]=<expression>`

you can also use the dot character (`.`).
`property["subproperty"]["subproperty-n"]=<expression>` and `property.subproperty["subproperty-n"]=<expression>`
are equivalent

## Regular expression

The library offers a limited support of regular expressions. To be more specific, only bracket expressions are currently supported.
A bracket expression. Matches a single character that is contained within the brackets.
For example, `[abc]` matches `a`, `b`, or `c`. `[a-z]` specifies a range which matches any lowercase letter from `a` to `z`.

`BracketExpression`s can be, as any other expressions  combined with any other expressions to build more complex expressions.

## Logical operators

Logicial operators can be used combine several instances of [IFilter][class-ifilter] together.

### And

Use the coma character `,` to combine multiple expressions using logical AND operator

| Query string         | JSON                                                                                                                                     |
| -------------------- | ---------------------------------------------------------------------------------------------------------------------------------------- |
| `nickname=Bat*,*man` | `{"logic": "and", filters[{"field":"nickname", "op":"startswith", "value":"Bat"}, {"field":"nckname", "op":"endswith", "value":"man"}]}` |

will result in a [IFilter][class-ifilter] instance equivalent to

```csharp
IFilter filter = new MultiFilter
{
    Logic = And,
    Filters = new IFilter[]
    {
        new Filter("nickname", StartsWith, "Bat"),
        new Filter("nickname", EndsWith, "man")
    }
}
```

### Or

Use the pipe character `|`  to combine several expressions using logical OR operator
Search for `vigilante` resources where the value of the `nickname` property either starts with `"Bat"` or
ends with `"man"`

| Query string          | JSON                                                                                                                                    |
| --------------------- | --------------------------------------------------------------------------------------------------------------------------------------- |
| `nickname=Bat*\|*man` | `{"logic": "or", filters[{"field":"nickname", "op":"startswith", "value":"Bat"}, {"field":"nckname", "op":"endswith", "value":"man"}]}` |

will result in

```csharp
IFilter filter = new MultiFilter
{
    Logic = Or,
    Filters = new IFilter[]
    {
        new Filter("nickname", StartsWith, "Bat"),
        new Filter("nickname", EndsWith, "man")
    }
}
```

### Not

To negate a filter, simply put a `!` before the expression to negate

Search for `vigilante` resources where the value of `nickname` property does not starts with `"B"`

| Query string   | JSON                                                    |
| -------------- | ------------------------------------------------------- |
| `nickname=!B*` | `{"field":"nickname", "op":"nstartswith", "value":"B"}` |

will be parsed into a [IFilter][class-ifilter] instance equivalent to

```csharp
IFilter filter = new Filter("nickname", DoesNotStartWith, "B");
```

Expressions can be arbitrarily complex.

```csharp
"nickname=(Bat*|Sup*)|(*man|*er)"
```

Explanation :

The criteria under construction will be applied to the value of `nickname` property and can be read as follow :

Searchs for `vigilante` resources that starts with `Bat` or `Sup` and ends with `man` or
`er`.

will be parsed into a

```csharp
IFilter filter = new MultiFilter
{
    Logic = Or,
    Filters = new IFilter[]
    {
        new MultiFilter
        {
            Logic = Or,
            Filters = new IFilter[]
            {
                new Filter("Firstname", StartsWith, "Bat"),
                new Filter("Firstname", StartsWith, "Sup"),
            }
        },
        new MultiFilter
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

The `(` and `)` characters allows to group two expressions together so that this group can be used as a more complex
expression unit.

## Special character handling

Sometimes, you'll be looking for a filter that match exactly a text that contains a character which has a special meaning.

The backslash character (`\`) can be used to escape characters that will be otherwise interpreted as
a special character.

| Query string  | JSON                                                | C#                                                                              |
| ------------- | --------------------------------------------------- | ------------------------------------------------------------------------------- |
| `comment=*\!` | `{"field":"comment", "op":"endswith", "value":"!"}` | `new Filter(field: "comments", @operator: FilterOperator.EndsWith, value: "!")` |

💡 For longer texts, just wrap it between quotes and you're good to go

| Query string   | JSON                                                | C#                                                                              |
| -------------- | --------------------------------------------------- | ------------------------------------------------------------------------------- |
| `comment=*"!"` | `{"field":"comment", "op":"endswith", "value":"!"}` | `new Filter(field: "comments", @operator: FilterOperator.EndsWith, value: "!")` |

## Sorting

This library also supports a custom syntax to sort elements.

`sort=nickname` or `sort=+nickname` sort items by their `nickname` properties in ascending
order.

You can sort by several properties at once by separating them with a `,`.

For example `sort=+nickname,-age` allows to sort by `nickname` ascending, then by `age` property descending.

## How to install

1. run `dotnet install DataFilters` : you can already start building [IFilter][class-ifilter] instances 😉 !
2. install one or more `DataFilters.XXXX`  extension packages to convert [IFilter][class-ifilter] instances to various target.

## How to use

So you have your API and want provide a great search experience ?

### On the client

The client will have the responsability of building search criteria.
Go to [filtering](#filters-syntax) and [sorting](#sorting) sections to see example on how to get started.

### On the backend

One way to start could be by having a dedicated resource which properties match the resource's properties search will
be performed onto.

Continuing with our `vigilante` API, we could have

```csharp
// Wraps the search criteria for Vigilante resources.
public class SearchVigilanteQuery
{
    public string Firstname {get; set;}

    public string Lastname {get; set;}

    public string Nickname {get; set;}

    public int? Age {get; set;}
}
```

and the following endpoint

```csharp
using DataFilters;

public class VigilantesController
{
    // code omitted for brievity

    [HttpGet("search")]
    [HttpHead("search")]
    public ActionResult Search([FromQuery] SearchVigilanteQuery query)
    {
        IList<IFilter> filters = new List<IFilter>();

        if(!string.IsNullOrWhitespace(query.Firstname))
        {
            filters.Add($"{nameof(Vigilante.Firstname)}={query.Firstname}".ToFilter<Vigilante>());
        }

        if(!string.IsNullOrWhitespace(query.Lastname))
        {
            filters.Add($"{nameof(Vigilante.Lastname)}={query.Lastname}".ToFilter<Vigilante>());
        }

        if(!string.IsNullOrWhitespace(query.Nickname))
        {
            filters.Add($"{nameof(Vigilante.Nickname)}={query.Nickname}".ToFilter<Vigilante>());
        }

        if(query.Age.HasValue)
        {
            filters.Add($"{nameof(Vigilante.Age)}={query.Age.Value}".ToFilter<Vigilante>());
        }


        IFilter  filter = filters.Count() == 1
            ? filters.Single()
            : new MultiFilter{ Logic = And, Filters = filters };

        // filter now contains our search criteria and is ready to be used 😊

    }
}

```

Some explanation on the controller's code above  :

1. The endpoint is bound to incoming HTTP `GET` and `HEAD` requests on `/vigilante/search`
2. The framework will parse incoming querystring and feeds the `query` parameter accordingly.
3. From this point we test each criterion to see if it's acceptable to turn it into a [IFilter][class-ifilter] instance.
   For that purpose, the handy `.ToFilter<T>()` string extension method is available. It turns a query-string key-value pair into a
   full [IFilter][class-ifilter].
4. we can then either :
   - use the filter directly is there was only one filter
   - or combine them using [composite filter][class-multi-filter] when there is more than one criterion.

💡 _Remarks_

You may have noticed that `SearchVigilanteQuery.Age` property is nullable whereas `Vigilante.Age` property is not.
This is to distinguish if the `Age` criterion was provided or not when calling the `vigilantes/search` endpoint.

#### Building expression trees to filtering data from any datasource

Most of the time, once you have an [IFilter][class-ifilter], you want to use it against a datasource.
Using `Expression<Func<T, bool>>` is the most common type used for this kind of purpose.
[DataFilters.Expressions][Datafilters.expressions] library adds `ToExpression<T>()` extension method on top of [IFilter][class-ifilter] instance to convert it
to an equivalent `System.Expression<Func<T, bool>>` instance.
Using the example of the `VigilantesController`, we can turn our `filter` into a `Expression<Func<T, bool>>`

```csharp
IFilter filter = ...
Expression<Func<Vigilante, bool>> predicate = filter.ToExpression<Vigilante>();
```

The `predicate` expression can now be used against any datasource that accepts `Expression<Func<Vigilante, bool>>` (👋🏾 EntityFramework and the likes )

#### Extending `IFIlter`s

What to do when you cannot use expression trees when querying your datasource ? Well, you can write your own method to render it duh !!!

DataFilters.Queries[![Nuget](https://img.shields.io/nuget/v/DataFilters.Queries?color=blue)](https://www.nuget.org/packages/DataFilters.Queries) adds `ToWhere<T>()` extension 
method on top of [IFilter][class-ifilter] instance to convert
it to an equivalent [`IWhereClause`](https://github.com/candoumbe/Queries/blob/develop/src/Queries.Core/Parts/Clauses/IWhereClause.cs) instance.
[`IWhereClause`](https://github.com/candoumbe/Queries/blob/develop/src/Queries.Core/Parts/Clauses/IWhereClause.cs) is an interface from the [Queries](https://github.com/candoumbe/Queries) that 
can later be translated a secure SQL string.
You can find more info on that directly in the Github repository.

| Package                                                                                                                                                             | Downloads                                                                                                            | Description                                                                                                                                                                                                                                  |
| ------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| [![Nuget](https://img.shields.io/nuget/v/Datafilters?label=Datafilters&color=blue)](https://www.nuget.org/packages/DataFilters)                                     | ![DataFilters download count](https://img.shields.io/nuget/dt/Datafilters?label=&color=blue)                         | provides core functionalities of parsing strings and converting to [IFilter][class-ifilter] instances.                                                                                                                                       |
| [![Nuget](https://img.shields.io/nuget/v/DataFilters.Expressions?label=Datafilters.Expressions&color=blue)](https://www.nuget.org/packages/DataFilters.Expressions) | ![DataFilters.Expressions download count](https://img.shields.io/nuget/dt/Datafilters.Expressions?label=&color=blue) | adds `ToExpression<T>()` extension method on top of [IFilter][class-ifilter] instance to convert it to an equivalent `System.Linq.Expressions.Expression<Func<T, bool>>` instance.                                                           |
| [![Nuget](https://img.shields.io/nuget/v/Datafilters.Queries?label=DataFilters.Queries&color=blue)](https://www.nuget.org/packages/DataFilters.Queries)             | ![DataFilters.Queries download count](https://img.shields.io/nuget/dt/Datafilters.Queries?label=&color=blue)         | adds `ToWhere<T>()` extension method on top of [IFilter][class-ifilter] instance to convert it to an equivalent [`IWhereClause`](https://github.com/candoumbe/Queries/blob/develop/src/Queries.Core/Parts/Clauses/IWhereClause.cs) instance. |

[class-multi-filter]: /src/DataFilters/MultiFilter.cs
[class-ifilter]: /src/DataFilters/IFilter.cs
[DataFilters.Expressions]: https://www.nuget.org/packages/DataFilters.Expressions
[DataFilters.Queries]: https://www.nuget.org/packages/DataFilters.Queries
