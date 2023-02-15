using RT.Serialization;

namespace RomanKeys;

class PressedKeyMonitorModule : IModule, IEnumerable<PressedKeyMonitorModule.Evt>
{
    public Hotkey ToggleHotkey = new(Key.Q, ctrl: true, alt: true, shift: true, win: true);
    public ScrollingTextPopup Indicator = new();

    [ClassifyIgnore]
    private bool _enabled = false;
    [ClassifyIgnore]
    private Evt[] _events = new Evt[50];
    [ClassifyIgnore]
    private int _lastEvent = 0;
    [ClassifyIgnore]
    private Dictionary<Key, bool> _isDown = new();

    public class Evt
    {
        public Key Key;
        public bool? Down;
    }

    public bool HandleKey(Hotkey key, bool down)
    {
        bool consume = false;
        if (down && key == ToggleHotkey)
        {
            consume = true;
            _enabled = !_enabled;
            for (int i = 0; i < _events.Length; i++)
                _events[i] = null;
            if (!_enabled)
                Indicator.Hide();
        }
        if (!_enabled)
            return consume;
        if (down && _isDown.ContainsKey(key.Key) && _isDown[key.Key])
            return consume; // autorepeat - we ignore these
        else if (_events[_lastEvent] != null && _events[_lastEvent].Key == key.Key && _events[_lastEvent].Down == true && !down)
            _events[_lastEvent].Down = null; // it's a full press
        else
        {
            _lastEvent = (_lastEvent + 1) % _events.Length;
            _events[_lastEvent] = new Evt { Key = key.Key, Down = down };
        }
        _isDown[key.Key] = down;
        Task.Run(() =>
        {
            Indicator.Events = this;
            Indicator.Display();
        });
        return consume;
    }

    public IEnumerator<PressedKeyMonitorModule.Evt> GetEnumerator()
    {
        for (int i = (_lastEvent + 2) % _events.Length; i != (_lastEvent + 1) % _events.Length; i = (i + 1) % _events.Length)
            if (_events[i] != null)
                yield return _events[i];
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

class ScrollingTextPopup : RectanglePopup
{
    public int Width = 300;
    public int Height = 50;

    [ClassifyIgnore]
    private Font _font = new("Segoe UI", 15);
    [ClassifyIgnore]
    private Brush _backBrush = new SolidBrush(Color.FromArgb(120, 120, 120));
    [ClassifyIgnore]
    public IEnumerable<PressedKeyMonitorModule.Evt> Events;

    protected override Size GetSize()
    {
        return new Size(Width, Height);
    }

    protected override void Paint(Graphics g)
    {
        base.Paint(g);
        var dpiscale = g.DpiX / 96;
        float sc(float v) => v * dpiscale;
        float isc(float v) => v / dpiscale;
        float x = Width - 5;
        float height = 0;
        g.SetClip(new RectangleF(sc(2), sc(2), sc(Width - 4), sc(Height - 4)));
        foreach (var evt in Events.Reverse())
        {
            var label = getKeyDisplay(evt.Key);
            var size = g.MeasureString(label, _font);
            height = isc(size.Height);
            x -= 7 + isc(size.Width) + 7;
            if (evt.Down == true)
                g.FillRectangle(_backBrush, sc(x), sc(5 + height - 3), sc(7 + 7) + size.Width, sc(3));
            else if (evt.Down == false)
                g.FillRectangle(_backBrush, sc(x), sc(5), sc(7 + 7) + size.Width, sc(3));
            else
                g.FillRectangle(_backBrush, sc(x), sc(5), sc(7 + 7) + size.Width, sc(height));
            g.DrawString(label, _font, Brushes.White, sc(x + 7), sc(5));
            x -= 4;
        }
        Height = (int) (5 + height + 5);
    }

    private string getKeyDisplay(Key key)
    {
        if (key >= Key.D0 && key <= Key.D9)
            return key.ToString().Replace("D", "");
        if (key >= Key.Num0 && key <= Key.Num9)
            return key.ToString().Replace("Num", "Num ");
        switch (key)
        {
            case Key.OemSemicolon: return ";";
            case Key.OemBacktick: return "`";
            case Key.OemOpenBracket: return "[";
            case Key.OemCloseBracket: return "]";
            case Key.OemPipe: return "|";
            case Key.OemComma: return ",";
            case Key.OemPeriod: return ".";
            case Key.OemQuestion: return "?";
            case Key.OemMinus: return "−";
            case Key.OemPlus: return "+";
            case Key.Escape: return "Esc";
            case Key.Left: return "←";
            case Key.Right: return "→";
            case Key.Up: return "↑";
            case Key.Down: return "↓";
            case Key.NumDivide: return "Num ÷";
            case Key.NumMultiply: return "Num ×";
            case Key.NumSubtract: return "Num −";
            case Key.NumAdd: return "Num +";
            case Key.NumDecimal: return "Num .";
            default: return key.ToString();
        }
    }
}
