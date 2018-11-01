# Datafilters


DataFilters is a small library that allow to convert a string to its strongly typed equivalent.

## Introduction
The idea came to me when working on a set of rest apis and trying to build `/search` endpoints.
I wanted to have a uniform way to query a collection of resources.



Let's say your api manage `vigilante` resources :
```c#
/* C# object */
[JsonObject]
class Vigilante {
    public string Firstname { get; set; }
    public string Lastname { get; set; }
    public int Age { get; set; }
    
}
```

```javascript
/* JSon */
vigilante : {
    firstname : string,
    lastname : string,
    nickname : string,
    age : number
}
```

## Filtering
### Starts with
```c#
"nickname=S*".ToFilter<Client>()
```
will result in a [IFilter](/src/DataFilters/IFilter.cs) instance equivalent to
```c#
x => x.Nickname.StartsWith("S")
```

### Ends with
```c#
"nickname=*S".ToFilter<Client>();
```
will result in a [IFilter](/src/DataFilters/IFilter.cs) instance equivalent to
```c#
x => x.Nickname.EndsWith("S")
```

### Greater than or equal
```c#
Expression<Func<Vigilante, bool>> filter = "age=20-".ToFilter<Client>();
```
will result in a [IFilter](./src/DataFilters/IFilter.cs) instance equivalent to
```c#
x => x.Age >= 20
```

### Less than or equal
```c#
Expression<Func<Vigilante, bool>> filter = "age=-35".ToFilter<Client>();
```
will result in a [IFilter](./src/DataFilters/IFilter.cs) instance equivalent to
```c#
x => x.Age <= 35
```

### Between
```c#
"age=20-35".ToFilter<Client>() 
```
will result in a [IFilter](./src/DataFilters/IFilter.cs) instance equivalent to
```
x => x.Age >= 20 && x.Age <= 35
```

### Logical operators
#### And
```c#
Expression<Func<Vigilante, bool>> filter = "nickname=Bat*,*man".ToFilter<Client>();
```
will result in a [IFilter](./src/DataFilters/IFilter.cs) instance equivalent to
```c#
x => x.Nickname.StartsWith("Bat") && x.Nickname.EndsWith("man")
```
#### Or
```c#
Expression<Func<Vigilante, bool>> filter = "nickname=Bat*|*man".ToFilter<Client>();
```
will result in 
```c#
x => x.Nickname.StartsWith("Bat") || x.Nickname.EndsWith("man")
```
#### Not
To negate a filter, simply put a `!` before the filter to negate
```c#
"nickname=!B*"
```
will result in
```c#
x => !x.StartsWith("B")
```

## Ordering
`sort=nickname` or `sort=+nickname` sort items by their `nickname` properties in ascending 
order.

You can sort by several properties at once by separating them with a `,`.

For example `sort=+nickname,-age` allows to sort by `nickname` ascending, then by `age` property descending. 


