using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using RT.Serialization;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace RomanKeys
{
    [Settings("RomanKeys", SettingsKind.UserSpecific)]
    sealed class Settings : SettingsBase
    {
        public List<IModule> Modules = new List<IModule>();
    }

    interface IModule
    {
        bool HandleKey(Hotkey key, bool down);
    }

    interface IIndicator
    {
        void Display();
    }

    interface ICaptionedIndicator : IIndicator
    {
        string Caption { get; set; }
    }

    interface IValueIndicator : ICaptionedIndicator
    {
        int Value { get; set; }
        int MaxValue { get; set; }
    }

    class HotkeyTypeOptions : IClassifySubstitute<Hotkey, string>
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

    class TimeSpanTypeOptions : IClassifySubstitute<TimeSpan, decimal>
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

    class ColorTypeOptions : IClassifySubstitute<Color, string>
    {
        Color IClassifySubstitute<Color, string>.FromSubstitute(string instance) { return FromSubstituteD(instance); }
        private Color FromSubstituteD(string instance)
        {
            try
            {
                if (!instance.StartsWith("#") || (instance.Length != 7 && instance.Length != 9))
                    throw new Exception();
                int alpha = instance.Length == 7 ? 255 : int.Parse(instance.Substring(1, 2), NumberStyles.HexNumber);
                int r = int.Parse(instance.Substring(instance.Length == 7 ? 1 : 3, 2), NumberStyles.HexNumber);
                int g = int.Parse(instance.Substring(instance.Length == 7 ? 3 : 5, 2), NumberStyles.HexNumber);
                int b = int.Parse(instance.Substring(instance.Length == 7 ? 5 : 7, 2), NumberStyles.HexNumber);
                return Color.FromArgb(alpha, r, g, b);
            }
            catch
            {
                return Color.Black; // XmlClassify doesn't currently let us specify "no value", so just use a random color
            }
        }

        public string ToSubstitute(Color instance)
        {
            return instance.A == 255 ? "#{0:X2}{1:X2}{2:X2}".Fmt(instance.R, instance.G, instance.B) : "#{0:X2}{1:X2}{2:X2}{3:X2}".Fmt(instance.A, instance.R, instance.G, instance.B);
        }
    }
}
