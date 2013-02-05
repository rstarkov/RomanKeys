using System;
using System.Linq;
using System.Management;
using RT.Util.ExtensionMethods;

namespace RomanKeys
{
    static class Brightness
    {
        public static void Initialize()
        {
            if (Program.Settings.BrightnessIndicator == null)
                Program.Settings.BrightnessIndicator = new BarPopup { Caption = "Brightness", Timeout = TimeSpan.FromSeconds(1.2) };
        }

        public static void Step(bool up)
        {
            var scope = new ManagementScope(@"\\.\root\wmi");
            using (var mcGet = new ManagementClass("WmiMonitorBrightness") { Scope = scope })
            using (var mcSet = new ManagementClass("WmiMonitorBrightnessMethods") { Scope = scope })
            using (var moGet = firstThenDispose(mcGet.GetInstances()))
            using (var moSet = firstThenDispose(mcSet.GetInstances()))
            {
                var levels = ((byte[]) moGet.GetPropertyValue("Level")).Order().ToList();
                var brightness = (byte) moGet.GetPropertyValue("CurrentBrightness");

                var index = levels.IndexOf(lvl => brightness <= lvl);
                if (index == -1)
                    index = levels.Count - 1;
                index = (index + (up ? 1 : -1)).Clip(0, levels.Count - 1);

                moSet.InvokeMethod("WmiSetBrightness", new object[] { 1, levels[index] });

                Program.Settings.BrightnessIndicator.MaxValue = levels.Count - 1;
                Program.Settings.BrightnessIndicator.Value = index;
                Program.Settings.BrightnessIndicator.Display();
            }
        }

        private static ManagementObject firstThenDispose(ManagementObjectCollection collection)
        {
            using (collection)
                foreach (ManagementObject item in collection)
                    return item;
            throw new ArgumentException("Collection is empty", "collection");
        }
    }
}
