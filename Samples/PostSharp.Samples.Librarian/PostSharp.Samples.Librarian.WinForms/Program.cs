using System;
using System.Runtime.Remoting;
using System.Windows.Forms;

namespace PostSharp.Samples.Librarian.WinForms
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            // Initializes remoting.
            RemotingConfiguration.Configure( "PostSharp.Samples.Librarian.WinForms.exe.config", false );


            /*
            // Load the server application domain.
            AppDomainSetup appDomainSetup = new AppDomainSetup();
            appDomainSetup.ConfigurationFile = "PostSharp.Samples.Librarian.BusinessProcess.config";
            appDomainSetup.LoaderOptimization = LoaderOptimization.MultiDomain;
            appDomainSetup.AppDomainInitializer = new AppDomainInitializer(Host.Initialize);
            AppDomain appDomain = AppDomain.CreateDomain("PostSharp.Samples.Librarian.Server", null, appDomainSetup);
             */

            // Load the GUI
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault( false );
            Application.Run( new MainForm() );
        }
    }
}