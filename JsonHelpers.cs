using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace rsRadio
{
    internal static class JsonHelpers 
    {
        public static IDictionary<string, string> ParseDictionary(string content)
        {
            content = content.Trim().TrimStart('{').TrimEnd('}');

            var result = new Dictionary<string, string>();
            var values = content.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var value in values.Select(x => x.Trim()))
            {
                var indexOfFirstSplitChar = value.IndexOf(':');
                if (indexOfFirstSplitChar == -1)
                {
                    throw new ArgumentOutOfRangeException($"Can't parse json.");
                }

                var key = value.Substring(0, indexOfFirstSplitChar).Trim().Trim('\"').Trim('\'');
                var val = value.Substring(indexOfFirstSplitChar + 1).Trim().Trim('\"').Trim('\'');

                result.Add(key, val);
            }

            return result;
        }

        public static IDictionary<string, string> ParseArray(string content)
        {
            content = content.Trim().TrimStart('[').TrimEnd(']');

            var result = new Dictionary<string, string>();
            var values = content.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var value in values.Select(x => x.Trim()))
            {
                var indexOfFirstSplitChar = value.IndexOf(':');
                if (indexOfFirstSplitChar == -1)
                {
                    throw new ArgumentOutOfRangeException($"Can't parse json.");
                }

                var key = value.Substring(0, indexOfFirstSplitChar).Trim().Trim('\"').Trim('\'');
                var val = value.Substring(indexOfFirstSplitChar + 1).Trim().Trim('\"').Trim('\'');

                result.Add(key, val);
            }

            return result;
        }
    }
}