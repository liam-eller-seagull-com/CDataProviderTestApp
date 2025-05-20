using CDataProviderTestApp.Providers;
using CDataProviderTestApp.Models;

using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;

using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace CDataProviderTestApp.Controllers
{
   [Route("api/data")]
   public class DataController : Controller
   {
      IConfiguration _configuration;
      public DataController(IConfiguration configuration)
      {
         _configuration = configuration;
      }

      /// <summary>
      /// Untested
      /// </summary>
      /// <param name="dto"></param>
      /// <returns></returns>
      [HttpPost("databases")]
      public async Task<ActionResult<IEnumerable<string>>> ListDatabases([FromBody] DatabaseRequestDto dto)
      {
         if (string.IsNullOrWhiteSpace(dto.ProviderType))
            return BadRequest(new { error = "Missing provider type" });
         else if (string.IsNullOrWhiteSpace(dto.ConnectionString))
            return BadRequest(new { error = "Missing connection string" });
         else if (string.IsNullOrWhiteSpace(dto.QueryString))
            return BadRequest(new { error = "Missing query string" });

         ConnectionOptions options = new(dto.ProviderType)
         {
            ConnectionString = dto.ConnectionString,
         };
         ConfigureLogging(options);

         ICDataProvider provider = CDataProviderFactory.CreateProvider(options);

         IEnumerable<string> databases = await provider.ListDatabasesAsync();

         return databases?.ToList();
      }

      /// <summary>
      /// Configure non default logging options
      /// </summary>
      void ConfigureLogging(ConnectionOptions options)
      {
         if (options is not null && _configuration is not null)
         {
            string value = _configuration["EnableCDataLogging"];
            if (bool.TryParse(value, out bool flagVal))
            {
               options.StoreDiagnostics = flagVal;
               options.DiagnosticLocation = _configuration["CDataLogLocation"];
               options.DiagnosticLevel = _configuration["CDataLogLevel"];
            }
         }
      }
   }
}
