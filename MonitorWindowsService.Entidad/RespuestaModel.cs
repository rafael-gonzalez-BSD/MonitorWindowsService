namespace MonitorWindowsService.Entidad
{
    public class RespuestaModel
    {
        public dynamic Datos { get; set; }
        public bool Satisfactorio { get; set; }
        public string Mensaje { get; set; }
        public int Id { get; set; }
        public int ErrorId { get; set; }

        public RespuestaModel()
        {
            Datos = null;
            Satisfactorio = true;
            Mensaje = "";
            Id = 0;
            ErrorId = 0;
        }

        public RespuestaModel(dynamic _datos, bool _satisfactorio, string _mensaje = "", int _id = 0, int _errorId = 0)
        {
            Datos = _datos;
            Satisfactorio = _satisfactorio;
            Mensaje = _mensaje;
            Id = _id;
            ErrorId = _errorId;
        }
    }
}