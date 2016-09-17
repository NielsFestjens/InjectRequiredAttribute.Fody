using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;

namespace InjectRequiredAttribute.Fody
{
    public static class Extensions
    {
        public static void AddProperty(this CustomAttribute attribute, string name, TypeReference type, object value)
        {
            attribute.Properties.Add(new CustomAttributeNamedArgument(name, new CustomAttributeArgument(type, value)));
        }

        public static T GetPropertyValue<T>(this CustomAttribute attribute, string name)
        {
            return attribute.Properties.Any(x => x.Name == name)
                ? (T)attribute.Properties.SingleOrDefault(x => x.Name == name).Argument.Value
                : default(T);
        }

        public static MethodReference GetConstructor(this ModuleDefinition moduleDefinition, string assemblyName, string name)
        {
            var assembly = moduleDefinition.AssemblyResolver.Resolve(assemblyName);
            var type = assembly.MainModule.Types.First(x => x.Name == name);
            return moduleDefinition.ImportReference(type.Methods.First(x => x.IsConstructor));
        }

        public static bool HasAttribute(this PropertyDefinition property, string attributeFullName)
        {
            return property.CustomAttributes.Any(x => x.AttributeType.FullName == attributeFullName);
        }

        public static TypeDefinition ResolveFromDirectory(this TypeReference typeReference, string directory, string solutionDirectoryPath)
        {
            var asmResolver = new DefaultAssemblyResolver();
            foreach (var relevantDirectory in GetRelevantDirectories(directory, solutionDirectoryPath))
            {
                asmResolver.AddSearchDirectory(relevantDirectory);
            }

            return new MetadataResolver(asmResolver).Resolve(typeReference);
        }

        public static IEnumerable<string> GetRelevantDirectories(string directory, string solutionDirectoryPath)
        {
            if (string.IsNullOrEmpty(solutionDirectoryPath))
                return new List<string> { directory };

            // C:\\Path\To\Solution\Project
            var projectDirectory = new DirectoryInfo(directory).Parent.Parent.FullName;

            // obj\Debug
            var relativePath = directory.Substring(projectDirectory.Length + 1);
            var items = new List<string>();
            foreach (var otherProjectDirectory in Directory.EnumerateDirectories(solutionDirectoryPath))
            {
                var otherProjectDirectoryName = Path.GetFileName(otherProjectDirectory);
                if (otherProjectDirectoryName != "packages" && !otherProjectDirectoryName.StartsWith("."))
                {
                    var otherDirectory = Path.Combine(otherProjectDirectory, relativePath);
                    if (Directory.Exists(otherDirectory))
                        items.Add(otherDirectory);
                }
            }
            return items;
        }
    }
}