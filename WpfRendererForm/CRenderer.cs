using System;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.esriSystem;
//ESRI references:
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
//für die StyleGallery
using ESRI.ArcGIS.Display;
// FÜR DEN sYMBOL-eXPORT.
using ESRI.ArcGIS.Output;
//using System.Windows.Forms;
using System.Windows;

using System.Collections;
using System.Collections.Generic;

using FeatureRenderer.Business;
using FeatureRenderer.Core;
using FeatureRenderer.Core.Enum;

namespace WpfRendererForm
{
    class CRenderer
    {
        #region class members:

        private IApplication m_application;//um auf die Map zuzugreifen, wird  vom Command via Konstruktor initialisiert 
        private IMxDocument pMxDoc;

        private IActiveView pActiveView;
        private IMap pMap;
       
        private string pExportFileDir;
        private string leg_tab_pfad = String.Empty;
        private string legendentabelle = String.Empty;
        private string layername = String.Empty;
        private string leg_ID_feld = String.Empty;// = cboVerknuepfung.Text;
        private string fillsymbol_feld = String.Empty;// = cboFlaeche.Text;
        private string linesymbol_feld = String.Empty;// = cboLinie.Text;
        private string markersymbol_feld = String.Empty;// = cboMarker.Text;
        //static private string stylefile_feld = String.Empty;// 0 cboStyleFile.Text; 
        //static private string config_feld = String.Empty;// txtKonfigurationsdatei.Text; 
        ////Variablen für den Legendentext:
        private string trennzeichen_feld = String.Empty;
        private string feld1_feld = String.Empty;
        private string feld2_feld = String.Empty;
        private string ueberschrift_feld = String.Empty;

       
        FeatureRendererWindow pRendererForm;//fuer die Formulardaten!   
        //our progress dialog window
        ProgressDialog pd;     
        #endregion


        #region constructors:

        public CRenderer(FeatureRendererWindow rendererForm, IApplication application)
        {
            if (null == application)
            {
                throw new Exception("Hook helper is not initialized!");
            }

           this.pRendererForm = rendererForm;
           this.m_application = application;
           this.pMxDoc = (IMxDocument)m_application.Document;
            //Export folder for the images are getted via get-Property form the window class
           pExportFileDir = pRendererForm.SymbolDirectory + "\\"; //pRendererForm.txtSymbolDirectory.Text + "\\";  //"D:\\";

        }
        
        #endregion               

        #region delegates and events
        //our delegate used for updating the UI
        public delegate void UpdateProgressDelegate(int percentage, int recordCount, int value);

        //Create a Delegate that matches the Signature of the ProgressBar's SetValue method          
        private delegate void UpdateProgressBarDelegate(System.Windows.DependencyProperty dp, System.Object value);

       // //Test:
       // public delegate void FinishedConceptUpdateHandler();
       // public delegate void FinishedUpdateProcessHandler(ResultType result);
       // public delegate void SetProgressBarMaximumHandler(int maximum);
       // public delegate void StartedUpdateProcessHandler();

       //////////////////////////////events/////////////////////////////////////////////
        
       // //Test:
       // public event FinishedConceptUpdateHandler FinishedConceptUpdate;
       // public event FinishedUpdateProcessHandler FinishedUpdateProcess;
       // public event SetProgressBarMaximumHandler SetProgressBarMaximum;
       // public event StartedUpdateProcessHandler StartedUpdateProcess;

        #endregion

