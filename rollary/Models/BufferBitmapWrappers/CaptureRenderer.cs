using SkiaSharp;
using System.Runtime.InteropServices;

namespace rollary
{
    public class CaptureRenderer : BufferBitmapWrapper
    {
        public void DrawFrame(FrameReadyEventArgs frame)
        {
            if (!BitmapIsCreated)
            {
                Bitmap = new SKBitmap(frame.ImageWidth, frame.ImageHeight, true);
            }

            // https://github.com/mono/SkiaSharp/issues/416#issuecomment-356950196
            // Points the Skia bitmap data to the frame byte array without copying any data
            var info = Bitmap.Info;
            var handle = GCHandle.Alloc(frame.ImageBytes, GCHandleType.Pinned);
            Bitmap.InstallPixels(info, handle.AddrOfPinnedObject(), info.RowBytes, delegate
            {
                handle.Free();
            });
        }
    }
}
