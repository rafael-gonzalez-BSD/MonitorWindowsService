using MonitorWindowsService.WS.Datos.Base;
using System.Collections.Generic;

namespace MonitorWindowsService.WS.Datos.Implementacion
{
    public class ExcepcionDao : Disposable
    {
        internal DBConnection _db;

        public ExcepcionDao()
        {
            _db = new DBConnection("Default");
        }

        public IEnumerable<T> Consultar<T>(Dictionary<string, dynamic> P)
        {
            return _db.Query<T>(P, "");
        }

        protected override void DisposeCore()
        {
            if (_db != null)
            {
                _db.Dispose();
            }
            Dispose();
        }
    }
}