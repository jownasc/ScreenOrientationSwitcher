using System;
using System.Windows.Forms;

namespace ScreenOrientationSwitcher
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            // Inicializa o aplicativo
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
