using System;
using InjectRequiredAttribute.ExampleTarget;
using InjectRequiredAttribute.ExampleTarget.Resources.Properties;

[assembly: InjectRequiredAttribute(TypeMatchPattern = "^InjectRequiredAttribute.ExampleTarget.*$", ResourceType = typeof(Resources), ResourceSuffix = "_Required")]

namespace InjectRequiredAttribute.ExampleTarget
{
    public class InjectRequiredAttributeAttribute : Attribute
    {
        public string TypeMatchPattern { get; set; }
        public Type ResourceType { get; set; }
        public string ResourceSuffix { get; set; }
    }
}