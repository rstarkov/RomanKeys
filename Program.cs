using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using RT.Util;
using RT.Util.Xml;

namespace RomanKeys
{
    static class Program
    {
        public static Settings Settings;

        private static GlobalKeyboardListener _keyboard;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            XmlClassify.DefaultOptions.AddTypeOptions(typeof(Hotkey), new HotkeyTypeOptions());
            XmlClassify.DefaultOptions.AddTypeOptions(typeof(TimeSpan), new TimeSpanTypeOptions());
            SettingsUtil.LoadSettings(out Settings);

            if (Settings.Modules.Count == 0)
            {
                Settings.Modules.Add(new BrightnessModule());
            }

            Settings.Save();

            _keyboard = new GlobalKeyboardListener();
            _keyboard.HookAllKeys = true;
            _keyboard.KeyDown += keyboard_KeyDown;

            Application.Run();
        }

        private static void keyboard_KeyDown(object sender, GlobalKeyEventArgs e)
        {
            foreach (var module in Program.Settings.Modules)
                if (module.HandleKey(new Hotkey((Key) e.VirtualKeyCode, e.ModifierKeys)))
                {
                    e.Handled = true;
                    return;
                }
        }
    }
}
