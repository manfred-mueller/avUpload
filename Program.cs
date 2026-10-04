using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Linq;
using System.Windows.Forms;

namespace avUpload
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// Exit-Codes: 0 = OK, 1 = Fehler, 2 = Abgebrochen (nur Silent-Mode).
        /// </summary>
        [STAThread]
        static int Main()
        {
            string[] args = Environment.GetCommandLineArgs();

            if (args.Skip(1).Any(a => a == "--silent" || a == "-s"))
            {
                // Kein WinForms-Pump notwendig – direkt async ausführen
                return SilentRunner.RunAsync(args).GetAwaiter().GetResult();
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            SingleInstanceController controller = new SingleInstanceController();
            controller.Run(args);
            return 0;
        }
    }

    public class SingleInstanceController : WindowsFormsApplicationBase
    {
        public SingleInstanceController()
        {
            IsSingleInstance = true;

            StartupNextInstance += this_StartupNextInstance;
        }

        void this_StartupNextInstance(object sender, StartupNextInstanceEventArgs e)
        {
            Form1 form = MainForm as Form1;
            if (e.CommandLine.Count > 1)
            {
                for (int i = 1; i < e.CommandLine.Count; i++)
                {
                    string file = e.CommandLine[i];
                    form.LoadFile(file);
                }
            }
        }

        protected override void OnCreateMainForm()
        {
            string[] args = Environment.GetCommandLineArgs();
            MainForm = new Form1(args);
        }
    }
}
