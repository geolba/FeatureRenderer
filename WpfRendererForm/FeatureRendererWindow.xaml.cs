using System;
using System.Resources;
using System.Collections;
using System.Collections.Generic;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
//neu:
using Microsoft.Win32;
//ESRI references:
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.ArcMapUI;
//für die StyleGallery
using ESRI.ArcGIS.Display;
//für languageCulture
using System.IO;

using System.Windows.Input;
using WpfRendererForm.Resources;
using WpfRendererForm.Help;

//ESRI references:
using ESRI.ArcGIS.Geodatabase;
using System.Windows.Threading;

using FeatureRenderer.Business;
using FeatureRenderer.Core;
//using FeatureRenderer.Core.CommonDialogWrappers;
using FeatureRenderer.Core.XmlSerialize;

namespace WpfRendererForm
{
    /// <summary>
    /// Interaktionslogik für FeatureRendererWindow.xaml
    /// </summary>
    public partial class FeatureRendererWindow : Window
    {
        #region class members

        private IMxDocument pMxDoc;
        private IApplication m_application;
        private IActiveView pActiveView;
        private IMap pMap;
        private IEnumLayer pEnumLayer;
        private ILayer pLayer;
        private string language;// = "de"; in den construktor verschachtelt!
        CRenderer rendererToolManager;
        private bool _isInitializing = false;
        private string pfadKonfigurationsdatei = String.Empty;
        private string symbolDirectory = String.Empty;

        protected AboutDialog aboutWindow;

        #region esri event member variables:

        private ESRI.ArcGIS.Carto.IActiveViewEvents_ItemAddedEventHandler m_ActiveViewEventsItemAdded;
        private ESRI.ArcGIS.Carto.IActiveViewEvents_ItemDeletedEventHandler m_ActiveViewEventsItemDeleted;
        private ESRI.ArcGIS.Carto.IActiveViewEvents_ItemReorderedEventHandler m_ActiveViewEventsItemReordered;
        private ESRI.ArcGIS.Carto.IActiveViewEvents_ContentsChangedEventHandler m_ActiveViewEventsContentsChanged;

        private ESRI.ArcGIS.ArcMapUI.IDocumentEvents_NewDocumentEventHandler m_DocumentEventNewDocument;
        private ESRI.ArcGIS.ArcMapUI.IDocumentEvents_OpenDocumentEventHandler m_DocumentEventOpenDocument;
        private ESRI.ArcGIS.ArcMapUI.IDocumentEvents_Event m_docEvents = null;

        #endregion
       
        #endregion

        #region constructors

        public FeatureRendererWindow()
        {
        }

        public FeatureRendererWindow(IApplication application)
        {
            if (null == application)
            {
                throw new Exception("Hook helper is not initialized");
            }

            this._isInitializing = true;
            InitializeComponent();
            this._isInitializing = false;       

            m_application = application;
            rendererToolManager = new CRenderer(this, m_application);
            //rendererToolManager.SetProgressBarMaximum += new CRenderer.SetProgressBarMaximumHandler(rendererToolManager_SetProgressBarMaximum);
            //rendererToolManager.StartedUpdateProcess += new CRenderer.StartedUpdateProcessHandler(rendererToolManager_StartedUpdateProcess);
            //rendererToolManager.FinishedUpdateProcess += new CRenderer.FinishedUpdateProcessHandler(rendererToolManager_FinishedUpdateProcess);
            //rendererToolManager.FinishedConceptUpdate += new CRenderer.FinishedConceptUpdateHandler(rendererToolManager_FinishedConceptUpdate);

            //language settings: detect the windows language settings and load the correct language:
            language = System.Globalization.CultureInfo.CurrentCulture.Name;
            if (language.Contains("de") == true)
            {
                language = "de";
                this.chkSpracheDeutsch.IsChecked = true;
            }
            else
            {
                language = "en";
                this.chkSpracheEnglisch.IsChecked = true;
            }
            Resource.Culture = new System.Globalization.CultureInfo(language);
            
        }

         #endregion       

        # region properties

        public string SymbolDirectory
        {
            get
            {
                return this.symbolDirectory;
            }
            set
            {
               this.symbolDirectory = value;
            }
        }

        #endregion

        #region main GIS - methods (called through the CRenderer class):

