# Datafilters


DataFilters is a small library that allow to convert a string to its strongly typed equivalent.

## Introduction
The idea came to me when working on a set of rest apis and trying to build `/search` endpoints.
I wanted to have a uniform way to query a collection of resources.



Let's say your api manage `vigilantes` resources :
```c#
/* JSon */
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

### Starts with :
```c#
Expression<Func<Vigilante, bool>> filter = "nickname=S*".ToFilter<Client>();
```
will result in 
```c#
x => x.Nickname.StartsWith("S")
```

### Ends with
```c#
Expression<Func<Vigilante, bool>> filter = "nickname=*S".ToFilter<Client>();
```
will result in 
```c#
x => x.Nickname.EndsWith("S")
```

### Greater than
```c#
Expression<Func<Vigilante, bool>> filter = "nickname=*S".ToFilter<Client>();
```
will result in 
```c#
x => x.EndsWith("S")
```

### Less than
```c#
Expression<Func<Vigilante, bool>> filter = "age=-35".ToFilter<Client>();
```
will result in 
```c#
x => x.Age.EndsWith("S")
```


### Logical operators
#### And
#### Or

#### Not
To negate a filter, simply put a `!` before the filter to negate
```c#
"nickname=!B*"
``

## Ordering
