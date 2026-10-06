using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using Sinapse.LGPD.Models;

namespace Sinapse.LGPD.Services
{
    public class LgpdWatermarkService
    {
        public byte[] ApplySecurityWatermark(byte[] imageData, WatermarkRequest request)
        {
            if (imageData == null || imageData.Length == 0) return null;

            try
            {
                using (var msInput = new MemoryStream(imageData))
                using (var sourceImage = Image.FromStream(msInput))
                {
                    var bmp = new Bitmap(sourceImage.Width, sourceImage.Height);

                    using (var graphics = Graphics.FromImage(bmp))
                    {
                        graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                        // 1. Desenha a imagem base
                        graphics.DrawImage(sourceImage, 0, 0, sourceImage.Width, sourceImage.Height);

                        // ==========================================
                        // 2. QUADRICULADO DE FUNDO MAIS APARENTE
                        // ==========================================
                        // Alpha subiu para 65 e mudamos para DarkGray para marcar bem em qualquer fundo claro
                        using (var gridPen = new Pen(Color.FromArgb(65, Color.DarkGray), 1f))
                        {
                            gridPen.DashStyle = DashStyle.DashDot;

                            // Quadriculado mais fechado (mais linhas na tela)
                            float gridSpacing = sourceImage.Width / 24f;

                            for (float x = 0; x < sourceImage.Width; x += gridSpacing)
                            {
                                graphics.DrawLine(gridPen, x, 0, x, sourceImage.Height);
                            }

                            for (float y = 0; y < sourceImage.Height; y += gridSpacing)
                            {
                                graphics.DrawLine(gridPen, 0, y, sourceImage.Width, y);
                            }
                        }

                        // ==========================================
                        // 3. TEXTOS DIAGONAIS ULTRA COMPACTOS
                        // ==========================================
                        string textToDraw = request.FullWatermarkText;

                        // FONTE BEM MENOR: Divisor alterado para 75f para miniaturizar o texto
                        float fontSize = sourceImage.Width / 75f;
                        if (fontSize < 7) fontSize = 7;

                        using (var fillBrush = new SolidBrush(Color.FromArgb(76, Color.White))) // 30% opacidade
                        using (var outlinePen = new Pen(Color.FromArgb(50, Color.DimGray), fontSize * 0.12f))
                        {
                            outlinePen.LineJoin = LineJoin.Round;

                            using (var measureFont = new Font("Arial", fontSize, FontStyle.Bold))
                            {
                                SizeF textSize = graphics.MeasureString(textToDraw, measureFont);

                                // ESPAÇAMENTO MINIMIZADO: Colando as linhas verticais e horizontais
                                float horizontalSpacing = textSize.Width + (fontSize * 5f);
                                float verticalSpacing = textSize.Height * 1.8f; // Reduzido drasticamente de 2.2 para 1.3

                                // Margens expandidas para garantir preenchimento total nas bordas rotacionadas
                                float startX = -sourceImage.Width;
                                float endX = sourceImage.Width * 2;
                                float startY = -sourceImage.Height;
                                float endY = sourceImage.Height * 2;

                                var fontFamily = new FontFamily("Arial");
                                int rowCount = 0;

                                for (float y = startY; y < endY; y += verticalSpacing)
                                {
                                    // Intercalação contínua estilo tijolo (quebra as colunas)
                                    float shiftX = (rowCount % 2 == 0) ? 0f : (horizontalSpacing / 2f);

                                    for (float x = startX + shiftX; x < endX; x += horizontalSpacing)
                                    {
                                        var state = graphics.Save();

                                        graphics.TranslateTransform(x, y);
                                        graphics.RotateTransform(-45);

                                        using (var path = new GraphicsPath())
                                        {
                                            path.AddString(
                                                textToDraw,
                                                fontFamily,
                                                (int)FontStyle.Bold,
                                                graphics.DpiY * fontSize / 72f,
                                                new PointF(0, 0),
                                                StringFormat.GenericDefault
                                            );

                                            graphics.DrawPath(outlinePen, path);
                                            graphics.FillPath(fillBrush, path);
                                        }

                                        graphics.Restore(state);
                                    }
                                    rowCount++;
                                }
                            }
                        }
                    }

                    using (var msOutput = new MemoryStream())
                    {
                        bmp.Save(msOutput, ImageFormat.Png);
                        return msOutput.ToArray();
                    }
                }
            }
            catch (Exception)
            {
                return imageData;
            }
        }
    }
}