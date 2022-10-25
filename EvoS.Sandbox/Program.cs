using System;
using System.Threading;
using EvoS.Framework.Assets;
using EvoS.Framework.Logging;
using McMaster.Extensions.CommandLineUtils;

namespace EvoS.Sandbox
{
    class Program
    {
        public static int Main(string[] args)
        {
            CommandLineApplication.Execute<Program>(args);
            return 0;
        }
        
        
        [Option(Description = "Path to AtlasReactor_Data", ShortName = "D")]
        public string Assets { get; }

        private void OnExecute()
        {
            Banner.PrintBanner();

            // Start Directory Server
            new Thread(() => StartDirServer()).Start();
            
            // Start Central Server
            CentralServer.CentralServer.Main(new string[] { });            
        }

        /// <summary>
        /// Starts the directory server, which is like an authenticator server/lobby load balancer
        /// but in our case it is in charge of giving the ip to the lobby server
        /// </summary>
        static void StartDirServer()
        {
            DirectoryServer.Program.Main();
        }
    }
}
