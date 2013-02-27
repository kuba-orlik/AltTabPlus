using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace AltTab_Plus {
    class KillingModule {

        // WINAPI consts
        const uint WM_DESTROY = 0x0002;
        const uint WM_NCDESTROY = 0x0082;
        const uint WM_CLOSE = 0x0010;

        [DllImport("User32.dll", CallingConvention = CallingConvention.Winapi)]
        static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        public static bool CloseWindowGentle(IntPtr hWnd) {
            return (SendMessage(hWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero) != IntPtr.Zero);
        }

        public static bool CloseWindowForced(IntPtr hWnd) {
            if (SendMessage(hWnd, WM_DESTROY, IntPtr.Zero, IntPtr.Zero) != IntPtr.Zero)
                if (SendMessage(hWnd, WM_NCDESTROY, IntPtr.Zero, IntPtr.Zero) != IntPtr.Zero)
                    return true;
            return false;
        }
    }
}
