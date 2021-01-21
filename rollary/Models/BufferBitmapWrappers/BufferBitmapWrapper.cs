using SkiaSharp;

namespace rollary
{
    public class BufferBitmapWrapper
    {
        public SKBitmap Bitmap { get; protected set; }

        public bool BitmapIsCreated => Bitmap != null;

        public int Width => Bitmap.Width;
        public int Height => Bitmap.Height;
    }
}
