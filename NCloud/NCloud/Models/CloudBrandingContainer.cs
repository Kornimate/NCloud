namespace NCloud.Models
{
    /// <summary>
    /// Container to hold cloud branding data, stored and populated from appSettings.json
    /// </summary>
    public class CloudBrandingContainer
    {
        public string AppName { get; set; } = String.Empty;
        public string LogoPath { get; set; } = String.Empty;
        public string LogoNoTextPath { get; set; } = String.Empty;
    }
}
