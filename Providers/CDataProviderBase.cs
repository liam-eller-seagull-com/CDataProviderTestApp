using System;
using System.Collections.Generic;
using System.Data;
using System.Data.CData.ExcelOnline;
using System.Data.Common;
using System.Threading.Tasks;

using CDataProviderTestApp.Primitives;
using CDataProviderTestApp.Utility;
using CDataProviderTestApp.Models;

namespace CDataProviderTestApp.Providers
{
   public abstract class BaseCDataProvider : ICDataProvider
   {
      protected BaseCDataProvider(ConnectionOptions options)
      {
         StoreDiagnostics = options.StoreDiagnostics ?? false;
         DiagnosticLevel = options.DiagnosticLevel;
         DiagnosticLocation = options.DiagnosticLocation;
      }

      #region Properties
      /// <summary>
      /// 
      /// </summary>
      public abstract string ProviderType { get; }

      /// <summary>
      /// 
      /// </summary>
      //public string ConnectionString
      //{ 
      //   get => Builder.ConnectionString;
      //   set => Builder.ConnectionString = value;
      //}

      /// <summary>
      /// Sheet specific list query string
      /// </summary>
      protected abstract string ListSheetsQuery { get; }

      /// <summary>
      /// Location of Diagnostic information - typically a log file.
      /// </summary>
      public string DiagnosticLocation { get; set; }
      public bool StoreDiagnostics { get; set; } // Store Diagnostic information for Provider actions, such as connection attempts.
      public string DiagnosticLevel { get; set; } // Level of Diagnostic information - typically 3.

      /// <summary>
      /// CData Run Time Key
      /// </summary>
      protected abstract string RunTimeKey { get; }
      #endregion Properties

      #region Methods
      /// <summary>
      /// 
      /// </summary>
      public abstract DbConnection CreateConnection();

      /// <summary>
      /// 
      /// </summary>
      public abstract DbCommand PrepareCommand(CommandType typeOfCommand, DbConnection connection, string commandText, List<(string, string)> parameters);

      /// <summary>
      /// 
      /// </summary>
      public abstract DbDataAdapter CreateDataAdapter();

      /// <summary>
      /// 
      /// </summary>
      public DbDataAdapter PrepareDataAdapter(CommandType typeOfCommand, string sqlcommand, DbConnection connection, List<(string, string)> parameters)
      {
         DbDataAdapter adapter = CreateDataAdapter();
         adapter.SelectCommand = PrepareCommand(typeOfCommand, connection, sqlcommand, parameters);
         return adapter;
      }

      /// <summary>
      /// Get the URL for OAuth Authorization negotiation.
      /// </summary>
      public string GetOAuthAuthorizationURL(string clientCallbackUrl, out string errorMessage)
      {
         errorMessage = string.Empty;
         OAuthNegotiator negotiator = new OAuthNegotiator(this);
         return negotiator.GetOAuthAuthorizationURL(clientCallbackUrl, out errorMessage);
      }

      /// <summary>
      /// Get Tokens needed to complete OAuth authorization.
      /// </summary>
      public (string, string, string) GetOAuthAuthorizationTokens(string verifierCode, string clientCallbackUrl, out string errorMessage)
      {
         errorMessage = string.Empty;
         OAuthNegotiator negotiator = new OAuthNegotiator(this);
         return negotiator.GetOAuthAuthorizationTokens(verifierCode, clientCallbackUrl, out errorMessage);
      }

      /// <summary>
      /// Get a list of available Databases/Spreadsheets
      /// </summary>
      public virtual async Task<IEnumerable<string>> ListDatabasesAsync()
      {
         var results = new List<string>();

         try
         {
            using var conn = CreateConnection();

            if (conn is DbConnection dbConn)
               await dbConn.OpenAsync();
            else
               conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = ListSheetsQuery;

            if (cmd is DbCommand dbCmd)
            {
               using var reader = await dbCmd.ExecuteReaderAsync();
               while (await reader.ReadAsync())
               {
                  var name = reader.GetString(0);
                  if (!string.IsNullOrWhiteSpace(name))
                     results.Add(name);
               }
            }
            else
            {
               using var reader = cmd.ExecuteReader();
               while (reader.Read())
               {
                  var name = reader.GetString(0);
                  if (!string.IsNullOrWhiteSpace(name))
                     results.Add(name);
               }
            }

            results.Sort();
         }
         catch (Exception ex)
         {
            // Optionally log
            Console.WriteLine($"[ListAvailableSheetsAsync] Error: {ex.Message}");
         }

         return results;
      }

      /// <summary>
      /// Prepare a Connection Descriptor for use with OAuth Authroization process.
      /// Step 1 obtains an Authorization URL
      /// </summary>
      public abstract void PrepareAuthorizationDescriptor(OAuthOperation operation);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="name"></param>
      /// <param name="value"></param>
      /// <returns></returns>
      protected IDbDataParameter CreateParameter(string name, object value)
      {
         var param = new ExcelOnlineParameter
         {
            ParameterName = name,
            Value = value
         };
         return param;
      }
      #endregion Methods
   }
}
