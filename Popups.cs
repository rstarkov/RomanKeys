using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using RT.Util;
using RT.Util.Serialization;
using RT.Util.Xml;

namespace RomanKeys
{
    abstract class PopupBase
    {
        public TimeSpan Timeout = TimeSpan.FromSeconds(1.2);
        public PosRel HorzAnchor = PosRel.Center;
        public PosRel VertAnchor = PosRel.RightOrBottom;
        public Pos HorzPos = new Pos { Rel = PosRel.Center, Scr = PosScreen.PrimaryScreen };
        public Pos VertPos = new Pos { Value = 90, Unit = PosUnits.Percent, Rel = PosRel.LeftOrTop, Scr = PosScreen.PrimaryScreen };

        [ClassifyIgnore]
        private DateTime _disappearAt;

        [ClassifyIgnore]
        private Timer _timer;
        [ClassifyIgnore]
        protected AlphaBlendedForm _form;

        protected abstract void Paint(Graphics graphics);

        public PopupBase()
        {
            _form = new AlphaBlendedForm(showWithoutActivation: true);
            _form.Paint += (_, args) => { Paint(args.Graphics); };

            _timer = new Timer { Interval = 100, Enabled = false };
            _timer.Tick += TimerTick;
        }

        public void Display()
        {
            _form.Invoke((Action) DoDisplay);
        }

        protected virtual void DoDisplay()
        {
            _form.Left = HorzPos.Calculate(HorzAnchor, _form.Width, true);
            _form.Top = VertPos.Calculate(VertAnchor, _form.Height, false);
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

    enum PosRel { LeftOrTop, Center, RightOrBottom }
    enum PosUnits { Pixels, Percent }
    enum PosScreen { Desktop, PrimaryScreen, ActiveScreen, Screen0, Screen1, Screen2, Screen3, Screen4, Screen5 };
    class Pos
    {
        public int Value = 0;
        public PosUnits Unit = PosUnits.Pixels;
        public PosRel Rel;
        public PosScreen Scr;
        public bool WorkingAreaOnly = false;

        public int Calculate(PosRel anchor, int widthOrHeight, bool horizontal)
        {
            Rectangle bounds;
            var screens = Screen.AllScreens;
            switch (Scr)
            {
                case PosScreen.Desktop:
                    int minX = screens.Min(s => (WorkingAreaOnly ? s.WorkingArea : s.Bounds).Left);
                    int minY = screens.Min(s => (WorkingAreaOnly ? s.WorkingArea : s.Bounds).Top);
                    int maxX = screens.Max(s => (WorkingAreaOnly ? s.WorkingArea : s.Bounds).Right);
                    int maxY = screens.Max(s => (WorkingAreaOnly ? s.WorkingArea : s.Bounds).Bottom);
                    bounds = new Rectangle(minX, minY, maxX - minX, maxY - minY);
                    break;
                case PosScreen.PrimaryScreen:
                    bounds = screens.Single(s => s.Primary).Bounds;
                    break;
                case PosScreen.ActiveScreen:
                    WinAPI.RECT rect = new WinAPI.RECT();
                    if (WinAPI.GetWindowRect(WinAPI.GetForegroundWindow(), ref rect))
                        bounds = (screens.FirstOrDefault(scr => scr.Bounds.Contains((rect.Left + rect.Right) / 2, (rect.Top + rect.Bottom) / 2)) ?? screens.Single(s => s.Primary)).Bounds;
                    else
                    {
                        Point pt;
                        WinAPI.GetCursorPos(out pt);
                        bounds = (screens.FirstOrDefault(scr => scr.Bounds.Contains(pt.X, pt.Y)) ?? screens.Single(s => s.Primary)).Bounds;
                    }
                    break;
                default:
                    bounds = screens[(int) Scr - (int) PosScreen.Screen0].Bounds;
                    break;
            }
            int screenPos = horizontal ? bounds.Left : bounds.Top;
            int screenWidthOrHeight = horizontal ? bounds.Width : bounds.Height;

            double result = screenPos;
            switch (Rel)
            {
                case PosRel.LeftOrTop: result += 0; break;
                case PosRel.Center: result += screenWidthOrHeight / 2.0; break;
                case PosRel.RightOrBottom: result += screenWidthOrHeight; break;
                default: throw new Exception();
            }
            switch (anchor)
            {
                case PosRel.LeftOrTop: result -= 0; break;
                case PosRel.Center: result -= widthOrHeight / 2.0; break;
                case PosRel.RightOrBottom: result -= widthOrHeight; break;
                default: throw new Exception();
            }
            result += Value * (Unit == PosUnits.Pixels ? 1.0 : (screenWidthOrHeight / 100.0));

            return (int) Math.Round(result);
        }
    }

    class RectanglePopup : PopupBase
    {
        public Color BackgroundColor = Color.FromArgb(247, 15, 15, 15);
        public Color BorderColor = Color.FromArgb(247, 255, 255, 255);

        [ClassifyIgnore]
        private Brush _brushBackground;
        [ClassifyIgnore]
        private Pen _penBorder;

        protected override void Paint(Graphics g)
        {
            if (_brushBackground == null)
                _brushBackground = new SolidBrush(BackgroundColor);
            if (_penBorder == null)
                _penBorder = new Pen(BorderColor, 1);

            g.FillRectangle(_brushBackground, 0, 0, _form.Width, _form.Height);
            g.DrawRectangle(_penBorder, 1, 1, _form.Width - 3, _form.Height - 3);
        }
    }

    class TextPopup : RectanglePopup, ICaptionedIndicator
    {
        public string Caption { get; set; }
        public int Width = 200;
        public int Height = 40;

        public string FontFamily = "Segoe UI";
        public double FontSize = 12;
        public Color FontColor = Color.White;
        public FontStyle FontStyle = FontStyle.Regular;

        [ClassifyIgnore]
        private Font _font;
        [ClassifyIgnore]
        private Brush _fontBrush;

        protected override void DoDisplay()
        {
            _form.Width = Width;
            _form.Height = Height;
            base.DoDisplay();
        }

        protected override void Paint(Graphics g)
        {
            base.Paint(g);

            if (_font == null)
                _font = new Font(FontFamily, (float) FontSize, FontStyle);
            if (_fontBrush == null)
                _fontBrush = new SolidBrush(FontColor);

            var size = g.MeasureString(Caption, _font);
            g.DrawString(Caption, _font, _fontBrush, (_form.Width - size.Width) / 2, (_form.Height - size.Height) / 2);
        }
    }

    class BarPopup : RectanglePopup, IValueIndicator
    {
        public string Caption { get; set; }

        [ClassifyIgnore]
        public int Value { get; set; }
        [ClassifyIgnore]
        public int MaxValue { get; set; }

        [ClassifyIgnore]
        private int _windowBorder, _barBorder;
        [ClassifyIgnore]
        private int _barWidth;
        [ClassifyIgnore]
        private Brush _brushBarOn = new SolidBrush(Color.FromArgb(61, 148, 255));
        [ClassifyIgnore]
        private Brush _brushBarOff = new SolidBrush(Color.FromArgb(50, 50, 50));
        [ClassifyIgnore]
        private Font _fontCaption = new Font("Segoe UI", 12);

        public BarPopup()
        {
            Caption = "Popup";
            Value = 3;
            MaxValue = 5;
            _form.Width = 330;
            _form.Height = 70;
        }

        protected override void DoDisplay()
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

            base.DoDisplay();
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
