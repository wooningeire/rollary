using System;
using System.Threading.Tasks;

namespace rollary
{
    public interface ICaptureControlService
    {
        bool Active { get; }

        Task StartMediaCapture();
        Task StopMediaCapture();
        Task TerminateMediaCapture();

        void SubscribeFrameReady(EventHandler<FrameReadyEventArgs> handler);
        void UnsubscribeFrameReady(EventHandler<FrameReadyEventArgs> handler);
    }
}
