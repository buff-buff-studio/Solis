using System;

[Serializable]
public class ExposedValueSelector
{
    public string fieldName;
}


[AttributeUsage(AttributeTargets.Field)]
public class ExposedFieldAttribute : Attribute
{
    public string DisplayName;
    public ExposedFieldAttribute(string displayName)
    {
        DisplayName = displayName;
    }
}
