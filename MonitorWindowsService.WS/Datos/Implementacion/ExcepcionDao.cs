using MonitorWindowsService.WS.Datos.Base;
using System.Collections.Generic;

namespace MonitorWindowsService.WS.Datos.Implementacion
{
    public class ExcepcionDao : Disposable
    {
        #region ===== Variables =====

        internal DBConnection _db;

        #endregion

        #region ==== Constructores ====

        public ExcepcionDao()
        {
            _db = new DBConnection("Default");
        }

        #endregion

        #region ==== Metodos ====

        public IEnumerable<T> Consultar<T>(Dictionary<string, dynamic> P)
        {
            return _db.Query<T>(P, "[Bitacora].[spObtenerConfiguracionesExcepciones_Consultar]");
        }

        public void Insertar<T>(Dictionary<string, dynamic> P)
        {
            _db.Query<T>(P, "[Bitacora].[spRegistrarExcepciones_Insertar]");
        }

        #endregion


        #region ==== Dispose ====

        protected override void DisposeCore()
        {
            if (_db != null)
            {
                _db.Dispose();
            }
            Dispose();
        } 

        #endregion
    }
}