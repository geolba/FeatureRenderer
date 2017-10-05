using System.Collections.Generic;
using FeatureRenderer.Data;
using ESRI.ArcGIS.Geodatabase;

using FeatureRenderer.Core.Enum;

namespace FeatureRenderer.Business
{
    public class FeatureRendererController : IFeatureRendererController
    {
        public List<string> GetTables(string file)
        {
            IFeatureRendererRepository repository = new FeatureRendererRepository();
            return repository.GetTables(file);
        }

        public List<string> GetColumnNames(string file, string table)
        {
            IFeatureRendererRepository repository = new FeatureRendererRepository();
            return repository.GetColumnNames(file, table);
        }

        public List<string> GetSqlServerTables(string server, string instance, string database, string version, bool directConnect)
        {
            IFeatureRendererRepository repository = new FeatureRendererRepository();
            return repository.GetSqlServerTables(server, instance, database, version, directConnect);
        }

        public List<string> GetPasswordSqlServerTables(string server, string instance, string database, string version, string user, string password, bool directConnect)
        {
            IFeatureRendererRepository repository = new FeatureRendererRepository();
            return repository.GetPasswordSqlServerTables(server, instance, database, version, user, password, directConnect);
        }

        public List<string> GetSqlServerColumnNames(string table, string server, string instance, string database, string version, bool directConnect)
        {
            IFeatureRendererRepository repository = new FeatureRendererRepository();
            return repository.GetSqlServerColumnNames(table, server, instance, database, version, directConnect);
        }

        public List<string> GetPasswordSqlServerColumnNames(string table, string server, string instance, string database, string version, string user, string password, bool directConnect)
        {
            IFeatureRendererRepository repository = new FeatureRendererRepository();
            return repository.GetPasswordSqlServerColumnNames(table, server, instance, database, version, user, password, directConnect);
        }

        public ITable getArcObjectsAccessTable(string leg_tab_pfad, string legendentabelle)
        {
            IFeatureRendererRepository repository = new FeatureRendererRepository();
            return repository.getArcObjectsAccessTable(leg_tab_pfad, legendentabelle);
        }

        public ResultType ArcObjectsAccessCheckDatabaseConnection(string leg_tab_pfad, string legendentabelle)
        {
            IFeatureRendererRepository repository = new FeatureRendererRepository();
            return repository.ArcObjectsAccessCheckDatabaseConnection(leg_tab_pfad, legendentabelle);
        }

        public ResultType ArcObjectsSDECheckDbConnWithoutAuthentication(string server, string instance, string database, string version, string legendentabelle)
        {
            IFeatureRendererRepository repository = new FeatureRendererRepository();
            return repository.ArcObjectsSDECheckDbConnWithoutAuthentication(server, instance, database, version, legendentabelle);
        }

        public ITable getArcObjectsSDETableWithoutAuthentication(string server, string instance, string database, string version, string legendentabelle)
        {
            IFeatureRendererRepository repository = new FeatureRendererRepository();
            return repository.getArcObjectsSDETableWithoutAuthentication(server, instance, database, version, legendentabelle);
        }

        public ResultType ArcObjectsSDECheckDbConnWithAuthentication(string server, string instance, string database, string user, string password, string version, string legendentabelle)
        {
            IFeatureRendererRepository repository = new FeatureRendererRepository();
            return repository.ArcObjectsSDECheckDbConnWithAuthentication(server, instance, database, user, password, version, legendentabelle);
        }

        public ITable getArcObjectsSDETableWithAuthentication(string server, string instance, string database, string user, string password, string version, string legendentabelle)
        {
            IFeatureRendererRepository repository = new FeatureRendererRepository();
            return repository.getArcObjectsSDETableWithAuthentication(server, instance, database, user, password, version, legendentabelle);
        }

    }
}
