using System.IO;
using System.Reflection;

namespace RedGate.AppHost.Server
{
    public class ChildProcessFactory
    {
        public IChildProcessHandle Create(string assemblyName, string assemblyLocation = null, bool openDebugConsole = false, bool is64Bit = false, bool monitorHostProcess = false)
        {
            IProcessStartOperation processStarter;

            if (is64Bit)
            {
                processStarter = new ProcessStarter64Bit();
            }
            else
            {
                processStarter = new ProcessStarter32Bit();
            }

            if (assemblyLocation == null)
            {
                assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }

            return new RemotedProcessBootstrapper(
                new StartProcessWithTimeout(
                    new StartProcessWithJobSupport(
                        processStarter))).Create(assemblyName, assemblyLocation, openDebugConsole, monitorHostProcess);
        }
    }
}
