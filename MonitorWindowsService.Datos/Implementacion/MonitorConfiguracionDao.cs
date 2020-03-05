using MonitorWindowsService.Datos.Base;
using System.Collections.Generic;

namespace MonitorWindowsService.Datos.Implementacion
{
    public class MonitorConfiguracionDao: Disposable
    {
        #region ===== Variables =====

        internal DBConnection _db;

        #endregion ===== Variables =====

        #region ==== Constructores ====

        public MonitorConfiguracionDao()
        {
            _db = new DBConnection("Default");
        }

        #endregion ==== Constructores ====

        #region ==== Metodos ====

        public T ConsultarPor<T>(Dictionary<string, dynamic> P)
        {
            return _db.QuerySingle<T>(P, "[dbo].[spObtenerConfiguracion_Consultar]");
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
