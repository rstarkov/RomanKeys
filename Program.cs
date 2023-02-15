using RT.Serialization;
using RT.Util;

namespace RomanKeys;

static class Program
{
    public static Settings Settings;

    private static GlobalKeyboardListener _keyboard;

    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        Classify.DefaultOptions.AddTypeSubstitution(new HotkeyTypeOptions());
        Classify.DefaultOptions.AddTypeSubstitution(new TimeSpanTypeOptions());
        Classify.DefaultOptions.AddTypeSubstitution(new ColorTypeOptions());
        SettingsUtil.LoadSettings(out Settings);

        if (Settings.Modules.Count == 0)
            Settings.Modules.Add(new BrightnessModule());

        Settings.Save();

        _keyboard = new GlobalKeyboardListener();
        _keyboard.HookAllKeys = true;
        _keyboard.KeyDown += keyboard_KeyDown;
        _keyboard.KeyUp += keyboard_KeyUp;

        Application.Run();
    }

    private static void keyboard_KeyDown(object sender, GlobalKeyEventArgs e)
    {
        foreach (var module in Program.Settings.Modules)
            if (module.HandleKey(new Hotkey((Key) e.VirtualKeyCode, e.ModifierKeys), true))
            {
                e.Handled = true;
                return;
            }
    }

    private static void keyboard_KeyUp(object sender, GlobalKeyEventArgs e)
    {
        foreach (var module in Program.Settings.Modules)
            if (module.HandleKey(new Hotkey((Key) e.VirtualKeyCode, e.ModifierKeys), false))
            {
                e.Handled = true;
                return;
            }
    }
}
