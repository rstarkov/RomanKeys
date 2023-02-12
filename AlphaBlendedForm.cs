using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace RomanKeys
{
    enum ClickAction
    {
        ClickThrough,
        Move,
        Dismiss,
    }

    class AlphaBlendedForm : Form
    {
        public ClickAction ClickHandling { get; set; }

        private bool _showWithoutActivation;

        public AlphaBlendedForm(bool showWithoutActivation = false)
        {
            _showWithoutActivation = showWithoutActivation;
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = false;
            TopMost = true;
            ClickHandling = ClickAction.ClickThrough;
            Width = 200; // the caller is expected to set this as desired, but initialize to something sensible
            Height = 100;

            MouseDown += HandleMouseDown;
            Load += delegate { Refresh(); };

            var dummy = Handle;
        }

        private void HandleMouseDown(object sender, MouseEventArgs e)
        {
            if (ClickHandling == ClickAction.Move && e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
            else if (ClickHandling == ClickAction.Dismiss)
                Close();
        }

        protected override bool ShowWithoutActivation { get { return _showWithoutActivation; } }
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= WS_EX_LAYERED | WS_EX_TOOLWINDOW | (ClickHandling == ClickAction.ClickThrough ? WS_EX_TRANSPARENT : 0);
                return cp;
            }
        }

        public override void Refresh()
        {
            var dcScreen = GetDC(IntPtr.Zero);
            var dcMem = CreateCompatibleDC(dcScreen);
            var hBitmap = IntPtr.Zero;
            var hBitmapOld = IntPtr.Zero;

            try
            {
                using (var bmp = new Bitmap(Width, Height, PixelFormat.Format32bppArgb))
                {
                    hBitmap = bmp.GetHbitmap(Color.FromArgb(0));
                    hBitmapOld = SelectObject(dcMem, hBitmap);
                    using (var graphics = Graphics.FromHdc(dcMem))
                        OnPaint(new PaintEventArgs(graphics, ClientRectangle));

                    var size = bmp.Size;
                    var pointSource = new Point(0, 0);
                    var topPos = new Point(Left, Top);

                    var blend = new BLENDFUNCTION
                    {
                        BlendOp = AC_SRC_OVER,
                        BlendFlags = 0,
                        SourceConstantAlpha = 255,
                        AlphaFormat = AC_SRC_ALPHA,
                    };

                    UpdateLayeredWindow(Handle, dcScreen, ref topPos, ref size, dcMem, ref pointSource, 0, ref blend, ULW_ALPHA);
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                if (hBitmapOld != IntPtr.Zero) // apparently one must deselect the bitmap before deleting both; not entirely sure if this is so...
                    SelectObject(dcMem, hBitmapOld);
                if (hBitmap != IntPtr.Zero)
                    DeleteObject(hBitmap);
                ReleaseDC(IntPtr.Zero, dcScreen);
                DeleteDC(dcMem);
            }
        }

        #region WinAPI

        private const int WM_NCHITTEST = 0x84;
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;
        private const int HT_TRANSPARENT = -1;

        [DllImportAttribute("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImportAttribute("user32.dll")]
        private static extern bool ReleaseCapture();

        private const int WS_EX_LAYERED = 0x00080000;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WS_EX_TRANSPARENT = 0x20;

        private const byte AC_SRC_OVER = 0x00;
        private const byte AC_SRC_ALPHA = 0x01;
        private const Int32 ULW_ALPHA = 0x00000002;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct BLENDFUNCTION
        {
            public byte BlendOp;
            public byte BlendFlags;
            public byte SourceConstantAlpha;
            public byte AlphaFormat;
        }

        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, ref Point pptDst, ref Size psize, IntPtr hdcSrc, ref Point pprSrc, Int32 crKey, ref BLENDFUNCTION pblend, Int32 dwFlags);

        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern IntPtr CreateCompatibleDC(IntPtr hDC);

        [DllImport("user32.dll", ExactSpelling = true)]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll", ExactSpelling = true)]
        private static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern bool DeleteObject(IntPtr hObject);

        #endregion
    }
}