        private void btnRenderer_Click(object sender, RoutedEventArgs e)
        {
            if (this.cboLayername.Text == "-- Please select a layer --" && chkAllLayers.IsChecked == false)
            {
                MessageBox.Show("Please select a layer for the rendering process!", "Layer selection", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //try
            //{
                rendererToolManager.rendering(sender);
            //}
            //catch
            //{
            //    MessageBox.Show("A general error during the rendering process is occured!", "Rendering error", MessageBoxButton.OK, MessageBoxImage.Error);
            //}
            //finally
            //{
            //    this.Cursor = System.Windows.Input.Cursors.Arrow;
            //}


        }

        #endregion

        #region windows wpf-window methods

        /// <summary>
        /// Set up the wiring of the events.
        /// </summary>
        /// <param name="myDocument"></param>
        /// <remarks></remarks>
        private void SetUpDocumentEvent(ESRI.ArcGIS.Framework.IDocument myDocument)
        {
            //parameter check:
            if (myDocument == null)
            {
                return;
            }
            //IDocumentEvents_Event docEvents = myDocument as ESRI.ArcGIS.ArcMapUI.IDocumentEvents_Event; 
            m_docEvents = myDocument as IDocumentEvents_Event;

            //// Create an instance of the delegate, add it to NewDocument event:
            //m_DocumentEventNewDocument = new IDocumentEvents_NewDocumentEventHandler(OnNewDocument);
            //// Wire the delegate to the   IDocumentEvents_Event event of the docEvents variable.
            //docEvents.NewDocument += m_DocumentEventNewDocument;
            m_DocumentEventNewDocument = new IDocumentEvents_NewDocumentEventHandler(OnNewDocument);
            m_docEvents.NewDocument += m_DocumentEventNewDocument;           

            //// Create an instance of the delegate, add it to OpenDocument event:
            //m_DocumentEventOpenDocument = new IDocumentEvents_OpenDocumentEventHandler(OnOpenDocument);
            //// Wire the delegate to the   IDocumentEvents_Event event of the docEvents variable.
            //docEvents.OpenDocument += m_DocumentEventOpenDocument;           
            m_DocumentEventOpenDocument = new IDocumentEvents_OpenDocumentEventHandler(OnOpenDocument);
            m_docEvents.OpenDocument += m_DocumentEventOpenDocument;
        }

        /// <summary>
        /// SECTION: Remove the event handlers for all of the IActiveViewEvents
        /// </summary>
        /// <param name="map">An IMap interface for which Event Handlers to remove.</param>
        /// <remarks>You do not have to remove Event Handlers for every event. You can pick and 
        /// choose which ones you want to use.</remarks>
        private void RemoveSetUpDocumentEvents(ESRI.ArcGIS.Framework.IDocument myDocument)
        {
            //parameter check
            if (myDocument == null)
            {
                return;
            }
            //ESRI.ArcGIS.ArcMapUI.IDocumentEvents_Event docEvents = myDocument as ESRI.ArcGIS.ArcMapUI.IDocumentEvents_Event;
            m_docEvents = myDocument as IDocumentEvents_Event;

            // Remove ItemAdded and ItemDeletd and ItemReordered and ContentsChanged Event Handler
            m_docEvents.NewDocument -= m_DocumentEventNewDocument;
            m_docEvents.OpenDocument -= m_DocumentEventOpenDocument;         
        }

        /// <summary>
        /// The NewDocument event handler. 
        /// </summary>
        /// <remarks></remarks>
        void OnNewDocument()
        {
            pMxDoc = m_docEvents as ESRI.ArcGIS.ArcMapUI.IMxDocument;
            pActiveView = pMxDoc.ActiveView;
            pMap = pActiveView.FocusMap;
            this.updateCboLayername();
            this.updateCboStyleFile();
        }

        /// <summary>
        /// The OpenDocument event handler.
        /// </summary>
        /// <remarks></remarks>
        void OnOpenDocument()
        {
            pMxDoc = m_docEvents as ESRI.ArcGIS.ArcMapUI.IMxDocument;
            pActiveView = pMxDoc.ActiveView;
            pMap = pActiveView.FocusMap;
            this.updateCboLayername();
            this.updateCboStyleFile();
        }


        /// <summary>
        /// SECTION: Set up the event handlers for all of the IActiveViewEvents
        /// </summary>
        /// <param name="map">An IMap interface for which to set up Event Handlers for</param>
        /// <remarks>You do not have to set up Event Handlers for every event. You can pick and 
        /// choose which ones you want to use.</remarks>
        private void SetupActiveViewEvents(ESRI.ArcGIS.Carto.IMap map)
        {
            //parameter check
            if (map == null)
            {
                return;
            }
            IActiveViewEvents_Event activeViewEvents = map as IActiveViewEvents_Event;

            // Create an instance of the delegate, add it to ItemAdded event
            m_ActiveViewEventsItemAdded = new IActiveViewEvents_ItemAddedEventHandler(OnActiveViewEventsItemAddedRemoved);
            //// Wire the delegate to the  IActiveViewEvents_Eventevent of the activeViewEvents variable.
            activeViewEvents.ItemAdded += m_ActiveViewEventsItemAdded;

            // Create an instance of the delegate, add it to ItemDeleted event
            m_ActiveViewEventsItemDeleted = new ESRI.ArcGIS.Carto.IActiveViewEvents_ItemDeletedEventHandler(OnActiveViewEventsItemAddedRemoved);
            activeViewEvents.ItemDeleted += m_ActiveViewEventsItemDeleted;

            // Create an instance of the delegate, add it to ItemReordered event
            m_ActiveViewEventsItemReordered = new ESRI.ArcGIS.Carto.IActiveViewEvents_ItemReorderedEventHandler(OnActiveViewEventsItemReordered);
            activeViewEvents.ItemReordered += m_ActiveViewEventsItemReordered;

            // Create an instance of the delegate, add it to ContentsChanged event
            m_ActiveViewEventsContentsChanged = new ESRI.ArcGIS.Carto.IActiveViewEvents_ContentsChangedEventHandler(OnActiveViewEventsContentsChanged);
            activeViewEvents.ContentsChanged += m_ActiveViewEventsContentsChanged;
        }

       /// <summary>
        /// SECTION: Remove the event handlers for all of the IActiveViewEvents
        /// </summary>
        /// <param name="map">An IMap interface for which Event Handlers to remove.</param>
        /// <remarks>You do not have to remove Event Handlers for every event. You can pick and 
        /// choose which ones you want to use.</remarks>
        private void RemoveActiveViewEvents(ESRI.ArcGIS.Carto.IMap map)
        {
            //parameter check
            if (map == null)
            {
                return;
            }
            ESRI.ArcGIS.Carto.IActiveViewEvents_Event activeViewEvents = map as ESRI.ArcGIS.Carto.IActiveViewEvents_Event;

            // Remove ItemAdded and ItemDeletd and ItemReordered and ContentsChanged Event Handler
            activeViewEvents.ItemAdded -= m_ActiveViewEventsItemAdded;
            activeViewEvents.ItemDeleted -= m_ActiveViewEventsItemDeleted;
            activeViewEvents.ItemReordered -= m_ActiveViewEventsItemReordered;
            activeViewEvents.ContentsChanged -= m_ActiveViewEventsContentsChanged;
        }

        /// <summary>
        /// ItemAdded Event handler; e.g an layer is added to the map:
        /// </summary>
        /// <param name="Item"></param>
        /// <remarks></remarks>
        private void OnActiveViewEventsItemAddedRemoved(System.Object Item)
        {
            //load all visible feature layers into the comboBox:
            this.updateCboLayername();
        }

        /// <summary>
        /// ItemReordered Event handler
        /// </summary>
        /// <param name="Item"></param>
        /// <param name="toIndex"></param>
        /// <remarks></remarks>
        private void OnActiveViewEventsItemReordered(System.Object Item, System.Int32 toIndex)
        {
            //load all visible feature layers into the comboBox:
            this.updateCboLayername();
        }

        /// <summary>
        /// ContentsChanged Event handler: e.g. toggling the visibility of the map layers
        /// </summary>
        /// <remarks></remarks>
        private void OnActiveViewEventsContentsChanged()
        {
            //load all visible feature layers into the comboBox:
            this.updateCboLayername();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.staLblMessage.Content = String.Empty;

            pMxDoc = (IMxDocument)m_application.Document;
            pActiveView = pMxDoc.ActiveView;
            pMap = pActiveView.FocusMap;
            SetupActiveViewEvents(pMap);
            SetUpDocumentEvent((IDocument) pMxDoc);

            //load all feature layers into the comboBox:
            this.updateCboLayername();

            //load the style file into the comboBox:
            this.updateCboStyleFile();

            //load temporary data if they already exist:
            ReloadSettings();

        }

        private void btnLoadAccessDB_Click(object sender, RoutedEventArgs e)
        {
            //Öffnen eines File Dialogs zur Auswahl der Access-Datenbank:
            OpenFileDialog oDlg = new OpenFileDialog();
            oDlg.Title = "Select Access Database";
            oDlg.Filter = "MDB (*.Mdb)|*.mdb|" +
                           "ACCDB (*.Accdb)|*.accdb";
            //oDlg.RestoreDirectory = true;
            string dir = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
            oDlg.InitialDirectory = dir;

            // Show open file dialog box
            Nullable<bool> result = oDlg.ShowDialog();

            // OK wurde gedrückt:
            if (result == true)
            {
                this.txtAccessdatenbank.Text = oDlg.FileName.ToString();
            }
            else
            {                
                //MessageBox.Show("The database finding dialog was closed!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnLoadSqlServerTables_Click(object sender, RoutedEventArgs e)
        {
            //controller comes from the business layer:
            IFeatureRendererController controller = new FeatureRendererController();
            bool directConnect = this.chkDirectConnect.IsChecked.Value;
            WaitDialog wd = null;
            try
            {
                wd = new WaitDialog();
                //ab.ShowDialog();//Modal - man kann das Hauptfenster nicht verändern!

                wd.Show();//nicht modal
            }
            catch
            {
                MessageBox.Show("Please restart the about window!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                wd.Close();
            }

            //if (this.chkSqlServer.IsChecked == true && this.chkAuthentifizierung.IsChecked == false)
            if (this.tabItemSde.IsSelected == true && this.chkAuthentifizierung.IsChecked == false)
            {
                try
                {
                    //eventuelle Tabbellen müssen gelöscht werden:
                    cboTable.ItemsSource = null;
                    cboTable.Items.Clear();
                    
                    //List<string> Tables = DatabaseInfoCollector.GetSqlServerTables(this.txtServer.Text, this.txtInstance.Text, this.txtDatabase.Text, this.txtVersion.Text);
                    List<string> Tables = controller.GetSqlServerTables (this.txtServer.Text, this.txtInstance.Text, this.txtDatabase.Text, this.txtVersion.Text, directConnect);
                    //foreach (string table in Tables)
                    //{
                    //    cboTable.Items.Add(table);
                    //    cboTable.SelectedIndex = 0;
                    //}
                    this.cboTable.DataContext = Tables;//Neu bei WPF
                    this.cboTable.ItemsSource = Tables;//Neu bei WPF
                    cboTable.SelectedIndex = 0;

                        
                }
                catch (Exception)
                {
                    MessageBox.Show("The legend table wasn't defined or the session has been canceled!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.staLblMessage.Content = "The legend table wasn't defined or the session has been canceled!";
                    //und wieder den default-Pfeilcursor anzeigen:
                    this.Cursor = System.Windows.Input.Cursors.Arrow;    
                    return;
                }
            }
            else if (this.tabItemSde.IsSelected == true && this.chkAuthentifizierung.IsChecked == true)
            {
                try
                {
                    //eventuelle Tabbellen müssen gelöscht werden:
                    cboTable.ItemsSource = null;
                    cboTable.Items.Clear();
                    
                    //List<string> Tables = DatabaseInfoCollector.GetPasswordSqlServerTables(this.txtServer.Text, this.txtInstance.Text, this.txtDatabase.Text, this.txtVersion.Text, this.txtUser.Text, this.txtPassword.Text);
                    List<string> Tables = controller.GetPasswordSqlServerTables(this.txtServer.Text, this.txtInstance.Text, this.txtDatabase.Text, this.txtVersion.Text, this.txtUser.Text, this.txtPassword.Text, directConnect);

                    //this.cboTable.DataContext = Tables;//Neu bei WPF
                    this.cboTable.ItemsSource = Tables;//Neu bei WPF
                    cboTable.SelectedIndex = 0;
                }
                catch (Exception)
                {
                    MessageBox.Show("The legend table wasn't defined or the session has been canceled!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.staLblMessage.Content = "The legend table wasn't defined or the session has been canceled!";
                    return;
                }
            }
            wd.Close();
        }

        private void cboTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //controller comes from the business layer:
            IFeatureRendererController controller = new FeatureRendererController();
            bool directConnect = this.chkDirectConnect.IsChecked.Value;

            WaitDialog wd = null;
            if (this.tabItemSde.IsSelected == true)
            {
                try
                {
                    wd = new WaitDialog();
                    //ab.ShowDialog();//Modal - man kann das Hauptfenster nicht verändern!

                    wd.Show();//nicht modal
                }
                catch
                {
                    MessageBox.Show("Please restart the about window!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    wd.Close();
                }
            }

            //pFeatws = (IFeatureWorkspace)pWorkspace;
            //pTable = pFeatws.OpenTable(cboTable.Text);
            cboVerknuepfung.ItemsSource = null;//.Clear();
            cboVerknuepfung.Items.Clear();
            cboFlaeche.ItemsSource = null;//.Items.Clear();
            cboFlaeche.Items.Clear();
            cboLinie.ItemsSource = null;//.Items.Clear();
            cboLinie.Items.Clear();
            cboMarker.ItemsSource = null;//.Items.Clear();
            cboMarker.Items.Clear();
            cboFeld1.ItemsSource = null;
            cboFeld1.Items.Clear();
            cboFeld2.ItemsSource = null;
            cboFeld2.Items.Clear();

            cboUeberschrift.ItemsSource = null;


            if (cboTable.SelectedItem == null)
            {
                cboTable.SelectedIndex = 0;
            }


            if (chkAccess.IsChecked == true && cboTable.SelectedItem != null && cboTable.SelectedItem.ToString() != "-- Please select a table --")
            //if (tabItemAccess.IsSelected == true && cboTable.SelectedItem != null)
            {
                try
                {
                    //List<string> Columns =
                    //DatabaseInfoCollector.GetColumnNames(this.txtAccessdatenbank.Text, cboTable.SelectedItem.ToString());
                    List<string> Columns =
                    controller.GetColumnNames(this.txtAccessdatenbank.Text, cboTable.SelectedItem.ToString());
                    Columns.Sort();
                    int indexVerknuepfung = GetItemIndex(Columns, "ID");
                    this.cboVerknuepfung.DataContext = Columns;
                    this.cboVerknuepfung.ItemsSource = Columns;
                    this.cboVerknuepfung.SelectedIndex = indexVerknuepfung;

                    int indexFlaeche = GetItemIndex(Columns, "FILL_SYMBOL");
                    if (indexFlaeche == 0)
                    {
                        indexFlaeche = GetItemIndex(Columns, "FILL_SYMBOLS");
                    }
                    this.cboFlaeche.DataContext = Columns;
                    this.cboFlaeche.ItemsSource = Columns;
                    this.cboFlaeche.SelectedIndex = indexFlaeche;


                    int indexLinie = GetItemIndex(Columns, "LINE_SYMBOL");
                    if (indexLinie == 0)
                    {
                        indexLinie = GetItemIndex(Columns, "LINE_SYMBOLS");
                    }
                    this.cboLinie.DataContext = Columns;
                    this.cboLinie.ItemsSource = Columns;
                    this.cboLinie.SelectedIndex = indexLinie;

                    int indexMarker = GetItemIndex(Columns, "MARKER_SYMBOL");
                    if (indexMarker == 0)
                    {
                        indexMarker = GetItemIndex(Columns, "MARKER_SYMBOLS");
                    }
                    this.cboMarker.DataContext = Columns;
                    this.cboMarker.ItemsSource = Columns;
                    this.cboMarker.SelectedIndex = indexMarker;

                    //Suchfelder für den Legendentext:
                    this.cboFeld1.DataContext = Columns;
                    this.cboFeld1.ItemsSource = Columns;
                    this.cboFeld1.SelectedIndex = 0;
                    this.cboFeld2.DataContext = Columns;
                    this.cboFeld2.ItemsSource = Columns;
                    this.cboFeld2.SelectedIndex = 0;
                    this.cboUeberschrift.DataContext = Columns;
                    this.cboUeberschrift.ItemsSource = Columns;
                    this.cboUeberschrift.SelectedIndex = 0;
                }
                catch
                {
                    MessageBox.Show("Please define a correct database!", "No correct database", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.cboTable.Items.Clear();
                }
            }
            else if (chkSde.IsChecked == true && chkAuthentifizierung.IsChecked == false && cboTable.SelectedItem != null && cboTable.SelectedItem.ToString() != "-- Please select a table --")
            //else if (tabItemSde.IsSelected == true && chkAuthentifizierung.IsChecked == false && cboTable.SelectedItem != null)
            {
                //List<string> Columns =
                //DatabaseInfoCollector.GetSqlServerColumnNames(cboTable.SelectedItem.ToString(), this.txtServer.Text, this.txtInstance.Text, this.txtDatabase.Text, this.txtVersion.Text);
                List<string> Columns =
                controller.GetSqlServerColumnNames(cboTable.SelectedItem.ToString(), this.txtServer.Text, this.txtInstance.Text, this.txtDatabase.Text, this.txtVersion.Text, directConnect);
                Columns.Sort();
                foreach (string column in Columns)
                {
                    int indexVerknuepfung = GetItemIndex(Columns, "ID");
                    this.cboVerknuepfung.DataContext = Columns;
                    this.cboVerknuepfung.ItemsSource = Columns;
                    this.cboVerknuepfung.SelectedIndex = indexVerknuepfung;

                    int indexFlaeche = GetItemIndex(Columns, "FILL_SYMBOL");
                    if (indexFlaeche == 0)
                    {
                        indexFlaeche = GetItemIndex(Columns, "FILL_SYMBOLS");
                    }
                    this.cboFlaeche.DataContext = Columns;
                    this.cboFlaeche.ItemsSource = Columns;
                    this.cboFlaeche.SelectedIndex = indexFlaeche;

                    int indexLinie = GetItemIndex(Columns, "LINE_SYMBOL");
                    if (indexLinie == 0)
                    {
                        indexLinie = GetItemIndex(Columns, "LINE_SYMBOLS");
                    }
                    this.cboLinie.DataContext = Columns;
                    this.cboLinie.ItemsSource = Columns;
                    this.cboLinie.SelectedIndex = indexLinie;

                    int indexMarker = GetItemIndex(Columns, "MARKER_SYMBOL");
                    if (indexMarker == 0)
                    {
                        indexMarker = GetItemIndex(Columns, "MARKER_SYMBOLS");
                    }
                    this.cboMarker.DataContext = Columns;
                    this.cboMarker.ItemsSource = Columns;
                    this.cboMarker.SelectedIndex = indexMarker;

                    //Suchfelder für den Legendentext:
                    this.cboFeld1.DataContext = Columns;
                    this.cboFeld1.ItemsSource = Columns;
                    this.cboFeld1.SelectedIndex = 0;
                    this.cboFeld2.DataContext = Columns;
                    this.cboFeld2.ItemsSource = Columns;
                    this.cboFeld2.SelectedIndex = 0;
                    this.cboUeberschrift.DataContext = Columns;
                    this.cboUeberschrift.ItemsSource = Columns;
                    this.cboUeberschrift.SelectedIndex = 0;
                }
            }

            //mit Authentifizierung:
            else if (chkSde.IsChecked == true && chkAuthentifizierung.IsChecked == true && cboTable.SelectedItem != null && cboTable.SelectedItem.ToString() != "-- Please select a table --")
            //else if (tabItemSde.IsSelected == true && chkAuthentifizierung.IsChecked == true && cboTable.SelectedItem != null)
            {
                //List<string> Columns =
                //DatabaseInfoCollector.GetPasswordSqlServerColumnNames(cboTable.SelectedItem.ToString(), this.txtServer.Text, this.txtInstance.Text, this.txtDatabase.Text, this.txtVersion.Text, this.txtUser.Text, this.txtPassword.Text);
                List<string> Columns =
                controller.GetPasswordSqlServerColumnNames(cboTable.SelectedItem.ToString(), this.txtServer.Text, this.txtInstance.Text, this.txtDatabase.Text, this.txtVersion.Text, this.txtUser.Text, this.txtPassword.Text, directConnect);
                Columns.Sort();
                foreach (string column in Columns)
                {
                    int indexVerknuepfung = GetItemIndex(Columns, "ID");
                    this.cboVerknuepfung.DataContext = Columns;
                    this.cboVerknuepfung.ItemsSource = Columns;
                    this.cboVerknuepfung.SelectedIndex = indexVerknuepfung;

                    int indexFlaeche = GetItemIndex(Columns, "FILL_SYMBOL");
                    if (indexFlaeche == 0)
                    {
                        indexFlaeche = GetItemIndex(Columns, "FILL_SYMBOLS");
                    }
                    this.cboFlaeche.DataContext = Columns;
                    this.cboFlaeche.ItemsSource = Columns;
                    this.cboFlaeche.SelectedIndex = indexFlaeche;

                    int indexLinie = GetItemIndex(Columns, "LINE_SYMBOL");
                    if (indexLinie == 0)
                    {
                        indexLinie = GetItemIndex(Columns, "LINE_SYMBOLS");
                    }
                    this.cboLinie.DataContext = Columns;
                    this.cboLinie.ItemsSource = Columns;
                    this.cboLinie.SelectedIndex = indexLinie;

                    int indexMarker = GetItemIndex(Columns, "MARKER_SYMBOL");
                    if (indexMarker == 0)
                    {
                        indexMarker = GetItemIndex(Columns, "MARKER_SYMBOLS");
                    }
                    this.cboMarker.DataContext = Columns;
                    this.cboMarker.ItemsSource = Columns;
                    this.cboMarker.SelectedIndex = indexMarker;

                    //Suchfelder für den Legendentext:
                    this.cboFeld1.DataContext = Columns;
                    this.cboFeld1.ItemsSource = Columns;
                    this.cboFeld1.SelectedIndex = 0;
                    this.cboFeld2.DataContext = Columns;
                    this.cboFeld2.ItemsSource = Columns;
                    this.cboFeld2.SelectedIndex = 0;
                    this.cboUeberschrift.DataContext = Columns;
                    this.cboUeberschrift.ItemsSource = Columns;
                    this.cboUeberschrift.SelectedIndex = 0;
                }
            }
            if (this.chkSde.IsChecked == true && wd != null)
            //if (this.tabItemSde.IsSelected == true && wd != null)
            {
                wd.Close();
            }

            if (cboTable.SelectedItem != null && cboTable.SelectedItem.ToString() != "-- Please select a table --")
            {
                this.cboVerknuepfung.IsEnabled = true;
                this.cboFlaeche.IsEnabled = true;
                this.cboLinie.IsEnabled = true;
                this.cboMarker.IsEnabled = true;
            }
            else
            {
                this.cboVerknuepfung.IsEnabled = false;
                this.cboFlaeche.IsEnabled = false;
                this.cboLinie.IsEnabled = false;
                this.cboMarker.IsEnabled = false;
            }

            if (cboTable.SelectedItem != null && cboLayername.SelectedItem != null)
            {

                if ((((MyLayer)cboLayername.SelectedItem).Name == "-- Please select a layer --" ||
                    (cboTable.SelectedItem.ToString() == "-- Please select a table --" || cboTable.SelectedItem.ToString() == String.Empty)) && chkAllLayers.IsChecked == false)
                {
                    this.btnRenderer.IsEnabled = false;
                }
                else
                {
                    this.btnRenderer.IsEnabled = true;
                }
            }
            else if (cboLayername.SelectedItem != null)
            {
                if ((((MyLayer)cboLayername.SelectedItem).Name == "-- Please select a layer --" ||
                    (cboTable.Text == "-- Please select a table --" || cboTable.Text == String.Empty)) && chkAllLayers.IsChecked == false)
                {
                    this.btnRenderer.IsEnabled = false;
                }
                else
                {
                    this.btnRenderer.IsEnabled = true;
                }
            }
        }

        private void mnuAppClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void chkSpracheDeutsch_Checked(object sender, RoutedEventArgs e)
        {
            this.chkSpracheEnglisch.IsChecked = false;
            this.language = "de";

            System.Globalization.CultureInfo test = new System.Globalization.CultureInfo(language);
            Resource.Culture = new System.Globalization.CultureInfo(language);

            Window window = this;
            List<string> ControlNames = new List<string>();

            ResourceSet resourceSet = Resource.ResourceManager.GetResourceSet(test, true, true);
            foreach (DictionaryEntry entry in resourceSet)
            {
                string resourceKey = entry.Key.ToString();//Schlüssel = Name des Controls
                object resource = entry.Value;//=Content der labels
                ControlNames.Add(resourceKey.ToString());
            }

            foreach (Label lbl in TreeHelper.FindChildren<Label>(window))
            {
                if (ControlNames.Contains(lbl.Name))
                {
                    // do something with lbl here
                    string content = lbl.Name;
                    lbl.Content = Resource.ResourceManager.GetString(content, Resource.Culture);
                }

            }

            foreach (System.Windows.Controls.Button btn in TreeHelper.FindChildren<System.Windows.Controls.Button>(window))
            {
                if (ControlNames.Contains(btn.Name))
                {
                    // do something with btn here
                    string content = btn.Name;
                    btn.Content = Resource.ResourceManager.GetString(content, Resource.Culture);
                }
            }

            foreach (CheckBox chk in TreeHelper.FindChildren<CheckBox>(window))
            {
                if (ControlNames.Contains(chk.Name))
                {
                    // do something with chk here
                    string content = chk.Name;
                    chk.Content = Resource.ResourceManager.GetString(content, Resource.Culture);
                }
            }

            foreach (GroupBox grp in TreeHelper.FindChildren<GroupBox>(window))
            {
                if (ControlNames.Contains(grp.Name))
                {
                    // do something with grp here
                    string header = grp.Name;
                    grp.Header = Resource.ResourceManager.GetString(header, Resource.Culture);
                }
            }

            foreach (MenuItem mni in TreeHelper.FindChildren<MenuItem>(window))
            {
                if (ControlNames.Contains(mni.Name))
                {
                    // do something with mni here
                    string header = mni.Name;
                    mni.Header = Resource.ResourceManager.GetString(header, Resource.Culture);
                }
            }

            foreach (TabItem tabi in TreeHelper.FindChildren<TabItem>(window))
            {
                if (ControlNames.Contains(tabi.Name))
                {
                    // do something with mni here
                    string header = tabi.Name;
                    tabi.Header = Resource.ResourceManager.GetString(header, Resource.Culture);
                }
            }



        }

        private void chkSpracheEnglisch_Checked(object sender, RoutedEventArgs e)
        {
            this.chkSpracheDeutsch.IsChecked = false;
            this.language = "en";
            System.Globalization.CultureInfo test = new System.Globalization.CultureInfo(language);
            Resource.Culture = new System.Globalization.CultureInfo(language);

            Window window = this;

            List<string> ControlNames = new List<string>();

            ResourceSet resourceSet = Resource.ResourceManager.GetResourceSet(test, true, true);
            foreach (DictionaryEntry entry in resourceSet)
            {
                string resourceKey = entry.Key.ToString();//Schlüssel = Name des Controls
                object resource = entry.Value;//=Content der labels
                ControlNames.Add(resourceKey.ToString());


            }


            foreach (Label lbl in TreeHelper.FindChildren<Label>(window))
            {
                if (ControlNames.Contains(lbl.Name))
                {
                    // do something with lbl here
                    string content = lbl.Name;
                    lbl.Content = Resource.ResourceManager.GetString(content, Resource.Culture);
                }

            }

            foreach (System.Windows.Controls.Button btn in TreeHelper.FindChildren<System.Windows.Controls.Button>(window))
            {
                if (ControlNames.Contains(btn.Name))
                {
                    // do something with btn here
                    string content = btn.Name;
                    btn.Content = Resource.ResourceManager.GetString(content, Resource.Culture);
                }
            }

            foreach (CheckBox chk in TreeHelper.FindChildren<CheckBox>(window))
            {
                if (ControlNames.Contains(chk.Name))
                {
                    // do something with chk here
                    string content = chk.Name;
                    chk.Content = Resource.ResourceManager.GetString(content, Resource.Culture);
                }
            }

            foreach (GroupBox grp in TreeHelper.FindChildren<GroupBox>(window))
            {
                if (ControlNames.Contains(grp.Name))
                {
                    // do something with grp here
                    string header = grp.Name;
                    grp.Header = Resource.ResourceManager.GetString(header, Resource.Culture);
                }
            }

            foreach (MenuItem mni in TreeHelper.FindChildren<MenuItem>(window))
            {
                if (ControlNames.Contains(mni.Name))
                {
                    // do something with mni here
                    string header = mni.Name;
                    mni.Header = Resource.ResourceManager.GetString(header, Resource.Culture);
                }
            }

            foreach (TabItem tabi in TreeHelper.FindChildren<TabItem>(window))
            {
                if (ControlNames.Contains(tabi.Name))
                {
                    // do something with mni here
                    string header = tabi.Name;
                    tabi.Header = Resource.ResourceManager.GetString(header, Resource.Culture);
                }
            }


        }

        private void chkSpracheDeutsch_Unchecked(object sender, RoutedEventArgs e)
        {
            this.chkSpracheEnglisch.IsChecked = true;
        }

        private void chkSpracheEnglisch_Unchecked(object sender, RoutedEventArgs e)
        {
            this.chkSpracheDeutsch.IsChecked = true;
        }   

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("Please select a style file, which will be used for rendering the layer!\n" +
                "If the style file is not loaded in the combobox, load the file into the Style Storage of ArcMap!\n" +
                "Therfore use the  menu 'Settings - Load Style file'!", "Style File Info", MessageBoxButton.OK);
        }

        private void chkAuthentifizierung_Checked(object sender, RoutedEventArgs e)
        {
            if (!this._isInitializing)
            {
                //this.chkSqlServer.IsChecked = true;
                this.txtUser.IsEnabled = true;
                this.txtPassword.IsEnabled = true;
            }
        }

        private void chkAuthentifizierung_Unchecked(object sender, RoutedEventArgs e)
        {
            this.txtUser.IsEnabled = false;
            this.txtPassword.IsEnabled = false;
        }

        private void mnuAppHelp_Click(object sender, RoutedEventArgs e)
        {
            FeatureRendererHelp frh = null;
            try
            {
                frh = new FeatureRendererHelp();
                //frh.ShowDialog();
                frh.Topmost = true;
                frh.Show();
            }
            catch
            {
                MessageBox.Show("Please restart the help window!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                frh.Close();
            }
        }
             
        private void chkAllLayers_Checked(object sender, RoutedEventArgs e)
        {
            this.lblLayername.IsEnabled = false;
            this.cboLayername.IsEnabled = false;

            if (cboTable.Text == "-- Please select a table --" || cboTable.Text == String.Empty)
            {
                this.btnRenderer.IsEnabled = false;
            }
            else
            {
                this.btnRenderer.IsEnabled = true;
            }

            
        }

        private void chkAllLayers_Unchecked(object sender, RoutedEventArgs e)
        {
            this.lblLayername.IsEnabled = true;
            this.cboLayername.IsEnabled = true;

            if ((cboLayername.Text == "-- Please select a layer --" || cboLayername.Text == String.Empty) ||
                    (cboTable.Text == "-- Please select a table --" || cboTable.Text == String.Empty))
            {
                this.btnRenderer.IsEnabled = false;
            }
            else
            {
                this.btnRenderer.IsEnabled = true;
            }
        }

        private void checkBox1_Checked(object sender, RoutedEventArgs e)
        {
            this.lblTrennzeichen.IsEnabled = true;
            this.cboTrennzeichen.IsEnabled = true;
            this.lblFeld1.IsEnabled = true;
            this.cboFeld1.IsEnabled = true;
            this.lblFeld2.IsEnabled = true;
            this.cboFeld2.IsEnabled = true;
            
            this.chkUeberschrift.IsEnabled = true;
        }

        private void checkBox1_Unchecked(object sender, RoutedEventArgs e)
        {
            this.lblTrennzeichen.IsEnabled = false;
            this.cboTrennzeichen.IsEnabled = false;
            this.lblFeld1.IsEnabled = false;
            this.cboFeld1.IsEnabled = false;
            this.lblFeld2.IsEnabled = false;
            this.cboFeld2.IsEnabled = false;
            this.lblUeberschrift.IsEnabled = false;
            this.cboUeberschrift.IsEnabled = false;
            this.chkUeberschrift.IsEnabled = false;
            //zweite Checkbox behandeln:
            this.chkUeberschrift.IsChecked = false;
        }

        private void chkUeberschrift_Checked(object sender, RoutedEventArgs e)
        {
            this.lblUeberschrift.IsEnabled = true;
            this.cboUeberschrift.IsEnabled = true;
        }

        private void chkUeberschrift_Unchecked(object sender, RoutedEventArgs e)
        {
            this.lblUeberschrift.IsEnabled = false;
            this.cboUeberschrift.IsEnabled = false;
        }

      

        private void mnuAppAbout_Click(object sender, RoutedEventArgs e)
        {
            DispatcherOperation op1 = Dispatcher.BeginInvoke(
                DispatcherPriority.Normal,
                new Action<bool>(SetWindowEnabled), false);

            try
            {
                aboutWindow = new AboutDialog();
                aboutWindow.Closed += new EventHandler(aboutWindow_Closed);
                aboutWindow.Topmost = true;
                aboutWindow.ShowDialog();
            }
            catch
            {
                MessageBox.Show("Please restart the about window!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                aboutWindow.Close();
            }
        }

        void aboutWindow_Closed(object sender, EventArgs e)
        {
            //SetWindowEnabled(true);
            DispatcherOperation op1 = Dispatcher.BeginInvoke(
                DispatcherPriority.Normal,
                new Action<bool>(SetWindowEnabled), true);
        }

        private void SetWindowEnabled(bool enabled)
        {
            this.IsEnabled = enabled;
        }

        private void mnuSymbol_Click(object sender, RoutedEventArgs e)
        {
            DispatcherOperation op1 = Dispatcher.BeginInvoke(
                DispatcherPriority.Normal,
                new Action<bool>(SetWindowEnabled), false);

            SymbolDialog sb = null;
            try
            {                
                sb = new SymbolDialog(this);
                sb.Closed += new EventHandler(aboutWindow_Closed);
                sb.Topmost = true;
                //ab.ShowDialog();
                sb.ShowDialog();//modal
                if (sb.BtnClicked == true)
                {
                    rendererToolManager.rendering(sender);
                    this.Cursor = System.Windows.Input.Cursors.Arrow;
                }
            }
            catch
            {
                MessageBox.Show("Please restart the about window!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                sb.Close();
            }
        }

        private void mnuLoadStylefile_Click(object sender, RoutedEventArgs e)
        {
            DispatcherOperation op1 = Dispatcher.BeginInvoke(
                DispatcherPriority.Normal,
                new Action<bool>(SetWindowEnabled), false);

            StylefileDialog stf = null;
            try
            {
                stf = new StylefileDialog(this);
                stf.Closed += new EventHandler(aboutWindow_Closed);
                stf.Topmost = true;
                //ab.ShowDialog();
                stf.ShowDialog();//modal
                if (stf.BtnClicked == true && File.Exists(stf.txtStyleFilePath.Text))
                {
                    string filename = stf.txtStyleFilePath.Text;
                    IStyleGallery pStyleGallery;
                    IStyleGalleryStorage pStyleGalleryStorage;
                    pStyleGallery = pMxDoc.StyleGallery;
                    pStyleGalleryStorage = (IStyleGalleryStorage)pStyleGallery;
                    pStyleGalleryStorage.AddFile(filename);
                    //refill the ComboBox for the Style files!!!
                    updateCboStyleFile(filename);
                }
            }
            catch
            {
                MessageBox.Show("Please restart the about window!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                stf.Close();
            }
        }

        //Projektfile speichern - Save:
        private void SaveObjectButton_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(this.pfadKonfigurationsdatei) == true)
            {
                // Create customer object based on Form values.
                FormularData formdata = this.CreateFormularData();

                //Save customer object to XML file using our ObjectXMLSerializer class...
                try
                {
                    ObjectXMLSerializer<FormularData>.Save(formdata, this.pfadKonfigurationsdatei);
                    //MessageBox.Show("Formular Data saved to XML file '" + this.pfadKonfigurationsdatei + "'!",
                    //   "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to save formular data object!" + Environment.NewLine + Environment.NewLine + ex.Message,
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                this.mnuAppSaveNew_Click(sender, e);//Menuepunkt Save As wird aufgerufen!!!
            }       
        }

        //Projektfile Speichern unter - Save As:
        private void mnuAppSaveNew_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog sXml = new Microsoft.Win32.SaveFileDialog();

            string dir = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
            sXml.InitialDirectory = dir;
            sXml.Title = "Select a folder for creating a new configurationfile!";
            sXml.Filter = "XML (*.xml)|*.xml";
            // OK wurde gedrückt:
            if (sXml.ShowDialog() == true)
            {
                this.pfadKonfigurationsdatei = sXml.FileName.ToString();
            }

            //if (File.Exists(this.txtKonfigurationsdatei.Text))
            if (this.pfadKonfigurationsdatei != String.Empty)
            {
                // Create customer object based on Form values.
                FormularData formdata = this.CreateFormularData();

                //Save customer object to XML file using our ObjectXMLSerializer class...
                try
                {
                    ObjectXMLSerializer<FormularData>.Save(formdata, this.pfadKonfigurationsdatei);
                    //MessageBox.Show("Formular Data saved to XML file '" + this.pfadKonfigurationsdatei + "'!",
                    //   "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    //this.staLblProjectfile.Content = this.pfadKonfigurationsdatei;
                    this.Title = "FeatureRenderer 5.0 für ArcMap 10.0 - " + this.pfadKonfigurationsdatei;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to save formular data object!" + Environment.NewLine + Environment.NewLine + ex.Message,
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void mnuAppLoad_Click(object sender, RoutedEventArgs e)
        {
            //this.btnKonfigurationsdatei_Click(sender, e);
            //Öffnen eines File Dialogs zur Auswahl der Konfigurationsdatei:
            Microsoft.Win32.OpenFileDialog oXml = new Microsoft.Win32.OpenFileDialog();
            oXml.Title = "Select XML-ConfigFile";
            oXml.Filter = "XML (*.xml)|*.xml";
            //oDlg.RestoreDirectory = true;
            string dir = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
            oXml.InitialDirectory = dir;

            // Show open file dialog box
            Nullable<bool> result = oXml.ShowDialog();

            //OK wurde gedrückt!
            if (result == true)
            {
                this.pfadKonfigurationsdatei = oXml.FileName.ToString();
            }

            //Konfigurationen laden:
            // Load the customer object from the existing XML file (if any)...
            if (File.Exists(this.pfadKonfigurationsdatei) == true)
            {
                // Load the customer object from the XML file using our custom class...
                FormularData formdata = ObjectXMLSerializer<FormularData>.Load(this.pfadKonfigurationsdatei);

                if (formdata == null)
                {
                    MessageBox.Show("Unable to load a formular data object from file '" + this.pfadKonfigurationsdatei + "'!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.pfadKonfigurationsdatei = null;
                }
                else  // Load customer properties into the form...
                {
                    this.LoadCustomerIntoForm(formdata);
                    //MessageBox.Show("Formular data loaded from file '" + this.pfadKonfigurationsdatei + "'!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    //show in the statusbar which projectfile is loaded:
                    //this.staLblProjectfile.Content = this.pfadKonfigurationsdatei;
                    this.Title = "FeatureRenderer 5.0 für ArcMap 10.0 - " + this.pfadKonfigurationsdatei;
                }
            }
            else
            {
                MessageBox.Show(this.CreateFileDoesNotExistMsg(), "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void btnLoadAccessTables_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(this.txtAccessdatenbank.Text) == true)
            {
                //controller comes from the business layer:
                IFeatureRendererController controller = new FeatureRendererController();                
                //List<string> Tables = DatabaseInfoCollector.GetTables(this.txtAccessdatenbank.Text);
                List<string> Tables = controller.GetTables(this.txtAccessdatenbank.Text);
                Tables.Insert(0, "-- Please select a table --");
                //this.cboTable.DataContext = Tables;//Neu bei WPF
                cboTable.ItemsSource = null;
                this.cboTable.Items.Clear();
                this.cboTable.ItemsSource = Tables;//Neu bei WPF                
                cboTable.SelectedIndex = 0;
            }
            else
            {
                MessageBox.Show("Please define a correct Access database!", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void chkAccess_Checked(object sender, RoutedEventArgs e)
        {
            if (!this._isInitializing)
            {
                if (this.chkAccess.IsChecked == true)
                {
                    this.txtAccessdatenbank.IsEnabled = true;
                    this.btnLoadAccessDb.IsEnabled = true;
                    this.btnLoadAccessTables.IsEnabled = true;
                    this.chkSde.IsChecked = false;
                    this.chkAuthentifizierung.IsChecked = false;//neu
                    this.chkAuthentifizierung.IsEnabled = false;//neu
                    this.txtServer.IsEnabled = false;
                    this.txtInstance.IsEnabled = false;
                    this.txtDatabase.IsEnabled = false;
                    this.txtVersion.IsEnabled = false;
                    this.txtUser.IsEnabled = false;
                    this.txtPassword.IsEnabled = false;
                    this.btnLoadSqlServerTables.IsEnabled = false;
                }
                else
                {
                    this.chkSde.IsChecked = true;
                }
            }
        }

        private void chkAccess_Unchecked(object sender, RoutedEventArgs e)
        {
            this.chkSde.IsChecked = true;
        }

        private void chkSde_Checked(object sender, RoutedEventArgs e)
        {
            if (!this._isInitializing)
            {
                if (this.chkSde.IsChecked == true)
                {
                    this.txtServer.IsEnabled = true;
                    this.txtInstance.IsEnabled = true;
                    this.txtDatabase.IsEnabled = true;
                    this.txtVersion.IsEnabled = true;
                    this.btnLoadSqlServerTables.IsEnabled = true;
                    this.chkAccess.IsChecked = false;
                    this.txtAccessdatenbank.IsEnabled = false;
                    this.btnLoadAccessDb.IsEnabled = false;
                    this.btnLoadAccessTables.IsEnabled = false;
                    this.chkAuthentifizierung.IsEnabled = true;
                }
                else
                {
                    this.chkAccess.IsChecked = true;
                }
            }
        }

        private void chkSde_Unchecked(object sender, RoutedEventArgs e)
        {
            this.chkAccess.IsChecked = true;
        }

        private void InfoImage_MouseEnter(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Help;
            PersonInfoPopup.IsOpen = !PersonInfoPopup.IsOpen;
        }

        private void InfoImage_MouseLeave(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Arrow;
            PersonInfoPopup.IsOpen = !PersonInfoPopup.IsOpen;
        }

        private void cboLayername_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboTable.SelectedItem != null && cboLayername.SelectedItem != null)
            {

                if ((((MyLayer)cboLayername.SelectedItem).Name == "-- Please select a layer --" ||
                    (cboTable.SelectedItem.ToString() == "-- Please select a table --" || cboTable.SelectedItem.ToString() == String.Empty)) && chkAllLayers.IsChecked == false)
                {
                    this.btnRenderer.IsEnabled = false;
                }
                else
                {
                    this.btnRenderer.IsEnabled = true;
                }
            }
            else if (cboLayername.SelectedItem != null)
            {
                if ((((MyLayer)cboLayername.SelectedItem).Name == "-- Please select a layer --" ||
                    (cboTable.Text == "-- Please select a table --" || cboTable.Text == String.Empty)) && chkAllLayers.IsChecked == false)
                {
                    this.btnRenderer.IsEnabled = false;
                }
                else
                {
                    this.btnRenderer.IsEnabled = true;
                }
            }


        }

        #endregion

        #region class helper methods

        private string CreateFileDoesNotExistMsg()
        {
            return "The  XML file '" + this.pfadKonfigurationsdatei + "' does not exist.";

        }

        int GetItemIndex(List<string> _list, string search)
        {
            int found = -1;
            int found2 = 0;
            if (_list != null)
            {
                foreach (string item in _list) // _list is an instance of List<string>
                {
                    found++;
                    //wenn FILL_SYMBOL, LINE_SYMBOL oder MARKER_SYMBOL gefunden wird:
                    if (item == search || item == search.ToLower())
                    {
                        found2 = found;
                        break;
                    }
                    else
                    {
                        //ansonsten soll der erste Eintrag selektiert werden:
                        found2 = 0;
                    }
                }
            }
            return found2;
        }

        private void updateCboStyleFile()
        {
            cboStyleFile.Items.Clear();
            //Styles in die Combobox laden:
            IStyleGallery pStyleGallery;
            IStyleGalleryStorage pStyleGalleryStorage;
            pStyleGallery = pMxDoc.StyleGallery;
            pStyleGalleryStorage = (IStyleGalleryStorage)pStyleGallery;
            for (int i = 0; i < pStyleGalleryStorage.FileCount; i++)
            {
                cboStyleFile.Items.Add(pStyleGalleryStorage.get_File(i));
                //cboStyleFile.SelectedIndex =1;
            }
            cboStyleFile.SelectedIndex = 0;
        }

        private void updateCboStyleFile(string fileName)
        {
            cboStyleFile.Items.Clear();
            //Styles in die Combobox laden:
            IStyleGallery pStyleGallery;
            IStyleGalleryStorage pStyleGalleryStorage;
            pStyleGallery = pMxDoc.StyleGallery;
            pStyleGalleryStorage = (IStyleGalleryStorage)pStyleGallery;
            for (int i = 0; i < pStyleGalleryStorage.FileCount; i++)
            {
                cboStyleFile.Items.Add(pStyleGalleryStorage.get_File(i));
            }
            cboStyleFile.SelectedIndex = cboStyleFile.Items.IndexOf(fileName);
        }

        private void updateCboLayername()
        {          
            cboLayername.ItemsSource = null;
            cboLayername.Items.Clear();           
            //Verfügbare FeatureLayer in die Combobox laden:           
            pEnumLayer = pMap.Layers;
            if (pEnumLayer == null)
            {
                return;
            }
            pLayer = pEnumLayer.Next();
            List<MyLayer> myLayers = new List<MyLayer>();
            int position = 0;

            MyLayer d = new MyLayer();
            d.Name = "-- Please select a layer --";
            d.Layer = null;
            d.Position = position;
            myLayers.Add(d);
            //cboLayername.Items.Add("-- Please select a layer --");
          
            while (pLayer != null)
            {
                //save position of each layer in the map collection:
                position = position + 1;

                if (pLayer is IFeatureLayer && pLayer.Visible == true)
                {
                    IFeatureLayer pFLayer = (IFeatureLayer)pLayer;
                    //Get the Symbology from the layer
                    IGeoFeatureLayer pGeoLayer;
                    IUniqueValueRenderer pUVRenderer = null;
                    
                    if (pFLayer.FeatureClass != null && pFLayer.FeatureClass.FeatureType == esriFeatureType.esriFTSimple && pFLayer.FeatureClass.CLSID != null)
                    {
                        pGeoLayer = (IGeoFeatureLayer)pFLayer;
                        pUVRenderer = pGeoLayer.Renderer as IUniqueValueRenderer;
                    }
                    if (pUVRenderer != null)
                    {
                        MyLayer l = new MyLayer();
                        l.Name = pLayer.Name;
                        l.Layer = pLayer;
                        l.Position = position;
                        myLayers.Add(l);
                    }
                   
                }
                pLayer = pEnumLayer.Next();
            }
            cboLayername.ItemsSource = myLayers;           
            //cboLayername.DisplayMemberPath = "FullName";
            cboLayername.SelectedIndex = 0;
          
        }

        #region XML-helper functions:

        private FormularData CreateFormularData()
        {
            FormularData formdata = new FormularData();

            //allgemeine Attribute:
            formdata.DateTimeValue = System.DateTime.Now;
            formdata.ClassPfadKonfigurationsdatei = this.pfadKonfigurationsdatei;

            //boolesche Attribute:
            formdata.ChkKeinUmriss = chkUmriss.IsChecked.Value;
            formdata.ChkNurHintergrund = chkHintergrund.IsChecked.Value;
            formdata.ChkRenderingAllLayers = chkAllLayers.IsChecked.Value;
            formdata.TabAccess = tabItemAccess.IsSelected;
            formdata.TabSde = tabItemSde.IsSelected;
            formdata.TabDatenbankspalten = tabItemDatabase.IsSelected;
            formdata.TabOptionaleDatenbankspalten = tabItemDatabaseOptional.IsSelected;
            formdata.ChkUseSqlAuthentifizierung = chkAuthentifizierung.IsChecked.Value;
            formdata.ChkFormularAccess = chkAccess.IsChecked.Value;
            formdata.ChkFormularSde = chkSde.IsChecked.Value;
            formdata.ChkFormularDirectConnect = chkDirectConnect.IsChecked.Value;
            //boolesche Attribute für die optionalen Datenbankspalten:
            formdata.ChkLegendentextVeraendern = checkBox1.IsChecked.Value;
            formdata.ChkOptionalUeberschrift = chkUeberschrift.IsChecked.Value;

            //Attribute fuer die Datenbankverbindung   
            formdata.SdeServername = this.txtServer.Text;
            formdata.SdeInstanzname = this.txtInstance.Text;
            formdata.SdeDatenbankname = this.txtDatabase.Text;
            formdata.SdeUsername = this.txtUser.Text;
            formdata.SdePasswordname = this.txtPassword.Text;
            formdata.SdeVersionsname = this.txtVersion.Text;
            formdata.AccessFilename = this.txtAccessdatenbank.Text;

            //Álle Tabellenspalten zu einer Datenbankverbindung:
            foreach (string Item in this.cboTable.Items)
            {
                formdata.LegendTables.Add(Item);
                //customer.Hobbies.Add(Item);
            }
            formdata.LegendTable = cboTable.Text;

            //Tabellenspalten im xml-File abspeichern:
            foreach (string Item in this.cboFlaeche.Items)
            {
                formdata.ColumnNames.Add(Item);
            }
            formdata.IdName = cboVerknuepfung.Text;
            formdata.Flaechensymbolname = cboFlaeche.Text;
            formdata.Liniensymbolname = cboLinie.Text;
            formdata.Markersymbolname = cboMarker.Text;

            //stylefile-Combobox abspeichern:
            foreach (string Item in this.cboStyleFile.Items)
            {
                formdata.StyleFiles.Add(Item);
            }
            formdata.StyleFile = cboStyleFile.Text;

            //cboLayername-Combobox abspeichern:
            //foreach (string Item in this.cboLayername.Items)
            //{
            //    formdata.LayerFiles.Add(Item);
            //}
            foreach (MyLayer item in this.cboLayername.Items)
            {
                formdata.LayerFiles.Add(item.Name);
            }
            formdata.LayerFile = cboLayername.Text;


            //Attribute für die Aufdruckfarben:
            formdata.Roc = this.txtRoc.Text; formdata.Rom = this.txtRom.Text; formdata.Roy = this.txtRoy.Text; formdata.Rok = this.txtRok.Text;
            formdata.Blc = this.txtBlc.Text; formdata.Blm = this.txtBlm.Text; formdata.Bly = this.txtBly.Text; formdata.Blk = this.txtBlk.Text;
            formdata.Grc = this.txtGrc.Text; formdata.Grm = this.txtGrm.Text; formdata.Gry = this.txtGry.Text; formdata.Grk = this.txtGrk.Text;
            formdata.Brc = this.txtBrc.Text; formdata.Brm = this.txtBrm.Text; formdata.Bry = this.txtBry.Text; formdata.Brk = this.txtBrk.Text;
            formdata.Gac = this.txtGac.Text; formdata.Gam = this.txtGam.Text; formdata.Gay = this.txtGay.Text; formdata.Gak = this.txtGak.Text;
            formdata.Mac = this.txtMac.Text; formdata.Mam = this.txtMam.Text; formdata.May = this.txtMay.Text; formdata.Mak = this.txtMak.Text;
            formdata.Cyc = this.txtCyc.Text; formdata.Cym = this.txtCym.Text; formdata.Cyy = this.txtCyy.Text; formdata.Cyk = this.txtCyk.Text;
            formdata.Cyc = this.txtCyc.Text; formdata.Cym = this.txtCym.Text; formdata.Cyy = this.txtCyy.Text; formdata.Cyk = this.txtCyk.Text;
            formdata.Gec = this.txtGec.Text; formdata.Gem = this.txtGem.Text; formdata.Gey = this.txtGey.Text; formdata.Gek = this.txtGek.Text;
            formdata.Orc = this.txtOrc.Text; formdata.Orm = this.txtOrm.Text; formdata.Ory = this.txtOry.Text; formdata.Ork = this.txtOrk.Text;

            //Attribute für die optionalen Datenbankspalten:
            formdata.Trennzeichen = this.cboTrennzeichen.Text;
            formdata.Feld1 = this.cboFeld1.Text;
            formdata.Feld2 = this.cboFeld2.Text;
            formdata.Ueberschrift = this.cboUeberschrift.Text;

            return formdata;
        }

        private void LoadCustomerIntoForm(FormularData formdata)
        {
            //boolesche Attribute:
            chkAccess.IsChecked = formdata.ChkFormularAccess;
            chkSde.IsChecked = formdata.ChkFormularSde;
            chkDirectConnect.IsChecked = formdata.ChkFormularDirectConnect;
            chkUmriss.IsChecked = formdata.ChkKeinUmriss;
            chkHintergrund.IsChecked = formdata.ChkNurHintergrund;
            chkAllLayers.IsChecked = formdata.ChkRenderingAllLayers;
            tabItemAccess.IsSelected = formdata.TabAccess;
            tabItemSde.IsSelected = formdata.TabSde;
            tabItemDatabase.IsSelected = formdata.TabDatenbankspalten;
            tabItemDatabaseOptional.IsSelected = formdata.TabOptionaleDatenbankspalten;
            chkAuthentifizierung.IsChecked = formdata.ChkUseSqlAuthentifizierung;

            //boolesche Attribute für die optionalen Datenbankspalten:
            checkBox1.IsChecked = formdata.ChkLegendentextVeraendern;
            chkUeberschrift.IsChecked = formdata.ChkOptionalUeberschrift;

            //allgemeine Attribute:
            this.pfadKonfigurationsdatei = formdata.ClassPfadKonfigurationsdatei;

            //Attribute fuer die Datenbankverbindung   
            this.txtServer.Text = formdata.SdeServername;
            this.txtInstance.Text = formdata.SdeInstanzname;
            this.txtDatabase.Text = formdata.SdeDatenbankname;
            this.txtUser.Text = formdata.SdeUsername;
            this.txtPassword.Text = formdata.SdePasswordname;
            this.txtVersion.Text = formdata.SdeVersionsname;
            this.txtAccessdatenbank.Text = formdata.AccessFilename;

            //LegendTables:
            this.cboTable.ItemsSource = null;//.Clear();
            cboTable.Items.Clear();
            foreach (string Item in formdata.LegendTables)
            {
                this.cboTable.Items.Add(Item);
            }
            this.cboTable.Text = formdata.LegendTable;

            //VerknuepfungsID:
            this.cboVerknuepfung.ItemsSource = null;//.Clear();
            cboVerknuepfung.Items.Clear();
            foreach (string Item in formdata.ColumnNames)
            {
                this.cboVerknuepfung.Items.Add(Item);
            }
            this.cboVerknuepfung.Text = formdata.IdName;

            //CboFlaeche:
            this.cboFlaeche.ItemsSource = null;//.Clear();
            cboFlaeche.Items.Clear();
            foreach (string Item in formdata.ColumnNames)
            {
                this.cboFlaeche.Items.Add(Item);
            }
            this.cboFlaeche.Text = formdata.Flaechensymbolname;

            //CboLinie:
            this.cboLinie.ItemsSource = null;//.Clear();
            cboLinie.Items.Clear();
            foreach (string Item in formdata.ColumnNames)
            {
                this.cboLinie.Items.Add(Item);
            }
            this.cboLinie.Text = formdata.Liniensymbolname;

            //CboMarker:
            this.cboMarker.ItemsSource = null;//.Clear();
            cboMarker.Items.Clear();
            foreach (string Item in formdata.ColumnNames)
            {
                this.cboMarker.Items.Add(Item);
            }
            this.cboMarker.Text = formdata.Markersymbolname;

            if (cboVerknuepfung.Items.Count != 0)
            {
                this.cboMarker.IsEnabled = true;
                this.cboLinie.IsEnabled = true;
                this.cboFlaeche.IsEnabled = true;
                this.cboVerknuepfung.IsEnabled = true;
            }
            else
            {
                this.cboMarker.IsEnabled = false;
                this.cboLinie.IsEnabled = false;
                this.cboFlaeche.IsEnabled = false;
                this.cboVerknuepfung.IsEnabled = false;
            }


            //Attribute für die Aufdruckfarben:
            this.txtRoc.Text = formdata.Roc; this.txtRom.Text = formdata.Rom; this.txtRoy.Text = formdata.Roy; this.txtRok.Text = formdata.Rok;
            this.txtBlc.Text = formdata.Blc; this.txtBlm.Text = formdata.Blm; this.txtBly.Text = formdata.Bly; this.txtBlk.Text = formdata.Blk;
            this.txtGrc.Text = formdata.Grc; this.txtGrm.Text = formdata.Grm; this.txtGry.Text = formdata.Gry; this.txtGrk.Text = formdata.Grk;
            this.txtBrc.Text = formdata.Brc; this.txtBrm.Text = formdata.Brm; this.txtBry.Text = formdata.Bry; this.txtBrk.Text = formdata.Brk;
            this.txtGac.Text = formdata.Gac; this.txtGam.Text = formdata.Gam; this.txtGay.Text = formdata.Gay; this.txtGak.Text = formdata.Gak;
            this.txtMac.Text = formdata.Mac; this.txtMam.Text = formdata.Mam; this.txtMay.Text = formdata.May; this.txtMak.Text = formdata.Mak;
            this.txtCyc.Text = formdata.Cyc; this.txtCym.Text = formdata.Cym; this.txtCyy.Text = formdata.Cyy; this.txtCyk.Text = formdata.Cyk;
            this.txtCyc.Text = formdata.Cyc; this.txtCym.Text = formdata.Cym; this.txtCyy.Text = formdata.Cyy; this.txtCyk.Text = formdata.Cyk;
            this.txtGec.Text = formdata.Gec; this.txtGem.Text = formdata.Gem; this.txtGey.Text = formdata.Gey; this.txtGek.Text = formdata.Gek;
            this.txtOrc.Text = formdata.Orc; this.txtOrm.Text = formdata.Orm; this.txtOry.Text = formdata.Ory; this.txtOrk.Text = formdata.Ork;

            //StyleFile-Combobox befüllen: 
            this.cboStyleFile.ItemsSource = null;//.Clear();
            cboStyleFile.Items.Clear();
            foreach (string Item in formdata.StyleFiles)
            {
                //mit if überprüfen, ob das Stylefile physikalisch auf der Platte noch existiert, ansonsten wird abgebrochen:
                if (File.Exists(Item))
                {
                    this.cboStyleFile.Items.Add(Item);
                }
                //else
                //{
                //    MessageBox.Show(Item + " existiert nicht mehr!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                //}
            }
            this.cboStyleFile.Text = formdata.StyleFile;

            //Layerfiles auffüllen:
            //this.cboLayername.ItemsSource = null;//.Clear();
            //cboLayername.Items.Clear();
            //foreach (string Item in formdata.LayerFiles)
            //{
            //    this.cboLayername.Items.Add(Item);               
            //}
            //this.cboLayername.Text = formdata.LayerFile;

            //Attribute für die optionalen Datenbankspalten:
            this.cboTrennzeichen.Text = formdata.Trennzeichen;
            this.cboFeld1.Text = formdata.Feld1;
            this.cboFeld2.Text = formdata.Feld2;
            this.cboUeberschrift.Text = formdata.Ueberschrift;
        }
        #endregion //XML-helper functions

        #endregion

        

        //Statische Klassenvariablen, damit bei erneutem Öffnen die vorgenommenen Einstellungen wieder geladen werden:
        //mittels den Methoden ReloadSettings() & SaveSettings():
        //Datenbankspalten:
        static string legendentabelle, leg_ID_feld;      
        static private string txtFlaecheStatic, txtLinieStatic, txtMarkerStatic;
        //Access-Datenbank:
        static private string leg_tab_pfad;
        static private bool chkAccessStatic = true;
        //Sde-Datenbank:
        static private bool chkSdeStatic = false, chkAuthentifizierungStatic = false, chkSdeDirectConnectStatic = false;
        static private string txtServerStatic, txtDatabaseStatic, txtInstanceStatic, txtUserStatic, txtPasswordStatic, txtVersionStatic;
        //Schmuckfarben:
        static private string txtRocStatic, txtRomStatic, txtRoyStatic, txtRokStatic,
                              txtBlcStatic, txtBlmStatic, txtBlyStatic, txtBlkStatic,
                              txtGrcStatic, txtGrmStatic, txtGryStatic, txtGrkStatic,
                              txtBrcStatic, txtBrmStatic, txtBryStatic, txtBrkStatic,
                              txtGacStatic, txtGamStatic, txtGayStatic, txtGakStatic,
                              txtMacStatic, txtMamStatic, txtMayStatic, txtMakStatic,
                              txtCycStatic, txtCymStatic, txtCyyStatic, txtCykStatic,
                              txtGecStatic, txtGemStatic, txtGeyStatic, txtGekStatic,
                              txtOrcStatic, txtOrmStatic, txtOryStatic, txtOrkStatic;
        //Sonstiges:
        static private string cboStyleFileStatic, cboLayernameStatic;
        static private bool? chkHintergrundStatic, chkUmrissStatic, chkAllLayersStatic;

        //optionale Datenbankspalten:
        static private string cboTrennzeichenStatic, cboFeld1Static, cboFeld2Static, cboUeberschriftStatic;
        static private bool? checkBox1Static, chkUeberschriftStatic;

        private void ReloadSettings()
        {
            //Datenbankspalten:
            if (legendentabelle != null)
            { cboTable.Text = legendentabelle; }
            if (leg_ID_feld != null)
            { 
                cboVerknuepfung.Text = leg_ID_feld;
                cboVerknuepfung.IsEnabled = true;
            }
            if (txtFlaecheStatic != null)
            { 
                cboFlaeche.Text = txtFlaecheStatic;
                cboFlaeche.IsEnabled = true;
            }
            if (txtLinieStatic != null)
            { 
                cboLinie.Text = txtLinieStatic;
                cboLinie.IsEnabled = true;
            }
            if (txtMarkerStatic != null)
            { 
                cboMarker.Text = txtMarkerStatic;
                cboMarker.IsEnabled = true;
            }

            //Access-Datenbank
            if (leg_tab_pfad != null)
            { txtAccessdatenbank.Text = leg_tab_pfad; }
            chkAccess.IsChecked = chkAccessStatic;
            //Sde-Datenbank:
            chkSde.IsChecked = chkSdeStatic;
            chkAuthentifizierung.IsChecked = chkAuthentifizierungStatic;
            chkDirectConnect.IsChecked = chkSdeDirectConnectStatic;
            if (txtServerStatic != null)
            { txtServer.Text = txtServerStatic; }
            if (txtInstanceStatic != null)
            { txtInstance.Text = txtInstanceStatic; }
            if (txtDatabaseStatic != null)
            { txtDatabase.Text = txtDatabaseStatic; }
            if (txtUserStatic != null)
            { txtUser.Text = txtUserStatic; }
            if (txtPasswordStatic != null)
            { txtPassword.Text = txtPasswordStatic; }
            if (txtVersionStatic != null)
            { txtVersion.Text = txtVersionStatic; }

            //Schmuckfarben:
            if (txtRocStatic != null)
            {
                txtRoc.Text = txtRocStatic; txtRom.Text = txtRomStatic; txtRoy.Text = txtRoyStatic; txtRok.Text = txtRokStatic;
                txtBlc.Text = txtBlcStatic; txtBlm.Text = txtBlmStatic; txtBly.Text = txtBlyStatic; txtBlk.Text = txtBlkStatic;
                txtGrc.Text = txtGrcStatic; txtGrm.Text = txtGrmStatic; txtGry.Text = txtGryStatic; txtGrk.Text = txtGrkStatic;
                txtBrc.Text = txtBrcStatic; txtBrm.Text = txtBrmStatic; txtBry.Text = txtBryStatic; txtBrk.Text = txtBrkStatic;
                txtGac.Text = txtGacStatic; txtGam.Text = txtGamStatic; txtGay.Text = txtGayStatic; txtGak.Text = txtGakStatic;
                txtMac.Text = txtMacStatic; txtMam.Text = txtMamStatic; txtMay.Text = txtMayStatic; txtMak.Text = txtMakStatic;
                txtCyc.Text = txtCycStatic; txtCym.Text = txtCymStatic; txtCyy.Text = txtCyyStatic; txtCyk.Text = txtCykStatic;
                txtGec.Text = txtGecStatic; txtGem.Text = txtGemStatic; txtGey.Text = txtGeyStatic; txtGek.Text = txtGekStatic;
                txtOrc.Text = txtOrcStatic; txtOrm.Text = txtOrmStatic; txtOry.Text = txtOryStatic; txtOrk.Text = txtOrkStatic;
            }

            //Sonstiges:
            if (cboStyleFileStatic != null)
            { cboStyleFile.Text = cboStyleFileStatic; }
            //if (cboLayernameStatic != null)
            //{ cboLayername.Text = cboLayernameStatic; }
            if (chkHintergrundStatic.HasValue)
            { chkHintergrund.IsChecked = chkHintergrundStatic; }
            if (chkUmrissStatic.HasValue)
            { chkUmriss.IsChecked = chkUmrissStatic; }
            if (chkAllLayersStatic.HasValue)
            { chkAllLayers.IsChecked = chkAllLayersStatic; } 

            //optionale Datenbankspalten:
            if (cboTrennzeichenStatic != null)
            { cboTrennzeichen.Text = cboTrennzeichenStatic; }
            if (cboFeld1Static != null)
            { cboFeld1.Text = cboFeld1Static; }
            if (cboFeld2Static != null)
            { cboFeld2.Text = cboFeld2Static; }
            if (cboUeberschriftStatic != null)
            { cboUeberschrift.Text = cboUeberschriftStatic; }
            if (checkBox1Static.HasValue)
            { checkBox1.IsChecked = checkBox1Static; }
            if (chkUeberschriftStatic.HasValue)
            { chkUeberschrift.IsChecked = chkUeberschriftStatic; }            
        
        }

        private void SaveSettings()
        {
            //Datenbankspalten:
            legendentabelle = cboTable.Text;
            leg_ID_feld = cboVerknuepfung.Text;
            txtFlaecheStatic = cboFlaeche.Text;
            txtLinieStatic = cboLinie.Text;
            txtMarkerStatic = cboMarker.Text;

            //Access-Datenbank:
            leg_tab_pfad = txtAccessdatenbank.Text;
            chkAccessStatic = chkAccess.IsChecked.Value;
            //Sde-Datenbank:
            chkSdeStatic = chkSde.IsChecked.Value;
            chkAuthentifizierungStatic = chkAuthentifizierung.IsChecked.Value;
            chkSdeDirectConnectStatic = chkDirectConnect.IsChecked.Value;
            txtServerStatic = txtServer.Text;
            txtInstanceStatic = txtInstance.Text;
            txtDatabaseStatic = txtDatabase.Text;
            txtUserStatic = txtUser.Text;
            txtPasswordStatic = txtPassword.Text;
            txtVersionStatic = txtVersion.Text;

            //Schmuckfarben:
            txtRocStatic = txtRoc.Text; txtRomStatic = txtRom.Text; txtRoyStatic = txtRoy.Text; txtRokStatic = txtRok.Text;
            txtBlcStatic = txtBlc.Text; txtBlmStatic = txtBlm.Text; txtBlyStatic = txtBly.Text; txtBlkStatic = txtBlk.Text;
            txtGrcStatic = txtGrc.Text; txtGrmStatic = txtGrm.Text; txtGryStatic = txtGry.Text; txtGrkStatic = txtGrk.Text;
            txtBrcStatic = txtBrc.Text; txtBrmStatic = txtBrm.Text; txtBryStatic = txtBry.Text; txtBrkStatic = txtBrk.Text;
            txtGacStatic = txtGac.Text; txtGamStatic = txtGam.Text; txtGayStatic = txtGay.Text; txtGakStatic = txtGak.Text;
            txtMacStatic = txtMac.Text; txtMamStatic = txtMam.Text; txtMayStatic = txtMay.Text; txtMakStatic = txtMak.Text;
            txtCycStatic = txtCyc.Text; txtCymStatic = txtCym.Text; txtCyyStatic = txtCyy.Text; txtCykStatic = txtCyk.Text;
            txtGecStatic = txtGec.Text; txtGemStatic = txtGem.Text; txtGeyStatic = txtGey.Text; txtGekStatic = txtGek.Text;
            txtOrcStatic = txtOrc.Text; txtOrmStatic = txtOrm.Text; txtOryStatic = txtOry.Text; txtOrkStatic = txtOrk.Text;

            //Sonstiges:
            cboStyleFileStatic = cboStyleFile.Text;
            cboLayernameStatic = cboLayername.Text;
            chkHintergrundStatic = chkHintergrund.IsChecked.Value;
            chkUmrissStatic = chkUmriss.IsChecked.Value;
            chkAllLayersStatic = chkAllLayers.IsChecked.Value;

            //optionale Datenbankspalten:
            cboTrennzeichenStatic = cboTrennzeichen.Text;
            cboFeld1Static = cboFeld1.Text;
            cboFeld2Static = cboFeld2.Text;
            cboUeberschriftStatic = cboUeberschrift.Text;
            checkBox1Static = checkBox1.IsChecked.Value;
            chkUeberschriftStatic = chkUeberschrift.IsChecked.Value;
            
        }

        private void Window_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //save the static variables for a new re-opening of the window:
            this.SaveSettings();

            //removing all esri-events:
            RemoveActiveViewEvents(pMap);
            RemoveSetUpDocumentEvents((IDocument)pMxDoc);
        }

        

    }

    

}
