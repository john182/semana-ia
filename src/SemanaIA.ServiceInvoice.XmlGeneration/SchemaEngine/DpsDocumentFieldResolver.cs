using System.Reflection;
using SemanaIA.ServiceInvoice.Domain.Models;

namespace SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;

public class DpsDocumentFieldResolver
{
    private readonly DpsDocument _document;

    public DpsDocumentFieldResolver(DpsDocument document)
    {
        _document = document;
    }

    public object? Resolve(string fieldPath)
    {
        return ResolvePropertyPath(_document, fieldPath);
    }

    public object? ResolveWithEnumName(string fieldPath)
    {
        return ResolvePropertyPathPreservingEnumNames(_document, fieldPath);
    }

    public static object? ResolvePropertyPath(object source, string path)
    {
        var segments = path.Split('.');
        object? current = source;

        foreach (var segment in segments)
        {
            if (current is null)
                return null;

            var type = current.GetType();
            var property = type.GetProperty(segment, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            if (property is null)
                return null;

            current = property.GetValue(current);

            // Convert enums to their underlying int value (consistent with legacy binder)
            if (current is not null && current.GetType().IsEnum)
                current = Convert.ToInt32(current);

            // Convert booleans to 0/1 (NFSe XML convention: "0"=false, "1"=true)
            if (current is bool boolValue)
                current = boolValue ? 1 : 0;
        }

        return current;
    }

    // --- Private methods ---

    private static object? ResolvePropertyPathPreservingEnumNames(object source, string path)
    {
        var segments = path.Split('.');
        object? current = source;

        foreach (var segment in segments)
        {
            if (current is null)
                return null;

            var type = current.GetType();
            var property = type.GetProperty(segment, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            if (property is null)
                return null;

            current = property.GetValue(current);

            // Keep enum as its string name (for enum mapping lookups)
            if (current is not null && current.GetType().IsEnum)
                current = current.ToString();
        }

        return current;
    }

}
