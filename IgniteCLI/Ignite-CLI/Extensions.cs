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
    }
}