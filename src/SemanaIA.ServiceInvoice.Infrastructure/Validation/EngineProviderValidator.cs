using System.Text.Json;
using SemanaIA.ServiceInvoice.Domain.Models;
using SemanaIA.ServiceInvoice.Domain.Services;
using SemanaIA.ServiceInvoice.XmlGeneration.SchemaEngine;

namespace SemanaIA.ServiceInvoice.Infrastructure.Validation;

public class EngineProviderValidator : IProviderValidator
{
    private const string CheckXsdSelection = "XsdSelection";
    private const string CheckSchemaAnalysis = "SchemaAnalysis";
    private const string CheckConfigGeneration = "ConfigGeneration";
    private const string CheckXmlSerialization = "XmlSerialization";
    private const string CheckXsdValidation = "XsdValidation";

    private const string DefaultRootComplexTypeName = "TCDPS";
    private const string DefaultRootElementName = "DPS";

    public Task<ProviderValidationResult> Validate(ManagedProvider provider)
    {
        var checks = new List<ProviderValidationCheck>();
        string? blockReason = null;

        string? tempDir = null;
        try
        {
            tempDir = CreateTempProviderDirectory(provider);
            var xsdDir = Path.Combine(tempDir, ProviderProfile.XsdDirectoryName);

            var profile = ResolveProfile(provider);

            var xsdSelectionCheck = ValidateXsdSelection(xsdDir, profile);
            checks.Add(xsdSelectionCheck);
            if (!xsdSelectionCheck.Passed)
            {
                blockReason = xsdSelectionCheck.Detail;
                return Task.FromResult(BuildResult(false, checks, blockReason));
            }

            var selectedXsdFile = GetSelectedXsdFile(xsdDir, profile);

            var schemaAnalysisCheck = ValidateSchemaAnalysis(selectedXsdFile!);
            checks.Add(schemaAnalysisCheck);
            if (!schemaAnalysisCheck.Passed)
            {
                blockReason = schemaAnalysisCheck.Detail;
                return Task.FromResult(BuildResult(false, checks, blockReason));
            }

            var analyzer = new XsdSchemaAnalyzer();
            var schemaDocument = analyzer.Analyze(selectedXsdFile!);

            if (profile is null)
            {
                var configGenerationCheck = ValidateConfigGeneration(tempDir, provider.Name);
                checks.Add(configGenerationCheck);

                if (configGenerationCheck.Passed)
                    profile = LoadGeneratedProfile(tempDir);
                else
                {
                    blockReason = configGenerationCheck.Detail;
                    return Task.FromResult(BuildResult(false, checks, blockReason));
                }
            }
            else
            {
                checks.Add(new ProviderValidationCheck(CheckConfigGeneration, true, "Profile loaded from rules JSON."));
            }

            var serializationCheck = ValidateXmlSerialization(schemaDocument, profile!, xsdDir);
            checks.Add(serializationCheck);

            if (!serializationCheck.Passed)
                blockReason = serializationCheck.Detail;

            var allPassed = checks.All(check => check.Passed);
            return Task.FromResult(BuildResult(allPassed, checks, blockReason));
        }
        catch (Exception validationException)
        {
            checks.Add(new ProviderValidationCheck("UnexpectedError", false, validationException.Message));
            return Task.FromResult(BuildResult(false, checks, validationException.Message));
        }
        finally
        {
            CleanupTempDirectory(tempDir);
        }
    }

    // --- Private methods ---

