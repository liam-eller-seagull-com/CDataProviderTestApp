using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

using CDataProviderTestApp.Primitives;
using CDataProviderTestApp.Providers;

namespace CDataProviderTestApp.Utility
{
   /// <summary>
   /// Utility class to assist with negotiating OAuth Authorization on the web.
   /// </summary>
   class OAuthNegotiator
   {
      #region Fields
      ICDataProvider _provider;
      #endregion Fields

      #region Constructor
      public OAuthNegotiator(ICDataProvider provider)
      {
         _provider = provider;
      }
      #endregion Constructor

      #region Methods
      /// <summary>
      /// Get the URL for OAuth Authorization negotiation.
      /// </summary>
      public string GetOAuthAuthorizationURL(string clientCallbackUrl, out string errorMessage)
      {
         errorMessage = string.Empty;

         if (_provider == null)
            throw new InvalidOperationException("NULL Provider");

         string oAuthUrl = string.Empty;
         try
         {
            // Prepare the provider-specific connection parameters for initiating OAuth queries.
            _provider.PrepareAuthorizationDescriptor(OAuthOperation.GetUrl);

            using (DbConnection connection = _provider.CreateConnection())
            {
               // A callback URL may be necessary...
               List<(string, string)> authParams = [];
               if (!string.IsNullOrWhiteSpace(clientCallbackUrl))
               {
                  authParams = [("CallbackUrl", clientCallbackUrl)];
               }
               // <-------------------------------------------------------------------------------------->
               // Note Excel Online does not pick up the callback url this way, we must add it to the connection string instead.
               // This behavior is a change from previous versions of the driver and the latest Google Sheets driver does not have this issue.
               // Connection string added in the ExcelOnlineProvider.InitializeConnectionString()
               // <--------------------------------------------------------------------------------------->
               using (IDbCommand command = _provider.PrepareCommand(CommandType.StoredProcedure, connection, "GetOAuthAuthorizationURL", authParams))
               {
                  connection.Open();

                  using (IDataReader reader = command.ExecuteReader())
                  {
                        if (reader.Read())
                           oAuthUrl = (string)reader["URL"];
                  }
               }
            }
         }
         catch (Exception ex)
         {
            errorMessage = ex.Message;
         }

         return oAuthUrl;
      }

      /// <summary>
      /// Get Tokens needed to complete OAuth authorization.
      /// </summary>
      public (string, string, string) GetOAuthAuthorizationTokens(string verifierCode, string clientCallbackUrl, out string errorMessage)
      {
         errorMessage = string.Empty;

         if (_provider == null)
               throw new InvalidOperationException("NULL Provider");

         try
         {
            // Prepare the provider-specific connection parameters for initiating OAuth queries.
            OAuthOperation operation = (string.IsNullOrWhiteSpace(clientCallbackUrl) ? OAuthOperation.GetTokensDesktop : OAuthOperation.GetTokensWeb);
            _provider.PrepareAuthorizationDescriptor(operation);

            using (DbConnection connection = _provider.CreateConnection())
            {
               string accessToken = string.Empty;
               string refreshToken = string.Empty;
               string tokenExpiresInSeconds = string.Empty;
               const string name = "Name";
               const string value = "Value";

               using (DbDataAdapter adapter = PrepareAdapter(connection, verifierCode, clientCallbackUrl))
               {
                  connection.Open();

                  DataTable table = new DataTable();
                  adapter.Fill(table);
                  foreach (DataRow row in table.Rows)
                  {
                     if (operation == OAuthOperation.GetTokensWeb)
                     {
                        // Result of Stored Procedure call
                        accessToken = row.Field<string>(OAuthConstants.OAuthAccessTokenColumn);
                        refreshToken = row.Field<string>(OAuthConstants.OAuthRefreshTokenColumn);
                        tokenExpiresInSeconds = row.Field<string>(OAuthConstants.OAuthTokenExpiryColumn);
                     }
                     else
                     {
                        // Result of SELECT
                        if (row.Field<string>(name) == OAuthConstants.OAuthAccessTokenProperty)
                           accessToken = row.Field<string>(value);
                        else if (row.Field<string>(name) == OAuthConstants.OAuthRefreshTokenProperty)
                           refreshToken = row.Field<string>(value);
                     }
                  }
                  return (accessToken, refreshToken, tokenExpiresInSeconds);
               }
            }
         }
         catch (Exception ex)
         {
            errorMessage = ex.Message;
         }
         return (string.Empty, string.Empty, string.Empty);
      }

      /// <summary>
      /// Prepare DbDataAdapter for reading the Authorization Tokens
      /// </summary>
      DbDataAdapter PrepareAdapter(DbConnection connection, string verifierCode, string clientCallbackUrl)
      {
         if (!string.IsNullOrWhiteSpace(clientCallbackUrl))
         {
            // If we have the callback url, we're running the Web Version of Authentication, which uses the
            // Client callback url in a Stored Procedure call
            List<(string, string)> authParams = new List<(string, string)>
            {
               ("CallbackUrl", clientCallbackUrl),
               ("Verifier", verifierCode)
            };
            return _provider.PrepareDataAdapter(CommandType.StoredProcedure, "GetOAuthAccessToken", connection, authParams);
         }
         else
         {
            // Otherwise, we're calling a SELECT statement.
            return _provider.PrepareDataAdapter(CommandType.Text, "SELECT * from sys_connection_props", connection, parameters: null);
         }
      }
      #endregion Methods
   }
}
