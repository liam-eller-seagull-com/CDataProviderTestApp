namespace CDataProviderTestApp.Models
{
   public class DatabaseRequestDto
   {
      public string ProviderType { get; set; }
      public string ConnectionString { get; set; }
      public string QueryString { get; set; }
   }
}
