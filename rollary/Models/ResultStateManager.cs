using System;
using System.Collections.Generic;
using System.Text;

namespace rollary
{
    public class ResultStateManager
    {
        public readonly ResultSettings Settings = new ResultSettings();
        public readonly CaptureRenderer CaptureRenderer = new CaptureRenderer();
        public readonly ResultRenderer ResultRenderer;

        // Result state flags primarily change the behavior for incoming frames; no autonomous action
        // prefer enum State(bool, bool, bool)?
        public bool Started { get; private set; } = false;
        public bool Running { get; private set; } = false;
        public bool Done { get; private set; } = false;

        /// <summary>
        /// Index of the most recently drawn frame. -1 if no frames have been drawn.
        /// </summary>
        public int NFrame { get; private set; } = -1;

        private bool WillBeDoneNextFrame => CaptureRenderer.BitmapIsCreated && (NFrame + 1) * Settings.FrameScanWidth >= CaptureRenderer.Bitmap.Width;

        public event EventHandler Finished;

        public ResultStateManager()
        {
            ResultRenderer = new ResultRenderer(Settings);
        }

        /// <summary>
        /// Starts or resumes the result rendering for newly incoming frames.
        /// </summary>
        public void Start()
        {
            if (Done) return;

            Started = true;
            Running = true;
        }

        /// <summary>
        /// Pauses the result rendering for newly incoming frames.
        /// </summary>
        public void Pause()
        {
            if (!Started || !Running) return;

            Running = false;
        }

        /// <summary>
        /// Resumes the result rendering for newly incoming frames, but does not start it if it has not already started.
        /// </summary>
        public void Resume()
        {
            if (!Started || Running || Done) return;

            Running = true;
        }

        private void Finish()
        {
            if (!Started || Done) return;

            Running = false;
            Done = true;

            Finished?.Invoke(this, EventArgs.Empty);
        }

        public void Reset()
        {
            Started = false;
            Running = false;
            Done = false;

            NFrame = -1;

            ResultRenderer.Clear();
        }

        public void RecordNewFrame(FrameReadyEventArgs frame)
        {
            if (Done) return;

            CaptureRenderer.DrawFrame(frame);

            if (!Running) return;

            NFrame++;
            ResultRenderer.DrawSection(NFrame, CaptureRenderer.Bitmap);

            if (WillBeDoneNextFrame)
            {
                Finish();
            }
        }
    }
}
