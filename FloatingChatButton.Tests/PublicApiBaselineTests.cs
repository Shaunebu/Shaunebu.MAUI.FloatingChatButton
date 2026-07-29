using System.Reflection;
using System.Runtime.InteropServices;
using Xunit;

namespace FloatingChatButton.Tests;

public sealed class PublicApiBaselineTests
{
    [Theory]
    [InlineData("net9.0-android")]
    [InlineData("net10.0-android")]
    public void AndroidAssemblyPublicApiMatchesBaseline(string targetFramework)
    {
        var assemblyPath = GetAndroidAssemblyPath(targetFramework);
        Assert.True(File.Exists(assemblyPath), $"Build the library before running API baseline tests. Missing: {assemblyPath}");

        var actual = GetPublicApi(assemblyPath);
        var baselinePath = Path.Combine(AppContext.BaseDirectory, "PublicApi.Shipped.txt");
        var expected = File.ReadAllLines(baselinePath)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(expected, actual);
    }

    private static string GetAndroidAssemblyPath(string targetFramework)
    {
        return Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            $"../../../../FloatingChatButton/bin/Release/{targetFramework}/FloatingChatButton.dll"));
    }

    private static string[] GetPublicApi(string assemblyPath)
    {
        var runtimeAssemblies = Directory.GetFiles(RuntimeEnvironment.GetRuntimeDirectory(), "*.dll");
        var outputAssemblies = Directory.GetFiles(Path.GetDirectoryName(assemblyPath)!, "*.dll");
        var mauiAssemblies = GetMauiPackageAssemblies(assemblyPath);
        var resolver = new PathAssemblyResolver(runtimeAssemblies.Concat(outputAssemblies).Concat(mauiAssemblies).Append(assemblyPath));

        using var context = new MetadataLoadContext(resolver);
        var assembly = context.LoadFromAssemblyPath(assemblyPath);
        var lines = new List<string>();

        foreach (var type in assembly.GetExportedTypes().Where(t => !t.IsNested && t.FullName != "FloatingChatButton.Resource"))
        {
            lines.Add($"T:{FriendlyName(type)}");

            foreach (var constructor in type.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                lines.Add($"M:{FriendlyName(type)}.#ctor({FormatParameters(constructor)})");
            }

            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                lines.Add($"F:{FriendlyName(type)}.{field.Name} : {FriendlyName(field.FieldType)}");
            }

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                var accessors = $"{(property.GetMethod is not null ? "get;" : string.Empty)}{(property.SetMethod is not null ? "set;" : string.Empty)}";
                lines.Add($"P:{FriendlyName(type)}.{property.Name} : {FriendlyName(property.PropertyType)} {{ {accessors} }}");
            }

            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName))
            {
                lines.Add($"M:{FriendlyName(type)}.{method.Name}({FormatParameters(method)}) : {FriendlyName(method.ReturnType)}");
            }
        }

        return lines.Order(StringComparer.Ordinal).ToArray();
    }

    private static string FormatParameters(MethodBase method)
    {
        return string.Join(", ", method.GetParameters().Select(parameter => FriendlyName(parameter.ParameterType)));
    }

    private static IEnumerable<string> GetMauiPackageAssemblies(string assemblyPath)
    {
        var packageRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".nuget",
            "packages");

        if (!Directory.Exists(packageRoot))
        {
            return [];
        }

        var targetMajor = assemblyPath.Contains($"{Path.DirectorySeparatorChar}net10.0-android", StringComparison.OrdinalIgnoreCase)
            ? "10."
            : "9.";
        var targetMoniker = targetMajor == "10." ? "net10.0" : "net9.0";
        var androidMoniker = targetMajor == "10." ? "net10.0-android" : "net9.0-android";

        return Directory.EnumerateFiles(packageRoot, "*.dll", SearchOption.AllDirectories)
            .Where(path =>
                path.Contains($"{Path.DirectorySeparatorChar}{targetMajor}", StringComparison.OrdinalIgnoreCase)
                && path.Contains($"{Path.DirectorySeparatorChar}lib{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                && (path.Contains($"{Path.DirectorySeparatorChar}{androidMoniker}", StringComparison.OrdinalIgnoreCase)
                    || path.Contains($"{Path.DirectorySeparatorChar}{targetMoniker}{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)))
            .Where(path => Path.GetFileName(path).StartsWith("Microsoft.Maui", StringComparison.OrdinalIgnoreCase))
            .GroupBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase)
            .Select(group => group
                .OrderByDescending(path => path.Contains(androidMoniker, StringComparison.OrdinalIgnoreCase))
                .First());
    }

    private static string FriendlyName(Type type)
    {
        if (type.IsGenericType)
        {
            var name = type.GetGenericTypeDefinition().FullName!;
            var tick = name.IndexOf('`', StringComparison.Ordinal);
            if (tick >= 0)
            {
                name = name[..tick];
            }

            return $"{name}<{string.Join(", ", type.GetGenericArguments().Select(FriendlyName))}>";
        }

        return type.FullName ?? type.Name;
    }
}
