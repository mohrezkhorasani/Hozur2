using System;
using System.Collections.Generic;
using System.Text;

namespace Application.General
{

    public static class IdGenerator
    {
        private static readonly Random _random = new Random();
        private static readonly string[] _typeMap = { "guest", "userBBB", "userVc2" };
        private static readonly string _chars = "0123456789abcdef";

        /// <summary>
        /// Generates a random ID in format: {type}-{random hex string}
        /// </summary>
        /// <param name="type">0 = guest, 1 = userBBB, 2 = userVc2</param>
        /// <returns>Generated ID string</returns>
        public static string GenerateId(int type)
        {
            if (type < 0 || type >= _typeMap.Length)
                throw new ArgumentOutOfRangeException(nameof(type), $"Type must be between 0 and {_typeMap.Length - 1}");

            string typeString = _typeMap[type];
            string randomPart = GenerateRandomHex(24); // 24 characters = 12 bytes
            return $"{typeString}-{randomPart}";
        }

        private static string GenerateRandomHex(int length)
        {
            var result = new StringBuilder(length);
            lock (_random) // For thread safety
            {
                for (int i = 0; i < length; i++)
                {
                    result.Append(_chars[_random.Next(_chars.Length)]);
                }
            }
            return result.ToString();
        }
    }
}
