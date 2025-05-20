namespace CDataProviderTestApp.Models
{
   public class OAuthProviderDto
   {
      public string ProviderType { get; set; }
      public string ClientId { get; set; }
      public string ClientSecret { get; set; }
      public string? CallbackUrl { get; set; }
      public string? AccessCode { get; set; }
   }
}
