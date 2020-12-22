# Datafilters [![Azure DevOps builds](https://img.shields.io/azure-devops/build/candoumbe/7684067c-f6c9-4e35-83ee-944ab5e7505b/29?style=for-the-badge)](https://dev.azure.com/candoumbe/DataFilters/_build/latest?definitionId=29&branchName=main) ![Coverage](https://img.shields.io/azure-devops/coverage/candoumbe/DataFilters/29?style=for-the-badge) ![Tests](https://img.shields.io/azure-devops/tests/candoumbe/DataFilters/29?style=for-the-badge&compact_message) [![Nuget](https://img.shields.io/nuget/v/Datafilters?label=Nuget&style=for-the-badge)](https://www.nuget.org/packages/DataFilters) 

DataFilters is a small library that allow to convert a string to a generic `IFilter`object.
Highly inspired by the elastic syntax, it offers a powerful way to build and query data
with a syntax that's not bound to a peculiar datasource.

## <a href='#' id='summary'>Summary</a>
<ol>
    <li><a href="#parsing">Parsing</a></li>
    <li><a href="#filtering">Filtering</a></li>
    <ol type="a">
        <li><a href="#equals-expression">Equals</a></li>
        <li><a href="#starts-with-expression">Starts with</a></li>
        <li><a href="#ends-with-expression">Ends with</a></li>
        <li><a href="#contains-expression">Contains</a></li>
        <li><a href="#isempty-expression">Is empty</a></li>
        <li><a href="#isnotempty-expression">Is not empty</a></li>
        <li><a href="#range-expressions">Range expressions</a></li>
        <ul type="bullet">
            <li><a href="#gte-expression">Greater than or equal</a></li>
            <li><a href="#lte-expression">Less than or equal</a></li>
            <li><a href="#btw-expression">Between</a></li>
        </ul>
    </ol>
    <li><a href="#special-character-handling">Special character handling</li>
    <li><a href="#sorting">Sorting</a></li>
    <li><a href="#how-to-use">How to use</a></li>
    <li><a href="#how-to-install">How to install</a></li>
    <ul>
        <li><a href="#how-to-use-client">on the client</a></li>
        <li><a href="#how-to-use-backend">on the backend</a></li>
    </ul>
    <li><a href="#extensions">Going a step further</a></li>
</ol>


The idea came to me when working on a set of REST apis and trying to build `/search` endpoints.
I wanted to have a uniform way to query a collection of resources whilst abstracting away underlying datasources.

Let's say your api manage `vigilante` resources :

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

and the base url of your api is `https://my-beautiful/api`.

`vigilante` resources could then be located at `https://my-beautiful/api/vigilantes/`

Wouldn't it be nice to be able to search any resource like so
`https://my-beautiful/api/vigilantes/search?nickname=Bat*|Super*` ?

This is exactly what this project is about : giving you an uniform syntax to query resources
without having to thing about the underlying datasource.

## <a href='#' id='parsing'>Parsing</a>

