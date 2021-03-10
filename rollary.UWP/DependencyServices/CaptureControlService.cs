using System;
using System.Threading.Tasks;
using Xamarin.Forms;

[assembly: Dependency(typeof(rollary.UWP.CaptureControlService))]
namespace rollary.UWP
{
    public class CaptureControlService : ICaptureControlService
    {
        private readonly MediaCaptureManager captureManager = new MediaCaptureManager();

        public bool Active => captureManager.Active;

        public async Task StartMediaCapture()
        {
            if (!captureManager.Prepared)
            {
                await captureManager.Prepare();
            }

            await captureManager.Start();
        }

        public Task StopMediaCapture() => captureManager.Stop();

        public Task TerminateMediaCapture() => captureManager.Terminate();

        public void SubscribeFrameReady(EventHandler<FrameReadyEventArgs> handler)
        {
            captureManager.FrameReady += handler;
        }

        public void UnsubscribeFrameReady(EventHandler<FrameReadyEventArgs> handler)
        {
            captureManager.FrameReady -= handler;
        }
    }
}
