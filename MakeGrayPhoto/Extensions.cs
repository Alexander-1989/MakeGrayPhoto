using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace MakeGrayPhoto
{
    static class Extension
    {
        private static readonly Random random = new Random();

        public static void MakeGray(this Bitmap bitmap)
        {
            Rectangle rectangle = new Rectangle(new Point(0, 0), bitmap.Size);
            BitmapData bitmapData = bitmap.LockBits(rectangle, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            IntPtr ptr = bitmapData.Scan0;

            int size = Math.Abs(bitmapData.Height * bitmapData.Stride);
            byte[] RGB = new byte[size];

            Marshal.Copy(ptr, RGB, 0, size);
            for (int i = 0; i + 2 < size; i += 3)
            {
                byte averageValue = (byte)((RGB[i] + RGB[i + 1] + RGB[i + 2]) / 3);
                RGB[i + 0] = averageValue;
                RGB[i + 1] = averageValue;
                RGB[i + 2] = averageValue;
            }

            Marshal.Copy(RGB, 0, ptr, size);
            bitmap.UnlockBits(bitmapData);
        }

        public static string GetRandomName(int min, int max, string extension = null)
        {
            string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            int minimum = min < 0 ? 0 : min;
            int maximum = max < min ? min : max;
            int nameLength = random.Next(minimum, maximum);
            int extensionLength = extension?.Length ?? 0;
            char[] result = new char[nameLength + extensionLength];

            for (int i = 0; i < nameLength; i++)
            {
                int index = random.Next(alphabet.Length);
                result[i] = alphabet[index];
            }

            for (int j = 0; j < extensionLength; j++)
            {
                result[nameLength + j] = extension[j];
            }

            return new string(result);
        }
    }
}