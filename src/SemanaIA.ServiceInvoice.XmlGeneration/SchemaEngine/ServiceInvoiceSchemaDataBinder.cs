using System.Globalization;
using System.Reflection;
using SemanaIA.ServiceInvoice.Domain.Models;

namespace SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;

public class ServiceInvoiceSchemaDataBinder
{
    public Dictionary<string, object?> Bind(DpsDocument document, ProviderProfile profile)
    {
        var data = new Dictionary<string, object?>();
        var resolver = new ProviderRuleResolver(profile);

        if (profile.WrapperBindings is not null)
        {
            foreach (var (schemaPath, expression) in profile.WrapperBindings)
            {
                var resolvedValue = ResolveExpression(document, expression, resolver, profile);
                if (resolvedValue is not null)
                    data[schemaPath] = resolvedValue;
            }
        }

        if (profile.Bindings is null)
            return data;

        var hasPathPrefix = !string.IsNullOrEmpty(profile.BindingPathPrefix);

        foreach (var (schemaPath, expression) in profile.Bindings)
        {
            var resolvedValue = ResolveExpression(document, expression, resolver, profile);
            if (resolvedValue is null)
                continue;

            var fullSchemaPath = hasPathPrefix
                ? $"{profile.BindingPathPrefix}.{schemaPath}"
                : schemaPath;

            data[fullSchemaPath] = resolvedValue;
        }

        return data;
    }

    // --- Private methods ---

    private static object? ResolveExpression(DpsDocument document, string expression, ProviderRuleResolver resolver, ProviderProfile profile)
    {
        var parts = expression.Split('|').Select(p => p.Trim()).ToArray();
        var source = parts[0];

        // Resolve source value
        object? value = source switch
        {
            "BuildId" => BuildDpsId(document, profile),
            _ when source.StartsWith("const:") => source[6..],
            _ => ResolvePropertyPath(document, source)
        };

        if (value is null)
            return null;

        // Apply pipe transformations
        for (var i = 1; i < parts.Length; i++)
        {
            value = ApplyPipe(value, parts[i], resolver);
            if (value is null)
                return null;
        }

        return value;
    }

    private static object? ResolvePropertyPath(object source, string path)
    {
        var segments = path.Split('.');
        object? current = source;

        foreach (var segment in segments)
        {
            if (current is null) return null;

            var type = current.GetType();
            var property = type.GetProperty(segment, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            if (property is null) return null;

            current = property.GetValue(current);

            // Convert enums to their underlying int value
            if (current is not null && current.GetType().IsEnum)
                current = Convert.ToInt32(current);
        }

        return current;
    }

    private static object? ApplyPipe(object? value, string pipe, ProviderRuleResolver resolver)
    {
        if (value is null) return null;
        var text = value.ToString() ?? string.Empty;

        if (pipe.StartsWith("format:"))
        {
            var format = pipe[7..];
            if (value is DateTimeOffset dto)
                return dto.ToString(format, CultureInfo.InvariantCulture);
            if (value is DateOnly dateOnly)
                return dateOnly.ToString(format, CultureInfo.InvariantCulture);
            if (value is DateTime dt)
                return dt.ToString(format, CultureInfo.InvariantCulture);
            return text;
        }

        if (pipe.StartsWith("padLeft:"))
        {
            var args = pipe[8..].Split(':');
            var length = int.Parse(args[0]);
            var padChar = args.Length > 1 ? args[1][0] : '0';
            return text.PadLeft(length, padChar);
        }

        if (pipe.StartsWith("enum:"))
        {
            var enumField = pipe[5..];
            return resolver.ResolveEnum(enumField, text)
                   ?? resolver.ResolveEnum(enumField, "_default")
                   ?? text;
        }

        if (pipe.StartsWith("nullable:"))
        {
            var fallback = pipe[9..];
            return string.IsNullOrEmpty(text) || text == "0" ? fallback : text;
        }

        if (pipe.StartsWith("decimal:"))
        {
            var digits = int.Parse(pipe[8..]);
            if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                return d.ToString($"F{digits}", CultureInfo.InvariantCulture);
            return text;
        }

        if (pipe == "digitsOnly")
            return new string(text.Where(char.IsDigit).ToArray());

        return text;
    }

    private static string BuildDpsId(DpsDocument doc, ProviderProfile profile)
    {
        var cnpj = !string.IsNullOrWhiteSpace(doc.Provider.Cnpj)
            ? doc.Provider.Cnpj.PadLeft(14, '0')
            : doc.Provider.FederalTaxNumber.ToString().PadLeft(14, '0');

        var resultTryParse = int.TryParse(doc.Series, out var seriesInt);

        if (resultTryParse)
            return $"DPS{doc.Provider.MunicipalityCode.PadLeft(7, '0')}2{cnpj}{seriesInt:00000}{doc.Number:000000000000000}";

        return $"DPS{doc.Provider.MunicipalityCode.PadLeft(7, '0')}2{cnpj}00000{doc.Number:000000000000000}";
    }
}
