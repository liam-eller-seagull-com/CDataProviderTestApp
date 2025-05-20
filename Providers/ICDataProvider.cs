using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using CDataProviderTestApp.Primitives;

namespace CDataProviderTestApp.Providers
{
   public interface ICDataProvider
   {
      string ProviderType { get; }
      //string ConnectionString { get; set; }

      string DiagnosticLocation { get; set; }
      bool StoreDiagnostics { get; set; }
      string DiagnosticLevel { get; set; }

      DbConnection CreateConnection();
      DbCommand PrepareCommand(CommandType typeOfCommand, DbConnection connection, string commandText, List<(string, string)> parameters);
      DbDataAdapter PrepareDataAdapter(CommandType typeOfCommand, string sqlcommand, DbConnection connection, List<(string, string)> parameters);
      string GetOAuthAuthorizationURL(string clientCallbackUrl, out string errorMessage);
      (string, string, string) GetOAuthAuthorizationTokens(string verifierCode, string clientCallbackUrl, out string errorMessage);
      Task<IEnumerable<string>> ListDatabasesAsync();
      void PrepareAuthorizationDescriptor(OAuthOperation operation);
   }
}
