using CDataProviderTestApp.Providers;
using CDataProviderTestApp.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace CDataProviderTestApp.Controllers
{
    [ApiController]
   [Route("api/oauth")]
   public class OAuthController : ControllerBase
   {
      private readonly IConfiguration _configuration;

      public OAuthController(IConfiguration configuration)
      {
         _configuration = configuration;
      }

      /// <summary>
      /// Post: api/oauth/url
      /// </summary>
      /// <param name="providerType"></param>
      /// <param name="clientId"></param>
      /// <param name="clientSecret"></param>
      /// <param name="callbackUrl"></param>
      /// <returns></returns>
      [HttpPost("url")]
      public IActionResult Authorize([FromBody] OAuthProviderDto dto)
      {
         if (string.IsNullOrWhiteSpace(dto.ProviderType))
            return BadRequest(new { error = "Missing provider type" });
         else if (string.IsNullOrWhiteSpace(dto.ClientId))
            return BadRequest(new { error = "Missing client id" });
         else if (string.IsNullOrWhiteSpace(dto.ClientSecret))
            return BadRequest(new { error = "Missing client secret" });
         else if (string.IsNullOrWhiteSpace(dto.CallbackUrl))
            return BadRequest(new { error = "Missing callback url" });

         ConnectionOptions options = new(dto.ProviderType)
         {
            OAuthClientSecret = dto.ClientSecret,
            OAuthClientId = dto.ClientId,
            OAuthCallbackUrl = dto.CallbackUrl,
         };

         ICDataProvider provider = CDataProviderFactory.CreateProvider(options);

         string url = provider.GetOAuthAuthorizationURL(dto.CallbackUrl, out string errorMessage);

         if (!string.IsNullOrWhiteSpace(errorMessage))
            return BadRequest(new { errorMessage });

         return Ok(new { authorizationUrl = url });
      }

      /// <summary>
      /// Untested
      /// </summary>
      /// <param name="providerType"></param>
      /// <param name="clientId"></param>
      /// <param name="clientSecret"></param>
      /// <param name="callbackUrl"></param>
      /// <param name="code"></param>
      /// <returns></returns>
      [HttpPost("tokens")]
      public IActionResult PostOAuthTokens([FromBody] OAuthProviderDto dto)
      {
         if (string.IsNullOrWhiteSpace(dto.ProviderType))
            return BadRequest(new { error = "Missing provider type" });
         else if (string.IsNullOrWhiteSpace(dto.AccessCode))
            return BadRequest(new { error = "Missing authorization code." });
         else if (string.IsNullOrWhiteSpace(dto.ClientId))
            return BadRequest(new { error = "Missing client id" });
         else if (string.IsNullOrWhiteSpace(dto.ClientSecret))
            return BadRequest(new { error = "Missing client secret" });
         else if (string.IsNullOrWhiteSpace(dto.CallbackUrl))
            return BadRequest(new { error = "Missing callback url" });

         ConnectionOptions options = new(dto.ProviderType)
         {
            OAuthClientSecret = dto.ClientSecret,
            OAuthClientId = dto.ClientId,
         };
         ConfigureLogging(options);

         ICDataProvider provider = CDataProviderFactory.CreateProvider(options);

         (string accessToken, string refreshToken, string tokenExpiresInSeconds) = provider.GetOAuthAuthorizationTokens(dto.AccessCode, dto.CallbackUrl, out string errorMessage);

         if (!string.IsNullOrWhiteSpace(errorMessage))
            return BadRequest(new { errorMessage });

         return Ok(new { accessToken, refreshToken, tokenExpiresInSeconds });
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
