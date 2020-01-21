using MonitorWindowsService.WS.Enum;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Timers;

namespace MonitorWindowsService.WS
{
    public partial class ProcesoLog : ServiceBase
    {
        private int eventId = 1;
        private int segundos = Convert.ToInt32(ConfigurationManager.AppSettings["TiempoIntervalo"]);
        private Procesos.Excepciones excepciones;
        private Procesos.Ejecuciones ejecuciones;

        public ProcesoLog()
        {
            InitializeComponent();
            eventLog1 = new EventLog();
            if (!EventLog.SourceExists("MiOrigen"))
            {
                EventLog.CreateEventSource("MiOrigen", "MiNuevoLog");
            }

            eventLog1.Source = "MiOrigen";
            eventLog1.Log = "MiNuevoLog";

            excepciones = new Procesos.Excepciones(eventLog1);
            ejecuciones = new Procesos.Ejecuciones(eventLog1);
        }

        protected override void OnStart(string[] args)
        {
            // Update the service state to Start Pending.
            ServiceStatus ss = new ServiceStatus
            {
                dwCurrentState = ServiceState.SERVICE_PAUSE_PENDING,
                dwWaitHint = 100000
            };
            SetServiceStatus(this.ServiceHandle, ref ss);

            eventLog1.WriteEntry("Iniciando Servicio");
            Timer timer = new Timer
            {
                Interval = 1000 * segundos // 60 seconds
            };
            timer.Elapsed += new ElapsedEventHandler(OnTimer);
            timer.Start();

            // Update the service state to Running.
            ss.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(this.ServiceHandle, ref ss);
        }

        protected override void OnStop()
        {
            // Update the service state to Stop Pending.
            ServiceStatus ss = new ServiceStatus
            {
                dwCurrentState = ServiceState.SERVICE_STOP_PENDING,
                dwWaitHint = 100000
            };
            SetServiceStatus(this.ServiceHandle, ref ss);

            // Update the service state to Stopped.
            ss.dwCurrentState = ServiceState.SERVICE_STOPPED;
            SetServiceStatus(this.ServiceHandle, ref ss);

            eventLog1.WriteEntry("Servicio Detenido");
        }

        public void OnTimer(object sender, ElapsedEventArgs args)
        {
            eventLog1.WriteEntry("Monitoreando El Sistema", EventLogEntryType.Information, eventId++);
            excepciones.Start_Visitas();
            ejecuciones.Start_Visitas();
        }

        protected override void OnContinue()
        {
            eventLog1.WriteEntry("Servicio Continuando.");
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(System.IntPtr handle, ref ServiceStatus ss);
    }
}