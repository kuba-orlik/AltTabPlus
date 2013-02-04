using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace AltTab_Plus {
    class InterceptAltTab {
        public delegate void FunPtr();
        [DllImport("key_interception.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern short InstallAltTabHook();
        [DllImport("key_interception.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern void setFunPtr(FunPtr fp);
        [DllImport("key_interception.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern void UninstallAltTabHook();

        public InterceptAltTab(FunPtr onAltTab) {
            setFunPtr(onAltTab);
            InstallAltTabHook();

        }

        ~InterceptAltTab() {
            UninstallAltTabHook();
        }
        //TODO free hook


    }
}
