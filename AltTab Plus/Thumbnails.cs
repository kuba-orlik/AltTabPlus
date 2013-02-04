using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;

namespace AltTab_Plus {

    struct wndList {
        public IntPtr hWnd;
        public IntPtr thumbnail;
        public string name;
        public long flag;
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
        const uint GWL_HWNDNEXT = 2;
        const long WS_VISIBLE = 0x10000000L;
        const long WS_EX_APPWINDOW = 0x00040000L;
        const long WS_CAPTION = 0x00C00000L;
        const long WS_VISUAL_STUDIO = 0x000D0000L;
        const long WS_FOURTH_WORD = 0x000f0000L;
        const long WS_NOT_ANY_WORD = 0x00000000L;
        const long WS_FOUR_LAST_WORDS = 0x0000ffffL;
        const int S_OK = 0x00000000;
        // end of WINAPI consts

        //other consts
        const int PREVIEWS_IN_ROW = 5;
        // end of other consts

        IntPtr hWnd;
        int maxWnd = 5; // current limitation
        int curWnd = 0; 
        public wndList[] windowList= null;
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
        [DllImport("Dwmapi.dll", CallingConvention = CallingConvention.Winapi)]
        static extern int DwmQueryThumbnailSourceSize(IntPtr thumbnail, out SIZE size);
        [DllImport("Dwmapi.dll", CallingConvention = CallingConvention.Winapi)]
        static extern int DwmRegisterThumbnail(IntPtr ownHWnd, IntPtr hWnd, out IntPtr thumbnail);
        [DllImport("Dwmapi.dll", CallingConvention = CallingConvention.Winapi)]
        static extern int DwmUpdateThumbnailProperties(IntPtr thumbnail, ref THUMBNAIL_PROPERTIES prnProperites);

        public Thumbnails(IntPtr handle) {
            windowList = new wndList [maxWnd];
            hWnd = handle;
            GetWindowList();
            maxWnd = curWnd;
        }

        void GetWindowList() {
            EnumWindows(EnumWindowsProc, new IntPtr(0));
        }

        bool IsWindowVisible(long windowStyle) {
            if ((windowStyle & (WS_VISIBLE | WS_CAPTION)) == (WS_VISIBLE | WS_CAPTION) &&
                !((windowStyle & WS_VISUAL_STUDIO) == WS_VISUAL_STUDIO && (windowStyle & WS_FOURTH_WORD) != WS_FOURTH_WORD) 
                && (windowStyle & WS_FOURTH_WORD) != WS_NOT_ANY_WORD && (windowStyle & WS_FOUR_LAST_WORDS) == WS_NOT_ANY_WORD) {
                    return true;
            }
            return false;
        }

        bool EnumWindowsProc(IntPtr handle, IntPtr lParam) {
            long windowStyle = GetWindowLong(handle, GWL_STYLE);
            StringBuilder str = new StringBuilder(256);
            if (handle != hWnd && IsWindowVisible(windowStyle)) { // checking whether a window is visible and is not our window
                if (GetWindowText(handle, str, 255) > 0) { // checking if a window has text in bar
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
            return true;
        }

        public void DisplayAllThumbnails(ref PictureBox image, int l, int t) {
            int i = 0;
            int left, width = 180, top, space = 10;
            TextDrawing text = new TextDrawing(new Font("Verdana", 11), ref image);
            while (i < maxWnd) {
                left = l + i % PREVIEWS_IN_ROW * (width + space);
                top = t + i / PREVIEWS_IN_ROW * (width + space);
                DisplayThumbnail(i++, left, top);
                //text.DrawText(windowList[i++].name, new Point(left + width + 5, top+width+5));
            }
        }

        bool DisplayThumbnail(int wndNum, int left, int top) {
            SIZE size;
            DwmQueryThumbnailSourceSize(windowList[wndNum].thumbnail, out size);
            THUMBNAIL_PROPERTIES props = new THUMBNAIL_PROPERTIES();
            props.dwFlags = DWM_TNP_OPACITY | DWM_TNP_RECTDESTINATION | DWM_TNP_VISIBLE;
            props.fVisible = true;
            props.opacity = 255;
            props.rcDest = new RECT(left, top, left + 180, top + 180);
            return (DwmUpdateThumbnailProperties(windowList[wndNum].thumbnail, ref props) == S_OK);
        }

        public string WindowName(int i) {
            return (i < maxWnd) ? windowList[i].name : null;
        }

        public long WindowFlag(int i) {
            return (i < maxWnd) ? windowList[i].flag : 0;
        }

        //public GetNextWindow

        public int ItemNumber {
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
