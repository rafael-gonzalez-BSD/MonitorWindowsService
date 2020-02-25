using MonitorWindowsService.Datos.Base;
using System.Collections.Generic;

namespace MonitorWindowsService.Datos.Implementacion
{
    public class EjecucionConfiguracionLecturaDao : Disposable
    {
        #region ===== Variables =====

        internal DBConnection _db;

        #endregion ===== Variables =====

        #region ==== Constructores ====

        public EjecucionConfiguracionLecturaDao()
        {
            _db = new DBConnection("Default");
        }

        #endregion ==== Constructores ====

        #region ==== Metodos ====

        public IEnumerable<T> Consultar<T>(Dictionary<string, dynamic> P)
        {
            return _db.Query<T>(P, "[Bitacora].[spEjecucionConfiguracionLectura_Consultar]");
        }

        public T ConsultarPor<T>(Dictionary<string, dynamic> P)
        {
            return _db.QuerySingle<T>(P, "[Bitacora].[spEjecucionConfiguracionLectura_Consultar]");
        }

        public T Insertar<T>(Dictionary<string, dynamic> P)
        {
            return _db.QuerySingle<T>(P, "[Bitacora].[spEjecucionConfiguracionLectura_Insertar]");
        }

        public T Actualizar<T>(Dictionary<string, dynamic> P)
        {
            return _db.QuerySingle<T>(P, "[Bitacora].[spEjecucionConfiguracionLectura_Actualizar]");
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