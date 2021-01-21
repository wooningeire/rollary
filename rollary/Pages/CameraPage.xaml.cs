using SkiaSharp.Views.Forms;
using System;
using Xamarin.Forms;

namespace rollary
{
    public partial class CameraPage : ContentPage
    {
        // currently does not handle resolution changes, camera switching
        // breaks (but does not error) on permissions denial
        // media capture does not restart on UWP VisibilityChanged

        private static readonly int FallbackWidth = 1920;
        private static readonly int FallbackHeight = 1080;

        private readonly ICaptureControlService captureControl;
        private readonly ResultStateManager resultStateManager;

        private readonly PreviewPainter previewPainter;
        
        public CameraPage(ICaptureControlService captureControl, ResultStateManager resultStateManager)
        {
            this.captureControl = captureControl;
            this.resultStateManager = resultStateManager;
            previewPainter = new PreviewPainter(resultStateManager);

            captureControl.SubscribeFrameReady(CaptureControl_FrameReady);
            resultStateManager.Finished += ResultManager_Finished;

            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            System.Diagnostics.Debug.WriteLine("I'm back");

            base.OnAppearing();
            //resultStateManager.Resume();

            if (!resultStateManager.Done)
            {
                LoadMediaCapture();
            }
        }

        protected override void OnDisappearing()
        {
            //System.Diagnostics.Debug.WriteLine("I disappeared!!");

            base.OnDisappearing();
            resultStateManager.Pause();

            captureControl.StopMediaCapture();
        }

        private void StartButton_Clicked(object sender, EventArgs eventArgs)
        {
            resultStateManager.Start();

            UpdateButtonControlVisibility();
        }

        private void SaveButton_Clicked(object sender, EventArgs eventArgs)
        {

        }

        private void PauseButton_Clicked(object sender, EventArgs eventArgs)
        {
            resultStateManager.Pause();

            UpdateButtonControlVisibility();
        }

        private void ResetButton_Clicked(object sender, EventArgs eventArgs)
        {
            resultStateManager.Reset();

            LoadMediaCapture();

            PreviewFeedCanvas.InvalidateSurface();
            UpdateButtonControlVisibility();
        }

        private void ResultManager_Finished(object sender, EventArgs eventArgs)
        {
            captureControl.StopMediaCapture();

            UpdateButtonControlVisibility();
        }

        private async void SettingsButton_Clicked(object sender, EventArgs eventArgs)
        {
            var captureRenderer = resultStateManager.CaptureRenderer;

            var args = captureRenderer.BitmapIsCreated
                ? new SettingsPageArgs()
                        {
                            ImageWidth = captureRenderer.Width,
                            ImageHeight = captureRenderer.Height,
                        }
                : new SettingsPageArgs()
                        {
                            ImageWidth = FallbackWidth,
                            ImageHeight = FallbackHeight,
                        };

            await Navigation.PushAsync(new SettingsPage(resultStateManager.Settings, args, !resultStateManager.Started));
        }

        private void UpdateButtonControlVisibility()
        {
            bool started = resultStateManager.Started;
            bool running = resultStateManager.Running;
            bool done = resultStateManager.Done;

            StartButton.IsVisible = !running && !done;
            PauseButton.IsVisible = started && running;
            SaveButton.IsVisible = started && !running;
            ResetButton.IsVisible = started && !running;
        }

        /// <summary>
        /// Starts the media capture and enables the activity indicator.
        /// </summary>
        public void LoadMediaCapture()
        {
            captureControl.StartMediaCapture();
            PreviewLoading.IsRunning = true;
        }

        private void CaptureControl_FrameReady(object sender, FrameReadyEventArgs eventArgs)
        {
            resultStateManager.RecordNewFrame(eventArgs);

            PreviewLoading.IsRunning = false;
            PreviewFeedCanvas.InvalidateSurface();
        }

        private void PreviewFeedCanvas_PaintSurface(object sender, SKPaintSurfaceEventArgs eventArgs)
        {
            previewPainter.PaintPreview(eventArgs.Surface.Canvas, eventArgs.Info, captureControl.Active);
        }
    }
}
