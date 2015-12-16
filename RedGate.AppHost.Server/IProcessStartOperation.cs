using System.Diagnostics;

namespace RedGate.AppHost.Server
{
    internal interface IProcessStartOperation
    {
        Process StartProcess(string assemblyName, string assemblyLocation, string remotingId, bool openDebugConsole, bool monitorHostProcess);
    }
}