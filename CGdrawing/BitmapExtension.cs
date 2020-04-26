using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
namespace CGdrawing
{
        /*Method implementing pixel operations from last project in form of the class extension*/
        public static class BitmapExtension
        {
        // set pixel operation on pointers
        public static void SetPixelUnsafe(this Bitmap bmp, int x, int y, Color color)
            {
                var newValues = new byte[] { color.B, color.G, color.R, 255 };

                BitmapData data = bmp.LockBits(
                    new Rectangle(0, 0, bmp.Width, bmp.Height),
                    ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb
                    );

                if (
                    data.Stride * y + 4 * x < data.Stride * data.Height
                    && data.Stride * y + 4 * x >= 0
                    && x * 4 < data.Stride
                    && y < data.Height
                    )
                    unsafe
                    {
                        byte* ptr = (byte*)data.Scan0;

                        for (int i = 0; i < 4; i++)
                            ptr[data.Stride * y + 4 * x + i] = newValues[i];
                    }

                    bmp.UnlockBits(data);
             }
        // faster get pixel operation on pointers
        public static Color GetPixelUnsafe(this Bitmap bmp, int x, int y)
        {
            BitmapData data = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb
                );

            Color col = Color.FromArgb(0, 0, 0, 0); ;

            if (data.Stride * y + 4 * x < data.Stride * data.Height && data.Stride * y + 4 * x >= 0)
                unsafe
                {
                    byte* ptr = (byte*)data.Scan0;

                    col = Color.FromArgb(
                        ptr[data.Stride * y + 4 * x + 3],
                        ptr[data.Stride * y + 4 * x + 2],
                        ptr[data.Stride * y + 4 * x + 1],
                        ptr[data.Stride * y + 4 * x + 0]
                    );
                }

            bmp.UnlockBits(data);

            return col;
        }


    }
    }

