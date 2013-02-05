using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using RT.Util;

namespace RomanKeys
{
    static class Program
    {
        private static GlobalKeyboardListener _keyboard;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            _keyboard = new GlobalKeyboardListener();
            _keyboard.HookAllKeys = true;
            _keyboard.KeyDown += keyboard_KeyDown;

            Brightness.Initialize();

            Application.Run();
        }

        private static void keyboard_KeyDown(object sender, GlobalKeyEventArgs e)
        {
            switch (e.VirtualKeyCode)
            {
                case Keys.Up:
                    if (e.ModifierKeys.Alt && e.ModifierKeys.Win)
                    {
                        Task.Run(() => Brightness.Step(up: true));
                        e.Handled = true;
                    }
                    break;
                case Keys.Down:
                    if (e.ModifierKeys.Alt && e.ModifierKeys.Win)
                    {
                        Task.Run(() => Brightness.Step(up: false));
                        e.Handled = true;
                    }
                    break;
            }
        }
    }
}