        public void rendering(object sender)
        {
            pMxDoc = (IMxDocument)m_application.Document;
            pActiveView = pMxDoc.ActiveView;
            pMap = pActiveView.FocusMap;

            System.Windows.Controls.Button rendererButton = pRendererForm.btnRenderer;
            //System.Windows.Controls.Button symbolButton = pRendererForm.btnSymbol;
            System.Windows.Controls.MenuItem symbolButton = pRendererForm.mnuSymbolExport;

            IStyleGallery pStyleGallery;
            IStyleGalleryStorage pStyleGalleryStorage;
            //zuerst wird überprüft ob der Geolba Style geladen ist: ansonsten wird abgebrochen!
            string styleSet = String.Empty;
            pStyleGallery = pMxDoc.StyleGallery;
            pStyleGalleryStorage = (IStyleGalleryStorage)pStyleGallery;
            //cboStyleFile.Items.Clear();
            for (int i = 0; i < pStyleGalleryStorage.FileCount; i++)
            {
                //cboStyleFile.Items.Add(StyleGalleryStorage.get_File(i));
                //if (pStyleGalleryStorage.get_File(i).Equals(pRendererForm.txtStyleManually.Text))
                if (pStyleGalleryStorage.get_File(i).Equals(pRendererForm.cboStyleFile.Text))
                {
                    //MessageBox.Show(pStyleGalleryStorage.get_File(i));
                    styleSet = pStyleGalleryStorage.get_File(i);
                    break;// und sofort wird anschließend die for-Schleife verlassen
                }
            }
            if (styleSet == String.Empty)
            {
                MessageBox.Show("For rendering the feature layer, a correct stylefile must be loaded!",
                   "Error in the style-finding-process", MessageBoxButton.OK, MessageBoxImage.Warning);
                //show the message in the statusbar::
                pRendererForm.staLblMessage.Content = "For rendering the feature layer, a stylefile must be loaded!" +
                     " Load a Style file therfore!";
                pRendererForm.Cursor = System.Windows.Input.Cursors.Arrow;
                return;
            }


            //show wait cursor
            pRendererForm.Cursor = System.Windows.Input.Cursors.Wait;
            ICmykColor pColorArot = null, pColorAblau = null, pColorAgruen = null, pColorAbraun = null,
                pColorGrau = null, pColorMagenta = null, pColorCyan = null, pColorBlack = null, pColorWhite = null, pColorFehler,
                pColorGelb = null, pColorOrange = null, pColorNull = null, pColorFEHLER = null;
            try
            {
                //define "Musterfarben" and error colors:          
                pColorArot = GetCMYKColor(Convert.ToInt32(pRendererForm.txtRoc.Text), Convert.ToInt32(pRendererForm.txtRom.Text), Convert.ToInt32(pRendererForm.txtRoy.Text), Convert.ToInt32(pRendererForm.txtRok.Text));// = new CmykColor();
                pColorAblau = GetCMYKColor(Convert.ToInt32(pRendererForm.txtBlc.Text), Convert.ToInt32(pRendererForm.txtBlm.Text), Convert.ToInt32(pRendererForm.txtBly.Text), Convert.ToInt32(pRendererForm.txtBlk.Text));
                pColorAgruen = GetCMYKColor(Convert.ToInt32(pRendererForm.txtGrc.Text), Convert.ToInt32(pRendererForm.txtGrm.Text), Convert.ToInt32(pRendererForm.txtGry.Text), Convert.ToInt32(pRendererForm.txtGrk.Text));
                pColorAbraun = GetCMYKColor(Convert.ToInt32(pRendererForm.txtBrc.Text), Convert.ToInt32(pRendererForm.txtBrm.Text), Convert.ToInt32(pRendererForm.txtBry.Text), Convert.ToInt32(pRendererForm.txtBrk.Text));
                pColorGrau = GetCMYKColor(Convert.ToInt32(pRendererForm.txtGac.Text), Convert.ToInt32(pRendererForm.txtGam.Text), Convert.ToInt32(pRendererForm.txtGay.Text), Convert.ToInt32(pRendererForm.txtGak.Text));
                pColorMagenta = GetCMYKColor(Convert.ToInt32(pRendererForm.txtMac.Text), Convert.ToInt32(pRendererForm.txtMam.Text), Convert.ToInt32(pRendererForm.txtMay.Text), Convert.ToInt32(pRendererForm.txtMak.Text));
                pColorCyan = GetCMYKColor(Convert.ToInt32(pRendererForm.txtCyc.Text), Convert.ToInt32(pRendererForm.txtCym.Text), Convert.ToInt32(pRendererForm.txtCyy.Text), Convert.ToInt32(pRendererForm.txtCyk.Text));
                pColorBlack = GetCMYKColor(0, 0, 0, 100);
                pColorWhite = GetCMYKColor(0, 0, 0, 0);
                pColorFehler = GetCMYKColor(0, 100, 100, 0);
                pColorGelb = GetCMYKColor(Convert.ToInt32(pRendererForm.txtGec.Text), Convert.ToInt32(pRendererForm.txtGem.Text), Convert.ToInt32(pRendererForm.txtGey.Text), Convert.ToInt32(pRendererForm.txtGek.Text));
                pColorOrange = GetCMYKColor(Convert.ToInt32(pRendererForm.txtOrc.Text), Convert.ToInt32(pRendererForm.txtOrm.Text), Convert.ToInt32(pRendererForm.txtOry.Text), Convert.ToInt32(pRendererForm.txtOrk.Text));
                pColorNull = new CmykColor();
                pColorNull.NullColor = true;
                pColorFEHLER = GetCMYKColor(0, 100, 100, 0);
            }
            catch (FormatException)
            {
                pRendererForm.staLblMessage.Content = "Falsches Eingabeformat für die Aufdruckfarben! Bitte geben Sie nur Zahlen ein!";
                pRendererForm.Cursor = System.Windows.Input.Cursors.Arrow;
                return;
            }
            catch (Exception ex)
            {
                pRendererForm.staLblMessage.Content = ex.Message;
                pRendererForm.Cursor = System.Windows.Input.Cursors.Arrow;
                return;
            }

            //initialize new variables for the input fields:
            try
            {
                leg_tab_pfad = pRendererForm.txtAccessdatenbank.Text;
                legendentabelle = pRendererForm.cboTable.Text;               
                leg_ID_feld = pRendererForm.cboVerknuepfung.Text;
                fillsymbol_feld = pRendererForm.cboFlaeche.Text;
                linesymbol_feld = pRendererForm.cboLinie.Text;
                markersymbol_feld = pRendererForm.cboMarker.Text;
                layername = pRendererForm.cboLayername.Text;//da programmatisch herausgesucht wird:
                trennzeichen_feld = pRendererForm.cboTrennzeichen.Text;
                feld1_feld = pRendererForm.cboFeld1.Text;
                feld2_feld = pRendererForm.cboFeld2.Text;
                ueberschrift_feld = pRendererForm.cboUeberschrift.Text;
            }
            catch (FormatException)
            {
                pRendererForm.staLblMessage.Content = "False format for the database fields!";
                pRendererForm.Cursor = System.Windows.Input.Cursors.Arrow;
                return;
            }

            #region Database:

            ITable pTable = null;
            IFeatureRendererController repository = new FeatureRendererController();

            if (pRendererForm.chkAccess.IsChecked == true)
            {
                ResultType resultCheckDatabaseConnection = repository.ArcObjectsAccessCheckDatabaseConnection(leg_tab_pfad, legendentabelle);
                if (resultCheckDatabaseConnection == ResultType.ErrorInvalidDatabaseConnection)
                {
                    MessageBox.Show("Please define a correct legend table!", "Error in the database finding process", MessageBoxButton.OK, MessageBoxImage.Warning);
                    pRendererForm.staLblMessage.Content = "Please define the legend table!";
                    pRendererForm.Cursor = System.Windows.Input.Cursors.Arrow;
                    //OnFinishedUpdateProcessWithError(resultCheckDatabaseConnection);
                    return;                                     
                }
                else
                {
                    pTable = repository.getArcObjectsAccessTable(leg_tab_pfad, legendentabelle);                    
                }
            }
            else //SDE
            {
                if (pRendererForm.chkAuthentifizierung.IsChecked == false)
                {
                    ResultType resultCheckDatabaseConnection = repository.ArcObjectsSDECheckDbConnWithoutAuthentication(pRendererForm.txtServer.Text, pRendererForm.txtInstance.Text, pRendererForm.txtDatabase.Text, pRendererForm.txtVersion.Text, legendentabelle);
                    if (resultCheckDatabaseConnection == ResultType.ErrorInvalidDatabaseConnection)
                    {
                        MessageBox.Show("Please define a correct legend table!", "Error in the database finding process", MessageBoxButton.OK, MessageBoxImage.Warning);
                        pRendererForm.staLblMessage.Content = "Please define the legend table!";
                        pRendererForm.Cursor = System.Windows.Input.Cursors.Arrow;
                        return;
                    }
                    else
                    {
                        pTable = repository.getArcObjectsSDETableWithoutAuthentication(pRendererForm.txtServer.Text, pRendererForm.txtInstance.Text, pRendererForm.txtDatabase.Text, pRendererForm.txtVersion.Text, legendentabelle);
                    }
                }

                else if (pRendererForm.chkAuthentifizierung.IsChecked == true)
                {
                    ResultType resultCheckDatabaseConnection = repository.ArcObjectsSDECheckDbConnWithAuthentication(pRendererForm.txtServer.Text, pRendererForm.txtInstance.Text, pRendererForm.txtDatabase.Text,
                        pRendererForm.txtUser.Text, pRendererForm.txtPassword.Text, pRendererForm.txtVersion.Text, legendentabelle);
                    
                    if (resultCheckDatabaseConnection == ResultType.ErrorInvalidDatabaseConnection)
                    {
                        MessageBox.Show("Please define a correct legend table!", "Error in the database finding process", MessageBoxButton.OK, MessageBoxImage.Warning);
                        pRendererForm.staLblMessage.Content = "Please define the legend table!";
                        pRendererForm.Cursor = System.Windows.Input.Cursors.Arrow;
                        return;
                    }
                    else
                    {
                        pTable = repository.getArcObjectsSDETableWithAuthentication(pRendererForm.txtServer.Text, pRendererForm.txtInstance.Text, pRendererForm.txtDatabase.Text,
                            pRendererForm.txtUser.Text, pRendererForm.txtPassword.Text, pRendererForm.txtVersion.Text, legendentabelle);
                    }
                }
            }
            #endregion

            # region - check database fields if they exist :

            if (pTable.FindField(leg_ID_feld) == -1)//the primaryKey field has another name:
            { 
                ErrorMessageBox2(leg_ID_feld, pRendererForm.lblVerknuepfung.Content.ToString()); return;
            }

            if (pTable.FindField(fillsymbol_feld) == -1)//Fill_symbol field has another name
            {
                ErrorMessageBox2(fillsymbol_feld, pRendererForm.lblFlaeche.Content.ToString()); return;
            }

            if (pTable.FindField(linesymbol_feld) == -1)//line_symbol has another name
            {
                ErrorMessageBox2(linesymbol_feld, pRendererForm.lblLinie.Content.ToString()); return;
            }

            if (pTable.FindField(markersymbol_feld) == -1)//marker_symbol has another name
            {
                ErrorMessageBox2(markersymbol_feld, pRendererForm.lblMarker.Content.ToString()); return;
            }

            #endregion

            IMultiLayerFillSymbol pMultiLayerFillSymbol = null;
            IMultiLayerLineSymbol pMultiLayerLineSymbol = null;
            IMultiLayerMarkerSymbol pMultiLayerMarkerSymbol = null;
            ISimpleFillSymbol pSFS;
            ICmykColor pCmyk;
            ICmykColor p2Cmyk = null;
            string Cyan1, Magenta1, Yellow1, Black1;

            //check welche Muster sich in alternativen Stylefiles befinden: für Fill, Line und Marker:
            List<string> alternativeFillSymbolList = new List<string>();
            List<string> alternativeLineSymbolList = new List<string>();
            List<string> alternativeMarkerSymbolList = new List<string>();
            List<string> annotationLayerList = new List<string>();
            int anzahlGerenderterLayer = 0;

            //int layerCount = pMap.LayerCount;
            int layerCount = pRendererForm.cboLayername.Items.Count;
            IFeatureLayer pFLayer = null;
            //Prüfen ob der Layername im ArcMap vorhanden ist:
           
            //foreach (MyLayer item in pRendererForm.cboLayername.Items)
            for (int k = 0; k < layerCount; k++)
            {
                //nur der ausgewählte Layer soll gerendet werden:
                if (pRendererForm.chkAllLayers.IsChecked == false)
                {
                    //get the layer out of the selected combobox item:
                    MyLayer selLayer =  pRendererForm.cboLayername.SelectedItem as MyLayer;
                    if (selLayer != null && selLayer.Layer != null && selLayer.Layer is IFeatureLayer)
                    {
                        pFLayer = (IFeatureLayer) selLayer.Layer;
                        layerCount = 1;//for Schleife wird abgebrochen!!! only oner layer should be rendered!
                    }

                }
                else if (pRendererForm.chkAllLayers.IsChecked == true)
                {
                    MyLayer selLayer2 = pRendererForm.cboLayername.Items.GetItemAt(k) as MyLayer;
                    if (selLayer2 != null && selLayer2.Layer != null && selLayer2.Layer is IFeatureLayer)
                    {
                        pFLayer = (IFeatureLayer)selLayer2.Layer;
                    }
                    else
                    {
                        continue;//the MyLayer object has no Layer property (at the actual index); hop to the next loop iteration!!
                    }
                }

                ////////////renderer
                ILineSymbol null_Umriss = new SimpleLineSymbol();
                null_Umriss.Width = 0;
                null_Umriss.Color.NullColor = true;

                ILineSymbol Umrisslinie;
                Umrisslinie = new SimpleLineSymbol();
                Umrisslinie.Width = 0.23;
                Umrisslinie.Color = pColorBlack;

                ILineSymbol fehlerLine = new SimpleLineSymbol();
                fehlerLine.Width = 3;
                fehlerLine.Color = pColorFEHLER;

                //areas and lines:
                stdole.IFontDisp myFont = (stdole.IFontDisp)new stdole.StdFont();
                myFont.Name = "geolba_standard";
                myFont.Size = 10;
                ICharacterMarkerSymbol fehlerMarkersymbol = new CharacterMarkerSymbol();
                fehlerMarkersymbol.Color = pColorFEHLER;
                fehlerMarkersymbol.Font = myFont;
                fehlerMarkersymbol.CharacterIndex = 99;
                fehlerMarkersymbol.Size = 16;

                //für eine Fehlerfläche -> Raute:
                ISimpleFillSymbol fehlerFillsymbol = new SimpleFillSymbol();
                fehlerFillsymbol.Color = pColorFEHLER;
                fehlerFillsymbol.Outline = fehlerLine;

                pStyleGallery = pMxDoc.StyleGallery;
                IEnumStyleGalleryItem pEnumStyleGalleryItem;
                IStyleGalleryItem pStyleGalleryItem;

                if (pTable != null)
                {
                    if (checkUniqueness(pTable, leg_ID_feld) == false)
                    {
                        //show a warning that the table is not unique:
                        MessageBox.Show("The primary key for the legend table is not unique", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("Please define the legend table", "No legend table", MessageBoxButton.OK, MessageBoxImage.Warning);
                    pRendererForm.staLblMessage.Content = "Please define the legend table";
                    return;//out of the function
                }
                

                //Get the Symbology from the layer
                IGeoFeatureLayer pGeoLayer;
                if (pFLayer.FeatureClass.FeatureType == esriFeatureType.esriFTSimple)
                {                    
                    pGeoLayer = (IGeoFeatureLayer)pFLayer;
                   
                }
                else
                {
                    //zur nächsten Zählvariable in der For-Schleife hüpfen:
                    //d.h. bei einem Layer == Abbruch:
                    annotationLayerList.Add(pFLayer.Name);
                    continue;
                }
                IUniqueValueRenderer pUVRenderer = pGeoLayer.Renderer as IUniqueValueRenderer;
                if (pUVRenderer == null && pRendererForm.chkAllLayers.IsChecked == false)//nur der Layer in der Combobox und dieser Layer hat keinen Unique Value Renderer!
                {
                    //falls dieser Layer keinen Uniquer Value Renderer besitzt, dann wird abgebrochen
                    MessageBox.Show("The unique value renderer is null! " +
                    "Please define a layer with an active 'unique value renderer'!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                //wenn durch alle Layer durchiteriert wird und der aktuelle Layer keinen Unique Value Renderer besitzt
                else if (pUVRenderer == null && pRendererForm.chkAllLayers.IsChecked == true)
                {
                    //dann wird nur der momentane Schleifendurchlauf übersprungen; auf das nächste k:
                    continue;
                }
                else
                {
                    anzahlGerenderterLayer += 1;
                }
                IQueryFilter pQueryFilter = new QueryFilter();
                ICursor tabCursor = null;
                IRow pRow = null;

                ///////////////////////////GIFExport-Class///////////////////////////////////////////////////////////////////////
                IExport pExport = new ExportGIFClass();
                IGraphicsContainer pGraphicsContainer = null;
                IActiveView pActiveView2 = (IActiveView)pMxDoc.PageLayout;
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                

                bool umrissChecked = pRendererForm.chkUmriss.IsChecked.Value;
                bool hintergrundChecked = pRendererForm.chkHintergrund.IsChecked.Value;
                
                int maxRecords = pUVRenderer.ValueCount;

                #region polygon rendering:
                string fillsym = String.Empty;
                if (pFLayer.FeatureClass.ShapeType == esriGeometryType.esriGeometryPolygon)
                {
                    if (pTable.FindField(fillsymbol_feld) == -1)
                    { 
                        ErrorMessageBox(fillsymbol_feld); return; 
                    }

                    this.pd = new ProgressDialog();                   
                    if (pRendererForm.chkAllLayers.IsChecked == false)//wenn nicht durch alle Layer durchiteriert wird -> nur dann wir der ProgressDialog angezeigt
                    {
                        //der Progress Dialog wird angezeigt!
                        pd.Topmost = true;
                        pd.Show();
                    }

                    //Configure the ProgressBar
                    pd.Progress.Minimum = 0;
                    pd.Progress.Maximum = pUVRenderer.ValueCount;
                    pd.Progress.Value = 0;
                    //Stores the value of the ProgressBar
                    double value = 0;
                    //Create a new instance of our ProgressBar Delegate that points
                    //  to the ProgressBar's SetValue method.
                    UpdateProgressBarDelegate updatePbDelegate = new UpdateProgressBarDelegate(pd.Progress.SetValue);
                    UpdateProgressDelegate update = new UpdateProgressDelegate(UpdateProgressText);
                  

                    //renderer- Werte werden durchgegangen
                    for (int i = 0; i < pUVRenderer.ValueCount; i++)
                    {
                        value = i;

                        /*Update the Value of the ProgressBar:
                        1)  Pass the "updatePbDelegate" delegate that points to the ProgressBar1.SetValue method
                        2)  Set the DispatcherPriority to "Background"
                        3)  Pass an Object() Array containing the property to update (ProgressBar.ValueProperty) and the new value */
                        pd.Dispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background,
                            new object[] { System.Windows.Controls.ProgressBar.ValueProperty, value });
                        ////Fortschrittsanzeige:
                        pd.Dispatcher.Invoke(update, Convert.ToInt32(((decimal)i / (decimal)maxRecords) * 100), maxRecords, (int)value);
                       
                        pSFS = new SimpleFillSymbol();
                        
                        int intFieldIndex = pTable.FindField(leg_ID_feld);
                        ESRI.ArcGIS.Geodatabase.IField pField = pTable.Fields.get_Field(intFieldIndex);
                        if (pField.Type == esriFieldType.esriFieldTypeInteger || pField.Type == esriFieldType.esriFieldTypeOID)
                        {
                            int intValue;
                            bool isIntValue = Int32.TryParse(pUVRenderer.get_Value(i), out intValue);
                            pQueryFilter.WhereClause = leg_ID_feld + " = " + intValue;
                        }
                        else if (pField.Type == esriFieldType.esriFieldTypeDouble)
                        {
                            double doubleValue;
                            bool isDoubleValue = Double.TryParse(pUVRenderer.get_Value(i), out doubleValue);
                            pQueryFilter.WhereClause = leg_ID_feld + " = " + doubleValue;
                        }
                        else if (pField.Type == esriFieldType.esriFieldTypeString)
                        {
                            pQueryFilter.WhereClause = leg_ID_feld + " = '" + pUVRenderer.get_Value(i) + "'";
                        }
                        
                        if (pTable.Search(pQueryFilter, false).NextRow() != null)
                        {
                            try
                            {
                                tabCursor = pTable.Search(pQueryFilter, false);
                                pRow = tabCursor.NextRow();
                                int index = pTable.FindField(fillsymbol_feld);
                                fillsym = Convert.ToString(pRow.get_Value(index));
                            }
                            catch (Exception ex)
                            {
                                pRendererForm.staLblMessage.Content = "Database Error: " + ex.Message;
                                pRendererForm.Cursor = System.Windows.Input.Cursors.Arrow;
                                return;
                            }

                            //pRow = tabCursor.NextRow();// für den nächsten Durchlauf
                            if (fillsym.Length > 3)
                            {
                                if (fillsym.Substring(0, 4) == "XXXX")//ab Position1 und 4 Stellen lang
                                {

                                    //pSFS = new SimpleFillSymbol();
                                    pSFS.Color = pColorNull;
                                }
                                else //dann sind die ersten 4 Zeichen anders belegt
                                {
                                    Cyan1 = fillsym.Substring(0, 1);
                                    Magenta1 = fillsym.Substring(1, 1);
                                    Yellow1 = fillsym.Substring(2, 1);
                                    Black1 = fillsym.Substring(3, 1);

                                    pCmyk = new CmykColor();
                                    pCmyk.Cyan = Hintergrundfarbe(Cyan1);
                                    pCmyk.Magenta = Hintergrundfarbe(Magenta1);
                                    pCmyk.Yellow = Hintergrundfarbe(Yellow1);
                                    pCmyk.Black = Hintergrundfarbe(Black1);
                                       
                                    pSFS.Color = pCmyk;
                                    pSFS.Outline = null_Umriss;
                                    //pSFS.Outline.Color.NullColor = true;
                                }
                            } //Ende: if (fillsym.Length > 3)
                            else //wenn kleiner als 3 bitte ein Fehlersymbol anzeigen: if smaller than 3 characters sho an error symbol!!!
                            {
                                if (ReferenceEquals(sender, rendererButton))
                                {
                                    MessageBoxResult dlgResult = MessageBox.Show("The length of the fillsymbol " + fillsym.ToString() + " in the legend for the layer "+ pFLayer.Name +" is shorter than 3 characters! Do you want to continue? \n" +
                                        "If you continue an error symbol will be prompted for that entry in the layer!", "Continue?", MessageBoxButton.YesNo, MessageBoxImage.Question);
                                    if (dlgResult == MessageBoxResult.Yes)
                                    {
                                        // Yes, continue: ein Fehlersymbol wird angezeigt!!!
                                        pUVRenderer.set_Symbol(pUVRenderer.get_Value(i), (ISymbol)fehlerMarkersymbol);
                                    }
                                    else if (dlgResult == MessageBoxResult.No)
                                    {                                           
                                            // No, stop; Vorgang wird abgebrochen!!!
                                            return;
                                    }                                       
                                }
                            }

                               
                            //nur Hintergrund aktiviert; oder FillSymbol enthält sowieso nur 4 Zeichen:
                            if (hintergrundChecked == true || fillsym.Length == 4)
                            {
                                // if keinUmriss is checked is true:
                                if (umrissChecked == true && pSFS != null)
                                {
                                    pSFS.Outline = null_Umriss;
                                }
                                //if keinUmriss is checked is false:
                                else if (umrissChecked == false && pSFS != null)
                                {
                                    pSFS.Outline = Umrisslinie;
                                }

                                if (ReferenceEquals(sender, rendererButton) && pSFS != null)
                                {
                                    pUVRenderer.set_Symbol(pUVRenderer.get_Value(i), (ISymbol)pSFS);
                                }
                                if (ReferenceEquals(sender, symbolButton) && pSFS != null)
                                {
                                    ExportFillSymbol("GIF", pSFS, pExport, pGraphicsContainer, pActiveView2, fillsym);
                                }                                    
                            }

                            //wenn Mustername dabei,; Option "nur Hintergrund" ist nich aktiviert:
                            if (fillsym.Length > 10 && hintergrundChecked == false)
                            {
                                //IStyleGallery pStyleGallery = pMxDoc.StyleGallery;
                                string mustername = fillsym.Substring(4, 7);//ab Position 5 und 7 Stellen lang
                                //pEnumStyleGalleryItem = pStyleGallery.get_Items("Fill Symbols", pRendererForm.cboStyleFile.Text, null);
                                pEnumStyleGalleryItem = pStyleGallery.get_Items("Fill Symbols", styleSet, null);
                                pEnumStyleGalleryItem.Reset();//neu
                                pStyleGalleryItem = pEnumStyleGalleryItem.Next();

                                //solange bis der Mustername im Style File gefunden wurde:
                                while (pStyleGalleryItem != null && pStyleGalleryItem.Name != mustername)//bricht ja dann mal ab, fall der Mustername nicht existiert
                                {
                                    //if (pStyleGalleryItem.Name == mustername)
                                    //{
                                    //    break;//while-Schleife wird beendet, falls die Namen überein stimmen!
                                    //}
                                    pStyleGalleryItem = pEnumStyleGalleryItem.Next();
                                }
                                if (pStyleGalleryItem != null && pStyleGalleryItem.Name == mustername)
                                {
                                    pMultiLayerFillSymbol = (IMultiLayerFillSymbol)pStyleGalleryItem.Item;                                      
                                }
                                else //wenn das Stylefile den definierten Musternamen nicht enthält: Suche des Musternamens in alternativen Stylefiles:
                                {                                      
                                    //alternative Stylefiles verwenden:
                                    for (int m = 0; m < pStyleGalleryStorage.FileCount; m++)
                                    {
                                        //MessageBox.Show(pStyleGalleryStorage.get_File(i));
                                        string alternativeStyleSet;
                                        alternativeStyleSet = pStyleGalleryStorage.get_File(m);
                                        pEnumStyleGalleryItem = pStyleGallery.get_Items("Fill Symbols", alternativeStyleSet, null);
                                        pEnumStyleGalleryItem.Reset();//neu
                                        pStyleGalleryItem = pEnumStyleGalleryItem.Next();

                                        //solange bis der Mustername im Style File gefunden wurde:
                                        while (pStyleGalleryItem != null && pStyleGalleryItem.Name != mustername)//bricht ja dann mal ab, falls der Mustername nicht existiert
                                        {                                           
                                                pStyleGalleryItem = pEnumStyleGalleryItem.Next();
                                        }
                                        if (pStyleGalleryItem != null && pStyleGalleryItem.Name == mustername)
                                        {
                                            //wenn die Namen übereinstimmen, dann wird das MultiLayerFillSymbol zugewiesen!!!
                                            pMultiLayerFillSymbol = (IMultiLayerFillSymbol)pStyleGalleryItem.Item;
                                            alternativeFillSymbolList.Add(pUVRenderer.get_Value(i).ToString() + " - fill symbol " + mustername + " was found in the alternative Stylefile: " + alternativeStyleSet);
                                            break;//for-Schleife wird beendet, falls die Namen überein stimmen!
                                        }
                                          
                                        //for-Schleife zu - wo durch alle alternativen Stylefile durch iteriert wird!!11
                                    }//for-Schleife zu - wo durch alle alternativen Stylefiles durch iteriert wird!!!
                                    if (pStyleGalleryItem == null) // && pStyleGalleryItem.Name != mustername)
                                    {
                                        MessageBox.Show("Error at ID "+pUVRenderer.get_Value(i).ToString() + " - Also the alternative stylefiles don't contain the fillSymbol pattern name " + mustername +
                                            " which is defined in the legend table! Please add a correct stylefile to the StyleGalleryStorage!",
                                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                        pRendererForm.Cursor = System.Windows.Input.Cursors.Arrow;
                                        pd.Close();
                                        //Refresh the TOC
                                        this.pMxDoc.UpdateContents();
                                        // Rfresh the map
                                        this.pActiveView.Refresh();
                                        //exit the application:
                                        return;
                                    }
                                }
                                //jeder fillSymbolLayer hat nie eine Umrisskontur:
                                //könnte jedoch fälschlicherweiese dabei sein. deswegen null setzen:
                                pMultiLayerFillSymbol.Outline = null_Umriss;

                                //´wenn eine Musterfarbe definiert wird:
                                if (fillsym.Length > 13)
                                {
                                    string musterfarbe = fillsym.Substring(11, 3);//ab Position 12 und 3 Stellen lang
                                    if (musterfarbe == "ROT" || musterfarbe == "RED")
                                    { p2Cmyk = pColorArot; }
                                    else if (musterfarbe == "BLA" || musterfarbe == "BLU")
                                    { p2Cmyk = pColorAblau; }
                                    else if (musterfarbe == "GRN")
                                    { p2Cmyk = pColorAgruen; }
                                    else if (musterfarbe == "BRN")
                                    { p2Cmyk = pColorAbraun; }
                                    else if (musterfarbe == "GRA" || musterfarbe == "GRY")
                                    { p2Cmyk = pColorGrau; }
                                    else if (musterfarbe == "CYN")
                                    { p2Cmyk = pColorCyan; }
                                    else if (musterfarbe == "MGT")
                                    { p2Cmyk = pColorMagenta; }
                                    else if (musterfarbe == "GLB" || musterfarbe == "YLW")
                                    { p2Cmyk = pColorGelb; }
                                    else if (musterfarbe == "ORA")
                                    { p2Cmyk = pColorOrange; }
                                    else if (musterfarbe == "BLK")
                                    { p2Cmyk = pColorBlack; }
                                    else if (musterfarbe == "WHT")
                                    { p2Cmyk = pColorWhite; }
                                    else
                                    {
                                        MessageBox.Show("The color '" + musterfarbe + "' out of the fillsymbol '" + fillsym + "' is unknown! " +
                                        "Please check your legend table!", "Missing color", MessageBoxButton.OK, MessageBoxImage.Error);
                                        pd.Close();
                                        // Rfresh the map
                                        this.pActiveView.Refresh();
                                        return;
                                            
                                    }
                                    for (int j = 0; j < pMultiLayerFillSymbol.LayerCount; j++)
                                    {
                                        //Fill Color des Musternamens zuweisen:
                                        pMultiLayerFillSymbol.get_Layer(j).Color = p2Cmyk;
                                    }
                                }//Musterfarbe fertig definiert 

                                //Beim Rendern von mehrschichtigen Symbolen soll die Outlinekontur nur einmal vorkommen:
                                //Deswegen wird am ersten Layer die Umrisslinie auf null gestellt:
                                //pSFS.Outline = null_Umriss;
                                pMultiLayerFillSymbol.AddLayer(pSFS);
                                //wenn Hintergrund nicht transparent, dann  nach unten im pMultiLayerFillSymbol:
                                if (fillsym.Substring(0, 4) != "XXXX")
                                {
                                    pMultiLayerFillSymbol.MoveLayer(pSFS, pMultiLayerFillSymbol.LayerCount - 1);
                                }

                                //Wenn der MultFillLayer einen transparenten Hintergrundlayer besitzt,
                                // dann soll dieser transparente Hintergrundlayer verschwinden:
                                if (fillsym.Substring(0, 4) == "XXXX")
                                {
                                    pMultiLayerFillSymbol.DeleteLayer(pSFS);
                                }

                                if (umrissChecked == true)
                                {
                                    pMultiLayerFillSymbol.Outline = null_Umriss;
                                }
                                else
                                {
                                    pMultiLayerFillSymbol.Outline = Umrisslinie;
                                }

                                if (ReferenceEquals(sender, symbolButton))
                                {
                                    //pFillSymbol = pMultiLayerFillSymbol;
                                    ExportFillSymbol("GIF", pMultiLayerFillSymbol, pExport, pGraphicsContainer, pActiveView2, fillsym);

                                }
                                if (ReferenceEquals(sender, rendererButton))
                                {
                                    pUVRenderer.set_Symbol(pUVRenderer.get_Value(i), (ISymbol)pMultiLayerFillSymbol);
                                    GC.Collect();
                                }
                            }
                            //Legendentext anpassen: Checkbox "Legendentext verändern":
                            if (pRendererForm.checkBox1.IsChecked == true)
                            {
                                string leg_text1 = string.Empty, leg_text2 = string.Empty, heading = string.Empty;
                                if (feld1_feld != String.Empty)
                                {
                                    leg_text1 = Convert.ToString(pRow.get_Value(pTable.FindField(feld1_feld)));
                                }
                                if (feld2_feld != String.Empty)
                                {
                                    leg_text2 = Convert.ToString(pRow.get_Value(pTable.FindField(feld2_feld)));
                                }
                                if (ueberschrift_feld != String.Empty)
                                {
                                    heading = Convert.ToString(pRow.get_Value(pTable.FindField(ueberschrift_feld)));
                                }
                                string pUvValue = pUVRenderer.get_Value(i);//dass i von der for-Schleife!
                                if (trennzeichen_feld == "whitespace")
                                {
                                    trennzeichen_feld = String.Empty;
                                }

                                if (pRendererForm.chkUeberschrift.IsChecked == true)
                                {
                                    pUVRenderer.set_Heading(pUvValue, heading);
                                }

                                if (leg_text1 != string.Empty && leg_text2 != string.Empty)
                                {
                                    pUVRenderer.set_Label(pUvValue, (leg_text1 + " " + trennzeichen_feld + " " + leg_text2));
                                }
                                else if (leg_text1 == string.Empty && leg_text2 == string.Empty)
                                {
                                    pUVRenderer.set_Label(pUvValue, pUvValue);
                                }
                                else if (leg_text1 == string.Empty && leg_text2 != string.Empty)
                                {
                                    pUVRenderer.set_Label(pUvValue, (leg_text2));
                                }
                                else if (leg_text1 != string.Empty && leg_text2 == string.Empty)
                                {
                                    pUVRenderer.set_Label(pUvValue, (leg_text1));
                                }
                               
                            } //Legendentext anpassen Ende!!!                              
                            //System.Runtime.InteropServices.Marshal.ReleaseComObject(tabCursor);//, oder in C# Aufruf des Garbage-Collectors:
                            GC.Collect();
                              

                        }// if WhereClause = leg_ID_feld + " = '" + pUVRenderer.get_Value(i) zugemacht -< get nextRow()
                        else //wenn der Renderer mehr Werte besitzt als die Legedentabelle:
                        {
                            //System.Runtime.InteropServices.Marshal.ReleaseComObject(tabCursor);//, oder in C# Aufruf des Garbage-Collectors:
                            GC.Collect();
                            if (ReferenceEquals(sender, rendererButton))
                            {
                                pUVRenderer.set_Symbol(pUVRenderer.get_Value(i), (ISymbol)fehlerMarkersymbol);//funktioniert, dann kommt error-schriftzug
                            }

                        }

                        if (pd.IsEnabled == false)
                        {
                            pRendererForm.staLblMessage.Content = "The rendering process has been canceled!";
                            //return;
                            break;
                        }

                    }//for loop of the feature renderer; 
                }//if Polygon;
                #endregion

                #region line rendering
                //für Linien
                if (pFLayer.FeatureClass.ShapeType == esriGeometryType.esriGeometryPolyline)
                {
                    if (pTable.FindField(linesymbol_feld) == -1)
                    { ErrorMessageBox(linesymbol_feld); return; }

                    this.pd = new ProgressDialog();
                    if (pRendererForm.chkAllLayers.IsChecked == false)//wenn nicht durch alle Layer durchiteriert wird -> nur dann wir der ProgressDialog angezeigt
                    {
                        pd.Topmost = true;
                        pd.Show();
                    }
                    //Configure the ProgressBar
                    pd.Progress.Minimum = 0;
                    pd.Progress.Maximum = pUVRenderer.ValueCount;
                    pd.Progress.Value = 0;
                    //Stores the value of the ProgressBar
                    double value = 0;
                    //Create a new instance of our ProgressBar Delegate that points
                    //  to the ProgressBar's SetValue method.
                    UpdateProgressBarDelegate updatePbDelegate = new UpdateProgressBarDelegate(pd.Progress.SetValue);
                    UpdateProgressDelegate update = new UpdateProgressDelegate(UpdateProgressText);

                    string linesym = String.Empty;
                    for (int i = 0; i < pUVRenderer.ValueCount; i++)// 1
                    {
                        value = i;
                        ///*Update the Value of the ProgressBar:
                        //1)  Pass the "updatePbDelegate" delegate that points to the ProgressBar1.SetValue method
                        //2)  Set the DispatcherPriority to "Background"
                        //3)  Pass an Object() Array containing the property to update (ProgressBar.ValueProperty) and the new value */
                        pd.Dispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background,
                            new object[] { System.Windows.Controls.ProgressBar.ValueProperty, value });
                        ////Fortschrittsanzeige:
                        pd.Dispatcher.Invoke(update, Convert.ToInt32(((decimal)i / (decimal)maxRecords) * 100), maxRecords, (int)value);          

                        
                        int intFieldIndex = pTable.FindField(leg_ID_feld);
                        ESRI.ArcGIS.Geodatabase.IField pField = pTable.Fields.get_Field(intFieldIndex);
                        if (pField.Type == esriFieldType.esriFieldTypeInteger)
                        {
                            int intValue;
                            bool isIntValue = Int32.TryParse(pUVRenderer.get_Value(i), out intValue);
                            pQueryFilter.WhereClause = leg_ID_feld + " = " + intValue;
                        }
                        else if (pField.Type == esriFieldType.esriFieldTypeDouble)
                        {
                            double doubleValue;
                            bool isDoubleValue = Double.TryParse(pUVRenderer.get_Value(i), out doubleValue);
                            pQueryFilter.WhereClause = leg_ID_feld + " = " + doubleValue;
                        }
                        else if (pField.Type == esriFieldType.esriFieldTypeString)
                        {
                            pQueryFilter.WhereClause = leg_ID_feld + " = '" + pUVRenderer.get_Value(i) + "'";
                        }
                       
                        if (pTable.Search(pQueryFilter, false).NextRow() != null)//2
                        {
                            try
                            {
                                tabCursor = pTable.Search(pQueryFilter, false);
                                pRow = tabCursor.NextRow();            
                                int index = pTable.FindField(linesymbol_feld);
                                linesym = Convert.ToString(pRow.get_Value(index));
                            }
                            catch (Exception ex)
                            {
                                pRendererForm.staLblMessage.Content = ex.Message;
                                pRendererForm.Cursor = System.Windows.Input.Cursors.Arrow;
                                return;
                            }

                            if (linesym.Length > 6) //3 
                            {
                                //Linientyp ermitteln und zuordnen
                                string mustername = linesym.Substring(0, 7);//ab Position 1 und 7 Stellen lang
                                //pEnumStyleGalleryItem = pStyleGallery.get_Items("Line Symbols", pRendererForm.cboStyleFile.Text, null);
                                pEnumStyleGalleryItem = pStyleGallery.get_Items("Line Symbols", styleSet, null);
                                pEnumStyleGalleryItem.Reset();
                                pStyleGalleryItem = pEnumStyleGalleryItem.Next();

                                while (pStyleGalleryItem != null && pStyleGalleryItem.Name != mustername)//bricht ja dann mal ab, fall der Mustername nicht existiert
                                {
                                    //if (pStyleGalleryItem.Name == mustername)
                                    //{
                                    //    break;//while-Schleife wird beendet, falls die Namen überein stimmen!
                                    //}
                                    pStyleGalleryItem = pEnumStyleGalleryItem.Next();
                                }

                                if (pStyleGalleryItem != null && pStyleGalleryItem.Name == mustername)
                                {
                                    pMultiLayerLineSymbol = (IMultiLayerLineSymbol)pStyleGalleryItem.Item;
                                }
                                else //wenn das Stylefile den definierten Musternamen nicht enthält: Suche des Musternamens in alternativen Stylefiles:
                                {
                                    //alternative Stylefiles verwenden:
                                    for (int m = 0; m < pStyleGalleryStorage.FileCount; m++)
                                    {
                                        //MessageBox.Show(pStyleGalleryStorage.get_File(i));
                                        string alternativeStyleSet;
                                        alternativeStyleSet = pStyleGalleryStorage.get_File(m);
                                        pEnumStyleGalleryItem = pStyleGallery.get_Items("Line Symbols", alternativeStyleSet, null);
                                        pEnumStyleGalleryItem.Reset();//neu
                                        pStyleGalleryItem = pEnumStyleGalleryItem.Next();

                                        //solange bis der Mustername im Style File gefunden wurde:
                                        while (pStyleGalleryItem != null && pStyleGalleryItem.Name != mustername)//bricht ja dann mal ab, fall der Mustername nicht existiert
                                        {
                                            //if (pStyleGalleryItem.Name == mustername)
                                            //{
                                            //    break;//while-Schleife wird beendet, falls die Namen überein stimmen!
                                            //}
                                            pStyleGalleryItem = pEnumStyleGalleryItem.Next();
                                        }
                                        if (pStyleGalleryItem != null && pStyleGalleryItem.Name == mustername)
                                        {
                                            //wenn die Namen übereinstimmen, dann wird das MultiLayerLineSymbol aus dem alternativen Stylefile zugewiesen!!!
                                            pMultiLayerLineSymbol = (IMultiLayerLineSymbol)pStyleGalleryItem.Item;
                                            alternativeLineSymbolList.Add(pUVRenderer.get_Value(i).ToString() + " - line symbol " + mustername + " was found in the alternative Stylefile: " + alternativeStyleSet);
                                            break;//for-Schleife wird beendet, falls die Namen überein stimmen!
                                        }

                                    }//for-Schleife zu - wo durch alle alternativen Stylefiles durch iteriert wird!!!
                                    if (pStyleGalleryItem == null)
                                    {
                                        MessageBox.Show("Error at ID " + pUVRenderer.get_Value(i).ToString() + " - Also the alternative stylefiles don't contain the lineSymbol pattern name " + mustername +
                                            " which is defined in the legend table! Please add a correct stylefile to the StyleGalleryStorage!",
                                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                        pRendererForm.Cursor = System.Windows.Input.Cursors.Arrow;
                                        pd.Close();
                                        //Refresh the TOC
                                        this.pMxDoc.UpdateContents();
                                        // Rfresh the map
                                        this.pActiveView.Refresh();
                                        //exit the application:
                                        return;
                                    }
                                }                                    

                                //wenn eine Musterfarbe definiert wird:
                                if (linesym.Length > 9)//4
                                {
                                    string musterfarbe = linesym.Substring(7, 3);//ab Position 8 und 3 Stellen lang
                                    if (musterfarbe == "ROT" || musterfarbe == "RED")
                                    { p2Cmyk = pColorArot; }
                                    else if (musterfarbe == "BLA" || musterfarbe == "BLU")
                                    { p2Cmyk = pColorAblau; }
                                    else if (musterfarbe == "GRN")
                                    { p2Cmyk = pColorAgruen; }
                                    else if (musterfarbe == "BRN")
                                    { p2Cmyk = pColorAbraun; }
                                    else if (musterfarbe == "GRA" || musterfarbe == "GRY")
                                    { p2Cmyk = pColorGrau; }
                                    else if (musterfarbe == "CYN")
                                    { p2Cmyk = pColorCyan; }
                                    else if (musterfarbe == "MGT")
                                    { p2Cmyk = pColorMagenta; }
                                    else if (musterfarbe == "GLB" || musterfarbe == "YLW")
                                    { p2Cmyk = pColorGelb; }
                                    else if (musterfarbe == "ORA")
                                    { p2Cmyk = pColorOrange; }
                                    else if (musterfarbe == "BLK")
                                    { p2Cmyk = pColorBlack; }
                                    else if (musterfarbe == "WHT")
                                    { p2Cmyk = pColorWhite; }
                                    else
                                    {                                        
                                        MessageBox.Show("The color '" + musterfarbe + "' out of the linesymbol '" + linesym + "' is unknown! " +
                                        "Please check your legend table!", "Missing color", MessageBoxButton.OK, MessageBoxImage.Error);
                                        pd.Close();
                                        // Rfresh the map
                                        this.pActiveView.Refresh();
                                        return;
                                    }
                                    for (int j = 0; j < pMultiLayerLineSymbol.LayerCount; j++)
                                    {
                                        //Fill Color des Musternamens zuweisen:
                                        pMultiLayerLineSymbol.get_Layer(j).Color = p2Cmyk;
                                    }
                                    //}//Musterfarbe fertig definiert
                                }

                                //falls der Renderer gedrückt wird:
                                if (ReferenceEquals(sender, pRendererForm.btnRenderer))
                                {
                                    pUVRenderer.set_Symbol(pUVRenderer.get_Value(i), (ISymbol)pMultiLayerLineSymbol);
                                }
                                //neu 27.04.2011: falls der SymbolExport aktiviert wird
                                if (ReferenceEquals(sender, symbolButton))
                                {
                                    //pFillSymbol = pMultiLayerFillSymbol;
                                    ExportLineSymbol("GIF", pMultiLayerLineSymbol, pExport, pGraphicsContainer, pActiveView2, linesym);
                                }
                                   
                            }//3 zumachen -> if (linesym.Length > 6); wenn kürzer als 6 bitte ein Fehlersymbol anzeigen!!!
                            else
                            {
                                MessageBoxResult dlgResult = MessageBox.Show("The length of the linesymbol " + linesym.ToString() + " in the legend for the layer " + pFLayer.Name + " is shorter than 6 characters! Do you want to continue? \n" +
                                        "If you continue an error symbol will be prompted for that entry in the layer!", "Continue?", MessageBoxButton.YesNo, MessageBoxImage.Question);
                                if (dlgResult == MessageBoxResult.Yes)
                                {
                                    // Yes, continue: ein Fehlersymbol wird angezeigt!!!
                                    pUVRenderer.set_Symbol(pUVRenderer.get_Value(i), (ISymbol)fehlerMarkersymbol);
                                }
                                else if (dlgResult == MessageBoxResult.No)
                                {
                                    //gesamte Methode wird abgebrochen
                                    return;
                                }         
                            }
                            //Legendentext anpassen: Checkbox "Legendentext verändern":
                            if (pRendererForm.checkBox1.IsChecked == true)
                            {
                                string leg_text1 = string.Empty, leg_text2 = string.Empty, heading = string.Empty;
                                if (feld1_feld != String.Empty)
                                {
                                    leg_text1 = Convert.ToString(pRow.get_Value(pTable.FindField(feld1_feld)));
                                }
                                if (feld2_feld != String.Empty)
                                {
                                    leg_text2 = Convert.ToString(pRow.get_Value(pTable.FindField(feld2_feld)));
                                }
                                if (ueberschrift_feld != String.Empty)
                                {
                                    heading = Convert.ToString(pRow.get_Value(pTable.FindField(ueberschrift_feld)));
                                }
                                string pUvValue = pUVRenderer.get_Value(i);//dass i von der for-Schleife!
                                if (trennzeichen_feld == "whitespace")
                                {
                                    trennzeichen_feld = String.Empty;
                                }

                                if (pRendererForm.chkUeberschrift.IsChecked == true)
                                {
                                    pUVRenderer.set_Heading(pUvValue, heading);
                                }

                                if (leg_text1 != string.Empty && leg_text2 != string.Empty)
                                {
                                    pUVRenderer.set_Label(pUvValue, (leg_text1 + " " + trennzeichen_feld + " " + leg_text2));
                                }
                                else if (leg_text1 == string.Empty && leg_text2 == string.Empty)
                                {
                                    pUVRenderer.set_Label(pUvValue, pUvValue);
                                }
                                else if (leg_text1 == string.Empty && leg_text2 != string.Empty)
                                {
                                    pUVRenderer.set_Label(pUvValue, (leg_text2));
                                }
                                else if (leg_text1 != string.Empty && leg_text2 == string.Empty)
                                {
                                    pUVRenderer.set_Label(pUvValue, (leg_text1));
                                }
                            } //Legendentext anpassen Ende!!!   
                            //System.Runtime.InteropServices.Marshal.ReleaseComObject(tabCursor);//, oder in C# Aufruf des Garbage-Collectors:
                            GC.Collect();
                        }
                        else //if the UniqueValueRenderer has more values than the legend table:
                        {
                            //System.Runtime.InteropServices.Marshal.ReleaseComObject(tabCursor);//, oder in C# Aufruf des Garbage-Collectors:
                            GC.Collect();
                            pUVRenderer.set_Symbol(pUVRenderer.get_Value(i), (ISymbol)fehlerMarkersymbol);
                        }          
                        if (pd.IsEnabled == false)
                        {
                            //pd.Close();
                            pRendererForm.staLblMessage.Content = "The rendering process has been canceled!";
                            return;
                        }

                    }//for loop through all renderer values
                }// if line!!
                #endregion

                #region Point rendering
                //für Punkte
                if (pFLayer.FeatureClass.ShapeType == esriGeometryType.esriGeometryPoint)
                {
                    if (pTable.FindField(markersymbol_feld) == -1)
                    { ErrorMessageBox(markersymbol_feld); return; }

                    this.pd = new ProgressDialog();
                    if (pRendererForm.chkAllLayers.IsChecked == false)//wenn nicht durch alle Layer durchiteriert wird -> nur dann wir der ProgressDialog angezeigt
                    {
                        pd.Topmost = true;
                        pd.Show();
                    }
                    //Configure the ProgressBar
                    pd.Progress.Minimum = 0;
                    pd.Progress.Maximum = pUVRenderer.ValueCount;
                    pd.Progress.Value = 0;
                    //Stores the value of the ProgressBar
                    double value = 0;
                    //Create a new instance of our ProgressBar Delegate that points
                    //  to the ProgressBar's SetValue method.
                    UpdateProgressBarDelegate updatePbDelegate = new UpdateProgressBarDelegate(pd.Progress.SetValue);
                    UpdateProgressDelegate update = new UpdateProgressDelegate(UpdateProgressText);

                    string markersym = String.Empty;
                    for (int i = 0; i < pUVRenderer.ValueCount; i++)// 1
                    {
                        value = i;
                        /*Update the Value of the ProgressBar:
                        1)  Pass the "updatePbDelegate" delegate that points to the ProgressBar1.SetValue method
                        2)  Set the DispatcherPriority to "Background"
                        3)  Pass an Object() Array containing the property to update (ProgressBar.ValueProperty) and the new value */
                        pd.Dispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background,
                            new object[] { System.Windows.Controls.ProgressBar.ValueProperty, value });
                        ////Fortschrittsanzeige:
                        pd.Dispatcher.Invoke(update, Convert.ToInt32(((decimal)i / (decimal)maxRecords) * 100), maxRecords, (int)value);

                        int intFieldIndex = pTable.FindField(leg_ID_feld);
                        ESRI.ArcGIS.Geodatabase.IField pField = pTable.Fields.get_Field(intFieldIndex);
                        if (pField.Type == esriFieldType.esriFieldTypeInteger)
                        {
                            int intValue;
                            bool isIntValue = Int32.TryParse(pUVRenderer.get_Value(i), out intValue);
                            pQueryFilter.WhereClause = leg_ID_feld + " = " + intValue;
                        }
                        else if (pField.Type == esriFieldType.esriFieldTypeDouble)
                        {
                            double doubleValue;
                            bool isDoubleValue = Double.TryParse(pUVRenderer.get_Value(i), out doubleValue);
                            pQueryFilter.WhereClause = leg_ID_feld + " = " + doubleValue;
                        }
                        else if (pField.Type == esriFieldType.esriFieldTypeString)
                        {
                            pQueryFilter.WhereClause = leg_ID_feld + " = '" + pUVRenderer.get_Value(i) + "'";
                        }
                       
                        if (pTable.Search(pQueryFilter, false).NextRow() != null)
                        {
                            try
                            {
                                tabCursor = pTable.Search(pQueryFilter, false);
                                pRow = tabCursor.NextRow();                
                                int index = pTable.FindField(markersymbol_feld);
                                markersym = Convert.ToString(pRow.get_Value(index));
                            }
                            catch (Exception ex)
                            {
                                pRendererForm.staLblMessage.Content = ex.Message;
                                pRendererForm.Cursor = System.Windows.Input.Cursors.Arrow;
                                return;
                            }

                            if (markersym.Length > 6)
                            {
                                //Markertyp ermitteln und zuornden:
                                string mustername = markersym.Substring(0, 7);//ab Position 1 und 7 Stellen lang
                                //pEnumStyleGalleryItem = pStyleGallery.get_Items("Marker Symbols", pRendererForm.cboStyleFile.Text, null);
                                pEnumStyleGalleryItem = pStyleGallery.get_Items("Marker Symbols", styleSet, null);
                                pEnumStyleGalleryItem.Reset();
                                pStyleGalleryItem = pEnumStyleGalleryItem.Next();

                                while (pStyleGalleryItem != null && pStyleGalleryItem.Name != mustername)//bricht ja dann mal ab, fall der Mustername nicht existiert
                                {
                                    //if (pStyleGalleryItem.Name == mustername)
                                    //{
                                    //    break;//while-Schleife wird beendet, falls die Namen überein stimmen!
                                    //}
                                    pStyleGalleryItem = pEnumStyleGalleryItem.Next();
                                }

                                if (pStyleGalleryItem != null && pStyleGalleryItem.Name == mustername)
                                {
                                    pMultiLayerMarkerSymbol = (IMultiLayerMarkerSymbol)pStyleGalleryItem.Item;
                                }
                                else //wenn das Stylefile den definierten Musternamen nicht enthält: Suche des Musternamens in alternativen Stylefiles:
                                {
                                    //alternative Stylefiles verwenden:
                                    for (int m = 0; m < pStyleGalleryStorage.FileCount; m++)
                                    {
                                        //MessageBox.Show(pStyleGalleryStorage.get_File(i));
                                        string alternativeStyleSet;
                                        alternativeStyleSet = pStyleGalleryStorage.get_File(m);
                                        pEnumStyleGalleryItem = pStyleGallery.get_Items("Marker Symbols", alternativeStyleSet, null);
                                        pEnumStyleGalleryItem.Reset();//neu
                                        pStyleGalleryItem = pEnumStyleGalleryItem.Next();

                                        //solange bis der Mustername im Style File gefunden wurde:
                                        while (pStyleGalleryItem != null && pStyleGalleryItem.Name != mustername)//bricht ja dann mal ab, fall der Mustername nicht existiert
                                        {                                            
                                            pStyleGalleryItem = pEnumStyleGalleryItem.Next();
                                        }
                                        if (pStyleGalleryItem != null && pStyleGalleryItem.Name == mustername)
                                        {
                                            //wenn die Namen übereinstimmen, dann wird das MultiLayerMarkerSymbol aus dem alternativen Stylefile zugewiesen!!!
                                            pMultiLayerMarkerSymbol = (IMultiLayerMarkerSymbol)pStyleGalleryItem.Item;
                                            alternativeMarkerSymbolList.Add(pUVRenderer.get_Value(i).ToString() + " - marker symbol " + mustername + " was found in the alternative Stylefile: " + alternativeStyleSet);
                                            break;//for-Schleife wird beendet, falls die Namen überein stimmen!
                                        }

                                    }//for-Schleife zu - wo durch alle alternativen Stylefiles durch iteriert wird!!!
                                    if (pStyleGalleryItem == null)
                                    {
                                        MessageBox.Show("Error at ID " + pUVRenderer.get_Value(i).ToString() + " - Also the alternative stylefiles don't contain the markerSymbol pattern name " + mustername +
                                            " which is defined in the legend table! Please add a correct stylefile to the StyleGalleryStorage!",
                                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                        pRendererForm.Cursor = System.Windows.Input.Cursors.Arrow;
                                        pd.Close();
                                        //Refresh the TOC
                                        this.pMxDoc.UpdateContents();
                                        // Rfresh the map
                                        this.pActiveView.Refresh();
                                        //exit the application:
                                        return;
                                    }
                                }                                 

                                //Musterfarbe
                                if (markersym.Length > 9)//4
                                {
                                    string musterfarbe = markersym.Substring(7, 3);//ab Position 8 und 3 Stellen lang
                                    if (musterfarbe == "ROT" || musterfarbe == "RED")
                                    { p2Cmyk = pColorArot; }
                                    else if (musterfarbe == "BLA" || musterfarbe == "BLU")
                                    { p2Cmyk = pColorAblau; }
                                    else if (musterfarbe == "GRN")
                                    { p2Cmyk = pColorAgruen; }
                                    else if (musterfarbe == "BRN")
                                    { p2Cmyk = pColorAbraun; }
                                    else if (musterfarbe == "GRA" || musterfarbe == "GRY")
                                    { p2Cmyk = pColorGrau; }
                                    else if (musterfarbe == "CYN")
                                    { p2Cmyk = pColorCyan; }
                                    else if (musterfarbe == "MGT")
                                    { p2Cmyk = pColorMagenta; }
                                    else if (musterfarbe == "GLB" || musterfarbe == "YLW")
                                    { p2Cmyk = pColorGelb; }
                                    else if (musterfarbe == "ORA")
                                    { p2Cmyk = pColorOrange; }
                                    else if (musterfarbe == "BLK")
                                    { p2Cmyk = pColorBlack; }
                                    else if (musterfarbe == "WHT")
                                    { p2Cmyk = pColorWhite; }
                                    else
                                    {
                                        //MessageBox.Show("Ther is one color which is not defined in the legend table", "Missing color", MessageBoxButton.OK, MessageBoxImage.Warning);
                                        MessageBox.Show("The color '" + musterfarbe + "' out of the linesymbol '" + markersym + "' is unknown! " +
                                        "Please check your legend table!", "Missing color", MessageBoxButton.OK, MessageBoxImage.Error);
                                        pd.Close();
                                        // Rfresh the map
                                        this.pActiveView.Refresh();
                                        return;
                                    }
                                    for (int j = 0; j < pMultiLayerMarkerSymbol.LayerCount; j++)
                                    {
                                        //Fill Color des Musternamens zuweisen:
                                        pMultiLayerMarkerSymbol.get_Layer(j).Color = p2Cmyk;
                                    }
                                }

                                //if the renderer is pressed:
                                if (ReferenceEquals(sender, pRendererForm.btnRenderer))
                                {
                                    pUVRenderer.set_Symbol(pUVRenderer.get_Value(i), (ISymbol)pMultiLayerMarkerSymbol);
                                }
                                //if the symbol export is activated:
                                if (ReferenceEquals(sender, symbolButton))
                                {
                                    //pFillSymbol = pMultiLayerFillSymbol;
                                    ExportMarkerSymbol("GIF", pMultiLayerMarkerSymbol, pExport, pGraphicsContainer, pActiveView2, markersym);
                                }

                            }//3 wieder zu ->  if !(markersym.Length > 6); wenn kürzer als 6 Zeichen bitte ein Fehlersymbol anzeigen!!!
                            else
                            {
                                MessageBoxResult dlgResult = MessageBox.Show("The length of the markersymbol " + markersym.ToString() + " in the legend for the layer " + pFLayer.Name + " is shorter than 6 characters! Do you want to continue? \n" +
                                        "If you continue an error symbol will be prompted for that entry in the layer!", "Continue?", MessageBoxButton.YesNo, MessageBoxImage.Question);
                                if (dlgResult == MessageBoxResult.Yes)
                                {
                                    // Yes, continue: ein Fehlersymbol wird angezeigt!!!
                                    pUVRenderer.set_Symbol(pUVRenderer.get_Value(i), (ISymbol)fehlerMarkersymbol);
                                }
                                else if (dlgResult == MessageBoxResult.No)
                                {                                        
                                        // Gesamte Methode wird abgebrochen
                                        return;
                                }         
                            }
                            //Legendentext anpassen: Checkbox "Legendentext verändern":
                            if (pRendererForm.checkBox1.IsChecked == true)
                            {
                                string leg_text1 = string.Empty, leg_text2 = string.Empty, heading = string.Empty;
                                if (feld1_feld != String.Empty)
                                {
                                    leg_text1 = Convert.ToString(pRow.get_Value(pTable.FindField(feld1_feld)));
                                }
                                if (feld2_feld != String.Empty)
                                {
                                    leg_text2 = Convert.ToString(pRow.get_Value(pTable.FindField(feld2_feld)));
                                }
                                if (ueberschrift_feld != String.Empty)
                                {
                                    heading = Convert.ToString(pRow.get_Value(pTable.FindField(ueberschrift_feld)));
                                }
                                string pUvValue = pUVRenderer.get_Value(i);//dass i von der for-Schleife!
                                if (trennzeichen_feld == "whitespace")
                                {
                                    trennzeichen_feld = String.Empty;
                                }

                                if (pRendererForm.chkUeberschrift.IsChecked == true)
                                {
                                    pUVRenderer.set_Heading(pUvValue, heading);
                                }

                                if (leg_text1 != string.Empty && leg_text2 != string.Empty)
                                {
                                    pUVRenderer.set_Label(pUvValue, (leg_text1 + " " + trennzeichen_feld + " " + leg_text2));
                                }
                                else if (leg_text1 == string.Empty && leg_text2 == string.Empty)
                                {
                                    pUVRenderer.set_Label(pUvValue, pUvValue);
                                }
                                else if (leg_text1 == string.Empty && leg_text2 != string.Empty)
                                {
                                    pUVRenderer.set_Label(pUvValue, (leg_text2));
                                }
                                else if (leg_text1 != string.Empty && leg_text2 == string.Empty)
                                {
                                    pUVRenderer.set_Label(pUvValue, (leg_text1));
                                }
                            } //Legendentext anpassen Ende!!!   
                            //System.Runtime.InteropServices.Marshal.ReleaseComObject(tabCursor), oder in C# Aufruf des Garbage-Collectors:
                            GC.Collect();
                        }//2 hier zu 
                        else //wenn der Renderer mehr Werte besitzt als die Legednetabelle:
                        {
                            GC.Collect();
                            pUVRenderer.set_Symbol(pUVRenderer.get_Value(i), (ISymbol)fehlerMarkersymbol);
                        }
                        //}
                        //catch (System.Runtime.InteropServices.COMException ce)
                        //{
                        //    MessageBox.Show(ce.Message , "DB Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        //    pd.Close();
                        //    return;
                        //}
                        //catch (Exception)
                        //{
                        //    pd.Close();
                        //}
                        if (pd.IsEnabled == false)
                        {
                            pRendererForm.staLblMessage.Content = "The rendering process has been canceled!";
                            return;
                        }
            
                    }//for-end of the renderer
                }
                #endregion


                //check if "UVRenderer Update" is pressed or "Symbol-Export":
                if (ReferenceEquals(sender, pRendererForm.btnRenderer))
                {
                    //alternativeFillSymbolList:
                    if (alternativeFillSymbolList.Count > 0 && pRendererForm.chkAllLayers.IsChecked == false)
                    {
                        string messageString = "Fill-Symbols not found in " + styleSet + ": \r\n";
                        foreach (string styleMessage in alternativeFillSymbolList) // Loop through List with foreach
                        {
                            messageString = messageString + styleMessage + "\r\n";
                        }
                        MessageBox.Show(messageString, "style conflict", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    //alternativeLineSymbolList:
                    if (alternativeLineSymbolList.Count > 0 && pRendererForm.chkAllLayers.IsChecked == false)
                    {
                        string messageString = "Line-Symbols not found in " + styleSet + ": \r\n";
                        foreach (string styleMessage in alternativeLineSymbolList) // Loop through List with foreach
                        {
                            messageString = messageString + styleMessage + "\r\n";
                        }
                        MessageBox.Show(messageString, "style conflict", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    //alternativeMarkerSymbolList:
                    if (alternativeMarkerSymbolList.Count > 0 && pRendererForm.chkAllLayers.IsChecked == false)
                    {
                        string messageString = "Marker-Symbols not found in " + styleSet + ": \r\n";
                        foreach (string styleMessage in alternativeMarkerSymbolList) // Loop through List with foreach
                        {
                            messageString = messageString + styleMessage + "\r\n";
                        }
                        MessageBox.Show(messageString, "style conflict", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    

                    //Refresh the TOC
                    this.pMxDoc.UpdateContents();
                    // Rfresh the map
                    this.pActiveView.Refresh();

                    if (pRendererForm.chkAllLayers.IsChecked == false)//wenn nicht durch alle Layer durchiteriert wird -> wird nur dann die MessageBox angezeigt, wenn nur ein einzelner Layer gerendert wird
                    {
                        //MessageBox.Show("The feature layer " + pFLayer.Name + " was successfully rendered!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                        pRendererForm.staLblMessage.Content = "The feature layer was successfully rendered!";
                    }                   
                }
                else if (ReferenceEquals(sender, symbolButton))
                {
                    if (pRendererForm.chkAllLayers.IsChecked == false)//wenn nicht durch alle Layer durchiteriert wird -> wird nur dann die MessageBox angezeigt, wenn nur ein einzelner Layer gerendert wird
                    {
                        MessageBox.Show("The images have been exported!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                        pRendererForm.staLblMessage.Content = "The images have been exported!";
                    }                   
                }               
                //esri database objects must be deleted through the garbage collector - esri-com bug: 
                GC.Collect();

                //close ProgressDialog:
                pd.Close();

            }//close for-loop!!!
     
            //check if "UVRenderer Update" is pressed or "Symbol-Export":
            if (ReferenceEquals(sender, pRendererForm.btnRenderer))
            {
                //if only oner layer is rendered:
                if (annotationLayerList.Count == 1 && pRendererForm.chkAllLayers.IsChecked == false)
                {
                    MessageBox.Show(annotationLayerList[0] + " could not be rendered!", "Annotation Layers", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                if (pRendererForm.chkAllLayers.IsChecked == true)//wenn durch alle Layer gerendert wird, kommt eine positive STatusmeldung beim letzten Layer
                {
                    //message for not rendered annotation layers:
                    if (annotationLayerList.Count > 0)
                    {
                        //MessageBox.Show(annotationLayerList[0] + " could not be rendered!");
                        string messageString = "The Annotation layers: ";
                        foreach (string styleMessage in annotationLayerList)
                        {
                            messageString = messageString + styleMessage + "  \r\n";
                        }
                        messageString = messageString + " could not be rendered!";
                        MessageBox.Show(messageString, "Annotation Layers", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    
                    MessageBox.Show( anzahlGerenderterLayer + "  feature layers, which have an active unique value renderer, have been successfully rendered!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    pRendererForm.staLblMessage.Content = "The feature layers have been successfully rendered!";
                   
                    
                }
            }
            //Symbol-Button:
            else if (ReferenceEquals(sender, symbolButton))
            {
                if (pRendererForm.chkAllLayers.IsChecked == true)//wenn durch alle Layer gerendert wird, kommt eine positive Statusmeldung beim letzten Layer
                {
                    MessageBox.Show("The images have been exported!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    pRendererForm.staLblMessage.Content = "The images have been exported!";
                }
            }
            anzahlGerenderterLayer = 0;
            
                   
        } //Methode rendering zu

       

        #region delegate+EventHandler:

        //our delegate used for updating the UI
        ////public delegate void UpdateProgressDelegate(int percentage, int recordCount, int value);
        //this is the method that the deleagte will execute
        public void UpdateProgressText(int percentage, int recordCount, int value)
        {
            //set our progress dialog text and value
            if (recordCount - 1 > value)
            {
                pd.ProgressText = string.Format("{0}% of {1} features are processed! ", percentage.ToString(), recordCount);
                //pd.ProgressValue = percentage;          
            }
            else
            {
                pd.ProgressText = string.Format("The Page Layout is now updating");
            }
        }

        //Create a Delegate that matches the Signature of the ProgressBar's SetValue method          
        ////private delegate void UpdateProgressBarDelegate(System.Windows.DependencyProperty dp, System.Object value);

        #endregion     

        #region class helper methods

        private void ExportMarkerSymbol(string ExportType, IMultiLayerMarkerSymbol pMultiLayerMarkerSymbol, IExport pExport, IGraphicsContainer pGraphicsContainer, IActiveView pActiveView2, string markersym)
        {
            tagRECT exportRect; //public struct tagRECT; struct aus ansi c++
            IEnvelope pExportEnv;
            IEnvelope pPixelBoundEnv;
            pExport.Resolution = 300;

            //Symbolgröße: 130*71 Pixel
            exportRect.left = 0;
            exportRect.top = 0;
            exportRect.right = 130;
            exportRect.bottom = 71;
            pPixelBoundEnv = new EnvelopeClass();
            pPixelBoundEnv.PutCoords(exportRect.left, exportRect.top, exportRect.right, exportRect.bottom);
            pExport.PixelBounds = pPixelBoundEnv;
            //for VisibleBounds
            pExportEnv = new EnvelopeClass();
            pExportEnv.PutCoords(0, 0, 1.1, 0.55);//Kästchengröße

            IPageLayout pPageLayout;
            IMarkerElement pMrkElm;
            IPoint pPnt;
            pPageLayout = pMxDoc.PageLayout;
            pPageLayout.Page.Units = esriUnits.esriCentimeters; //pPageLayout.Page.Units = 8 ?????
            pGraphicsContainer = (IGraphicsContainer)pPageLayout;
            IElement pElement;
            IGeometry pGeometry;

            /////////////////////////insert////////////////////////////////////////////////////////////////////////////////////////////
            pPnt = new PointClass();
            pPnt.PutCoords(0.55, 0.27);
            pGeometry = pPnt;
            //Draw the point
            pMrkElm = new MarkerElementClass();
            pMrkElm.Symbol = pMultiLayerMarkerSymbol;
            pElement = (IElement)pMrkElm;
            pElement.Geometry = pGeometry;
            pGraphicsContainer.AddElement(pElement, 0);
            //string pExportFileDir = pRendererForm.txtSymbolDirectory.Text + "\\";  //"D:\\";
            string pExportFileDir = pRendererForm.SymbolDirectory + "\\";  //"D:\\";
            pExport.ExportFileName = pExportFileDir + markersym + ".gif";
            System.Int32 hDC = pExport.StartExporting();
            pActiveView2.Output(hDC, (System.Int16)pExport.Resolution, exportRect, pExportEnv, null);
            // Finish writing the export file and cleanup any intermediate files
            pExport.FinishExporting();
            pExport.Cleanup();
            pGraphicsContainer.DeleteElement(pElement);

        }

        private void ExportLineSymbol(string ExportType, IMultiLayerLineSymbol pMultiLayerLineSymbol, IExport pExport, IGraphicsContainer pGraphicsContainer, IActiveView pActiveView2, string linesym)
        {
            tagRECT exportRect; //public struct tagRECT 
            IEnvelope pExportEnv;
            IEnvelope pPixelBoundEnv;
            pExport.Resolution = 300;

            //Symbolgröße: 130*71 Pixel
            exportRect.left = 0;
            exportRect.top = 0;
            exportRect.right = 130;
            exportRect.bottom = 71;
            pPixelBoundEnv = new EnvelopeClass();
            pPixelBoundEnv.PutCoords(exportRect.left, exportRect.top, exportRect.right, exportRect.bottom);
            pExport.PixelBounds = pPixelBoundEnv;
            //for VisibleBounds
            pExportEnv = new EnvelopeClass();
            pExportEnv.PutCoords(0, 0, 1.1, 0.55);//Kästchengröße

            IPageLayout pPageLayout;
            ILineElement pLineElm;
            IPolyline pPlyLn;
            IPoint fromPt;
            IPoint toPt;
            pPageLayout = pMxDoc.PageLayout;
            pPageLayout.Page.Units = esriUnits.esriCentimeters; //pPageLayout.Page.Units = 8 ?????
            pGraphicsContainer = (IGraphicsContainer)pPageLayout;
            IElement pElement;
            IGeometry pGeometry;

            /////////////////////////insert////////////////////////////////////////////////////////////////////////////////////////////
            pPlyLn = new PolylineClass();
            fromPt = new PointClass();
            toPt = new PointClass();
            fromPt.PutCoords(0.1, 0.07);
            toPt.PutCoords(1, 0.48);
            pPlyLn.FromPoint = fromPt;
            pPlyLn.ToPoint = toPt;
            pGeometry = pPlyLn;
            pLineElm = new LineElementClass();
            pLineElm.Symbol = pMultiLayerLineSymbol;
            pElement = (IElement)pLineElm;
            pElement.Geometry = pGeometry;

            pGraphicsContainer.AddElement(pElement, 0);
            //string pExportFileDir = pRendererForm.txtSymbolDirectory.Text + "\\";  //"D:\\";
            string pExportFileDir = pRendererForm.SymbolDirectory + "\\";  //"D:\\";
            pExport.ExportFileName = pExportFileDir + linesym + ".gif";
            System.Int32 hDC = pExport.StartExporting();
            pActiveView2.Output(hDC, (System.Int16)pExport.Resolution, exportRect, pExportEnv, null);
            // Finish writing the export file and cleanup any intermediate files
            pExport.FinishExporting();
            pExport.Cleanup();
            pGraphicsContainer.DeleteElement(pElement);

        }

        private void ExportFillSymbol(string ExportType, IFillSymbol pFillSymbol, IExport pExport, IGraphicsContainer pGraphicsContainer, IActiveView pActiveView2, string fillsym)
        {
            tagRECT exportRect; //public struct tagRECT 
            IEnvelope pExportEnv;
            IEnvelope pPixelBoundEnv;
            //pExport = new ExportGIFClass();
            pExport.Resolution = 300;

            //Symbolgröße: 130*71 Pixel
            exportRect.left = 0;
            exportRect.top = 0;
            exportRect.right = 130;
            exportRect.bottom = 71;
            pPixelBoundEnv = new EnvelopeClass();
            pPixelBoundEnv.PutCoords(exportRect.left, exportRect.top, exportRect.right, exportRect.bottom);
            pExport.PixelBounds = pPixelBoundEnv;
            //for VisibleBounds
            pExportEnv = new EnvelopeClass();
            pExportEnv.PutCoords(0, 0, 1.1, 0.55);//Kästchengröße

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            IPageLayout pPageLayout;
            IFillShapeElement pFillElm;
            WKSEnvelope nPatch;
            IEnvelope pFillEnv;
            pPageLayout = pMxDoc.PageLayout;
            pPageLayout.Page.Units = esriUnits.esriCentimeters; //pPageLayout.Page.Units = 8 ?????
            pGraphicsContainer = (IGraphicsContainer)pPageLayout;
            IElement pElement;
            IGeometry pGeometry;

            /////////////////////////insert////////////////////////////////////////////////////////////////////////////////////////////
            pFillEnv = new EnvelopeClass();
            nPatch.XMin = -0.1;//testen wegen der Zeichenfläche - Randpixelproblem!!!j
            nPatch.YMax = 0.65;
            nPatch.XMax = 1.2;
            nPatch.YMin = -0.1;
            pFillEnv.PutCoords(nPatch.XMin, nPatch.YMin, nPatch.XMax, nPatch.YMax);
            pGeometry = pFillEnv;
            pFillElm = new RectangleElementClass();
            pFillElm.Symbol = pFillSymbol; //Vom Polygon Renderer
            pElement = (IElement)pFillElm;
            pElement.Geometry = pGeometry;

            pGraphicsContainer.AddElement(pElement, 0);
            //pExportFileDir = pRendererForm.txtSymbolDirectory.Text + "\\";  //"D:\\";
            pExportFileDir = pRendererForm.SymbolDirectory + "\\";  //"D:\\";
            pExport.ExportFileName = pExportFileDir + fillsym + ".gif";
            System.Int32 hDC = pExport.StartExporting();
            pActiveView2.Output(hDC, (System.Int32)pExport.Resolution, exportRect, pExportEnv, null);
            // Finish writing the export file and cleanup any intermediate files
            pExport.FinishExporting();
            pExport.Cleanup();
            pGraphicsContainer.DeleteElement(pElement);

        }

        //Create CMYK Color from Cyan, Magenta, Yellow, Black value:
        private ICmykColor GetCMYKColor(int c, int m, int y, int k)
        {

            ICmykColor cmykColor = new CmykColor();
            cmykColor.Cyan = c;
            cmykColor.Magenta = m;
            cmykColor.Yellow = y;
            cmykColor.Black = k;
            return cmykColor;
        }

        private int Hintergrundfarbe(string bgc_str)
        {
            int bgc_int;// = 0;
            if (bgc_str == "S")
                bgc_int = 6;
            else if (bgc_str == "F")
                bgc_int = 15;
            else if (bgc_str == "V")
                bgc_int = 100;
            else if (bgc_str == "X")
                bgc_int = 0;
            else if (bgc_str == "0")
                bgc_int = 0;
            else if (bgc_str == "1")
                bgc_int = 10;
            else if (bgc_str == "2")
                bgc_int = 20;
            else if (bgc_str == "3")
                bgc_int = 30;
            else if (bgc_str == "4")
                bgc_int = 40;
            else if (bgc_str == "5")
                bgc_int = 50;
            else if (bgc_str == "6")
                bgc_int = 60;
            else if (bgc_str == "7")
                bgc_int = 70;
            else
                bgc_int = 0;


            return bgc_int;
        }

        private void ErrorMessageBox(string feld)
        {
            string errorMessage = "ERROR - field name " + feld + " doesn't exist!";
            pRendererForm.staLblMessage.Content = errorMessage;
            MessageBox.Show(errorMessage, "Missing database field", MessageBoxButton.OK, MessageBoxImage.Warning);
            pRendererForm.Cursor = System.Windows.Input.Cursors.Arrow;
        }

        private void ErrorMessageBox2(string feld, string label)
        {
            string errorMessage;
            if (feld == String.Empty)
            {
                errorMessage = "ERROR - field name for " + label + " is empty!";
            }
            else
            {
                errorMessage = "ERROR - field name " + feld + " for the label " + label + " doesn't exist!";
            }
            pRendererForm.staLblMessage.Content = errorMessage;
            MessageBox.Show(errorMessage);
            pRendererForm.Cursor = System.Windows.Input.Cursors.Arrow;
        }

        private bool checkUniqueness(ITable dTable, string colName)
        {            
            Hashtable hTable = new Hashtable();
            ArrayList duplicateList = new ArrayList();
            IQueryFilter queryFilter = null; // create QueryFilter or leave it null
            //eigen Klasse CursoGS, da ESRI keine RowCollection zur Verfügung stellt:
            //CursorGS cursorGS = new CursorGS(dTable, queryFilter);
            //ICursor cursorGS = dTable.GetRows(null, true);
            ICursor cursor = dTable.Search(queryFilter, false);
            int index = dTable.FindField(colName);

            //Add list of all the unique item value to hashtable, which stores combination of key, value pair.
            //And add duplicate item value in arraylist.
            //foreach (IRow drow in cursorGS)
            IRow drow;
            while ((drow = cursor.NextRow()) != null)
            {
                if (hTable.Contains(drow.get_Value(index)))
                {
                    duplicateList.Add(drow);
                }
                else
                {
                    hTable.Add(drow.get_Value(index), string.Empty);
                }
            }
            //Wenn die duplicateList leer ist, dann ist die Tabelle unique und deshalb return true:
            if (duplicateList.Count == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
       
        #endregion       

    }
}
