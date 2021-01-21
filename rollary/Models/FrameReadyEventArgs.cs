//using SkiaSharp;
using System;

namespace rollary
{
    public class FrameReadyEventArgs : EventArgs
    {
        public byte[] ImageBytes;
        //public Stream ImageStream;
        public int NBytesPerPixel;
        public int ImageWidth;
        public int ImageHeight;

        /*public int PixelByteOffset(int x, int y)
        {
            return (x + y * ImageWidth) * NBytesPerPixel;
        }

        public SKColor PixelColor(int x, int y)
        {
            int offset = PixelByteOffset(x, y);

            // BGRA8 pixel format
            byte blueValue = ImagePixelData[offset];
            byte greenValue = ImagePixelData[offset + 1];
            byte redValue = ImagePixelData[offset + 2];

            return new SKColor(redValue, greenValue, blueValue);
        }*/
    }
}
