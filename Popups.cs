using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using RT.Util.Xml;

namespace RomanKeys
{
    abstract class PopupBase
    {
        public TimeSpan Timeout = TimeSpan.FromSeconds(1.2);

        [XmlIgnore]
        private DateTime _disappearAt;

        [XmlIgnore]
        private Timer _timer;
        [XmlIgnore]
        protected AlphaBlendedForm _form;

        protected abstract void Paint(Graphics graphics);

        public PopupBase()
        {
            _form = new AlphaBlendedForm(showWithoutActivation: true);
            _form.Paint += (_, args) => { Paint(args.Graphics); };

            _timer = new Timer { Interval = 100, Enabled = false };
            _timer.Tick += TimerTick;
        }

        public virtual void Display()
        {
            var rect = Screen.PrimaryScreen.Bounds;
            _form.Left = (rect.Left + rect.Right - _form.Width) / 2;
            _form.Top = (int) (rect.Top + (rect.Bottom - rect.Top) * 0.9 - _form.Height);
            _timer.Enabled = true;
            _disappearAt = DateTime.UtcNow + Timeout;
            _form.Refresh();
            ShowWindow(_form.Handle, SW_SHOWNOACTIVATE);
            SetWindowPos(_form.Handle.ToInt32(), HWND_TOPMOST, _form.Left, _form.Top, _form.Width, _form.Height, SWP_NOACTIVATE);
        }

        void TimerTick(object sender, EventArgs e)
        {
            if (DateTime.UtcNow >= _disappearAt)
            {
                _form.Hide();
                _timer.Enabled = false;
            }
        }

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
        [XmlIgnore]
        private Brush _brushBackground = new SolidBrush(Color.FromArgb(247, 15, 15, 15));
        [XmlIgnore]
        private Pen _penBorder = new Pen(Color.FromArgb(247, 255, 255, 255), 1);

        protected override void Paint(Graphics g)
        {
            g.FillRectangle(_brushBackground, 0, 0, _form.Width, _form.Height);
            g.DrawRectangle(_penBorder, 1, 1, _form.Width - 3, _form.Height - 3);
        }
    }

    class BarPopup : RectanglePopup, IValueIndicator
    {
        public string Caption { get; set; }

        [XmlIgnore]
        public int Value { get; set; }
        [XmlIgnore]
        public int MaxValue { get; set; }

        [XmlIgnore]
        private int _windowBorder, _barBorder;
        [XmlIgnore]
        private int _barWidth;
        [XmlIgnore]
        private Brush _brushBarOn = new SolidBrush(Color.FromArgb(61, 148, 255));
        [XmlIgnore]
        private Brush _brushBarOff = new SolidBrush(Color.FromArgb(50, 50, 50));
        [XmlIgnore]
        private Font _fontCaption = new Font("Segoe UI", 12);

        public BarPopup()
        {
            Caption = "Popup";
            Value = 3;
            MaxValue = 5;
            _form.Width = 330;
            _form.Height = 70;
        }

        void IValueIndicator.Display()
        {
            _form.Invoke((Action) Display);
        }

        public override void Display()
        {
            _windowBorder = 12;
            _barBorder = 3;
            while (true)
            {
                _barWidth = (_form.Width - 2 * _windowBorder - (MaxValue + 1) * _barBorder) / MaxValue;
                if (_barWidth >= 1.8 * _barBorder || _barBorder == 0)
                    break;
                _barBorder--;
            }
            if (_barWidth <= 0)
                _barWidth = 1;

            _form.Width = 2 * _windowBorder + (MaxValue + 1) * _barBorder + MaxValue * _barWidth;

            base.Display();
        }

        protected override void Paint(Graphics g)
        {
            base.Paint(g);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var size = g.MeasureString(Caption, _fontCaption);
            g.DrawString(Caption, _fontCaption, Brushes.White, _form.Width / 2 - (size.Width / 2), 5);
            int y = 29;
            g.FillRectangle(Brushes.Black, 0.5f + _windowBorder - 1, 0.5f + y, _form.Width - 2 * _windowBorder, _form.Height - y - _windowBorder - 1);
            for (int i = 0; i < MaxValue; i++)
                g.FillRectangle(i < Value ? _brushBarOn : _brushBarOff,
                    x: 0.5f + _windowBorder - 1 + (_barBorder + _barWidth) * i + _barBorder, y: 0.5f + y + _barBorder,
                    width: _barWidth, height: _form.Height - y - _windowBorder - 2 * _barBorder - 1);
        }
    }
}
