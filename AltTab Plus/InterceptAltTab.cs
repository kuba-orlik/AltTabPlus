using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace AltTab_Plus {
    class InterceptAltTab {
        public delegate void FunPtr();
        [DllImport("key_interceptor.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern short installAltTabHooks();
        [DllImport("key_interceptor.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern void setFunctionPointers(FunPtr fp, FunPtr fp2);
        [DllImport("key_interceptor.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern void uninstallAltTabHooks();

        public InterceptAltTab(FunPtr onAltTab, FunPtr onShiftAltTab) {
            setFunctionPointers(onAltTab, onShiftAltTab);
            installAltTabHooks();

        }

        ~InterceptAltTab() {
            uninstallAltTabHooks();
        }
        //TODO free hook


    }
}
