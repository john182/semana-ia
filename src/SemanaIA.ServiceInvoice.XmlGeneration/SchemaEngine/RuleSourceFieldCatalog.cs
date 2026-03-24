using System.Reflection;
using SemanaIA.ServiceInvoice.Domain.Models;

namespace SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;

public record SourceFieldEntry(string Path, string TypeName, string Description, List<string>? AllowedValues = null);

public class RuleSourceFieldCatalog
{
    private static readonly Lazy<List<SourceFieldEntry>> CachedFields = new(BuildFieldEntries);

    public static List<SourceFieldEntry> GetFields() => CachedFields.Value;

    /// <summary>
    /// Prefix that was used when fiscal fields lived in a nested Values object.
    /// Now those fields are flat on DpsDocument, so we strip the prefix for lookup.
    /// </summary>
    private const string LegacyValuesPrefix = "Values.";

    public static bool Contains(string fieldPath)
    {
        var normalizedPath = NormalizeLegacyPath(fieldPath);
        return CachedFields.Value.Any(entry =>
            string.Equals(entry.Path, normalizedPath, StringComparison.OrdinalIgnoreCase));
    }

    // --- Private methods ---

    private static List<SourceFieldEntry> BuildFieldEntries()
    {
        var entries = new List<SourceFieldEntry>();
        var visitedTypes = new HashSet<Type>();

        WalkProperties(typeof(DpsDocument), "", entries, visitedTypes);

        return entries;
    }

    private static void WalkProperties(Type type, string pathPrefix, List<SourceFieldEntry> entries, HashSet<Type> visitedTypes)
    {
        if (!visitedTypes.Add(type))
            return;

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            var propertyPath = string.IsNullOrEmpty(pathPrefix)
                ? property.Name
                : $"{pathPrefix}.{property.Name}";

            var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
            var typeName = GetFriendlyTypeName(propertyType);

            var allowedValues = propertyType.IsEnum
                ? Enum.GetNames(propertyType).ToList()
                : null;

            entries.Add(new SourceFieldEntry(propertyPath, typeName, BuildDescription(property, propertyPath), allowedValues));

            if (IsNavigableComplexType(propertyType))
            {
                WalkProperties(propertyType, propertyPath, entries, visitedTypes);
            }
        }

        visitedTypes.Remove(type);
    }

    private static bool IsNavigableComplexType(Type type)
    {
        if (type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal)
            || type == typeof(DateTimeOffset) || type == typeof(DateOnly) || type == typeof(DateTime)
            || type == typeof(Guid))
            return false;

        if (type.IsGenericType)
            return false;

        return type.IsClass;
    }

    private static string GetFriendlyTypeName(Type type)
    {
        if (type == typeof(string)) return "string";
        if (type == typeof(int)) return "int";
        if (type == typeof(long)) return "long";
        if (type == typeof(decimal)) return "decimal";
        if (type == typeof(bool)) return "bool";
        if (type == typeof(DateTimeOffset)) return "DateTimeOffset";
        if (type == typeof(DateOnly)) return "DateOnly";
        if (type.IsEnum) return $"enum:{type.Name}";

        return type.Name;
    }

    private static string BuildDescription(PropertyInfo property, string path)
    {
        var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

        if (propertyType.IsEnum)
            return $"Enum {propertyType.Name} — valores: {string.Join(", ", Enum.GetNames(propertyType))}";

        return $"Campo {path} ({GetFriendlyTypeName(propertyType)})";
    }

    private static string NormalizeLegacyPath(string path)
    {
        if (path.StartsWith(LegacyValuesPrefix, StringComparison.OrdinalIgnoreCase))
            return path[LegacyValuesPrefix.Length..];

        return path;
    }
}
