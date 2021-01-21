using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;

namespace rollary.UWP
{
    public class MediaCaptureManager
    {
        #region MediaCapture controls 

        public bool Prepared { get; private set; } = false;
        public bool Active { get; private set; } = false;

        private MediaCapture mediaCapture;
        private MediaFrameReader mediaFrameReader;

        /// <summary>
        /// *Adapted from https://docs.microsoft.com/en-us/windows/uwp/audio-video-camera/process-media-frames-with-mediaframereader*
        /// </summary>
        /// <returns></returns>
        public async Task Prepare()
        {
            if (Prepared) return;

            mediaCapture = new MediaCapture();
            var frameSourceGroups = await MediaFrameSourceGroup.FindAllAsync();

            MediaFrameSourceGroup colorSourceGroup = null;
            MediaFrameSourceInfo colorSourceInfo = null;

            foreach (var frameSourceGroup in frameSourceGroups)
            {
                foreach (var sourceInfo in frameSourceGroup.SourceInfos)
                {
                    if (sourceInfo.MediaStreamType != MediaStreamType.VideoPreview
                            && sourceInfo.SourceKind != MediaFrameSourceKind.Color)
                    {
                        continue;
                    }

                    System.Diagnostics.Debug.WriteLine($"sourceInfo.SourceKind = {sourceInfo.SourceKind}");

                    colorSourceGroup = frameSourceGroup;
                    colorSourceInfo = sourceInfo;

                    System.Diagnostics.Debug.WriteLine($"colorSourceInfo.SourceKind = {colorSourceInfo.SourceKind}");

                    break;
                }

                if (colorSourceGroup != null && colorSourceInfo != null)
                {
                    break;
                }
            }

            await mediaCapture.InitializeAsync(new MediaCaptureInitializationSettings()
            {
                SourceGroup = colorSourceGroup,
                SharingMode = MediaCaptureSharingMode.SharedReadOnly,
                MemoryPreference = MediaCaptureMemoryPreference.Cpu,
                StreamingCaptureMode = StreamingCaptureMode.Video,
            });

            var colorFrameSource = mediaCapture.FrameSources[colorSourceInfo.Id];
            // TODO enumerate through? don't only make 0 available
            var preferredFormat = colorFrameSource.SupportedFormats[0];/*.FirstOrDefault(
                    format => format.Subtype == EncodingSubtype
            );*/

            if (preferredFormat == null)
            {
                throw new NotSupportedException("required format does not exist");
            }

            mediaFrameReader = await mediaCapture.CreateFrameReaderAsync(colorFrameSource, preferredFormat.Subtype);
            mediaFrameReader.AcquisitionMode = MediaFrameReaderAcquisitionMode.Buffered;
            mediaFrameReader.FrameArrived += ColorFrameReader_FrameArrived;

            Prepared = true;
        }

        public async Task Start()
        {
            if (Active) return;

            if (mediaFrameReader == null)
            {
                throw new NullReferenceException("not ready; call Prepare first");
            }

            await mediaFrameReader.StartAsync();

            Active = true;
        }
        public async Task Stop()
        {
            if (!Active) return;

            if (mediaFrameReader == null)
            {
                throw new NullReferenceException("not ready; call Prepare first");
            }

            await mediaFrameReader.StopAsync();

            Active = false;
        }

        public async Task Terminate()
        {
            if (!Prepared) return;

            await Stop();

            mediaFrameReader.Dispose();
            mediaFrameReader = null;
            mediaCapture.Dispose();
            mediaCapture = null;

            Prepared = false;
        }

        #endregion

        #region handle incoming frames

        private const BitmapPixelFormat PixelFormat = BitmapPixelFormat.Bgra8;
        private const int NBytesPerPixel = 4;

        private WriteableBitmap writeableBitmap;

        private SoftwareBitmap bitmapBackbuffer;
        private bool taskRunning = false;

        public event EventHandler<FrameReadyEventArgs> FrameReady;

        private void ColorFrameReader_FrameArrived(MediaFrameReader mediaFrameReader, MediaFrameArrivedEventArgs args)
        {
            // https://docs.microsoft.com/en-us/windows/uwp/audio-video-camera/process-media-frames-with-mediaframereader

            var mediaFrameReference = mediaFrameReader.TryAcquireLatestFrame();
            var mediaFrameBitmap = mediaFrameReference?.VideoMediaFrame?.SoftwareBitmap;

            if (mediaFrameBitmap == null) return;

            if (mediaFrameBitmap.BitmapPixelFormat != PixelFormat || mediaFrameBitmap.BitmapAlphaMode != BitmapAlphaMode.Ignore)
            {
                var bufferBitmap = SoftwareBitmap.Convert(mediaFrameBitmap, PixelFormat, BitmapAlphaMode.Ignore);
                mediaFrameBitmap.Dispose();
                mediaFrameBitmap = bufferBitmap;
            }

            mediaFrameBitmap = Interlocked.Exchange(ref bitmapBackbuffer, mediaFrameBitmap);
            mediaFrameBitmap?.Dispose();

            mediaFrameReference.Dispose();

            // `WriteableBitmap` with correct size must be instantiated in window dispatcher thread
            var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
            _ = dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    if (taskRunning) return;
                    taskRunning = true;

                    SoftwareBitmap nextBitmap;
                    while ((nextBitmap = Interlocked.Exchange(ref bitmapBackbuffer, null)) != null)
                    {
                        InvokeFrameReady(nextBitmap);
                        nextBitmap.Dispose();
                    }

                    taskRunning = false;
                });
        }

        private void InvokeFrameReady(SoftwareBitmap softwareBitmap)
        {
            // https://www.davidbritch.com/2013/06/accessing-image-pixel-data-in-c-windows.html

            // Use one `WriteableBitmap` to avoid progressively consuming more memory
            if (writeableBitmap == null || writeableBitmap?.PixelWidth != softwareBitmap.PixelWidth || writeableBitmap?.PixelHeight != softwareBitmap.PixelHeight)
            {
                writeableBitmap = new WriteableBitmap(softwareBitmap.PixelWidth, softwareBitmap.PixelHeight);
            }
            softwareBitmap.CopyToBuffer(writeableBitmap.PixelBuffer);

            var imageBytes = new byte[NBytesPerPixel * softwareBitmap.PixelWidth * softwareBitmap.PixelHeight];
            writeableBitmap.PixelBuffer.AsStream().Read(imageBytes, 0, imageBytes.Length);

            FrameReady?.Invoke(this, new FrameReadyEventArgs()
            {
                ImageBytes = imageBytes,
                //ImageStream = writeableBitmap.PixelBuffer.AsStream(),
                NBytesPerPixel = NBytesPerPixel,
                ImageWidth = softwareBitmap.PixelWidth,
                ImageHeight = softwareBitmap.PixelHeight,
            });
        }

        #endregion
    }
}
