using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace AltTab_Plus {
    class InterceptAltTab {
        public delegate void FunPtr();
        [DllImport("key_interceptor.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern short installAltTabHooks();
        [DllImport("key_interceptor.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern void setFunctionPointers(IntPtr hWnd, FunPtr fp, FunPtr fp2);
        [DllImport("key_interceptor.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern void uninstallAltTabHooks();

        public InterceptAltTab(IntPtr hWnd, FunPtr onAltTab, FunPtr onShiftAltTab) {
            try {
                setFunctionPointers(hWnd, onAltTab, onShiftAltTab);
                installAltTabHooks();
            }
            catch (DllNotFoundException e) {
                MessageBox.Show("Couldn't found the module key_interceptor.dll. Program will exit.", "Module not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
            catch (EntryPointNotFoundException e) {
                MessageBox.Show("The module key_interceptor.dll appears to be damaged. Program will exit.", "Module is damaged", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }
       
        ~InterceptAltTab() {
            try {
                uninstallAltTabHooks();
            }
            catch (DllNotFoundException e) {}
            catch (EntryPointNotFoundException e) {}
            
        }

    }
}
