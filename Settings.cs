using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RT.Util;

namespace RomanKeys
{
    [Settings("RomanKeys", SettingsKind.UserSpecific)]
    sealed class Settings : SettingsBase
    {
        public List<IModule> Modules = new List<IModule>();
    }

    interface IModule
    {
        bool HandleKey(Key key, ModifierKeysState modifiers);
    }

    interface IValueIndicator
    {
        int Value { get; set; }
        int MaxValue { get; set; }
        void Display();
    }
}
