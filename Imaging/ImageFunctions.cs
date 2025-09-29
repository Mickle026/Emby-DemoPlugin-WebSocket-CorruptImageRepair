using MediaBrowser.Model.Drawing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Emby.CorruptImageRepair
{
    public class ImageDetection
    {
        public static bool ValidImage(string Path)
        {
            // added fileinfo to check bytes size is greater than 500 bytes
            // otherwise just accept this might be a failed ffmpeg extraction, that is only a header 
            if (File.Exists(Path))
            {
                FileInfo info = new FileInfo(Path);
                long length = info.Length;

                FileStream stream = new FileStream(Path, FileMode.Open, FileAccess.Read);
                byte[] block = new byte[4];
                while (stream.Read(block, 0, 4) > 0)
                {
                    var img = GetImageFormat(block);

                    switch (img)
                    {
                        case ImageFormat.Bmp:
                            return true; //file exists in cache and is a valid image

                        case ImageFormat.Gif:
                            // check the file is not just a valid header from a failed extraction
                            if (length > 930) // allow 9 padding bytes
                            {
                                return true;  //file exists in cache and is a valid image
                            }
                            else { return false; } //file doesnt exist in cache or is invalid

                        case ImageFormat.Png:
                            return true; //file exists in cache and is a valid image

                        case ImageFormat.Jpg:
                            // check the file is not just a valid header from a failed extraction
                            if (length > 214)// allow 9 padding bytes 
                            {
                                return true;//file exists in cache and is a valid image
                            }
                            else { return false; } //file doesnt exist in cache or is invalid
                        case ImageFormat.Webp:
                            if (length > 930) // allow 9 padding bytes
                            {
                                return true;  //file exists in cache and is a valid image
                            }
                            else { return false; } //file doesnt exist in cache or is invalid

                        default:
                            return false; //file doesnt exist in cache
                    }
                }
            }
            return false;
        }
        public static ImageFormat GetImageFormat(byte[] bytes)
        {
            var bmp = Encoding.ASCII.GetBytes("BM");     // BMP
            var gif = Encoding.ASCII.GetBytes("GIF");    // GIF
            var png = new byte[] { 137, 80, 78, 71 };    // PNG
            var png1 = new byte[] { 80, 75, 3, 4 };
            var tiff = new byte[] { 73, 73, 42 };         // TIFF
            var tiff2 = new byte[] { 77, 77, 42 };         // TIFF
            var jpeg = new byte[] { 255, 216, 255, 224 }; // jpeg
            var jpeg2 = new byte[] { 255, 216, 255, 225 }; // jpeg canon
            var jpeg3 = new byte[] { 255, 216, 255, 237 };
            var webp = Encoding.ASCII.GetBytes("RIFF"); // webp

            if (bmp.SequenceEqual(bytes.Take(bmp.Length)))
                return ImageFormat.Bmp;
            if (gif.SequenceEqual(bytes.Take(gif.Length)))
                return ImageFormat.Gif;
            if (png.SequenceEqual(bytes.Take(png.Length)))
                return ImageFormat.Png;
            if (png1.SequenceEqual(bytes.Take(png1.Length)))
                return ImageFormat.Png;
            if (jpeg.SequenceEqual(bytes.Take(jpeg.Length)))
                return ImageFormat.Jpg;
            if (jpeg2.SequenceEqual(bytes.Take(jpeg2.Length)))
                return ImageFormat.Jpg;
            if (jpeg3.SequenceEqual(bytes.Take(jpeg3.Length)))
                return ImageFormat.Jpg;
            if (webp.SequenceEqual(bytes.Take(webp.Length)))
                return ImageFormat.Webp;

            return 0;
        }

        private static bool GetImageExtension(string fileName)
        {
            string extension = Path.GetExtension(fileName);
            if (string.IsNullOrEmpty(extension))
                throw new ArgumentException(
                    string.Format("Unable to determine file extension for fileName: {0}", fileName));

            switch (extension.ToLower())
            {
                case @".bmp":
                    return true;

                case @".gif":
                    return true;

                case @".jpg":
                case @".jpeg":
                    return true;

                case @".png":
                    return true;

                case @".avif":
                      return true;

                case @".webp":
                    return true;

                default:
                    return false;
            }
        }

    }
}
