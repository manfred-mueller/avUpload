using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Windows.Forms;

namespace avUpload
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            string[] args = Environment.GetCommandLineArgs();
            SingleInstanceController controller = new SingleInstanceController();
            controller.Run(args);
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
