using System.Xml.Serialization;  // Does XML serializing for a class.
using System.ComponentModel;//for the IDataError-Interface 

/// <summary>
/// FormularData test class to demonstrate how to include custom metadata attributes in a 
/// class so that it can be serialized/deserialized to/from XML.
/// </summary>

namespace FeatureRenderer.Core
{
    // Set this 'Formular_Data' class as the root node of any XML file its serialized to.
    [XmlRootAttribute("Formular_Data", Namespace = "", IsNullable = false)]
    public class FormularData// : IDataErrorInfo 
    {

        #region constructor
        /// <summary>
        /// Default constructor for this class (required for serialization).
        /// </summary>
        public FormularData()
        {
            legendTables = new System.Collections.ArrayList();
            columnNames = new System.Collections.ArrayList();
            styleFiles = new System.Collections.ArrayList();
            layerFiles = new System.Collections.ArrayList();
        }

        #endregion

        #region fields

        protected System.DateTime dateTimeValue;

        //allgemeine Attribute:
        protected string classPfadKonfigurationsdatei;

        //boolean Values:
        protected bool chkNurHintergrund;
        protected bool chkKeinUmriss;
        protected bool chkRenderingAllLayers;
        protected bool tabAccess;
        protected bool tabSde;
        protected bool tabDatenbankspalten;
        protected bool tabOptionaleDatenbankspalten;
        protected bool chkUseSqlAuthentifizierung;
        protected bool chkFormularAccess;
        protected bool chkFormularSde;
        protected bool chkFormularDirectConnect;

        //boolean values für die optionalen Datenbankspalten:
        protected bool chkLegendentextVeraendern;
        protected bool chkOptionalUeberschrift;

        //Attribute fuer die Datenbankverbindung:
        protected string sdeServername;// LegendTable;        
        protected string sdeInstanzname, sdeDatenbankname, sdeUsername, sdePasswordname, sdeVersionsname;
        protected string accessFilename;

        //Legendentabelle:
        protected System.Collections.ArrayList legendTables;
        protected string legendTable;

        //Datenbankspalten:
        protected System.Collections.ArrayList columnNames;// = new System.Collections.ArrayList();
        protected string idName;
        protected string flaechensymbolname;
        protected string liniensymbolname;
        protected string markersymbolname;
        protected string stylefilename;

        //Stylefileinformationen:
        protected System.Collections.ArrayList styleFiles;// = new System.Collections.ArrayList();
        protected string styleFile;

        //Layerinformationen:
        protected System.Collections.ArrayList layerFiles;// = new System.Collections.ArrayList();
        protected string layerFile;

        protected string trennzeichen;
        protected string feld1;
        protected string feld2;
        protected string ueberschrift;

        #endregion

        #region properties

        // Set this 'DateTimeValue' field to be an attribute of the root node.
        [XmlAttributeAttribute(DataType = "date")]
        public System.DateTime DateTimeValue
        {
            get { return dateTimeValue; }
            set { dateTimeValue = value; }
        }

        //allgemeine Attribute:
        public string ClassPfadKonfigurationsdatei
        {
            get { return classPfadKonfigurationsdatei; }
            set { classPfadKonfigurationsdatei = value; }
        }
        //boolean Values:
        public bool ChkNurHintergrund
        {
            get { return chkNurHintergrund; }
            set { chkNurHintergrund = value; }
        }
        public bool ChkKeinUmriss
        {
            get { return chkKeinUmriss; }
            set { chkKeinUmriss = value; }
        }
        public bool ChkRenderingAllLayers
        {
            get { return chkRenderingAllLayers; }
            set { chkRenderingAllLayers = value; }
        }
        public bool TabAccess
        {
            get { return tabAccess; }
            set { tabAccess = value; }
        }
        public bool TabSde
        {
            get { return tabSde; }
            set { tabSde = value; }
        }
        public bool TabDatenbankspalten
        {
            get { return tabDatenbankspalten; }
            set { tabDatenbankspalten = value; }
        }
        public bool TabOptionaleDatenbankspalten
        {
            get { return tabOptionaleDatenbankspalten; }
            set { tabOptionaleDatenbankspalten = value; }
        }
        public bool ChkUseSqlAuthentifizierung
        {
            get { return chkUseSqlAuthentifizierung; }
            set { chkUseSqlAuthentifizierung = value; }
        }
        public bool ChkFormularAccess
        {
            get { return chkFormularAccess; }
            set { chkFormularAccess = value; }
        }
        public bool ChkFormularSde
        {
            get { return chkFormularSde; }
            set { chkFormularSde = value; }
        }
        public bool ChkFormularDirectConnect
        {
            get { return chkFormularDirectConnect; }
            set { chkFormularDirectConnect = value; }
        }
        //boolean values für die optionalen Datenbankspalten:
        public bool ChkLegendentextVeraendern
        {
            get { return chkLegendentextVeraendern; }
            set { chkLegendentextVeraendern = value; }
        }
        public bool ChkOptionalUeberschrift
        {
            get { return chkOptionalUeberschrift; }
            set { chkOptionalUeberschrift = value; }
        }

