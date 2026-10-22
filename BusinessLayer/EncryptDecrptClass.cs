using System;
using System.Text;

namespace AkumsPrintingApplication
{
    public static class EncryptDecrptClass
    {
        public static string Encrypt_data(string str)
        {
            char[] arr = str.ToCharArray();
            Array.Reverse(arr);
            str = new string(arr);
            return Convert.ToBase64String(Encoding.Unicode.GetBytes(str));
        }
        /// <summary>
        /// its used for decrypt the given string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Decrypt_data(string str)
        {
            char[] arr = Encoding.Unicode.GetString(Convert.FromBase64String(str)).ToCharArray();
            Array.Reverse(arr);
            return new string(arr);
        }
    }
}
