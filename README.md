# MapThis

MapThis is a Visual Studio 2022 Extension that adds code refactoring 
to map between two types in any given method.

You can download it from within Visual Studio in the Extensions menu or
directly from the 
[Market Place](https://marketplace.visualstudio.com/items?itemName=MarcosDimitrio.MapThisForVisualStudio2022). 
The previous version for 
[VS2019 is also available](https://marketplace.visualstudio.com/items?itemName=MarcosDimitrio.MapThisForVisualStudio2019), 
but no longer being maintained.

## How it works

When you have a method like:

```csharp
public SomeClass Map(SomeClassDto item)
{
}
```
    
You can open the Quick Actions menu (usually CTRL+.), select "Map 
this" and it'll automatically map your classes:

```csharp
public SomeClass Map(SomeClassDto item)
{
    var newItem = new SomeClass()
    {
        Id = item.Id,
        CreatedOn = item.CreatedOn,
        // and so on...
    }

    return newItem;
}
```

It will also map lists and children classes that exist in the 
return type, so if you have a class `Parent` that has a list of 
`Child` and another property `OtherChild`:

```csharp
public class Parent
{
    public IList<Child> Child { get; set; }
    public OtherChild OtherChild { get; set; }
}
public class ParentDto
{
    public IList<ChildDto> Child { get; set; }
    public OtherChildDto OtherChild { get; set; }
}
public class Child { public int Id { get; set; } }
public class ChildDto { public int Id { get; set; } }
public class OtherChild { public int Id { get; set; } }
public class OtherChildDto { public int Id { get; set; } }
```

you can map a method like:

```csharp
public Parent Map(ParentDto item)
{
}
```

into:

```csharp
public Parent Map(ParentDto item)
{
    var newItem = new Parent()
    {
        Child = Map(item.Child),
        OtherChild = Map(item.OtherChild),
    };

    return newItem;
}

private IList<Child> Map(IList<ChildDto> source)
{
    var destination = new List<Child>();

    foreach (var item in source)
    {
        destination.Add(Map(item));
    }

    return destination;
}

private Child Map(ChildDto item)
{
    var newItem = new Child()
    {
        Id = item.Id,
    };

    return newItem;
}

private OtherChild Map(OtherChildDto item)
{
    var newItem = new OtherChild()
    {
        Id = item.Id,
    };

    return newItem;
}
```

Other features are:

- New methods are always private and always placed 
  after other public/internal/protected methods in the class.
- Automatically adds usings as needed.
- Keeps static access modifier if the method being mapped 
  is static, as well as any other characteristics it may have.
- Won't repeat mappings that already exist.
- Use namespace aliases, if they are used in the method
  being mapped (`using Dto = MyNamespace.DataTransferObjects`).
- It'll match properties by their name and write code 
  to map properties even if they are missing in the source 
  class, so that the developer can fix it.

## About

Written by Marcos Dimitrio, using C# and Roslyn.

Many thanks to Cezary Piątek and his 
[MappingGenerator](https://github.com/cezarypiatek/MappingGenerator), 
which was a great source of knowledge when I started learning how to
work with Roslyn.

The [RoslynQuoter](https://roslynquoter.azurewebsites.net/) 
(source [here](https://github.com/KirillOsenkov/RoslynQuoter)) is a 
must-use tool when writing extensions with code analysis.
