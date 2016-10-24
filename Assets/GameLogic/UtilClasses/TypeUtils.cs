using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

public static class TypeUtils
{
  
    public static bool HasPublicStaticMethod(Type type, string methodName)
    {
        return type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static) != null;
    }

    public static object RunPublicStaticMethod(Type type, string methodName, string singleArgument)
    {
        return type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { singleArgument });
    }

}