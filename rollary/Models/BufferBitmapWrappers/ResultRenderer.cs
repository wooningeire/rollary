using SkiaSharp;

namespace rollary
{
    public class ResultRenderer : BufferBitmapWrapper
    {
        private readonly ResultSettings settings;

        private SKCanvas canvas;

        public ResultRenderer(ResultSettings settings)
        {
            this.settings = settings;
        }

        public void Clear()
        {
            if (!BitmapIsCreated) return;

            canvas.Clear();
        }

        private void CreateBitmapAndCanvas(int width, int height)
        {
            Bitmap = new SKBitmap(width, height);
            canvas = new SKCanvas(Bitmap);
        }

        public void DrawSection(int nFrame, SKBitmap bitmap)
        {
            if (!BitmapIsCreated)
            {
                CreateBitmapAndCanvas(bitmap.Width, bitmap.Height);
            }

            DrawSectionToCanvas(nFrame, bitmap);
        }

        private void DrawSectionToCanvas(int nFrame, SKBitmap bitmap)
        {
            float left = nFrame * settings.FrameScanWidth;
            float right = (nFrame + 1) * settings.FrameScanWidth;
            var destRect = new SKRect(left, 0, right, Height);

            canvas.DrawBitmap(bitmap, destRect, destRect);
        }

    }
}
