﻿using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using RT.Util;

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

            SettingsUtil.LoadSettings(out Settings);

            Brightness.Initialize();

            Settings.Save();

            _keyboard = new GlobalKeyboardListener();
            _keyboard.HookAllKeys = true;
            _keyboard.KeyDown += keyboard_KeyDown;

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
