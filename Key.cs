using RT.Util;
using RT.Util.ExtensionMethods;

namespace RomanKeys;

sealed class Hotkey
{
    public Key Key { get; private set; }
    public ModifierKeysState Modifiers { get; private set; }

    public Hotkey(Key key, ModifierKeysState modifiers)
    {
        Key = key;
        Modifiers = modifiers;
    }

    public Hotkey(Key key, bool ctrl = false, bool alt = false, bool shift = false, bool win = false)
        : this(key, new ModifierKeysState(ctrl, alt, shift, win))
    {
    }

    public static bool operator ==(Hotkey key1, Hotkey key2)
    {
        return key1.Key == key2.Key && key1.Modifiers == key2.Modifiers;
    }

    public static bool operator !=(Hotkey key1, Hotkey key2)
    {
        return key1.Key != key2.Key || key1.Modifiers != key2.Modifiers;
    }

    public override bool Equals(object obj)
    {
        return (obj is Hotkey hotkey) && (this == hotkey);
    }

    public override int GetHashCode()
    {
        return Key.GetHashCode() * 7919 + Modifiers.GetHashCode();
    }

    public override string ToString()
    {
        return (Modifiers.Ctrl ? "Ctrl+" : "") + (Modifiers.Alt ? "Alt+" : "") + (Modifiers.Shift ? "Shift+" : "") + (Modifiers.Win ? "Win+" : "") + ToNiceKeyString(Key);
    }

    public static Hotkey Parse(string str)
    {
        var parts = str.Split('+');
        bool ctrl = false, alt = false, shift = false, win = false;
        for (int i = 0; i < parts.Length - 1; i++)
        {
            if (parts[i].EqualsIgnoreCase("Ctrl"))
                ctrl = true;
            else if (parts[i].EqualsIgnoreCase("Alt"))
                alt = true;
            else if (parts[i].EqualsIgnoreCase("Shift"))
                shift = true;
            else if (parts[i].EqualsIgnoreCase("Win"))
                win = true;
            else
                throw new ArgumentException("Cannot parse Hotkey part: “" + parts[i] + "”");
        }
        return new Hotkey(ParseNiceKeyString(parts[^1]), new ModifierKeysState(ctrl, alt, shift, win));
    }

    public static Key ParseNiceKeyString(string str)
    {
        if (int.TryParse(str, out int result) && result >= 0 && result <= 9)
            return result + Key.D0;
        return EnumStrong.Parse<Key>(str, true);
    }

    public static string ToNiceKeyString(Key key)
    {
        if (key >= Key.D0 && key <= Key.D9)
            return ((int) key - (int) Key.D0).ToString();
        return key.ToString();
    }
}

enum Key
{
    //MouseLeft = 1,
    //MouseRight = 2,
    Break = 3,
    //MouseMiddle = 4,
    //MouseBack = 5,
    //MouseForward = 6,
    Backspace = 8,
    Tab = 9,
    LineFeed = 10,
    Clear = 12, // Produced by NumPad5 without NumLock
    Enter = 13,
    Shift = 16,
    Ctrl = 17,
    Alt = 18,
    Pause = 19,
    CapsLock = 20,
    KanaMode = 21,
    JunjaMode = 23,
    FinalMode = 24,
    KanjiMode = 25,
    Escape = 27,
    IMEConvert = 28,
    IMENonconvert = 29,
    IMEAccept = 30,
    IMEModeChange = 31,
    Space = 32,
    PageUp = 33,
    PageDown = 34,
    End = 35,
    Home = 36,
    Left = 37,
    Up = 38,
    Right = 39,
    Down = 40,
    Select = 41,
    Print = 42,
    Execute = 43,
    PrintScreen = 44,
    Insert = 45,
    Delete = 46,
    Help = 47,
    D0 = 48,
    D1 = 49,
    D2 = 50,
    D3 = 51,
    D4 = 52,
    D5 = 53,
    D6 = 54,
    D7 = 55,
    D8 = 56,
    D9 = 57,
    A = 65,
    B = 66,
    C = 67,
    D = 68,
    E = 69,
    F = 70,
    G = 71,
    H = 72,
    I = 73,
    J = 74,
    K = 75,
    L = 76,
    M = 77,
    N = 78,
    O = 79,
    P = 80,
    Q = 81,
    R = 82,
    S = 83,
    T = 84,
    U = 85,
    V = 86,
    W = 87,
    X = 88,
    Y = 89,
    Z = 90,
    LWin = 91,
    RWin = 92,
    Apps = 93,
    Sleep = 95,
    Num0 = 96,
    Num1 = 97,
    Num2 = 98,
    Num3 = 99,
    Num4 = 100,
    Num5 = 101,
    Num6 = 102,
    Num7 = 103,
    Num8 = 104,
    Num9 = 105,
    NumMultiply = 106,
    NumAdd = 107,
    NumSeparator = 108,
    NumSubtract = 109,
    NumDecimal = 110,
    NumDivide = 111,
    F1 = 112,
    F2 = 113,
    F3 = 114,
    F4 = 115,
    F5 = 116,
    F6 = 117,
    F7 = 118,
    F8 = 119,
    F9 = 120,
    F10 = 121,
    F11 = 122,
    F12 = 123,
    F13 = 124,
    F14 = 125,
    F15 = 126,
    F16 = 127,
    F17 = 128,
    F18 = 129,
    F19 = 130,
    F20 = 131,
    F21 = 132,
    F22 = 133,
    F23 = 134,
    F24 = 135,
    NumLock = 144,
    ScrollLock = 145,
    LShift = 160,
    RShift = 161,
    LCtrl = 162,
    RCtrl = 163,
    LAlt = 164,
    RAlt = 165,
    BrowserBack = 166,
    BrowserForward = 167,
    BrowserRefresh = 168,
    BrowserStop = 169,
    BrowserSearch = 170,
    BrowserFavorites = 171,
    BrowserHome = 172,
    VolumeMute = 173,
    VolumeDown = 174,
    VolumeUp = 175,
    MediaNextTrack = 176,
    MediaPreviousTrack = 177,
    MediaStop = 178,
    MediaPlayPause = 179,
    LaunchMail = 180,
    LaunchMedia = 181,
    LaunchApplication1 = 182,
    LaunchCalculator = 183,
    OemSemicolon = 186,
    OemPlus = 187,
    OemComma = 188,
    OemMinus = 189,
    OemPeriod = 190,
    OemQuestion = 191,
    OemTilde = 192,
    OemOpenBracket = 219,
    OemPipe = 220,
    OemCloseBracket = 221,
    OemQuotes = 222,
    OemBacktick = 223,
    OemBackslash = 226,
    ProcessKey = 229,
    Packet = 231,
    Attn = 246,
    Crsel = 247,
    Exsel = 248,
    EraseEof = 249,
    Play = 250,
    Zoom = 251,
    NoName = 252,
    Pa1 = 253,
    OemClear = 254,
    // Not in the standard Keys enum
    //MouseWheelUp = 256,
    //MouseWheelDown = 257,
    //MouseWheelLeft = 258,
    //MouseWheelRight = 259,
    NumEnter = 260,
}
