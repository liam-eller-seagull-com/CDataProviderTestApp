namespace CDataProviderTestApp.Primitives
{
   public enum OAuthOperation
   {
      GetUrl,
      GetTokensDesktop,
      GetTokensWeb
   }

   public static class OAuthConstants
   {
      public static readonly string InitiateOAuthGetAndRefresh = "GETANDREFRESH";
      public static readonly string InitiateOAuthRefresh = "REFRESH";
      public static readonly string InitiateOAuthOFF = "OFF";
      public static readonly string OAuthAccessTokenProperty = "OAuth Access Token";
      public static readonly string OAuthRefreshTokenProperty = "OAuth Refresh Token";
      public static readonly string OAuthAccessTokenColumn = "OAuthAccessToken";
      public static readonly string OAuthRefreshTokenColumn = "OAuthRefreshToken";
      public static readonly string OAuthTokenExpiryColumn = "ExpiresIn";
      public static readonly string OAuthModeApplication = "APP";
      public static readonly string OAuthModeWeb = "WEB";
      public static readonly string AccessTokenKey = "oauth access token";
      public static readonly string RefreshTokenKey = "oauth refresh token";
      public static readonly string VerifierTokenKey = "oauth verifier";
      public static readonly string OAuthSettingsLocationKey = "oauth settings location";
      public static readonly string ConnectionTimeOutString = "WebServerTimeout=45";
   }
}
