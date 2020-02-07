using MonitorWindowsService.WS.Datos.Base;
using System.Collections.Generic;

namespace MonitorWindowsService.WS.Datos.Implementacion
{
    public class ExcepcionDao : Disposable
    {
        #region ===== Variables =====

        internal DBConnection _db;

        #endregion ===== Variables =====

        #region ==== Constructores ====

        public ExcepcionDao()
        {
            _db = new DBConnection("Default");
        }

        #endregion ==== Constructores ====

        #region ==== Metodos ====

        public IEnumerable<T> Consultar<T>(Dictionary<string, dynamic> P)
        {
            return _db.Query<T>(P, "[Bitacora].[spExcepcionConfiguracion_Consultar]");
        }

        public T Insertar<T>(Dictionary<string, dynamic> P)
        {
            return _db.QuerySingle<T>(P, "[Bitacora].[spRegistrarExcepciones_Insertar]");
        }

        public T Actualizar<T>(Dictionary<string, dynamic> P)
        {
            return _db.QuerySingle<T>(P, "[Bitacora].[spExcepcionConfiguracion_Actualizar]");
        }

        #endregion ==== Metodos ====

        #region ==== Dispose ====

        protected override void DisposeCore()
        {
            if (_db != null)
            {
                _db.Dispose();
            }
            Dispose();
        }

        #endregion ==== Dispose ====
    }
}