using SkiaSharp;
using System;

namespace rollary
{
    class PreviewPainter
    {
        private static readonly SKColor ScanEdgeMarkColor = new SKColor(0xCC, 0xFF, 0xAA, 0xCF);
        private const int ScanEdgeMarkWidth = 4;
        private const byte ResultPreviewAlphaWhenPaused = 0xCF;

        private readonly ResultStateManager stateManager;

        public PreviewPainter(ResultStateManager stateManager)
        {
            this.stateManager = stateManager;
        }

        public void PaintPreview(SKCanvas canvas, SKImageInfo info, bool captureControlActive)
        {
            if (!stateManager.CaptureRenderer.BitmapIsCreated) return;

            canvas.Clear();

            var captureRenderer = stateManager.CaptureRenderer;
            var resultRenderer = stateManager.ResultRenderer;

            bool shouldDrawCapture = captureControlActive && !stateManager.Done;
            bool shouldDrawResult = stateManager.Started && resultRenderer.BitmapIsCreated;
            bool shouldDrawScanEdgeMark = shouldDrawCapture && shouldDrawResult;

            var (x, y, scale) = AspectFitTransformValues(info, captureRenderer.Bitmap);
            var aspectFitRect = new SKRect(x, y, x + scale * captureRenderer.Width, y + scale * captureRenderer.Height);

            if (shouldDrawCapture)
            {
                canvas.DrawBitmap(captureRenderer.Bitmap, aspectFitRect);
            }

            if (shouldDrawResult)
            {
                if (stateManager.Running || !shouldDrawCapture)
                {
                    canvas.DrawBitmap(resultRenderer.Bitmap, aspectFitRect);
                }
                else
                {
                    // Draws the result with translucency
                    var alphaPaint = new SKPaint()
                    {
                        Color = SKColor.Empty.WithAlpha(ResultPreviewAlphaWhenPaused),
                    };

                    canvas.DrawBitmap(resultRenderer.Bitmap, aspectFitRect, alphaPaint);
                }
            }

            if (shouldDrawScanEdgeMark)
            {
                canvas.Save();

                canvas.Translate(x, y);
                canvas.Scale(scale);

                PaintScanEdgeMark(canvas, resultRenderer.Bitmap, new SKPaint()
                {
                    Style = SKPaintStyle.Stroke,
                    Color = ScanEdgeMarkColor,
                    StrokeWidth = ScanEdgeMarkWidth / scale,
                });

                canvas.Restore();
            }
        }

        /// <summary>
        /// Computes the transform values needed to draw an aspect-fit bitmap onto the preview canvas.
        /// </summary>
        /// <param name="canvasInfo"></param>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        private TranslateScaleValues AspectFitTransformValues(SKImageInfo canvasInfo, SKBitmap bitmap)
        {
            float scale = Math.Min((float)canvasInfo.Width / bitmap.Width, (float)canvasInfo.Height / bitmap.Height);

            return new TranslateScaleValues()
            {
                X = (canvasInfo.Width - scale * bitmap.Width) / 2,
                Y = (canvasInfo.Height - scale * bitmap.Height) / 2,
                Scale = scale,
            };
        }

        private struct TranslateScaleValues
        {
            public float X;
            public float Y;
            public float Scale;

            public void Deconstruct(out float x, out float y, out float scale)
            {
                x = X;
                y = Y;
                scale = Scale;
            }
        }

        private void PaintScanEdgeMark(SKCanvas canvas, SKBitmap bitmap, SKPaint paint)
        {
            var path = new SKPath();
            float x = (stateManager.NFrame + 1) * stateManager.Settings.FrameScanWidth;
            path.MoveTo(x, 0);
            path.LineTo(x, bitmap.Height);

            canvas.DrawPath(path, paint);
        }
    }
}
