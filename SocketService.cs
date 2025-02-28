using SocketService.Services;
using System.ServiceProcess;
using System.Threading;

namespace SocketService
{
    public partial class SocketService : ServiceBase
    {
        private SocketServer _socketServer;
        private Thread _serverThread;

        public SocketService()
        {
            this.ServiceName = "SocketService";
        }

        protected override void OnStart(string[] args)
        {
            _socketServer = new SocketServer();
            _serverThread = new Thread(_socketServer.Start);
            _serverThread.Start();
        }

        protected override void OnStop()
        {
            _socketServer.Stop();
        }
    }
}
