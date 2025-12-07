using System;
using System.Security.Cryptography;
using System.Text;

namespace Application.General
{

    public static class FriendlyIDGenerator
    {
        private static readonly char[] _alphabet = "abcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();

        /// <summary>
        /// تولید کلید تصادفی با قالب پیش‌فرض 4 گروه هر گروه 3 کاراکتر: xxx-xxx-xxx-xxx
        /// </summary>
        public static string Generate(int groups = 4, int groupLength = 3)
        {
            if (groups <= 0) throw new ArgumentOutOfRangeException(nameof(groups));
            if (groupLength <= 0) throw new ArgumentOutOfRangeException(nameof(groupLength));

            var sb = new StringBuilder(groups * (groupLength + 1) - 1);

            for (int g = 0; g < groups; g++)
            {
                for (int i = 0; i < groupLength; i++)
                {
                    int idx = RandomNumberGenerator.GetInt32(_alphabet.Length);
                    sb.Append(_alphabet[idx]);
                }

                if (g < groups - 1)
                    sb.Append('-');
            }

            return sb.ToString();
        }
    }

}
