using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;

namespace AltTab_Plus {

    struct wndList {
        public long flag;
        public IntPtr hWnd;
        public Icon icon;
        public string name;
        public IntPtr thumbnail;
    }

    struct RECT {
        public RECT(int l, int t, int r, int b) {
            left = l;
            top = t;
            right = r;
            bottom = b;
        }

        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    struct THUMBNAIL_PROPERTIES {
        public int dwFlags;
        public RECT rcDest;
        public RECT rcSrc;
        public Byte opacity;
        public bool fVisible;
        public bool fSrcClientAreaOnly;
    }

    struct SIZE {
        public SIZE(int Cx, int Cy) {
            cx = Cx;
            cy = Cy; 
        }

        public int cx;
        public int cy;
    }

    class Thumbnails {
        // WINAPI consts
        const int DWM_TNP_VISIBLE = 0x8;
        const int DWM_TNP_OPACITY = 0x4;
        const int DWM_TNP_RECTDESTINATION = 0x1;
        const int GWL_STYLE = -16;
        const int GW_OWNER = 4;
        const long WS_VISIBLE = 0x10000000L;
        const long WS_FOURTH_WORD = 0x000f0000L;
        const int S_OK = 0x00000000;
        const int GCLP_HICON = -14;
        const int GCLP_HICONSM = -34;
        const uint WM_GETICON = 0x007f;
        // end of WINAPI consts

        //other consts
        const int PREVIEWS_IN_ROW = 5;
        // end of other consts

        IntPtr hWnd;
        int maxWnd = 5; // current limitation
        int curWnd = 0; 
        wndList[] windowList = null;
        delegate bool BoolDelegate(IntPtr handle, IntPtr lParam);
        // TODO DwmUnregisterThumbnail
        [DllImport("User32.dll", CallingConvention = CallingConvention.Winapi)]
        static extern bool EnumWindows(BoolDelegate d, IntPtr lParam);
        [DllImport("User32.dll", CallingConvention = CallingConvention.Winapi)]
        static extern long GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("User32.dll", CallingConvention = CallingConvention.Winapi)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder str, int num); 
        [DllImport("User32.dll", CallingConvention = CallingConvention.Winapi)]
        static extern bool BringWindowToTop(IntPtr hWnd);
        [DllImport("User32.dll", CallingConvention = CallingConvention.Winapi)]
        static extern IntPtr GetForegroundWindow();
        [DllImport("User32.dll", CallingConvention = CallingConvention.Winapi)]
        static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);
        [DllImport("User32.dll", CallingConvention = CallingConvention.Winapi)]
        static extern long GetWindowThreadProcessId(IntPtr hWnd, out ulong lpdwProcessId);
        [DllImport("User32.dll", CallingConvention = CallingConvention.Winapi)]
        static extern IntPtr GetParent(IntPtr hWnd);
        [DllImport("User32.dll", CallingConvention = CallingConvention.Winapi)]
        static extern IntPtr GetClassLong(IntPtr hWnd, int nIndex);
        [DllImport("User32.dll", CallingConvention = CallingConvention.Winapi)]
        static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        [DllImport("Dwmapi.dll", CallingConvention = CallingConvention.Winapi)]
        static extern int DwmQueryThumbnailSourceSize(IntPtr thumbnail, out SIZE size);
        [DllImport("Dwmapi.dll", CallingConvention = CallingConvention.Winapi)]
        static extern int DwmRegisterThumbnail(IntPtr ownHWnd, IntPtr hWnd, out IntPtr thumbnail);
        [DllImport("Dwmapi.dll", CallingConvention = CallingConvention.Winapi)]
        static extern int DwmUpdateThumbnailProperties(IntPtr thumbnail, ref THUMBNAIL_PROPERTIES prnProperites);
        [DllImport("Dwmapi.dll", CallingConvention = CallingConvention.Winapi)]
        static extern int DwmUnregisterThumbnail(IntPtr thumbnail); 

        public Thumbnails(IntPtr handle) {
            windowList = new wndList [maxWnd];
            hWnd = handle;
            getWindowList();
            maxWnd = curWnd;
        }

        void getWindowList() {
            IntPtr tmp = new IntPtr(0);
            EnumWindows(EnumWindowsProc, tmp);
        }

        bool isWindowVisible(IntPtr hWnd) {
            long windowStyle = GetWindowLong(hWnd, GWL_STYLE);
            return ((windowStyle & WS_VISIBLE) == WS_VISIBLE);
        }

        bool isWindowVisibleEx(IntPtr hWnd) {
            long windowStyle = GetWindowLong(hWnd, GWL_STYLE);
            return ((windowStyle & WS_VISIBLE) == WS_VISIBLE && ((windowStyle & WS_FOURTH_WORD) > 0 || (windowStyle & 0xffff) > 0));
        }

