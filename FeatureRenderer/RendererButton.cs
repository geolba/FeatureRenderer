using System;
using System.Windows.Forms;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;


//neu:
using System.Windows.Forms.Integration;//for enabling keyboard input
using WpfRendererForm;


namespace FeatureRenderer
{
    public class RendererButton : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        #region class members:

        private IApplication m_application;

        #endregion

        #region constructors:

        public RendererButton()
        {
            m_application = ArcMap.Application;
            //this.OnUpdate();
        }

        #endregion

        #region methods:

        protected override void OnClick()
        {
            //pMxDoc = (IMxDocument)m_application.Document;
            IMxDocument pMxDoc = ArcMap.Document;
            IActiveView pActiveView = pMxDoc.ActiveView;
            IMap pMap = pActiveView.FocusMap;
            //unique value Renderer:
            IEnumLayer pEnumLayer = pMap.Layers;
            ILayer pLayer = pEnumLayer.Next();
            int i = 0;
            //check only for featurelayers:
            while (pLayer != null)
            {
                if (pLayer is IFeatureLayer)
                {
                   i = i + 1;
                }
                pLayer = pEnumLayer.Next();
            }

            // Fail if map has no layers.
             if (pMap.LayerCount != 0 && i != 0)
             {
                 try
                 {                     
                     
                     FeatureRendererWindow wpfwindow = new FeatureRendererWindow (m_application);
                     //Enable Keyboard Input:
                     ElementHost.EnableModelessKeyboardInterop(wpfwindow);
                     //wpfwindow.ShowDialog();//modal
              
                     System.Windows.Interop.WindowInteropHelper helper = new System.Windows.Interop.WindowInteropHelper(wpfwindow);
                     //helper.Owner = winFormWindow.Handle;
                     helper.Owner = (IntPtr)ArcMap.Application.hWnd;//winFormWindow.Handle.
                     wpfwindow.Show();
                     //wpfwindow.Show(NativeWindow.FromHandle(new IntPtr(ArcMap.Application.hWnd)));

                     ArcMap.Application.CurrentTool = null;
                 }
                 catch 
                 {
                     //System.Diagnostics.Trace.WriteLine(ex.Message);
                     MessageBox.Show("allgemeiner Fehler", "Fehler");
                 }
             }
             else
             {
                 MessageBox.Show("Please load a Feature Layer and load the unique value renderer!");
             }
            
            
            ArcMap.Application.CurrentTool = null;
        }
            
        protected override void OnUpdate()//to update the enabled state
        {
            //pMxDoc = (IMxDocument)m_application.Document;
            IMxDocument pMxDoc = ArcMap.Document;
            IActiveView pActiveView = pMxDoc.ActiveView;
            IMap pMap = pActiveView.FocusMap;

            // Bail if map has no layers.
            if (pMap.LayerCount != 0)
            {
                this.Enabled = true;
            }
            else
            {
                this.Enabled = false;
            }

            IEnumLayer pEnumLayer = pMap.Layers;
            ILayer pLayer = pEnumLayer.Next();
            int i = 0;
            //check only for featurelayers:
            while (pLayer != null)
            {
                if (pLayer is IFeatureLayer)
                {
                    i = i + 1;
                }
                pLayer = pEnumLayer.Next();
            }
            if (i == 0)
            {
                this.Enabled = false;
            }

            if (ArcMap.Application == null)
            {
                this.Enabled = true;
            }
        }

        #endregion

    }
}
