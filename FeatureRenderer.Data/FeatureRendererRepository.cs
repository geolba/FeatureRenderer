using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.esriSystem;
using System;
using System.Windows;

using FeatureRenderer.Core.Enum;


namespace FeatureRenderer.Data
{
    public class FeatureRendererRepository : IFeatureRendererRepository
    {
        public List<string> GetTables(string file)
        {
            List<string> Tables = new List<string>();
            System.Data.DataTable tables;
            string connString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + file;

            // Verbindung erzeugen
            OleDbConnection conn = new OleDbConnection(connString);
            try
            {
                // Verbindung öffnen
                conn.Open();
                tables = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });

                for (int i = 0; i < tables.Rows.Count; i++)
                {
                    Tables.Add(tables.Rows[i][2].ToString());
                }
            }
            catch (Exception ex)
            {
                // Gegebenenfalls Fehlerbehandlung
                System.Windows.MessageBox.Show("Error in finding the DB tables for the defined database! " + ex.Message,
                     "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Verbindung schließen
                //conn.Dispose();
                conn.Close();
            }
            Tables.Sort();
            return Tables;
        }

        public List<string> GetColumnNames(string file, string table)
        {
            List<string> columns = new List<string>();
            DataTable dataSet = new DataTable();
            string connString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + file;

            if (!table.StartsWith("~TMPCLP"))
            {

                string cmdString = "SELECT * FROM " + table;

                // Verbindung erzeugen
                OleDbConnection conn = new OleDbConnection(connString);

                // Kommando festlegen
                OleDbCommand cmd = new OleDbCommand(cmdString, conn);
                try
                {
                    // Verbindung öffnen
                    conn.Open();
                    using (OleDbDataAdapter dataAdapter = new OleDbDataAdapter(cmd))
                    {
                        dataAdapter.Fill(dataSet);
                    }
                    for (int i = 0; i < dataSet.Columns.Count; i++)
                    {
                        columns.Add(dataSet.Columns[i].ColumnName);
                    }
                }
                catch (Exception ex)
                {
                    // Gegebenenfalls Fehlerbehandlung
                    MessageBox.Show("Error in finding the column names for the defined table! " + ex.Message,
                         "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    // Verbindung schließen
                    //conn.Dispose();
                    conn.Close();
                }
            }
            columns.Sort();
            return columns;
        }

        //fuer sde-server:
        public List<string> GetSqlServerTables(string server, string instance, string database, string version, bool directConnect)
        {
            //SDE connection
            // Create an ArcSDE workspace factory.
            Type factoryType = Type.GetTypeFromProgID("esriDataSourcesGDB.SdeWorkspaceFactory");
            IWorkspaceFactory workspaceFactory = (IWorkspaceFactory)Activator.CreateInstance(factoryType);

            // Create a property set and populate it with connection properties.
            IPropertySet propertySet = new PropertySet();
            if (directConnect == false)
            {
                propertySet.SetProperty("dbclient", "SQLServer");
                propertySet.SetProperty("SERVER", server);
                propertySet.SetProperty("INSTANCE", instance);
                propertySet.SetProperty("DATABASE", database);
                propertySet.SetProperty("authentication_mode", "OSA");
                propertySet.SetProperty("VESRSION", version);
            }
            else
            {
                propertySet.SetProperty("DB_CONNECTION_PROPERTIES", server);
                propertySet.SetProperty("INSTANCE", instance);
                propertySet.SetProperty("DATABASE", database);
                propertySet.SetProperty("AUTHENTICATION_MODE", "OSA");
                propertySet.SetProperty("VERSION", version); 
            }
            //Creating a new List<T> of type <string>
            List<string> Tables = new List<string>();
            try
            {
                //IWorkspace workspace = pFact.Open(pPropSet, 0);
                IWorkspace workspace = workspaceFactory.Open(propertySet, 0);
                //ISqlWorkspace sqlWorkspace = (ISqlWorkspace)workspace;
                //SqlWorkspace sqlWorkspace = new SqlWorkspace(workspace); // Use this constructor instead 
                IEnumDatasetName eDSNames = workspace.get_DatasetNames(esriDatasetType.esriDTTable);
                IDatasetName DSName = eDSNames.Next();
                while (DSName != null)
                {
                    Tables.Add(DSName.Name);
                    DSName = eDSNames.Next();
                }
            }
            catch (Exception ex)
            {
                // Gegebenenfalls Fehlerbehandlung
                MessageBox.Show("Error in finding the DB tables for the defined database! " + ex.Message,
                     "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Tables.Sort();
            return Tables;
        }

        //fuer sde-server:
        public List<string> GetPasswordSqlServerTables(string server, string instance, string database, string version, string user, string password, bool directConnect)
        {
            //SDE connection
            // Create an ArcSDE workspace factory.
            Type factoryType = Type.GetTypeFromProgID("esriDataSourcesGDB.SdeWorkspaceFactory");
            IWorkspaceFactory workspaceFactory = (IWorkspaceFactory)Activator.CreateInstance(factoryType);

            // Create a property set and populate it with connection properties.
            //IPropertySet propertySet = new PropertySetClass();
            IPropertySet propertySet = new PropertySet();
            propertySet.SetProperty("dbclient", "SQLServer");
            propertySet.SetProperty("SERVER", server);
            propertySet.SetProperty("INSTANCE", instance);
            propertySet.SetProperty("DATABASE", database);
            propertySet.SetProperty("USER", user);
            propertySet.SetProperty("PASSWORD", password);
            propertySet.SetProperty("authentication_mode", "DBMS");
            propertySet.SetProperty("VESRSION", version);

            List<string> Tables = new List<string>();
            try
            {
                IWorkspace workspace = workspaceFactory.Open(propertySet, 0);
                IEnumDatasetName eDSNames = workspace.get_DatasetNames(esriDatasetType.esriDTTable);
                IDatasetName DSName = eDSNames.Next();

                while (DSName != null)
                {
                    Tables.Add(DSName.Name);
                    DSName = eDSNames.Next();
                }
            }
            catch (Exception ex)
            {
                // Gegebenenfalls Fehlerbehandlung
                MessageBox.Show("Error in finding the DB tables for the defined database! " + ex.Message,
                     "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Tables.Sort();
            return Tables;
        }

        //fuer sde-server:
        public List<string> GetSqlServerColumnNames(string table, string server, string instance, string database, string version, bool directConnect)
        {
            //SDE connection
            Type factoryType = Type.GetTypeFromProgID("esriDataSourcesGDB.SdeWorkspaceFactory");
            IWorkspaceFactory workspaceFactory = (IWorkspaceFactory)Activator.CreateInstance(factoryType);

            // Create a property set and populate it with connection properties.
            //IPropertySet propertySet = new PropertySetClass();
            IPropertySet propertySet = new PropertySet();
            if (directConnect == false)
            {
                propertySet.SetProperty("dbclient", "SQLServer");
                propertySet.SetProperty("SERVER", server);
                propertySet.SetProperty("INSTANCE", instance);
                propertySet.SetProperty("DATABASE", database);
                propertySet.SetProperty("authentication_mode", "OSA");
                propertySet.SetProperty("VESRSION", version);
            }
            else
            {
                propertySet.SetProperty("DB_CONNECTION_PROPERTIES", server);
                propertySet.SetProperty("INSTANCE", instance);
                propertySet.SetProperty("DATABASE", database);
                propertySet.SetProperty("AUTHENTICATION_MODE", "OSA");
                propertySet.SetProperty("VERSION", version); 
            }

            List<string> Columns = new List<string>();
            try
            {
                IFeatureWorkspace pFeatws = (IFeatureWorkspace)workspaceFactory.Open(propertySet, 0);
                ITable pTable = pFeatws.OpenTable(table);
                for (int i = 0; i < pTable.Fields.FieldCount; i++)
                {
                    Columns.Add(pTable.Fields.get_Field(i).Name);
                }
            }
            catch (Exception ex)
            {
                // Gegebenenfalls Fehlerbehandlung
                MessageBox.Show("Error in finding the DB tables for the defined database! " + ex.Message,
                     "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Columns.Sort();
            return Columns;

        }

        //fuer sde-server:
        public List<string> GetPasswordSqlServerColumnNames(string table, string server, string instance, string database, string version, string user, string password, bool directConnect)
        {
            //SDE connection
            Type factoryType = Type.GetTypeFromProgID("esriDataSourcesGDB.SdeWorkspaceFactory");
            IWorkspaceFactory workspaceFactory = (IWorkspaceFactory)Activator.CreateInstance(factoryType);

            // Create a property set and populate it with connection properties.
            //IPropertySet propertySet = new PropertySetClass();
            IPropertySet propertySet = new PropertySet();
            if (directConnect == false)
            {
                propertySet.SetProperty("dbclient", "SQLServer");
                propertySet.SetProperty("SERVER", server);
                propertySet.SetProperty("INSTANCE", instance);
                propertySet.SetProperty("DATABASE", database);
                propertySet.SetProperty("authentication_mode", "OSA");
                propertySet.SetProperty("VESRSION", version);
            }
            else
            {
                propertySet.SetProperty("DB_CONNECTION_PROPERTIES", server);
                propertySet.SetProperty("INSTANCE", instance);
                propertySet.SetProperty("DATABASE", database);
                propertySet.SetProperty("AUTHENTICATION_MODE", "OSA");
                propertySet.SetProperty("VERSION", version);
            }

            List<string> Columns = new List<string>();
            try
            {
                IFeatureWorkspace pFeatws = (IFeatureWorkspace)workspaceFactory.Open(propertySet, 0);
                ITable pTable = pFeatws.OpenTable(table);
                for (int i = 0; i < pTable.Fields.FieldCount; i++)
                {
                    Columns.Add(pTable.Fields.get_Field(i).Name);
                }
            }
            catch (Exception ex)
            {
                // Gegebenenfalls Fehlerbehandlung
                MessageBox.Show("Error in finding the DB tables for the defined database! " + ex.Message,
                     "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Columns.Sort();
            return Columns;
        }

        public List<string> GetIDNumbers(string file, string table, string primaryKeyColumn)
        {
            List<string> result = new List<string>();
            string connString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + file;
            string cmdString = "SELECT * FROM " + table;

            // Verbindung erzeugen
            OleDbConnection conn = new OleDbConnection(connString);

            // Kommando festlegen
            OleDbCommand cmd = new OleDbCommand(cmdString, conn);
            OleDbDataReader reader = null;

            try
            {
                // Verbindung öffnen
                conn.Open();

                // Kommando ausführen, liefert einen DataReader
                reader = cmd.ExecuteReader();

                // Kontrollieren, ob Zeilen vorhanden
                if (reader.HasRows)
                {
                    // Lesen bis Ende
                    while (reader.Read())
                    {
                        // Autorenname auswerten - Achtung: NULL-Werte sind erlaubt, kontrollieren
                        string currentName = reader[primaryKeyColumn] as string;
                        if (currentName != null)
                        {
                            result.Add(currentName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Gegebenenfalls Fehlerbehandlung
                MessageBox.Show("Error in finding the values for the defined primary key! " + ex.Message,
                     "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Reader und Verbindung schließen
                reader.Close();
                conn.Close();
            }
            return result;
        }

        public ResultType ArcObjectsAccessCheckDatabaseConnection(string leg_tab_pfad, string legendentabelle)
        {
            IWorkspaceFactory pFact;
            IWorkspace pWorkspace;
            IFeatureWorkspace pFeatws;
            ITable pTable = null;

            //Legendentabelle für die Abfrage vorbereiten:
            pFact = new AccessWorkspaceFactory();
            try
            {
                pWorkspace = pFact.OpenFromFile(leg_tab_pfad, 0);
                pFeatws = (IFeatureWorkspace)pWorkspace;
                pTable = pFeatws.OpenTable(legendentabelle);
            }
            catch
            {
                return ResultType.ErrorInvalidDatabaseConnection;
            }

            return ResultType.DatabaseSuccess;
        }

        public ITable getArcObjectsAccessTable(string leg_tab_pfad, string legendentabelle)
        {
            IWorkspaceFactory pFact;
            IWorkspace pWorkspace;
            IFeatureWorkspace pFeatws;
            ITable pTable = null;

            //Legendentabelle für die Abfrage vorbereiten:
            pFact = new AccessWorkspaceFactory();
            try
            {
                pWorkspace = pFact.OpenFromFile(leg_tab_pfad, 0);
                pFeatws = (IFeatureWorkspace)pWorkspace;
                pTable = pFeatws.OpenTable(legendentabelle);
            }
            catch (Exception)
            {
                pTable = null;
            }
            return pTable;
        }

        public ResultType ArcObjectsSDECheckDbConnWithoutAuthentication(string server, string instance, string database, string version, string legendentabelle)
        {
             IWorkspaceFactory pFact;
            //IWorkspace pWorkspace;
            IFeatureWorkspace pFeatws;
            IPropertySet pPropSet;//für eine SDE-Verbindung!!!
            ITable pTable = null;
            //Write some Code for the SDE connection
            pPropSet = new PropertySet();
            pPropSet.SetProperty("SERVER", server);
            pPropSet.SetProperty("INSTANCE", instance);
            pPropSet.SetProperty("DATABASE", database);
            pPropSet.SetProperty("authentication_mode", "OSA");
            pPropSet.SetProperty("VESRSION", version);
            pFact = new SdeWorkspaceFactory();
            try
            {
                pFeatws = (IFeatureWorkspace)pFact.Open(pPropSet, 0);
                pTable = pFeatws.OpenTable(legendentabelle);
            }
            catch (Exception)
            {
                return ResultType.ErrorInvalidDatabaseConnection;
            }
            return ResultType.DatabaseSuccess;
        }

        public ITable getArcObjectsSDETableWithoutAuthentication(string server, string instance, string database, string version, string legendentabelle)
        {
            IWorkspaceFactory pFact;
            //IWorkspace pWorkspace;
            IFeatureWorkspace pFeatws;
            IPropertySet pPropSet;//für eine SDE-Verbindung!!!
            ITable pTable = null;
            //Write some Code for the SDE connection
            pPropSet = new PropertySet();
            pPropSet.SetProperty("SERVER", server);
            pPropSet.SetProperty("INSTANCE", instance);
            pPropSet.SetProperty("DATABASE", database);
            pPropSet.SetProperty("authentication_mode", "OSA");
            pPropSet.SetProperty("VESRSION", version);
            pFact = new SdeWorkspaceFactory();
            try
            {
                pFeatws = (IFeatureWorkspace)pFact.Open(pPropSet, 0);
                pTable = pFeatws.OpenTable(legendentabelle);
            }
            catch (Exception)
            {
                pTable = null;
            }
            return pTable;
        }

        public ResultType ArcObjectsSDECheckDbConnWithAuthentication(string server, string instance, string database, string user, string password, string version, string legendentabelle)
        {
            IWorkspaceFactory pFact;
            //IWorkspace pWorkspace;
            IFeatureWorkspace pFeatws;
            IPropertySet pPropSet;//für eine SDE-Verbindung!!!
            ITable pTable = null;
            //Write some Code for the SDE connection
            pPropSet = new PropertySet();
            pPropSet.SetProperty("SERVER", server);
            pPropSet.SetProperty("INSTANCE", instance);
            pPropSet.SetProperty("DATABASE", database);
            pPropSet.SetProperty("authentication_mode", "DBMS");
            pPropSet.SetProperty("USER", user);
            pPropSet.SetProperty("PASSWORD", password);
            pPropSet.SetProperty("VESRSION", version);
            pFact = new SdeWorkspaceFactory();
            try
            {
                pFeatws = (IFeatureWorkspace)pFact.Open(pPropSet, 0);
                pTable = pFeatws.OpenTable(legendentabelle);
            }
            catch (Exception)
            {
                return ResultType.ErrorInvalidDatabaseConnection;
            }
            return ResultType.DatabaseSuccess;
        }

        public ITable getArcObjectsSDETableWithAuthentication(string server, string instance, string database, string user, string password, string version, string legendentabelle)
        {
            IWorkspaceFactory pFact;
            //IWorkspace pWorkspace;
            IFeatureWorkspace pFeatws;
            IPropertySet pPropSet;//für eine SDE-Verbindung!!!
            ITable pTable = null;

            //Write some Code for the SDE connection
            pPropSet = new PropertySet();
            pPropSet.SetProperty("SERVER", server);
            pPropSet.SetProperty("INSTANCE", instance);
            pPropSet.SetProperty("DATABASE", database);
            pPropSet.SetProperty("authentication_mode", "DBMS");
            pPropSet.SetProperty("USER", user);
            pPropSet.SetProperty("PASSWORD", password);
            pPropSet.SetProperty("VESRSION", version);
            pFact = new SdeWorkspaceFactory();
            try
            {
                pFeatws = (IFeatureWorkspace)pFact.Open(pPropSet, 0);
                pTable = pFeatws.OpenTable(legendentabelle);
            }
            catch (Exception)
            {
                pTable = null;
            }
            return pTable;
        }

    }
}
