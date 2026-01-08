using System;

namespace Sackrany.Unit.ModuleSystem
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class DependencyAttribute : Attribute
    {
        public bool Optional;
        public DependencyAttribute(bool optional = false)
        {
            Optional = optional;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class TemplateAttribute : Attribute
    {
        public TemplateAttribute() { }
    }
    
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class HashKeyAttribute : Attribute
    {
        public readonly int Precision;
        public readonly bool IgnoreDefault;

        public HashKeyAttribute(int precision = 3, bool ignoreDefault = false)
        {
            Precision = precision;
            IgnoreDefault = ignoreDefault;
        }
        public HashKeyAttribute(bool ignoreDefault = false)
        {
            Precision = 3;
            IgnoreDefault = ignoreDefault;
        }
        public HashKeyAttribute()
        {
            Precision = 3;
            IgnoreDefault = false;
        }
    }
}