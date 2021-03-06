﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FrogFoot.Utilities
{
    public static class StringTruncator
    {
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }
}