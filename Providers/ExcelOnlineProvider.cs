using CDataProviderTestApp.Models;
using CDataProviderTestApp.Primitives;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.CData.ExcelOnline;
using System.Data.Common;

namespace CDataProviderTestApp.Providers
{
   public class ExcelOnlineProvider : BaseCDataProvider
   {
      #region Properties
      public override string ProviderType => "ExcelOnline";
      protected override string ListSheetsQuery => "SELECT * FROM sys_tables";
      protected override string RunTimeKey => "";
      protected ExcelOnlineConnectionStringBuilder ConnectionStringBuilder { get; set; } = new ExcelOnlineConnectionStringBuilder();
      #endregion Properties

      public ExcelOnlineProvider(ConnectionOptions options) 
         : base(options) 
      {
         InitializeConnectionString(options.OAuthClientId, options.OAuthClientSecret, options.OAuthCallbackUrl);
      }

      #region Initialization
      /// <summary>
      /// Prepare some standard Connection String values
      /// </summary>
      void InitializeConnectionString(string clientId, string clientSecret, string callbackUrl)
      {
         // <-------------------------------------------------------------------------------->
         // Note the callback url must be added to the connection string...
         // Latest version of CData ExcelOnine driver requires the callback url in the connection string unlike GoogleSheets
         // See OAuthNegotiator.GetOAuthAuthorizationURL() for additional details.
         // <--------------------------------------------------------------------------------->
         //ConnectionStringBuilder.CallbackURL = callbackUrl; // Commented out to demonstrate issue.
         ConnectionStringBuilder.OAuthClientId = clientId;
         ConnectionStringBuilder.OAuthClientSecret = clientSecret;
         ConnectionStringBuilder.RTK = RunTimeKey;
         ConnectionStringBuilder.InitiateOAuth = OAuthConstants.InitiateOAuthRefresh; // Overridden in PrepareAuthorizationDescriptor()
         ConnectionStringBuilder.Other = "WebServerTimeout=120";

         // Always use ColumnFormat type detection for excel online as the provider has issues displaying DateTime data in any other type detection format.
         if (!string.IsNullOrWhiteSpace(ConnectionStringBuilder.TypeDetectionScheme))
            ConnectionStringBuilder.TypeDetectionScheme = "ColumnFormat";

         // Set the OAuthSettingsLocation connection property to empty string to avoid automatic caching of oAuth settings.
         ConnectionStringBuilder.OAuthSettingsLocation = string.Empty;
      }
      #endregion Initialization

      #region Methods
      /// <summary>
      /// Prepare a Connection String for use with OAuth Authroization process.
      /// </summary>
      public override void PrepareAuthorizationDescriptor(OAuthOperation operation)
      {
         switch (operation)
         {
            case OAuthOperation.GetUrl:
               ConnectionStringBuilder.InitiateOAuth = OAuthConstants.InitiateOAuthOFF;
               break;

            case OAuthOperation.GetTokensDesktop:
               ConnectionStringBuilder.InitiateOAuth = OAuthConstants.InitiateOAuthRefresh;
               ConnectionStringBuilder.OAuthAccessToken = string.Empty;
               ConnectionStringBuilder.OAuthRefreshToken = string.Empty;
               break;

            case OAuthOperation.GetTokensWeb:
               ConnectionStringBuilder.InitiateOAuth = OAuthConstants.InitiateOAuthOFF;
               ConnectionStringBuilder.OAuthAccessToken = string.Empty;
               ConnectionStringBuilder.OAuthRefreshToken = string.Empty;
               break;
         }
      }

      /// <summary>
      ///
      /// </summary>
      public override DbConnection CreateConnection()
      {
         string connStr = ConnectionStringBuilder.ConnectionString;

         // Splice in Log File settings if specified.
         if (StoreDiagnostics && !string.IsNullOrWhiteSpace(DiagnosticLocation))
         {
            ExcelOnlineConnectionStringBuilder builder = new ExcelOnlineConnectionStringBuilder(connStr);
            builder.Verbosity = DiagnosticLevel;
            builder.Logfile = DiagnosticLocation;
            connStr = builder.ConnectionString;
         }
         return new ExcelOnlineConnection(connStr);
      }

      /// <summary>
      /// Create ExcelOnlineCommand with passed arguments
      /// </summary>
      public override DbCommand PrepareCommand(CommandType typeOfCommand, DbConnection connection, string commandText, List<(string, string)> parameters)
      {
         if ((connection == null) || string.IsNullOrWhiteSpace(commandText))
            throw new ArgumentException("null Parameter");

         ExcelOnlineCommand command = connection.CreateCommand() as ExcelOnlineCommand;
         command.CommandText = commandText;
         command.CommandType = typeOfCommand;

         parameters?.ForEach(p => command.Parameters.Add(new ExcelOnlineParameter(p.Item1, p.Item2)));

         return command;
      }

      /// <summary>
      /// Create ExcelOnlineDataAdapter with no arguments
      /// </summary>
      public override DbDataAdapter CreateDataAdapter()
      {
         return new ExcelOnlineDataAdapter();
      }
      #endregion Methods
   }
}
