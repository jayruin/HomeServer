using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeServer.Utility
{
    public static class Base64
    {
        public static string Base64Encode(string plainText)
        {
            return System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(plainText));
        }

        public static string Base64Decode(string base64String)
        {
            return System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(base64String));
        }
    }
}
