using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
        
namespace Capybara
{
    public partial class FormMain : Form
    {
        //TODO:
        //- Recording interpolation between non-zero flag events
        //- Option to ignore movement-only events, and reset mouse position after event 
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


        public FormMain()
        {
            InitializeComponent();

            //For some reason VS complains about this being present in the Designer.cs
            this.trackBarReplaySpeed.Scroll += (o, e) =>
            {
                toolTipReplaySpeed.SetToolTip(this.trackBarReplaySpeed, System.String.Format("{0:0.0000}x", System.Math.Pow(2, trackBarReplaySpeed.Value / 10f)));
            };

            //Keep this form on the top
            this.TopMost = true;

            //Attempt to register Pause as a hotkey for our application
            //Fails if another process has registered Pause as a hotkey
            if ((_id = Enumerable.Range(1, 100).FirstOrDefault(v => Windows.RegisterHotKey(this.Handle, v, 0, (int)Keys.Pause))) == 0)
            {
                MessageBox.Show("Failed to register Pause as the interrupt hotkey", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            //TODO: Add hotkeys for replaying and recording
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Unregister the presumably set hotkey
            Windows.UnregisterHotKey(this.Handle, _id);

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
                            (newState.HasFlag(MouseButtons.Left) && !oldState.HasFlag(MouseButtons.Left)? Windows.LEFT_DOWN : 0) |
                            (!newState.HasFlag(MouseButtons.Left) && oldState.HasFlag(MouseButtons.Left) ? Windows.LEFT_UP : 0) |
                            (newState.HasFlag(MouseButtons.Right) && !oldState.HasFlag(MouseButtons.Right) ? Windows.RIGHT_DOWN : 0) |
                            (!newState.HasFlag(MouseButtons.Right) && oldState.HasFlag(MouseButtons.Right) ? Windows.RIGHT_UP : 0)
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
            foreach (Button b in this.Controls.OfType<Button>())
                b.Enabled = true;
        }

        private void buttonPlay_Click(object sender, EventArgs e)
        {
            StopWorkerThread();

            //Stop if there's nothing to play
            if (_record == null || _record.Count == 0)
                return;

            this.Text = "Capybara - Playing";

            bool _repeat = checkBoxRepeat.Checked;
            //We expect trackBarReplaySpeed to be a LOGARITHMIC scale, so we need to transform the value
            double _replayRate = Math.Pow(2, trackBarReplaySpeed.Value / 10f);

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
                        Windows.SendClick(entry.Flag);
                        Thread.Sleep((int)(1000 / (EventsPerSecond * _replayRate)));
                    }
                } while (_repeat);
            });

            _running = true;
            //Disable everything except buttonStop
            foreach (Control c in this.Controls)
                if (c != buttonStop)
                    c.Enabled = false;

            _worker.Start();
        }

        protected override void WndProc(ref Message m)
        {
            //Bailout
            //WM_HOTKEY = 0x0312
            if (m.Msg == 0x0312 && m.WParam.ToInt32() == _id)
            {
                //If nothing is running, begin running
                if (_worker == null || !_worker.IsAlive)
                    buttonPlay.PerformClick();
                //Otherwise, should act the same as pressing the Stop button
                else
                    buttonStop.PerformClick();
            }
            base.WndProc(ref m);
        }

        protected void StopWorkerThread()
        {
            //Notify the worker thread to stop running
            if (_worker != null && _worker.IsAlive)
            {
                //Signal the thread to stop
                _running = false;
                //Give the worker thread 50ms to finish
                if (!_worker.Join(50))
                {
                    //Otherwise, forcibly abort it
                    _worker.Abort();
                }
            }
        }
    }

    public sealed class EventInformation
    {
        /// <summary>
        /// Position at which the event was recorded
        /// </summary>
        public readonly Point Position;
        
        /// <summary>
        /// The dwFlags value passed to SendInput
        /// </summary>
        public readonly uint Flag;
        
        public EventInformation(Point pos, uint flag)
        {
            Position = pos;
            Flag = flag;
        }
    }
}
