// This is an open source non-commercial project. Dear PVS-Studio, please check it.

// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

//Extensions.cs: Class extensions

namespace ScoobyRom.Extensions
{
using System.Collections.Generic;

using PropertyDictionary = System.Collections.Generic.Dictionary<string, object>;

//Extend any object with abitrary properties
//
//Set property with o.SetValue("prop name", an_object)
//Get property with o.GetValue("prop name"). Returns null if not set.
//o.RemoveValue("prop name") removes property
//o.RemoveAllValues() removes all properties

public static class ExtensionProperties
{
    // Properties storage
    private static Dictionary<object, PropertyDictionary>
        PropertyValues = new Dictionary<object, PropertyDictionary>();

    // Sets value for a property
    public static void SetValue(this object obj, string name, object value)
    {
        // Creates a dictionary for an object if it's absent
        if (!PropertyValues.ContainsKey(obj))
            PropertyValues[obj] = new PropertyDictionary();

        // Set value
        PropertyValues[obj][name] = value;
    }

    // Returns value
    public static object GetValue(this object obj,string name, object default_value=null)
    {
        // Return default value if dictionary for object is absent
        if (!PropertyValues.ContainsKey(obj)) return default_value;

        // Return default value if value is absent
        if (!PropertyValues[obj].ContainsKey(name))
            return default_value;

        // Return saved value
        return PropertyValues[obj][name];
    }

    // Remove property
    public static void RemoveValue(this object obj, string name)
    {
        // Do nothing if dictionary for object is absent
        if (!PropertyValues.ContainsKey(obj)) return;

        // Do nothing if poperty is absent
        if (!PropertyValues[obj].ContainsKey(name)) return;

        // Delete property
        PropertyValues[obj].Remove(name);

        // Remove dictionary if empty
        if (PropertyValues[obj].Count == 0)
            PropertyValues.Remove(PropertyValues[obj]);
    }

    // Remove all properties
    public static void RemoveAllValues(this object obj)
    {
        // Delete dictionary for object if present
        if (PropertyValues.ContainsKey(obj))
            PropertyValues.Remove(PropertyValues[obj]);
    }

}//ExtensionProperties
}//ScoobyRom.Extensions
