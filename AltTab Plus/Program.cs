using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace AltTab_Plus {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>

        const int S_OK = 0x00000000;

        [DllImport("Dwmapi.dll", CallingConvention = CallingConvention.Winapi)]
        static extern IntPtr DwmIsCompositionEnabled(out bool enabled);
        [STAThread]
        static void Main() {
            bool tmp;
            if (Environment.OSVersion.Version.Major >= 6 && DwmIsCompositionEnabled(out tmp).ToInt32() == S_OK && tmp) {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            }
            else {
                MessageBox.Show("Visual composition is not enabled. Therefore, the application will exit.", "Visual composition is disabled", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
