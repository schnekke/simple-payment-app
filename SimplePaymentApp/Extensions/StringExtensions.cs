using System;
using System.IO;

namespace SimplePaymentApp
{
    public static class StringExtensions
    {
        public static Stream GenerateStream(this String source)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(source);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}