This is the first step on filtering data. Thanks to [SuperPower](https://github.com/datalust/superpower/),
the library supports a custom syntax that can be used to specified one or more criteria resources must fullfill.
The currently supported syntax mimic the query string syntax : a key-value pair separated by _ampersand_ (`&` character) where :

- `field` is the name of a property of the resource to filter
- `value` is an expression which syntax is highly inspired by the [Lucene syntax](http://www.lucenetutorial.com/lucene-query-syntax.html)

To parse an expression, simply call  `ToFilter<T>` extension method
(see unit tests for more details on the syntax)

## <a href='#' id='filtering'>Filtering</a>

Several expressions are supported and here's how you can start using them in your search queries.

**_<a href='#' id='equals-expression'>Equals</a>_**

Search for any vigilante resource where `nickname` value is `manbat`

| Query string      | JSON                                                  |
| ----------------- | ----------------------------------------------------- |
| `nickname=manbat` | `{ "field":"nickname", "op":"eq", "value":"manbat" }` |

will result in a [IFilter][class-ifilter] instance equivalent to

```csharp
IFilter filter = new Filter("nickname", EqualsTo, "bat");
```

**_<a href='#' id='starts-with-expression'>Starts with</a>_**

Search for any vigilante resource that starts with `"bat"` in the `nickname` property

| Query string    | JSON                                                       |
| --------------- | ---------------------------------------------------------- |
| `nickname=bat*` | `{ "field":"nickname", "op":"startswith", "value":"bat" }` |

will result in a [IFilter][class-ifilter] instance equivalent to

```csharp
IFilter filter = new Filter("nickname", StartsWith, "bat");
```

**_<a href='#' id='ends-with-expression'>Ends with</a>_**

Search for `vigilante` resource that ends with `man` in the `nickname` property.

| Query string    | JSON                                                     |
| --------------- | -------------------------------------------------------- |
| `nickname=*man` | `{ "field":"nickname", "op":"endswith", "value":"man" }` |

will result in a [IFilter][class-ifilter] instance equivalent to

```csharp
IFilter filter = new Filter("nickname", Contains, "bat");
```

**_<a href='#' id='contains-expression'>Contains</a>_**

Search for `vigilante` resources that contains `bat` in the `nickname` property.

| Query string     | JSON                                                     |
| ---------------- | -------------------------------------------------------- |
| `nickname=*bat*` | `{ "field":"nickname", "op":"contains", "value":"bat" }` |

will result in a [IFilter][class-ifilter] instance equivalent to 

```csharp
IFilter filter = new Filter("nickname", Contains, "bat");
```

### **_<a href='#' id='isempty-expression'>Is empty</a>_**

Search for `vigilante` resources that have no powers.

| Query string | JSON                                   |
| ------------ | -------------------------------------- |
| `powers=!*`  | `{ "field":"powers", "op":"isempty" }` |


### **_<a href='#' id='range-expressions'>Range expressions</a>_**

Range expressions are delimited by upper and a lower bound. The generic syntax is

`<field>=<min> TO <max>`

where

- `field` is the name of the property current range expression will be apply to
- `min` is the lowest bound of the interval
- `max` is the highest bound of the interval

#### **_<a href='#' id='gte-expression'>Greater than or equal</a>_**

Search for `vigilante` resources where the value of `age` property is greater than or equal to `18`

| Query string    | JSON                                      |
| --------------- | ----------------------------------------- |
| `age=[18 TO *[` | `{"field":"age", "op":"gte", "value":18}` |

will result in a [IFilter][class-ifilter] instance equivalent to

```csharp
IFilter filter = new Filter("age", GreaterThanOrEqualTo, 18);
```

**_<a href='#' id='lte-expression'>Less than or equal</a>_**

Search for `vigilante` resource where the value of `age` property is lower than `30`

| Query string    | JSON                                      |
| --------------- | ----------------------------------------- |
| `age=]* TO 30]` | `{"field":"age", "op":"lte", "value":30}` |

will be parsed into a [IFilter][class-filter] equivalent to

```csharp
IFilter filter = new Filter("age", LessThanOrEqualTo, 30);
```

**_<a href='#' id='btw-expression'>Between</a>_**

Search for vigilante resources where `age` property is between `20` and `35`

| Query string     | JSON                                                                                                          |
| ---------------- | ------------------------------------------------------------------------------------------------------------- |
| `age=[20 TO 35]` | `{"logic": "and", filters[{"field":"age", "op":"gte", "value":20}, {"field":"age", "op":"lte", "value":35}]}` |

will result in a [IFilter][class-ifilter] instance equivalent to

```csharp
IFilter filter = new MultiFilter
{
    Logic = And,
    Filters = new IFilter[]
    {
        new Filter("age", GreaterThanOrEqualTo, 20),
        new Filter("age", LessThanOrEqualTo, 35)
    }
}
```

---
💡 You can exclude the lower (resp. upper) bound by using `]` (resp. `[`). 
- `age=]20 TO 35[` means `age` strictly greater than `20` and strictly less than`35`
- `age=[20 TO 35[` means `age` greater than or equal to `20` and strictly less than`35`
- `age=]20 TO 35]` means `age` greater than `20` and less than or equal to `35`
  
💡 Dates and times must be specified in [ISO 8601 format](https://en.wikipedia.org/wiki/ISO_8601)

Examples :
- `]1998-10-26 TO 2000-12-10[`
- `my/beautiful/api/search?date=]1998-10-26 10:00 TO 1998-10-26 10:00[`

💡 You can apply filters to any sub-property or a given collection

Example : 
`acolytes["name"]='robin'` will filter any `vigilante` resource where at least one item in `acolytes` array with `name` equals to robin.

The generic syntax for filtering on in a hierarchical tree
`property["subproperty"]...["subproperty-n"]=<expression>`



---


## <a href="logic-operators">Logical operators</a>

Logicial operators can be used combine several instances of [IFilter][class-ifilter] together.

**_<a href='#' id='and-expression'>And</a>_**

Use the coma character `,` to combine multiple expressions using logical AND operator 
| Query string          | JSON                                                                                                                                     |
| --------------------- | ---------------------------------------------------------------------------------------------------------------------------------------- |
| `"nickname=Bat*,*man` | `{"logic": "and", filters[{"field":"nickname", "op":"startswith", "value":"Bat"}, {"field":"nckname", "op":"endswith", "value":"man"}]}` |


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
**_<a href='#' id='or-expression'>Or</a>_**

Use the pipe character `|`  to combine multiple expressions using logical AND operator 
Search for `vigilante` resources where the value of the `nickname` property either starts with `"Bat"` or
ends with `"man"`
| Query string          | JSON                                                                                                                                    |
| --------------------- | --------------------------------------------------------------------------------------------------------------------------------------- |
| `"nickname=Bat*|*man` | `{"logic": "or", filters[{"field":"nickname", "op":"startswith", "value":"Bat"}, {"field":"nckname", "op":"endswith", "value":"man"}]}` |

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

**_<a href='#' id='not-expression'>Not</a>_**

To negate a filter, simply put a `!` before the expression to negate

Search for `vigilante` resources where the value of `nickname` property does not starts with `"B"`

| Query string   | JSON                                                    |
| -------------- | ------------------------------------------------------- |
| `"nickname=!B` | `{"field":"nickname", "op":"nstartswith", "value":"B"}` |

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

The `(` and `)` allows to group two expressions together so that this group can be used as a more complex
expression unit.

## <a href='#' id='special-character-handling'>Special character handling</a>

Sometimes, you'll be looking for a filter that match exactly a text that contains a character which has a special meaning.

The backslash character (`\`) can be used to escape characters that will be otherwise interpreted as
a special character.


| Query string   | JSON                                                |
| -------------- | --------------------------------------------------- |
| `"comment=*\!` | `{"field":"comment", "op":"endswith", "value":"!"}` |

will be parsed into a [IFilter][class-ifilter] instance equivalent to

```csharp
IFilter filter = new Filter("comment", EndsWith, "!");
```

## <a href='#' id='sorting'>Sorting</a>

This library also supports a custom syntax to sort elements.

`sort=nickname` or `sort=+nickname` sort items by their `nickname` properties in ascending
order.

You can sort by several properties at once by separating them with a `,`.

For example `sort=+nickname,-age` allows to sort by `nickname` ascending, then by `age` property descending.

# <a href='#' id='how-to-use'>How to use</a>

So you have your API and want provide a great search experience ?

## <a href='#' id='how-to-use-client'>On the client</a>

The client will have the responsability of building search criteria.
Go to [filtering](#filtering) and [sorting](#sorting) sections to see example on how to get started.

## <a href='#' id='how-to-use-backend'>On the backend</a>

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

        // filter now contains how search criteria and is ready to be used 😊

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
   - or combine them using [composite filter][class-multi-filter] if there were more than one criterion.

You may have noticed that `SearchVigilanteQuery.Age` property is nullable whereas `Vigilante.Age` property is not.
This is to distinguish if the `Age` criterion was provided or not when calling the `vigilantes/search` endpoint.

# <a href='#' id='how-to-install'>How to install</a>

1. run `dotnet install DataFilters` : you can already start to build [IFilter][class-ifilter] instances 😉 !
2. install one or more `DataFilters.XXXX  extension packages to convert [IFilter][class-ifilter] instances to various target.

|                           | Package                                                                                                                                         | Description                                                                                                                                                                         |
| ------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `DataFilters`             | [![Nuget](https://img.shields.io/nuget/v/Datafilters?style=for-the-badge)](https://www.nuget.org/packages/DataFilters)                          | provides core functionalities of parsing strings and converting to [IFilter][class-ifilter] instances.                                                                              |
| `DataFilters.Expressions` | [![Nuget](https://img.shields.io/nuget/v/DataFilters.Expressions?&style=for-the-badge)](https://www.nuget.org/packages/DataFilters.Expressions) | adds `ToExpression<T>()` extension method on top of [IFilter][class-ifilter] instance to convert it to an equivalent `System.Linq.Expressions.Expression<Func<T, bool>>` instance.  |
| `DataFilters Queries`     | [![Nuget](https://img.shields.io/nuget/v/Datafilters.Queries?style=for-the-badge)](https://www.nuget.org/packages/DataFilters.Queries)          | adds `ToWhere<T>()` extension method on top of [IFilter][class-ifilter] instance to convert it to an equivalent [`IWhereClause`](https://dev.azure.com/candoumbe/Queries) instance. |


[class-multi-filter]: /src/DataFilters/MultiFilter.cs
[class-ifilter]: /src/DataFilters/IFilter.cs
[class-filter]: /src/DataFilters/Filter.cs
[datafilters-expressions]: https://www.nuget.org/packages/DataFilters.Expressions
[datafilters-queries]: https://www.nuget.org/packages/DataFilters.Queries