        bool isAltTabWindow(IntPtr hWnd) {
            /* We assume that alt+tab window has to:
             * be visible
             * in case it doesn't have parent, it shouldn't have owner either
             * in it is child, its parent should not be visible and be in the same thread
             * it should have an icon associated (or a parent with an icon) */
            if (isWindowVisibleEx(hWnd)) {
                ulong NULL = 0;
                IntPtr parentHwnd = GetParent(hWnd);
                IntPtr ownerHwnd = GetWindow(hWnd, GW_OWNER);
                return !((parentHwnd == IntPtr.Zero && ownerHwnd.ToInt64() > 0 && isWindowVisible(ownerHwnd)) || (parentHwnd != IntPtr.Zero && isWindowVisible(parentHwnd) && GetWindowThreadProcessId(parentHwnd, out NULL) == GetWindowThreadProcessId(hWnd, out NULL)));
            }
            return false;
        }

        bool getWindowIcon(IntPtr hWnd) {
            IntPtr iconHandle = GetClassLong(hWnd, GCLP_HICON);
            if (iconHandle != IntPtr.Zero)
                if ((windowList[curWnd].icon = Icon.FromHandle(iconHandle)) != null)
                    return true;
            IntPtr iconBigFlag = new IntPtr(1);
            if ((iconHandle = SendMessage(hWnd, WM_GETICON, iconBigFlag, IntPtr.Zero)) != IntPtr.Zero)
                if ((windowList[curWnd].icon = Icon.FromHandle(iconHandle)) != null)
                    return true;
            IntPtr ownerHwnd = GetWindow(hWnd, GW_OWNER);
            if (ownerHwnd != IntPtr.Zero && (iconHandle = SendMessage(ownerHwnd, WM_GETICON, iconBigFlag, IntPtr.Zero)) != IntPtr.Zero)
                if ((windowList[curWnd].icon = Icon.FromHandle(iconHandle)) != null)
                    return true;
            return false;
        }

        bool EnumWindowsProc(IntPtr handle, IntPtr lParam) {
            long windowStyle = GetWindowLong(handle, GWL_STYLE);
            StringBuilder str = new StringBuilder(256);
            if (handle != hWnd && isAltTabWindow(handle)) { // checking whether a window is visible and is not our window
                if (GetWindowText(handle, str, 255) > 0) { // checking if a window has text in bar
                    if (getWindowIcon(handle)) { // checking if a window has icon associated with
                        if (DwmRegisterThumbnail(hWnd, handle, out windowList[curWnd].thumbnail) == S_OK) { // checking if its thumbnail is available
                            windowList[curWnd].name = str.ToString();
                            windowList[curWnd].flag = windowStyle;
                            windowList[curWnd++].hWnd = handle;
                            if (curWnd == maxWnd) { // checking if the index of array does not exceed its size
                                maxWnd += 5;
                                Array.Resize<wndList>(ref windowList, maxWnd);
                            }
                        }
                    }
                }
            }
            return true;
        }

        public void displayAllThumbnails(ref PictureBox image, int l, int t) {
            int i = 0;
            int left, width = 180, top, space = 10;
            Graphics gfx = image.CreateGraphics();
            //TextDrawing text = new TextDrawing(new Font("Verdana", 11), ref image);
            while (i < maxWnd) {
                left = l + i % PREVIEWS_IN_ROW * (width + space);
                top = t + i / PREVIEWS_IN_ROW * (width + space);
                displayThumbnail(i, left, top);
                gfx.DrawIcon(windowList[i++].icon, left, top);
                //text.DrawText(windowList[i++].name, new Point(left + width + 5, top+width+5));
            }
        }

        public void eraseAllThumbnails() {
            for (int i = 0; i < maxWnd; ++i) {
                DwmUnregisterThumbnail(windowList[i].thumbnail);
            }
            Array.Clear(windowList, 0, maxWnd);
            maxWnd = 5;
            curWnd = 0;
        }

        bool displayThumbnail(int wndNum, int left, int top) {
            SIZE size;
            DwmQueryThumbnailSourceSize(windowList[wndNum].thumbnail, out size);
            THUMBNAIL_PROPERTIES props = new THUMBNAIL_PROPERTIES();
            props.dwFlags = DWM_TNP_OPACITY | DWM_TNP_RECTDESTINATION | DWM_TNP_VISIBLE;
            props.fVisible = true;
            props.opacity = 255;
            props.rcDest = new RECT(left, top, left + 180, top + 180);
            return (DwmUpdateThumbnailProperties(windowList[wndNum].thumbnail, ref props) == S_OK);
        }

        public wndList windowListItem(int i) {
            if (i < maxWnd)
                return windowList[i];
            throw new Exception();
        }

        //public GetNextWindow

        public int itemNumber {
            get {
                return maxWnd;
            }
        }
    }

    class TextDrawing {
        Font drawFont;
        SolidBrush drawBrush = new SolidBrush(Color.Black);
        Graphics gfx;
        public TextDrawing(Font dF, ref PictureBox image) {
            gfx = image.CreateGraphics();
            drawFont = dF;
        }

        public void DrawText(string str, Point drawPoint) {
            int strmax = 10;
            if (str.Length < 10)
                strmax = str.Length;
            gfx.DrawString(str.Substring(0, strmax) , drawFont, drawBrush, drawPoint); 
        }
    }
}
