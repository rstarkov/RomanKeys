using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace RomanKeys
{
    abstract class PopupBase : AlphaBlendedForm
    {
        public TimeSpan Timeout { get; set; }

        private DateTime _disappearAt;
        private Timer _timer;

        protected abstract void PaintPopup(Graphics graphics);

        public PopupBase()
        {
            Paint += (_, args) => { PaintPopup(args.Graphics); };

            Timeout = TimeSpan.FromSeconds(1.2);
            _timer = new Timer { Interval = 100, Enabled = false };
            _timer.Tick += TimerTick;
        }

        public virtual void DisplayPopup()
        {
            var rect = Screen.PrimaryScreen.Bounds;
            Left = (rect.Left + rect.Right - Width) / 2;
            Top = (int) (rect.Top + (rect.Bottom - rect.Top) * 0.9 - Height);
            _timer.Enabled = true;
            _disappearAt = DateTime.UtcNow + Timeout;
            Refresh();
            ShowWindow(Handle, SW_SHOWNOACTIVATE);
            SetWindowPos(Handle.ToInt32(), HWND_TOPMOST, Left, Top, Width, Height, SWP_NOACTIVATE);
        }

        void TimerTick(object sender, EventArgs e)
        {
            if (DateTime.UtcNow >= _disappearAt)
            {
                Hide();
                _timer.Enabled = false;
            }
        }

        protected override bool ShowWithoutActivation { get { return true; } }

        #region WinAPI

        private const int SW_SHOWNOACTIVATE = 4;
        private const int HWND_TOPMOST = -1;
        private const uint SWP_NOACTIVATE = 0x0010;

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        static extern bool SetWindowPos(int hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        #endregion
    }

    class RectanglePopup : PopupBase
    {
        private Brush _brushBackground = new SolidBrush(Color.FromArgb(247, 15, 15, 15));
        private Pen _penBorder = new Pen(Color.FromArgb(247, 255, 255, 255), 1);

        protected override void PaintPopup(Graphics g)
        {
            g.FillRectangle(_brushBackground, 0, 0, Width, Height);
            g.DrawRectangle(_penBorder, 1, 1, Width - 3, Height - 3);
        }
    }

    class BarPopup : RectanglePopup
    {
        public string Caption { get; set; }
        public int Value { get; set; }
        public int MaxValue { get; set; }

        private int _windowBorder, _barBorder;
        private int _barWidth;
        private Brush _brushBarOn = new SolidBrush(Color.FromArgb(61, 148, 255));
        private Brush _brushBarOff = new SolidBrush(Color.FromArgb(50, 50, 50));
        private Font _fontCaption = new Font("Segoe UI", 12);

        public BarPopup()
        {
            Caption = "Popup";
            Value = 3;
            MaxValue = 5;
            Width = 330;
            Height = 70;
        }

        public override void DisplayPopup()
        {
            _windowBorder = 12;
            _barBorder = 3;
            while (true)
            {
                _barWidth = (Width - 2 * _windowBorder - (MaxValue + 1) * _barBorder) / MaxValue;
                if (_barWidth >= 1.8 * _barBorder || _barBorder == 0)
                    break;
                _barBorder--;
            }
            if (_barWidth <= 0)
                _barWidth = 1;

            Width = 2 * _windowBorder + (MaxValue + 1) * _barBorder + MaxValue * _barWidth;

            base.DisplayPopup();
        }

        protected override void PaintPopup(Graphics g)
        {
            base.PaintPopup(g);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var size = g.MeasureString(Caption, _fontCaption);
            g.DrawString(Caption, _fontCaption, Brushes.White, Width / 2 - (size.Width / 2), 5);
            int y = 29;
            g.FillRectangle(Brushes.Black, 0.5f + _windowBorder - 1, 0.5f + y, Width - 2 * _windowBorder, Height - y - _windowBorder - 1);
            for (int i = 0; i < MaxValue; i++)
                g.FillRectangle(i < Value ? _brushBarOn : _brushBarOff,
                    x: 0.5f + _windowBorder - 1 + (_barBorder + _barWidth) * i + _barBorder, y: 0.5f + y + _barBorder,
                    width: _barWidth, height: Height - y - _windowBorder - 2 * _barBorder - 1);
        }
    }
}
