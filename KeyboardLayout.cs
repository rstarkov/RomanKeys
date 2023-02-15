using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace RomanKeys
{
    class KeyboardLayoutModule : IModule
    {
        public List<LayoutConfig> Layouts = new List<LayoutConfig>();

        public class LayoutConfig
        {
            public Hotkey Hotkey = null; // null means this entire entry is inactive
            public string LayoutNameRegex = null; // null means activate Nth installed layout, where N is the index of this entry in KeyboardLayoutModule's list of layouts
            public string DisplayName = null; // null means show its true name
            public ICaptionedIndicator Indicator = null;
        }

        public bool HandleKey(Hotkey key, bool down)
        {
            for (int i = 0; i < Layouts.Count; i++)
            {
                if (Layouts[i] == null || Layouts[i].Hotkey != key)
                    continue;
                if (down)
                {
                    var layouts = KeyboardLayouts.Current;
                    var selected = layouts.Count == 0 ? null : layouts[Math.Min(i, layouts.Count)];
                    if (Layouts[i].LayoutNameRegex != null)
                        selected = layouts.FirstOrDefault(li => Regex.IsMatch(li.Name, Layouts[i].LayoutNameRegex));
                    if (selected != null)
                    {
                        WinAPI.PostMessage(GetAncestor(WinAPI.GetForegroundWindow(), 3 /* GA_ROOTOWNER */), 0x0050 /* WM_INPUTLANGCHANGEREQUEST */, 0, selected.Ptr);
                        if (Layouts[i].Indicator != null)
                        {
                            Layouts[i].Indicator.Caption = Layouts[i].DisplayName ?? selected.Name;
                            Layouts[i].Indicator.Display();
                        }
                    }
                }
                return true;
            }
            return false;
        }

        [DllImport("user32.dll", ExactSpelling = true)]
        static extern nint GetAncestor(nint hwnd, uint flags);
    }

    static class KeyboardLayouts
    {
        public class LayoutInfo { public IntPtr Ptr; public string Name; };
        public static IList<LayoutInfo> Current;

        static KeyboardLayouts()
        {
            var was = WinAPI.GetKeyboardLayout(0);
            var buf = new IntPtr[64];
            int num = (int) WinAPI.GetKeyboardLayoutList(buf.Length, buf);
            var list = new List<LayoutInfo>();
            foreach (var ptr in buf.Take(num))
            {
                WinAPI.ActivateKeyboardLayout(ptr, 0x00000100);
                var sb = new StringBuilder();
                WinAPI.GetKeyboardLayoutName(sb);
                var name = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Keyboard Layouts\{0}".Fmt(sb.ToString()), "Layout Text", "n/a") as string;
                list.Add(new LayoutInfo { Ptr = ptr, Name = name + " (" + sb.ToString() + ")" });
            }
            WinAPI.ActivateKeyboardLayout(was, 0x00000100);
            Current = list.AsReadOnly();
        }
    }
}
