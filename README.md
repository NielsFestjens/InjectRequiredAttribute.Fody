# InjectRequiredAttribute.Fody
A Fody add-in to inject the MVC Required Attribute.

Checkout InjectRequiredAttribute.ExampleTarget for usage.

Add an InjectRequiredAttributeAttribute like this:

	public class InjectRequiredAttributeAttribute : Attribute
    {
        public string TypeMatchPattern { get; set; }
        public Type ResourceType { get; set; }
        public string ResourceSuffix { get; set; }
    }

Then reference and configure it like this:
	[assembly: InjectRequiredAttribute(TypeMatchPattern = "^InjectRequiredAttribute.ExampleTarget.*$", ResourceType = typeof(Resources), ResourceSuffix = "_Required")]

TypeMatchPattern is a regex pattern, all classes with FullName matching the regex will be processed.
Nested classes will be processed.
ResourceType is the type of the resource file to use.
ResourceSuffix can be set to define a suffix for the name of the resource
Properties that already have the Required attribute defined will not be changed.
If the resource file does not contain an item with the name of the property, no Required attribute will be defined.