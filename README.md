# MapThis

This is a Visual Studio Extension that adds a code refactoring 
to map between two types in any given method.

When you have a method like:

    public SomeClass Map(SomeClassDto item)
    {
    }
    
You can open the Quick Actions menu (usually CTRL+.), select "Map 
this" and it'll automatically map your classes:

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

# About

Written by Marcos Dimitrio, using C# and Roslyn.