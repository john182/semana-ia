using Microsoft.AspNetCore.Mvc;
using SemanaIA.ServiceInvoice.Domain.Services;

namespace SemanaIA.ServiceInvoice.Api.Controllers;

[ApiController]
[Route("api/v1/providers/onboarding")]
[Tags("Provider Onboarding (Legacy)")]
public class ProviderOnboardingController : ControllerBase
{
    [HttpPost("onboard")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult Onboard(
        [FromForm] string providerName,
        [FromForm] List<IFormFile> xsdFiles,
        [FromForm] string municipalityCodes,
        [FromServices] IProviderOnboardingService onboardingService)
    {
        if (string.IsNullOrWhiteSpace(providerName))
            return BadRequest(new { error = "providerName is required." });

        if (xsdFiles is null || xsdFiles.Count == 0)
            return BadRequest(new { error = "At least one XSD file is required." });

        var parsedMunicipalityCodes = ParseMunicipalityCodes(municipalityCodes);
        var providerXsdFiles = ConvertToProviderXsdFiles(xsdFiles);

        var onboardingResult = onboardingService.Onboard(providerName, providerXsdFiles, parsedMunicipalityCodes);

        if (onboardingResult.ErrorMessage is not null)
            return Conflict(new { error = onboardingResult.ErrorMessage });

        return Ok(onboardingResult);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult List([FromServices] IProviderOnboardingService onboardingService)
    {
        return Ok(onboardingService.ListProviders());
    }

    [HttpGet("{name}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetStatus(string name, [FromServices] IProviderOnboardingService onboardingService)
    {
        return Ok(onboardingService.GetProviderStatus(name));
    }

    // --- Private methods ---

    private static List<string> ParseMunicipalityCodes(string? municipalityCodes)
    {
        if (string.IsNullOrWhiteSpace(municipalityCodes))
            return new List<string>();

        return municipalityCodes
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
    }

    private static List<ProviderXsdFile> ConvertToProviderXsdFiles(List<IFormFile> formFiles)
    {
        var providerXsdFiles = new List<ProviderXsdFile>();

        foreach (var formFile in formFiles)
        {
            using var memoryStream = new MemoryStream();
            formFile.CopyTo(memoryStream);
            providerXsdFiles.Add(new ProviderXsdFile(formFile.FileName, memoryStream.ToArray()));
        }

        return providerXsdFiles;
    }
}
