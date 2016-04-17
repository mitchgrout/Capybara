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
        /// Notifies the worker thread if it should stop, instead of using Thread.Abort
        /// </summary>
        protected volatile bool _running;

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

            StopWorkerThread();
        }
        
        private void buttonRecord_Click(object sender, EventArgs e)
        {
            this.Text = "Capybara - Recording";

            StopWorkerThread();
            
            //Clear out the record list
            if (_record == null)
                _record = new LinkedList<EventInformation>();
            else
                _record.Clear();

            //Recreate the worker thread
            _worker = new Thread(() =>
            {
                MouseButtons oldState = default(MouseButtons), newState = default(MouseButtons);
                
                while(_running)
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

                //TODO: Change this so we only add a 0x4 if there's an unterminated 0x2,
                //      and same for 0x10 and 0x8
                _record.AddLast(new EventInformation(Cursor.Position, 0x6 | 0x18));
            });

            _running = true;
            //Disable everything except buttonStop
            //this.buttonRecord.Enabled = false;
            //this.buttonPlay.Enabled = false;
            foreach(Control c in this.Controls)
                if(c != buttonStop)
                    c.Enabled = false;

            _worker.Start();
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            this.Text = "Capybara";

            StopWorkerThread();
            
            //Re-enable all disabled controls
            foreach (Control c in this.Controls)
                c.Enabled = true;
        }

        private void buttonPlay_Click(object sender, EventArgs e)
        {
            StopWorkerThread();

            //Stop if there's nothing to play
            if (_record == null || _record.Count == 0)
                return;

            this.Text = "Capybara - Playing";

            bool _repeat = checkBoxRepeat.Checked;

            //Recreate the worker thread
            _worker = new Thread(() =>
            {
                do
                {
                    foreach (var entry in _record)
                    {
                        //Early termination
                        if (!_running)
                            break;
                        Cursor.Position = entry.Position;
                        SendClick(entry.Flag);
                        Thread.Sleep(1000 / EventsPerSecond);
                    }
                } while (_repeat);
            });

            _running = true;
            //Disable everything except buttonStop
            //this.buttonRecord.Enabled = false;
            //this.buttonPlay.Enabled = false;
            foreach (Control c in this.Controls)
                if (c != buttonStop)
                    c.Enabled = false;

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

        protected void StopWorkerThread()
        {
            //Notify the worker thread to stop running
            if (_worker != null && _worker.IsAlive)
            {
                //Give the worker thread 50ms to finish
                if (!_worker.Join(50))
                {
                    //Otherwise, forcibly abort it
                    _worker.Abort();
                }
            }
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
