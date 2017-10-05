using System.Collections.Generic;
using ESRI.ArcGIS.Geodatabase;

using FeatureRenderer.Core.Enum;

namespace FeatureRenderer.Data
{
    public interface IFeatureRendererRepository
    {
        #region Operations (6)
        
        List<string> GetTables(string file);

        List<string> GetColumnNames(string file, string table);

        List<string> GetSqlServerTables(string server, string instance, string database, string version, bool directConnect);

        List<string> GetPasswordSqlServerTables(string server, string instance, string database, string version, string user, string password, bool directConnect);

        List<string> GetSqlServerColumnNames(string table, string server, string instance, string database, string version, bool directConnect);

        List<string> GetPasswordSqlServerColumnNames(string table, string server, string instance, string database, string version, string user, string password, bool directConnect);

        List<string> GetIDNumbers(string file, string table, string primaryKeyColumn);

        ResultType ArcObjectsAccessCheckDatabaseConnection(string leg_tab_pfad, string legendentabelle);

        ITable getArcObjectsAccessTable(string leg_tab_pfad, string legendentabelle);

        ResultType ArcObjectsSDECheckDbConnWithoutAuthentication(string server, string instance, string database, string version, string legendentabelle);

        ITable getArcObjectsSDETableWithoutAuthentication(string server, string instance, string database, string version, string legendentabelle);

        ResultType ArcObjectsSDECheckDbConnWithAuthentication(string server, string instance, string database, string user, string password, string version, string legendentabelle);

        ITable getArcObjectsSDETableWithAuthentication(string server, string instance, string database, string user, string password, string version, string legendentabelle);

        #endregion
    }
}
