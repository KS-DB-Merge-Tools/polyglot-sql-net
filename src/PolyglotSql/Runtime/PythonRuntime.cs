using System;
using System.Collections.Generic;
using System.Linq;

namespace PolyglotSql.Runtime
{
    /// <summary>
    /// Runtime helpers for Python-compatible operations.
    /// </summary>
    public static class PythonRuntime
    {
        /// <summary>
        /// Python-like string slicing.
        /// </summary>
        public static string Slice(string text, int start, int? end = null)
        {
            if (text == null) return "";
            
            int actualStart = start < 0 ? Math.Max(0, text.Length + start) : start;
            int actualEnd = end ?? text.Length;
            actualEnd = actualEnd < 0 ? Math.Max(0, text.Length + actualEnd) : actualEnd;
            
            if (actualStart >= text.Length) return "";
            if (actualEnd <= actualStart) return "";
            
            return text.Substring(actualStart, Math.Min(actualEnd, text.Length) - actualStart);
        }
        
        /// <summary>
        /// Python-like list slicing.
        /// </summary>
        public static List<T> Slice<T>(List<T> list, int start, int? end = null)
        {
            if (list == null) return new List<T>();
            
            int actualStart = start < 0 ? Math.Max(0, list.Count + start) : start;
            int actualEnd = end ?? list.Count;
            actualEnd = actualEnd < 0 ? Math.Max(0, list.Count + actualEnd) : actualEnd;
            
            if (actualStart >= list.Count) return new List<T>();
            if (actualEnd <= actualStart) return new List<T>();
            
            return list.GetRange(actualStart, Math.Min(actualEnd, list.Count) - actualStart);
        }
        
        /// <summary>
        /// Python-like dictionary get with default.
        /// </summary>
        public static TValue Get<TKey, TValue>(Dictionary<TKey, TValue> dict, TKey key, TValue defaultValue = default)
        {
            if (dict == null) return defaultValue;
            return dict.TryGetValue(key, out var value) ? value : defaultValue;
        }
        
        /// <summary>
        /// Python-like containment check for tuple literals.
        /// </summary>
        public static bool Contains<T>(T value, params T[] items) => items.Contains(value);
        
        /// <summary>
        /// Check if all characters in a string are digits.
        /// </summary>
        public static bool AllDigits(string s) => !string.IsNullOrEmpty(s) && s.All(char.IsDigit);
        
        /// <summary>
        /// Python-like truthiness check for strings (truthy if not empty).
        /// </summary>
        public static bool IsTruthy(string value) => !string.IsNullOrEmpty(value);
        
        /// <summary>
        /// Python-like truthiness check for lists (truthy if not empty).
        /// </summary>
        public static bool IsTruthy<T>(List<T> value) => value != null && value.Count > 0;
        
        /// <summary>
        /// Python-like truthiness check for size (size is truthy if > 0).
        /// </summary>
        public static bool IsTruthy(int value) => value > 0;
        
        /// <summary>
        /// Python-like string find (returns -1 if not found).
        /// </summary>
        public static int Find(string text, string value, int start = 0)
        {
            if (text == null || value == null) return -1;
            int index = text.IndexOf(value, start, StringComparison.Ordinal);
            return index;
        }
        
        /// <summary>
        /// Python-like string rfind (returns -1 if not found).
        /// </summary>
        public static int RFind(string text, string value, int? start = null)
        {
            if (text == null || value == null) return -1;
            int startIndex = start ?? text.Length - 1;
            int index = text.LastIndexOf(value, startIndex, StringComparison.Ordinal);
            return index;
        }
        
        /// <summary>
        /// Python-like str.join().
        /// </summary>
        public static string Join(string separator, IEnumerable<object> items)
        {
            if (items == null) return "";
            return string.Join(separator, items);
        }
        
        /// <summary>
        /// Python-like str.startswith().
        /// </summary>
        public static bool StartsWith(string text, string prefix)
        {
            if (text == null || prefix == null) return false;
            return text.StartsWith(prefix, StringComparison.Ordinal);
        }
        
        /// <summary>
        /// Python-like str.endswith().
        /// </summary>
        public static bool EndsWith(string text, string suffix)
        {
            if (text == null || suffix == null) return false;
            return text.EndsWith(suffix, StringComparison.Ordinal);
        }
        
        /// <summary>
        /// Python-like str.count().
        /// </summary>
        public static int Count(string text, string substring)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(substring)) return 0;
            int count = 0;
            int index = 0;
            while ((index = text.IndexOf(substring, index, StringComparison.Ordinal)) != -1)
            {
                count++;
                index += substring.Length;
            }
            return count;
        }
        
        /// <summary>
        /// Python-like str.rstrip().
        /// </summary>
        public static string RStrip(string text, string chars = null)
        {
            if (text == null) return "";
            if (chars == null)
                return text.TrimEnd();
            return text.TrimEnd(chars.ToCharArray());
        }
        
        /// <summary>
        /// Check if character is a letter or digit (alphanumeric).
        /// </summary>
        public static bool IsAlnum(char c)
        {
            return char.IsLetterOrDigit(c);
        }
        
        /// <summary>
        /// Check if character is a letter.
        /// </summary>
        public static bool IsAlpha(char c)
        {
            return char.IsLetter(c);
        }
        
        /// <summary>
        /// Check if string is identifier (starts with letter/underscore, rest is alphanumeric).
        /// </summary>
        public static bool IsIdentifier(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            if (!char.IsLetter(text[0]) && text[0] != '_') return false;
            for (int i = 1; i < text.Length; i++)
            {
                if (!char.IsLetterOrDigit(text[i]) && text[i] != '_')
                    return false;
            }
            return true;
        }
        
        /// <summary>
        /// Python-like any() for collections.
        /// </summary>
        public static bool Any<T>(IEnumerable<T> collection, Func<T, bool> predicate)
        {
            if (collection == null) return false;
            foreach (var item in collection)
            {
                if (predicate(item)) return true;
            }
            return false;
        }
        
        /// <summary>
        /// Python-like all() for collections.
        /// </summary>
        public static bool All<T>(IEnumerable<T> collection, Func<T, bool> predicate)
        {
            if (collection == null) return true;
            foreach (var item in collection)
            {
                if (!predicate(item)) return false;
            }
            return true;
        }
        
        /// <summary>
        /// Python-like len().
        /// </summary>
        public static int Length(object obj)
        {
            if (obj == null) return 0;
            if (obj is string s) return s.Length;
            if (obj is System.Collections.ICollection col) return col.Count;
            return 0;
        }
    }
}
