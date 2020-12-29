using System.ServiceProcess;
using System.Threading;

namespace Empresa.integracao.ftp
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
#if !DEBUG
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new Service1()
            };
            ServiceBase.Run(ServicesToRun);
#else 
            Service1 project = new Service1();
            project.OnDebug();
            System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);

#endif

        }
    }
}