    private static string CreateTempProviderDirectory(ManagedProvider provider)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"provider-validation-{provider.Id}-{Guid.NewGuid():N}");
        var xsdDir = Path.Combine(tempDir, ProviderProfile.XsdDirectoryName);
        Directory.CreateDirectory(xsdDir);

        foreach (var xsdFile in provider.XsdFiles)
        {
            var targetPath = Path.Combine(xsdDir, xsdFile.FileName);
            File.WriteAllBytes(targetPath, xsdFile.Content);
        }

        if (provider.RulesJson is not null)
        {
            var rulesDir = Path.Combine(tempDir, ProviderProfile.RulesDirectoryName);
            Directory.CreateDirectory(rulesDir);
            var rulesPath = Path.Combine(rulesDir, ProviderProfile.RulesFileName);
            File.WriteAllText(rulesPath, provider.RulesJson);
        }

        return tempDir;
    }

    private static ProviderProfile? ResolveProfile(ManagedProvider provider)
    {
        if (provider.RulesJson is null)
            return null;

        try
        {
            return JsonSerializer.Deserialize<ProviderProfile>(provider.RulesJson);
        }
        catch
        {
            return null;
        }
    }

    private static ProviderValidationCheck ValidateXsdSelection(string xsdDir, ProviderProfile? profile)
    {
        var selector = new SendXsdSelector();
        var selectionResult = selector.Select(xsdDir, profile);

        return selectionResult.SelectedFile is not null
            ? new ProviderValidationCheck(CheckXsdSelection, true, $"Selected: {Path.GetFileName(selectionResult.SelectedFile)}")
            : new ProviderValidationCheck(CheckXsdSelection, false, $"No suitable send XSD found: {selectionResult.Reason}");
    }

    private static string? GetSelectedXsdFile(string xsdDir, ProviderProfile? profile)
    {
        var selector = new SendXsdSelector();
        var selectionResult = selector.Select(xsdDir, profile);
        return selectionResult.SelectedFile;
    }

    private static ProviderValidationCheck ValidateSchemaAnalysis(string xsdFilePath)
    {
        try
        {
            var analyzer = new XsdSchemaAnalyzer();
            var schemaDocument = analyzer.Analyze(xsdFilePath);
            return new ProviderValidationCheck(CheckSchemaAnalysis, true,
                $"Analyzed {schemaDocument.ComplexTypes.Count} complex types.");
        }
        catch (Exception analysisException)
        {
            return new ProviderValidationCheck(CheckSchemaAnalysis, false,
                $"Schema analysis failed: {analysisException.Message}");
        }
    }

    private static ProviderValidationCheck ValidateConfigGeneration(string tempDir, string providerName)
    {
        try
        {
            var configGenerator = new ProviderConfigGenerator(tempDir);
            configGenerator.GenerateConfig(providerName);
            return new ProviderValidationCheck(CheckConfigGeneration, true, "Config generated successfully.");
        }
        catch (Exception configException)
        {
            return new ProviderValidationCheck(CheckConfigGeneration, false,
                $"Config generation failed: {configException.Message}");
        }
    }

    private static ProviderProfile? LoadGeneratedProfile(string tempDir)
    {
        var rulesPath = Path.Combine(tempDir, ProviderProfile.RulesDirectoryName, ProviderProfile.RulesFileName);

        if (!File.Exists(rulesPath))
        {
            var generatedPath = Path.Combine(tempDir, "generated", "suggested-rules.json");
            if (File.Exists(generatedPath))
                rulesPath = generatedPath;
        }

        return ProviderProfile.LoadFromFile(rulesPath);
    }

    private static ProviderValidationCheck ValidateXmlSerialization(
        SchemaDocument schemaDocument, ProviderProfile profile, string xsdDir)
    {
        try
        {
            var binder = new ServiceInvoiceSchemaDataBinder();
            var sampleGenerator = new ProviderSampleDocumentGenerator();
            var sampleDocument = sampleGenerator.Generate(profile);
            var boundData = binder.Bind(sampleDocument, profile);

            var ruleResolver = new ProviderRuleResolver(profile);
            var rootComplexTypeName = profile.RootComplexTypeName ?? DefaultRootComplexTypeName;
            var rootElementName = profile.RootElementName ?? DefaultRootElementName;

            var serializer = new SchemaBasedXmlSerializer();
            var serializationResult = serializer.SerializeAndValidate(
                schemaDocument, boundData, ruleResolver,
                rootComplexTypeName, rootElementName,
                xsdDir, profile.Version);

            if (serializationResult.IsValid && serializationResult.Xml is not null)
            {
                return new ProviderValidationCheck(CheckXmlSerialization, true,
                    "XML serialization and XSD validation succeeded.");
            }

            if (serializationResult.Xml is not null && serializationResult.ValidationErrors.Count > 0)
            {
                var validationSummary = string.Join("; ", serializationResult.ValidationErrors.Take(5));
                return new ProviderValidationCheck(CheckXsdValidation, false,
                    $"XSD validation errors: {validationSummary}");
            }

            var errorSummary = string.Join("; ", serializationResult.Errors
                .Take(5)
                .Select(error => $"[{error.Kind}] {error.Field}: {error.Message}"));

            return new ProviderValidationCheck(CheckXmlSerialization, false,
                $"Serialization errors: {errorSummary}");
        }
        catch (Exception serializationException)
        {
            return new ProviderValidationCheck(CheckXmlSerialization, false,
                $"Serialization failed: {serializationException.Message}");
        }
    }

    private static ProviderValidationResult BuildResult(
        bool passed, List<ProviderValidationCheck> checks, string? blockReason)
    {
        return new ProviderValidationResult(passed, checks, blockReason, DateTimeOffset.UtcNow);
    }

    private static void CleanupTempDirectory(string? tempDir)
    {
        if (tempDir is null || !Directory.Exists(tempDir))
            return;

        try
        {
            Directory.Delete(tempDir, recursive: true);
        }
        catch
        {
            // Best-effort cleanup; temp directory will be cleaned by OS eventually
        }
    }
}
