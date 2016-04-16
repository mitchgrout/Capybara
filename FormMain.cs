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
        protected static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, uint dwExtraInfo);

        //mouse_event dwFlags:
        public const uint LEFT_DOWN  = 0x02,
                          LEFT_UP    = 0x04,
                          RIGHT_DOWN = 0x08,
                          RIGHT_UP   = 0x10;

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
            mouse_event(LEFT_UP | RIGHT_UP, 0, 0, 0, 0);
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
                    if (entry.Flag != 0)
                        mouse_event(entry.Flag, 0, 0, 0, 0);
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
