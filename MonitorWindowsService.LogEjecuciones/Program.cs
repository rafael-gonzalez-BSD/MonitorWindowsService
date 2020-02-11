namespace MonitorWindowsService.LogEjecuciones
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main()
        {
#if (!DEBUG)
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new ProcesoLog()
            };
            ServiceBase.Run(ServicesToRun);
#else
            Ejecuciones ejecuciones = new Ejecuciones();
            ejecuciones.Start_Visitas();

#endif
        }
    }
}