        //Attribute fuer die Datenbankverbindung:
        public string SdeServername
        {
            get { return sdeServername; }
            set { sdeServername = value; }
        }
        public string SdeInstanzname
        {
            get { return sdeInstanzname; }
            set { sdeInstanzname = value; }
        }
        public string SdeDatenbankname
        {
            get { return sdeDatenbankname; }
            set { sdeDatenbankname = value; }
        }
        public string SdeUsername
        {
            get { return sdeUsername; }
            set { sdeUsername = value; }
        }
        public string SdePasswordname
        {
            get { return sdePasswordname; }
            set { sdePasswordname = value; }
        }
        public string SdeVersionsname
        {
            get { return sdeVersionsname; }
            set { sdeVersionsname = value; }
        }
        public string AccessFilename
        {
            get { return accessFilename; }
            set { accessFilename = value; }
        }

        ////Attribute für die Legendenspalten:
        // Serializes an ArrayList as a "Legend_Tables" array of XML elements of type string named "Legend_Table".
        [XmlArray("Legend_Tables"), XmlArrayItem("Legend_Table", typeof(string))]
        //public System.Collections.ArrayList LegendTables = new System.Collections.ArrayList();
        public System.Collections.ArrayList LegendTables
        {
            get { return legendTables; }
        }
        public string LegendTable
        {
            get { return legendTable; }
            set { legendTable = value; }
        }

        //weitere Attribute fuer die Tabellenspalten:
        [XmlArray("Column_Names"), XmlArrayItem("Column_Name", typeof(string))]
        public System.Collections.ArrayList ColumnNames
        {
            get { return columnNames; }
        }
        public string IdName
        {
            get { return idName; }
            set { idName = value; }
        }
        public string Flaechensymbolname
        {
            get { return flaechensymbolname; }
            set { flaechensymbolname = value; }
        }
        public string Liniensymbolname
        {
            get { return liniensymbolname; }
            set { liniensymbolname = value; }
        }
        public string Markersymbolname
        {
            get { return markersymbolname; }
            set { markersymbolname = value; }
        }
        public string Stylefilename
        {
            get { return stylefilename; }
            set { stylefilename = value; }
        }

        //Stylefile ComboBox und TextBox wieder auffüllen:
        [XmlArray("Style_Files"), XmlArrayItem("Style_File", typeof(string))]
        public System.Collections.ArrayList StyleFiles
        {
            get { return styleFiles; }
        }
        public string StyleFile
        {
            get { return styleFile; }
            set { styleFile = value; }
        }

        //cboLayername ComboBox und TextBox wieder auffüllen:
        [XmlArray("Layer_Files"), XmlArrayItem("Layer_File", typeof(string))]
        public System.Collections.ArrayList LayerFiles
        {
            get { return layerFiles; }
        }
        public string LayerFile
        {
            get { return layerFile; }
            set { layerFile = value; }
        }

        //Attribute für die Aufdruckfarben:
        public string Roc, Rom, Roy, Rok;
        public string Blc, Blm, Bly, Blk;
        public string Grc, Grm, Gry, Grk;
        public string Brc, Brm, Bry, Brk;
        public string Gac, Gam, Gay, Gak;
        public string Mac, Mam, May, Mak;
        public string Cyc, Cym, Cyy, Cyk;
        public string Gec, Gem, Gey, Gek;
        public string Orc, Orm, Ory, Ork;
        public string G6c, G6m, G6y, G6k;

        //Attribute für die optionalen Datenbankspalten:
        public string Trennzeichen
        {
            get { return trennzeichen; }
            set { trennzeichen = value; }
        }
        public string Feld1
        {
            get { return feld1; }
            set { feld1 = value; }
        }
        public string Feld2
        {
            get { return feld2; }
            set { feld2 = value; }
        }
        public string Ueberschrift
        {
            get { return ueberschrift; }
            set { ueberschrift = value; }
        }

        #endregion

    }
}
