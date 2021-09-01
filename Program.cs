using System;
using System.Threading;
using System.Windows.Forms;

namespace avUpload
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static Mutex mutex = new Mutex(true, "{69C9E15E-40D0-4FE6-BA54-AF931203DB90}");
        [STAThread]
        static void Main(string[] args)
        {
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Mainform(args));
                mutex.ReleaseMutex();
            }
            else
            {
                MessageBox.Show(avUpload.Properties.Resources.AnotherInstanceIsAlreadyRunning, Properties.Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
