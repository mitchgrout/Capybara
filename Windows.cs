using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Capybara
{
    /// <summary>
    /// Contains utility functions provided by the operating system
    /// </summary>
    abstract class Windows
    {
        //Required extern functions
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);
        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        [DllImport("user32.dll")]
        protected static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        //MOUSEINPUT dwFlags:
        public const uint LEFT_DOWN = 0x02,
                          LEFT_UP = 0x04,
                          RIGHT_DOWN = 0x08,
                          RIGHT_UP = 0x10;

        [StructLayout(LayoutKind.Sequential)]
        protected struct INPUT
        {
            public uint type; //mouse = 0, keyboard = 1, hardware = 2
            public MOUSEINPUT mi;
            //Keyboard and hardware emulation is not required by Capybara
            //MOUSEINPUT is also larger than KEYBDINPUT and HARDWAREINPUT so INPUT remains the correct size

            public INPUT(uint dwFlags)
                : this(dwFlags, 0, 0)
            { }

            public INPUT(uint dwFlags, int dx, int dy)
            {
                this.type = 0;
                this.mi = new MOUSEINPUT
                {
                    dwFlags = dwFlags,
                    dx = dx,
                    dy = dy,
                };
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        protected struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        public static void SendClick(uint flag, int dx = 0, int dy = 0)
        {
            if (flag != 0)
            {
                if (SendInput(1, new INPUT[] { new INPUT(flag, dx, dy) }, Marshal.SizeOf(typeof(INPUT))) == 0)
                {
                    #if DEBUG
                    System.Diagnostics.Debug.Fail("SendInput returned zero");
                    #endif
                }
            }
        }
    }
}
