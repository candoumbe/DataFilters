# Datafilters [![Build Status](https://dev.azure.com/candoumbe/DataFilters/_apis/build/status/DataFilters?branchName=master)](https://dev.azure.com/candoumbe/DataFilters/_build/latest?definitionId=20&branchName=master)


DataFilters is a small library that allow to convert a string to a generic `IFilter`object.
Highly inspired by the elastic syntax, it offers a powerful way to build and query data 
with a syntax that's not bound to a peculiar datasource.

<ol type='1'>
    <li><a href='#intro'>Introduction</a></li>
    <li><a href='#parsing'>Parsing</a></li>
    <li><a href='#filtering'>Filtering</a></li>
    <ol type="i">
        <li><a href='#starts-with-expression'>Starts with</a></li>
        <li><a href='#ends-with-expression'>Ends with</a></li>
        <li><a href='#contains-expression'>Contains</a></li>
        <ul type='bullet'><a href='#range-expressions'></>
            <li><a href='#gte-expression'>Greater than or equal</a></li>
            <li><a href='#lte-expression'>Less than or equal</a></li>
            <li><a href='#btw-expression'>Between</a></li>
        </ul>
    </ol>
    <li><a href="#special-characters">Special character handling</li>
    <li><a href='#sorting'>Sorting</a></li>
    <li><a href='#how-to-install'>How to install</a></li>
    <li><a href='#how-to-use'>How to use</a></li>
</ol>


## <a href='#' id='intro'>Introduction</a>
The idea came to me when working on a set of REST apis and trying to build `/search` endpoints.
I wanted to have a uniform way to query a collection of resources whilst abstracting away underlying datasources.


Let's say your api manage `vigilante` resources :
```csharp
class Vigilante
{
    public string Firstname { get; set; }
    public string Lastname { get; set; }
    public string Nickname {get; set; }
    public int Age { get; set; }
    public string Description {get; set;}
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

To parse an expression, simply call the string extension `ToFilter<T>` extension method
 (see unit tests for more details on the syntax)

## <a href='#' id='filtering'>Filtering</a>

Several expressions are supported and here's how you can start using them in your search queries.

**<a href='#' id='starts-with-expression'>Starts with</a>**

Search for any vigilante resource that starts with `"bat"` in the `nickname` property
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



**<a href='#' id='range-expressions'>Range expressions</a>**

Range expressions are delimited by upper and a lower bound. The generic syntax is 

`<field>=<min> TO <max>`

where
- `field` is the name of the property current range expression will be apply to
- `min` is the lowest bound of the interval
- `max` is the highest bound of the interval

***<a href='#' id='gte-expression'>Greater than or equal</a>***

Search for `vigilante` resources where the value of `age` property is greater than or equal to `18`

```csharp
"age=[18 TO *["
```
will result in a [IFilter][class-ifilter] instance equivalent to
```csharp
IFilter filter = new Filter("age", GreaterThanOrEqualTo, 18);
```

***<a href='#' id='lte-expression'>Less than or equal</a>***

Search for `vigilante` resource where the value of `age` property is lower than `30`
```csharp
"age=]* TO 30]"
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
IFilter filter = new MultiFilter
{
    Logic = Or,
    Filters = new IFilter[]
    {
        new Filter("age", GreaterThanOrEqualTo, 20),
        new Filter("age", LessThanOrEqualTo, 35)
    }
}
```


<strong><u>Remarks</u></strong>

You can exclude the lower (resp. upper) bound by using `]` (resp. `[`).
`age=]20 TO 35[` means `age` strictly greater than `20` and  strictly less than`35` 
`age=[20 TO 35[` means `age` greater than or equal to `20` and  strictly less  than`35` 
`age=]20 TO 35]` means `age` greater than `20` and  less than or equal to `35` 

#### Logical operators

Logicial operators can be used combine several instances of [IFilter][class-ifilter] together.

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
#### <a href='#' id='not-expression'>Not</a>
To invert a filter, simply put a `!` before the expression to negate

Search for `vigilante` resources where the value of `nickname` property does not starts with `"B"`
```csharp
"nickname=!B*"
```
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

Searchs for `vigilante` resources that starts with  `Bat` or `Sup` and ends with `man` or 
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
Sometime, you'll be looking for a filter that match exactly a text that contains a character that has a special meaning.

```csharp
comment=@"*\!"
```
will be converted



The backslash character (`\`) can be used to escape characters that will be otherwise interpreted as 
a special character.

 


## <a href='#' id='sorting'>Sorting</a>
This library also supports a custom syntax to sort elements.

`sort=nickname` or `sort=+nickname` sort items by their `nickname` properties in ascending 
order.

You can sort by several properties at once by separating them with a `,`.

For example `sort=+nickname,-age` allows to sort by `nickname` ascending, then by `age` property descending. 

# <a href='#' id='how-to-install'>How to use</a>

So you have your API and want provide a great search experience ?

## On the client
The client will have the responsability of building search criteria.
Go to [filtering](#filtering) and [sorting](#sorting) sections to see example on how to get started.

## On the backend
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

        // filter now contains how search criteria and is ready to be used :-) 

    }
}

```

Some explanation on the controller's code above (assuming ):

1. The endpoint is bound to incoming HTTP GET requests  on `/vigilante/search`
2. The framework will parse incoming querystring and feeds the `query` parameter accordingly.
3. From this point we test each criterion to see if it's acceptable to turn it into a [IFilter][class-ifilter] instance.
For that purpose, a handy `.ToFilter<T>()` string extension is available. It turns a query-string key-value pair into a 
full [IFilter][class-ifilter].
4. we can then either :
   - use the filter directly is there was only one filter
   - or combine them using [composite filter][class-multi-filter] if there were more than one criteria


You may have notice that `SearchVigilanteQuery.Age` property is nullable whereas `Vigilante.Age` is not. 
This is to distinguuish if the `Age` criterion was provided or not when calling the `vigilantes/search` endpoint

# <a href='#' id='how-to-install'>How to install</a>

1. run `dotnet install DataFilters` : you can already start to build [IFilter][class-ifilter] instances
2. You can then add `Datafilters.Expressions` package to build `Expression<Func<T, bool>>`


[class-ifilter]: /src/DataFilters/IFilter.cs
[class-filter]: /src/DataFilters/Filter.cs
[class-multi-filter]: /src/DataFilters/MultiFilter.cs