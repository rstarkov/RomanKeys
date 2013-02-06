using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RT.Util;
using RT.Util.Xml;

namespace RomanKeys
{
    [Settings("RomanKeys", SettingsKind.UserSpecific)]
    sealed class Settings : SettingsBase
    {
        public List<IModule> Modules = new List<IModule>();
    }

    interface IModule
    {
        bool HandleKey(Hotkey key);
    }

    interface IValueIndicator
    {
        int Value { get; set; }
        int MaxValue { get; set; }
        void Display();
    }

    class HotkeyTypeOptions : XmlClassifyTypeOptions, IXmlClassifySubstitute<Hotkey, string>
    {
        public string ToSubstitute(Hotkey instance)
        {
            return instance.ToString();
        }

        public Hotkey FromSubstitute(string instance)
        {
            return Hotkey.Parse(instance);
        }
    }

    class TimeSpanTypeOptions : XmlClassifyTypeOptions, IXmlClassifySubstitute<TimeSpan, decimal>
    {
        public decimal ToSubstitute(TimeSpan instance)
        {
            return Math.Round((decimal) instance.TotalSeconds, 3);
        }

        public TimeSpan FromSubstitute(decimal instance)
        {
            return TimeSpan.FromSeconds((double) instance);
        }
    }
}
