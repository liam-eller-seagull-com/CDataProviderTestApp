namespace CDataProviderTestApp.Models
{
   public class ConnectionOptions
   {
      public string ProviderType { get; set; }

      public string? ConnectionString { get; set; }

      public string? OAuthClientId { get; set; }
      public string? OAuthClientSecret { get; set; }
      public string? OAuthCallbackUrl { get; set; }

      public string? DiagnosticLocation { get; set; }
      public bool? StoreDiagnostics { get; set; }
      public string? DiagnosticLevel { get; set; }

      public ConnectionOptions(string providerType)
      {
         ProviderType = providerType;
      }
   }
}
