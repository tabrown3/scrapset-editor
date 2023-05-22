using System;
using UnityEngine;

public class ScrapsetValue
{
    public ScrapsetTypes Type { get; }
    object _value;
    public object Value
    {
        get
        {
            return _value;
        }
        set
        {
            var inType = value.GetType();
            var translatedScrapsetType = TranslateScrapsetTypeToCSharpType(Type);
            if (inType != translatedScrapsetType)
            {
                throw new Exception($"Type mismatch: attempted to assign {inType} to Scrapset type {Type}, which is represented by C# type {translatedScrapsetType}");
            }
            _value = value;
        }
    } // TODO: check to make sure the dynamic value passed in is of ScrapsetType type

    public ScrapsetValue(ScrapsetTypes type)
    {
        Type = type;
        var defaultVal = GetDefaultForType(type);
        Value = defaultVal;
    }

    private object GetDefaultForType(ScrapsetTypes type)
    {
        switch (type)
        {
            case ScrapsetTypes.Angle: return 0f;
            case ScrapsetTypes.Bool: return false;
            case ScrapsetTypes.Number: return 0f;
            case ScrapsetTypes.String: return "";
            case ScrapsetTypes.Vec2: return Vector2.zero;
            default: return null;

        }
    }

    private Type TranslateScrapsetTypeToCSharpType(ScrapsetTypes type)
    {
        switch (type)
        {
            case ScrapsetTypes.Angle: return typeof(float);
            case ScrapsetTypes.Bool: return typeof(bool);
            case ScrapsetTypes.Number: return typeof(float);
            case ScrapsetTypes.String: return typeof(string);
            case ScrapsetTypes.Vec2: return typeof(Vector2);
            default: return null;

        }
    }
}
