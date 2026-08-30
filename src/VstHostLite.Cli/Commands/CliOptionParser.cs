using System;
using System.Collections.Generic;

namespace VstHostLite.Cli.Commands
{
    internal static class CliOptionParser
    {
        public static bool TryParse(string[] args, int minPositional, HashSet<string> knownOptions, out List<string> positionalArguments, out Dictionary<string, string> options, out string error)
        {
            positionalArguments = new List<string>();
            options = new Dictionary<string, string>(StringComparer.Ordinal);
            error = string.Empty;

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                if (arg.StartsWith("--", StringComparison.Ordinal))
                {
                    if (!knownOptions.Contains(arg))
                    {
                        error = $"unknown option: {arg}";
                        return false;
                    }

                    if (i + 1 >= args.Length)
                    {
                        error = $"missing value for option: {arg}";
                        return false;
                    }

                    string value = args[i + 1];
                    options[arg] = value;
                    i++; // skip the value
                }
                else
                {
                    positionalArguments.Add(arg);
                }
            }

            if (positionalArguments.Count < minPositional)
            {
                error = $"expected at least {minPositional} argument(s), got {positionalArguments.Count}";
                return false;
            }

            return true;
        }
    }
}