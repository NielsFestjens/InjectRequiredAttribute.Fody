using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using NUnit.Framework;

[TestFixture]
public class WeaverTests
{
    private Assembly _assembly;
    private string _assemblyPath;

    [TestFixtureSetUp]
    public void Setup()
    {
        var projectPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\..\InjectRequiredAttribute.ExampleTarget\InjectRequiredAttribute.ExampleTarget.csproj"));
        _assemblyPath = Path.Combine(Path.GetDirectoryName(projectPath), @"bin\Debug\InjectRequiredAttribute.ExampleTarget.dll");
#if (!DEBUG)
        _assemblyPath = _assemblyPath.Replace("Debug", "Release");
#endif
        
        var moduleDefinition = ModuleDefinition.ReadModule(_assemblyPath);
        var weavingTask = new ModuleWeaver
        {
            ModuleDefinition = moduleDefinition
        };

        weavingTask.Execute();
        moduleDefinition.Write(_assemblyPath);

        _assembly = Assembly.LoadFile(_assemblyPath);

        AppDomain currentDomain = AppDomain.CurrentDomain;
        currentDomain.AssemblyResolve += (sender, args) =>
        {
            var assemblyPath = Path.Combine(Path.GetDirectoryName(_assemblyPath), new AssemblyName(args.Name).Name + ".dll");
            return File.Exists(assemblyPath) ? Assembly.LoadFrom(assemblyPath) : null;
        };
    }

    [Test]
    public void ValidateAttributeIsInjected()
    {
        var model = _assembly.GetType("InjectRequiredAttribute.ExampleTarget.TargetModel");
        var propertyToGetAnAttribute = model.GetProperty("PropertyToGetAnAttribute");
        var propertyToGetAnAttributeAttribute = propertyToGetAnAttribute.GetCustomAttributes(typeof (RequiredAttribute), false).Single() as RequiredAttribute;
        Assert.AreEqual(propertyToGetAnAttributeAttribute.ErrorMessageResourceName, "PropertyToGetAnAttribute_Required");
        Assert.AreEqual(propertyToGetAnAttributeAttribute.ErrorMessageResourceType.FullName, "InjectRequiredAttribute.ExampleTarget.Resources.Properties.Resources");

        var dto = _assembly.GetType("InjectRequiredAttribute.ExampleTarget.TargetModel+TargetDto");
        var dtoPropertyToGetAnAttribute = dto.GetProperty("PropertyToGetAnAttribute");
        var dtoPropertyToGetAnAttributeAttribute = dtoPropertyToGetAnAttribute.GetCustomAttributes(typeof(RequiredAttribute), false).Single() as RequiredAttribute;
        Assert.AreEqual(dtoPropertyToGetAnAttributeAttribute.ErrorMessageResourceName, "PropertyToGetAnAttribute_Required");
        Assert.AreEqual(dtoPropertyToGetAnAttributeAttribute.ErrorMessageResourceType.FullName, "InjectRequiredAttribute.ExampleTarget.Resources.Properties.Resources");

        var propertyThatHasNoTranslation = model.GetProperty("PropertyThatHasNoTranslation");
        var propertyThatHasNoTranslationAttribute = propertyThatHasNoTranslation.GetCustomAttributes(typeof(RequiredAttribute), false).SingleOrDefault();
        Assert.IsNull(propertyThatHasNoTranslationAttribute);

        var propertyToLeaveAsIs = model.GetProperty("PropertyToLeaveAsIs");
        var propertyToLeaveAsIsAttribute = propertyToLeaveAsIs.GetCustomAttributes(typeof(RequiredAttribute), false).Single() as RequiredAttribute;
        Assert.IsNull(propertyToLeaveAsIsAttribute.ErrorMessageResourceName);
        Assert.IsNull(propertyToLeaveAsIsAttribute.ErrorMessageResourceType);
    }
}