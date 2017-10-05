using System;
using System.Collections.Generic;
using ESRI.ArcGIS.Geodatabase;

namespace FeatureRenderer.Core
{
    public class CursorGS : IEnumerator<IRow>, IEnumerable<IRow>, IDisposable
    {
        ITable _table;
        IQueryFilter _queryFilter;

        //using ComReleaser to manage the lifetime of cursor in .NET:
        ESRI.ArcGIS.ADF.ComReleaser _comReleaser;

        IRow _currentRow = null;
        ICursor _cursor = null;



        public CursorGS(ITable table, IQueryFilter queryFilter)
        {
            _table = table;
            _queryFilter = queryFilter;
            _comReleaser = new ESRI.ArcGIS.ADF.ComReleaser();
        }

        #region IEnumerator<IRow> Members

        public IRow Current
        {
            get { return _currentRow; }
        }

        #endregion

        //#region IEnumerator Members

        object System.Collections.IEnumerator.Current
        {
            get { return _currentRow; }
        }

        public bool MoveNext()
        {
            if (_cursor == null) // initialize the cursor
            {

                _cursor = _table.Search(_queryFilter, false);
                _comReleaser.ManageLifetime(_cursor);
            }
            _currentRow = _cursor.NextRow();
            return _currentRow != null;
        }

        public void Reset()
        {
            _cursor = null;
        }

        //#endregion

        #region IEnumerable<IRow> Members

        public IEnumerator<IRow> GetEnumerator()
        {
            return (IEnumerator<IRow>)this;
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (System.Collections.IEnumerator)this;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (_comReleaser != null)
            {
                _comReleaser.Dispose();
                _comReleaser = null;
            }

        }

        #endregion



    }
}