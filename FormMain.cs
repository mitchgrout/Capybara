using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Capybara
{
    public partial class FormMain : Form
    {
        //TODO:
        //- Recording interpolation between non-zero flag events
        //- Option to ignore movement-only events, and reset mouse position after event 
        //- Changeable replay speed
        //- Potentially allow direct editing of a record

        /// <summary>
        /// Single thread used to replay a recording
        /// </summary>
        protected Thread _worker;
        
        /// <summary>
        /// Global hotkey ID
        /// </summary>
        protected int _id;

        /// <summary>
        /// List of recorded events
        /// </summary>
        protected volatile LinkedList<EventInformation> _record;

        /// <summary>
        /// The number of mouse events to record per second
        /// </summary>
        public const int EventsPerSecond = 60;

        //Required extern functions
        [DllImport("user32.dll")]
        protected static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);
        [DllImport("user32.dll")]
        protected static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        [DllImport("user32.dll")]
        protected static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        //MOUSEINPUT dwFlags:
        public const uint LEFT_DOWN  = 0x02,
                          LEFT_UP    = 0x04,
                          RIGHT_DOWN = 0x08,
                          RIGHT_UP   = 0x10;

        [StructLayout(LayoutKind.Sequential)]
        protected struct INPUT
        {
            public uint type; //mouse = 0, keyboard = 1, hardware = 2
            public MOUSEINPUT mi;
            //Keyboard and hardware emulation
            //is not required by Capybara
            //MOUSEINPUT is also larger than KEYBDINPUT and
            //HARDWAREINPUT so INPUT remains the correct size

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

        public FormMain()
        {
            InitializeComponent();

            //Keep this form on the top
            this.TopMost = true;

            //Attempt to register Pause as a hotkey for our application
            //Fails if another process has registered Pause as a hotkey
            if((_id = Enumerable.Range(1, 100).FirstOrDefault(v => RegisterHotKey(this.Handle, v, 0, (int)Keys.Pause))) == 0)
            {
                MessageBox.Show("Failed to register Pause as the interrupt hotkey", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Unregister the presumably set hotkey
            UnregisterHotKey(this.Handle, _id);
            //Kill the worker thread if it's still running
            if (_worker != null && _worker.IsAlive)
                _worker.Abort();
        }
        
        private void buttonRecord_Click(object sender, EventArgs e)
        {
            this.Text = "Capybara - Recording";
            //Kill the worker thread if it's currently doing something
            if (_worker != null && _worker.IsAlive)
                _worker.Abort();
            
            //Clear out the record list
            if (_record == null)
                _record = new LinkedList<EventInformation>();
            else
                _record.Clear();

            //Recreate the worker thread
            _worker = new Thread(() =>
            {
                MouseButtons oldState = default(MouseButtons), newState = default(MouseButtons);
                
                while(true)
                {
                    newState = Control.MouseButtons;
                    
                    _record.AddLast(new EventInformation(Cursor.Position,
                            (newState.HasFlag(MouseButtons.Left) && !oldState.HasFlag(MouseButtons.Left)? LEFT_DOWN : 0) |
                            (!newState.HasFlag(MouseButtons.Left) && oldState.HasFlag(MouseButtons.Left) ? LEFT_UP : 0) |
                            (newState.HasFlag(MouseButtons.Right) && !oldState.HasFlag(MouseButtons.Right) ? RIGHT_DOWN : 0) |
                            (!newState.HasFlag(MouseButtons.Right) && oldState.HasFlag(MouseButtons.Right)? RIGHT_UP : 0)
                        ));

                    oldState = newState;

                    Thread.Sleep(1000 / EventsPerSecond);
                }
            });
            _worker.Start();
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            this.Text = "Capybara";
            //Kill the worker thread if it's currently doing something
            if (_worker != null && _worker.IsAlive)
                _worker.Abort();
            //Un-click all buttons in case we stopped midway through an extended click
            SendClick(LEFT_UP | RIGHT_UP);
        }

        private void buttonPlay_Click(object sender, EventArgs e)
        {
            //Kill the worker thread if it's currently doing something
            if (_worker != null && _worker.IsAlive)
                _worker.Abort();

            //Stop if there's nothing to play
            if (_record == null || _record.Count == 0)
                return;

            this.Text = "Capybara - Playing";
            //Recreate the worker thread
            _worker = new Thread(() =>
            {
                foreach(var entry in _record)
                {
                    Cursor.Position = entry.Position;
                    SendClick(entry.Flag);
                    Thread.Sleep(1000 / EventsPerSecond);
                }
            });
            _worker.Start();
        }

        protected override void WndProc(ref Message m)
        {
            //Bailout
            if (m.Msg == 0x0312 && m.WParam.ToInt32() == _id)
            {
                //Pause key should act the same as pressing the Stop button
                buttonStop.PerformClick();
            }
            base.WndProc(ref m);
        }

        protected void SendClick(uint flag, int dx = 0, int dy = 0)
        {
            if(flag != 0)
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

    public sealed class EventInformation
    {
        public readonly Point Position;
        public readonly uint Flag;
        public EventInformation(Point pos, uint flag)
        {
            Position = pos;
            Flag = flag;
        }
    }
}
