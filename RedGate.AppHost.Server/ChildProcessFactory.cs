﻿using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using RedGate.AppHost.Interfaces;

namespace RedGate.AppHost.Server
{
    public class ChildProcessFactory
    {
        private const string c_FileName = "RedGate.AppHost.Client.exe";

        private static readonly TimeSpan s_TimeOut = TimeSpan.FromSeconds(20);

        private readonly string m_Id = string.Format("{0}.IPC.{{{1}}}", c_FileName, Guid.NewGuid());

        public IAppHostChildHandle Create(string assemblyName, string typeName)
        {
            using (EventWaitHandle signal = new EventWaitHandle(false, EventResetMode.ManualReset, m_Id))
            {
                string executingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string quotedAssemblyPath = "\"" + Path.Combine(executingDirectory, assemblyName) + "\"";

                Process process = Process.Start(Path.Combine(executingDirectory, c_FileName), String.Join(" ", m_Id, quotedAssemblyPath, typeName));
                try
                {
                    if (process.CanAssignToJobObject())
                    {
                        new Job().AssignProcessToJobObject(process);
                    }
                    
                    WaitForReadySignal(signal);

                    return new AppHostChildHandle(InitializeRemoting()); //The Database CI code wraps this up to handle disposing. Do we need to?
                }
                catch
                {
                    process.KillAndDispose();
                    throw;
                }
            }
        }

        private static void WaitForReadySignal(EventWaitHandle signal)
        {
            if (!signal.WaitOne(s_TimeOut))
            {
                throw new ApplicationException("WPF child process didn't respond quickly enough");
            }
        }

        private ISafeAppHostChildHandle InitializeRemoting()
        {
            Remoting.Remoting.RegisterChannels(false, m_Id);

            return Remoting.Remoting.ConnectToService<ISafeAppHostChildHandle>(m_Id);
        }
    }
}
