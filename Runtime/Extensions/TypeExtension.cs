using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public static class TypeExtension
{
    public static IEnumerable<Type> GetChildrenTypes(this Type type)
    {
        var subclassTypes = Assembly.GetAssembly(type).GetTypes().Where(t => t.IsSubclassOf(type) && t.BaseType == type);
        return subclassTypes;
    }
}
