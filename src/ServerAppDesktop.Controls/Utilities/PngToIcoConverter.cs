namespace ServerAppDesktop.Controls;

public static class PngToIcoConverter
{
    public static byte[] ConvertPngToIco(this byte[] data, int width, int height)
    {
        if (data == null || data.Length == 0)
            throw new ArgumentException("Data cannot be null or empty.");

        using var inStream = new MemoryStream(data);
        using var image = System.Drawing.Image.FromStream(inStream);

        int bpp = System.Drawing.Image.GetPixelFormatSize(image.PixelFormat);

        using var outStream = new MemoryStream();
        using var writer = new BinaryWriter(outStream);

        writer.Write((short)0);
        writer.Write((short)1);
        writer.Write((short)1);

        writer.Write((byte)width);
        writer.Write((byte)height);
        writer.Write((byte)0);
        writer.Write((byte)0);
        writer.Write((short)1);
        writer.Write((short)bpp);
        writer.Write(data.Length);
        writer.Write(22);

        writer.Write(data);

        return outStream.ToArray();
    }
}
