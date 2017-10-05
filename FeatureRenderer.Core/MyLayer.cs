using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace FeatureRenderer.Core
{
    //Class to store an attribute and an esri layer
    public class MyLayer : INotifyPropertyChanged
    {
        private string name;
        private int position;
        private ESRI.ArcGIS.Carto.ILayer layer;


        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public int Position
        {
            get { return position; }
            set { position = value; }
        }
 
        public ESRI.ArcGIS.Carto.ILayer Layer
        {
            get { return layer; }
            set { layer = value; }
        }

        public string FullName
        {
            get
            {
                return name + " (" + position.ToString() + ")";
            }
        }

        public override string ToString()
        {
            return name + " " + position.ToString();
        }

        #region INotifyPropertyChanged Member

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged<TValue>(System.Linq.Expressions.Expression<Func<MyLayer, TValue>> propertySelector)
        {
            if (PropertyChanged != null)
            {
                var memberExpression = propertySelector.Body as System.Linq.Expressions.MemberExpression;
                if (memberExpression != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(memberExpression.Member.Name));
                }
            }
        }
        #endregion
    }
}
