using Windows.Storage;
using Windows.Storage.Streams;

namespace ServerAppDesktop.Controls;

public static class ImageSourceExtensions
{
    public static async Task<Icon> ToIconAsync(this ImageSource imageSource, bool usingWindowsDPI = true)
    {
        if (imageSource is not BitmapImage bitmapImage || bitmapImage.UriSource == null)
            throw new ArgumentException(ResourceHelper.GetString("Err_ImageSourceEmpty"));

        byte[] rawBytes = await GetImageBytesAsync(bitmapImage.UriSource);

        int targetWidth = PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CXSMICON);
        int targetHeight = PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CYSMICON);

        if (!usingWindowsDPI)
        {
            using var msDimensions = new MemoryStream(rawBytes);
            using var img = System.Drawing.Image.FromStream(msDimensions);
            targetWidth = img.Width;
            targetHeight = img.Height;
        }

        using var msInput = new MemoryStream(rawBytes);
        using var originalImg = System.Drawing.Image.FromStream(msInput);

        using var resizedImg = new System.Drawing.Bitmap(targetWidth, targetHeight);
        using (var g = System.Drawing.Graphics.FromImage(resizedImg))
        {
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(originalImg, 0, 0, targetWidth, targetHeight);
        }

        using var msPng = new MemoryStream();
        resizedImg.Save(msPng, System.Drawing.Imaging.ImageFormat.Png);
        byte[] scaledPngBytes = msPng.ToArray();

        byte[] finalIcoBytes = PngToIcoConverter.ConvertPngToIco(scaledPngBytes, targetWidth, targetHeight);

        using var msOutput = new MemoryStream(finalIcoBytes);
        return new Icon(msOutput);
    }

    public static async Task<byte[]> GetImageBytesAsync(Uri uri)
    {
        return uri.Scheme switch
        {
            "ms-appx" or "ms-appx-web" => await GetMsAppxBytes(uri),
            "http" or "https" => await new HttpClient().GetByteArrayAsync(uri),
            "file" or _ when Path.IsPathRooted(uri.LocalPath) || uri.IsFile => await File.ReadAllBytesAsync(uri.IsFile ? uri.LocalPath : uri.ToString()),
            _ => throw new NotSupportedException(
                    string.Format(ResourceHelper.GetString("Err_UriSchemeNotSupported"), uri.Scheme)
                 )
        };
    }

    private static async Task<byte[]> GetMsAppxBytes(Uri uri)
    {
        try
        {
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(uri);
            using IRandomAccessStreamWithContentType stream = await file.OpenReadAsync();
            byte[] bytes = new byte[stream.Size];
            using var reader = new DataReader(stream);
            await reader.LoadAsync((uint)stream.Size);
            reader.ReadBytes(bytes);
            return bytes;
        }
        catch
        {
            string path = Path.Combine(AppContext.BaseDirectory, uri.LocalPath.TrimStart('/'));
            return await File.ReadAllBytesAsync(path);
        }
    }
}
