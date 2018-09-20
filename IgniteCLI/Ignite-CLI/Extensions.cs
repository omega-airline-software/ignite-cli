using System;
using System.Collections.Generic;
using System.Text;
namespace IgniteCLI
{
    public static class StringExtensions
    {
        public static T ToEnum<T>(this string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        /// <summary>
        /// Levenshtein Distance https://www.dotnetperls.com/levenshtein
        /// </summary>
        /// <returns></returns>
        public static int DistanceFrom(this string str, string other)
        {
            int n = str.Length;
            int m = other.Length;
            int[,] d = new int[n + 1, m + 1];
            
            if (n == 0)
            {
                return m;
            }

            if (m == 0)
            {
                return n;
            }

            for (int i = 0; i <= n; d[i, 0] = i++) { }

            for (int j = 0; j <= m; d[0, j] = j++) { }
            
            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (other[j - 1] == str[i - 1]) ? 0 : 1;
                    
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }

            return d[n, m];
        }
    }
}