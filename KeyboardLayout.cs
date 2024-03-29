﻿using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace RomanKeys;

class KeyboardLayoutModule : IModule
{
    public List<LayoutConfig> Layouts = new();

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
                    SetKeyboardLayout(WinAPI.GetForegroundWindow(), selected.Ptr);
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

    private void SetKeyboardLayout(nint hWnd, nint hLayout)
    {
        hWnd = GetAncestor(hWnd, 3 /* GA_ROOTOWNER */); // support changing layout for dialogs like Save As and Win+R "Run"
        var buf = new StringBuilder(256);
        GetClassName(hWnd, buf, buf.Capacity);
        if (buf.ToString() == "#32770") // support changing layout for top-level dialogs such as PuTTY: https://stackoverflow.com/a/51118612/33080
            EnumChildWindows(hWnd, postChangeToChildProc, hLayout);
        else
            WinAPI.PostMessage(hWnd, 0x0050 /* WM_INPUTLANGCHANGEREQUEST */, 0, hLayout);

        static bool postChangeToChildProc(nint hwnd, nint lParam)
        {
            WinAPI.PostMessage(hwnd, 0x0050 /* WM_INPUTLANGCHANGEREQUEST */, 0, lParam);
            return true;
        }
    }

    [DllImport("user32.dll", ExactSpelling = true)]
    static extern nint GetAncestor(nint hwnd, uint flags);
    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    static extern int GetClassName(nint hWnd, StringBuilder lpClassName, int nMaxCount);
    delegate bool EnumWindowsProc(IntPtr hwnd, IntPtr lParam);
    [DllImport("user32.dll")]
    static extern bool EnumChildWindows(IntPtr hwndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);
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
