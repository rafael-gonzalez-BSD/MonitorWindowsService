using System.ServiceProcess;

namespace MonitorWindowsService.LogConectores
{
    public partial class ProcesoLog : ServiceBase
    {
        public ProcesoLog()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
        }

        protected override void OnStop()
        {
        }
    }
}