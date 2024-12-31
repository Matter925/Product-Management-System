using System.Text;

namespace ProductManagement.API.Helpers;

public class PdfHelper
{
    public static bool ValidatePdf(string path)
    {
        try
        {
            var pdfString = "%PDF-";
            var pdfBytes = Encoding.ASCII.GetBytes(pdfString);
            var len = pdfBytes.Length;
            var buf = new byte[len];
            var remaining = len;
            var pos = 0;

            using (var f = File.OpenRead(path))
            {
                while (remaining > 0)
                {
                    var amtRead = f.Read(buf, pos, remaining);
                    if (amtRead == 0) return false;
                    remaining -= amtRead;
                    pos += amtRead;
                }
            }
            var result = pdfBytes.SequenceEqual(buf);

            return result;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
