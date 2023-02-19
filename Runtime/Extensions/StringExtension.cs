using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

namespace Codesign
{
    public static class StringExtension
    {
        public static string AddCamelCasingSpace(this string _string)
        {
            var correction = Regex.Replace(_string, "(\\B[A-Z])", " $1");
            return correction;
        }
    }
}
