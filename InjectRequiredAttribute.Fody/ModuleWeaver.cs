using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using InjectRequiredAttribute.Fody;
using Mono.Cecil;

public class ModuleWeaver
{
    private TypeReference _typeTypeDefinition;
    private MethodReference _attributeConstructor;

    public ModuleDefinition ModuleDefinition { get; set; }
    public string SolutionDirectoryPath { get; set; }

    public void Execute()
    {
        _typeTypeDefinition = ModuleDefinition.ImportReference(typeof(Type));
        _attributeConstructor = ModuleDefinition.GetConstructor("System.ComponentModel.DataAnnotations", "RequiredAttribute");

        var assemblyAttributes = ModuleDefinition.Assembly.CustomAttributes.Where(x => x.AttributeType.Name == "InjectRequiredAttributeAttribute");
        foreach (var assemblyAttribute in assemblyAttributes)
        {
            var typeMatchPattern = assemblyAttribute.GetPropertyValue<string>("TypeMatchPattern");
            var resourceType = assemblyAttribute.GetPropertyValue<TypeReference>("ResourceType");
            var suffix = assemblyAttribute.GetPropertyValue<string>("ResourceSuffix");
            AddRequiredAttribute(typeMatchPattern, resourceType, suffix);
        }
    }

    private void AddRequiredAttribute(string typeMatchPattern, TypeReference resourceType, string suffix)
    {
        var resource = resourceType.ResolveFromDirectory(Path.GetDirectoryName(ModuleDefinition.FullyQualifiedName), SolutionDirectoryPath);

        foreach (var type in ModuleDefinition.Types)
        {
            AddRequiredAttributeToType(type, resourceType, resource, typeMatchPattern, suffix);
        }
    }


    private void AddRequiredAttributeToType(TypeDefinition type, TypeReference resourceType, TypeDefinition resource, string typeMatchPattern, string suffix)
    {
        if (Regex.IsMatch(type.FullName, typeMatchPattern))
        {
            foreach (var property in type.Properties.Where(property => !property.HasAttribute("System.ComponentModel.DataAnnotations.RequiredAttribute")))
            {
                var propertyName = property.Name + suffix;
                if (!resource.Properties.Any(x => x.Name == propertyName))
                    continue;

                var requiredAttribute = new CustomAttribute(_attributeConstructor);
                requiredAttribute.AddProperty("ErrorMessageResourceName", ModuleDefinition.TypeSystem.String, propertyName);
                requiredAttribute.AddProperty("ErrorMessageResourceType", _typeTypeDefinition, resourceType);
                property.CustomAttributes.Add(requiredAttribute);
            }
        }

        foreach (var nestedType in type.NestedTypes)
        {
            AddRequiredAttributeToType(nestedType, resourceType, resource, typeMatchPattern, suffix);
        }
    }
}