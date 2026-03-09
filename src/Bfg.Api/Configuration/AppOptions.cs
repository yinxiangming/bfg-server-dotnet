namespace Bfg.Api.Configuration;

/// <summary>
/// App-level settings loaded from environment (FRONTEND_URL, SITE_NAME).
/// Single source for these values to avoid hardcoded URLs.
/// </summary>
public class AppOptions
{
    public const string SectionName = "App";

    public string FrontendUrl { get; set; } = "";
    public string SiteName { get; set; } = "BFG";
}
