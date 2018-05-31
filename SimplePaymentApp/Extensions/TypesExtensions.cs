using System;
using System.IO;
using System.Text;
using System.Web;

namespace SimplePaymentApp
{
    public static class StringExtensions
    {
        private static readonly DateTime unixEpoch = 
            new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).ToLocalTime();

        public static int ToUnixTimestamp(this DateTime dateTime)
        {
            return (int)(dateTime - unixEpoch).TotalSeconds;
        }

        public static int ToUnixTimestampMiliseconds(this DateTime dateTime)
        {
            return (int)(dateTime - unixEpoch).TotalMilliseconds;
        }

        public static DateTime ParseAsUnixTimestamp(this int timestamp)
        {
            return unixEpoch.AddSeconds(timestamp);
        }

        public static DateTime ParseAsUnixTimestamp(this long timestamp)
        {
            return unixEpoch.AddSeconds(timestamp);
        }

        public static Stream GenerateStream(this String source)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(source);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static string EncodeObject(this object data)
        {
            var isAnonymous = data.GetType().Name.Contains("AnonymousType");
            var props = data.GetType().GetProperties();
            var sb = new StringBuilder();
            foreach (var prop in props)
            {
                object value = prop.GetValue(data, null);
                if (value != null)
                {
                    sb.AddKeyValuePair(prop.Name.ToLower(), value, isAnonymous);
                }
            }

            return sb.ToString();
        }

        public static void AddKeyValuePair(
            this StringBuilder sb, string key, object value, bool isAnonymousType)
        {
            if (null == value)
                return;

            string reply = String.Empty;
            var charset = Encoding.UTF8;

            try
            {
                key = HttpUtility.UrlEncode(key.ToLower(), charset);

                if (value.GetType().IsEnum)
                {
                    reply = value.ToString().ToLower();
                }
                else if (value.GetType().Equals(typeof(DateTime)))
                {
                    if (value.Equals(DateTime.MinValue))
                    {
                        reply = String.Empty;
                    }
                    else if (isAnonymousType)
                    {
                        reply = ((DateTime)value).ToUnixTimestamp().ToString();
                    }
                    else
                    {
                        reply = ((DateTime)value).ToString("y");
                    }
                }
                else
                {
                    reply = HttpUtility.UrlEncode(value.ToString(), charset);
                }

                if (!isAnonymousType || !String.IsNullOrEmpty(reply))
                {
                    if (sb.Length > 0)
                        sb.Append("&");

                    sb.Append($"{key}={reply}");
                }

            }
            catch
            {
                throw new Exception($"Unsupported or invalid character set encoding '{charset}'.");
            }
        }
    }
}