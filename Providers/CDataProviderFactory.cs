using CDataProviderTestApp.Models;
using System;

namespace CDataProviderTestApp.Providers
{
    public static class CDataProviderFactory
   {
      public static BaseCDataProvider CreateProvider(ConnectionOptions options)
      {
         return options.ProviderType.ToLower() switch
         {
            "excelonline" => new ExcelOnlineProvider(options),
            _ => throw new ArgumentException("Unsupported provider")
         };
      }
   }

}
