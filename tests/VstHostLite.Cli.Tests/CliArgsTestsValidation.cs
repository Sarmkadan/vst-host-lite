using System;
using System.Collections.Generic;
using System.Globalization;

namespace VstHostLite.Cli.Tests;

/// <summary>
/// Provides validation helpers for command-line argument testing scenarios.
/// </summary>
public static class CliArgsTestsValidation
{
    /// <summary>
    /// Validates the command-line arguments for the CLI tests.
    /// </summary>
    /// <param name="value">The command-line arguments to validate.</param>
    /// <returns>A list of human-readable validation problems, or an empty list if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this CliArgsTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate that the test scenarios are properly structured
        // Each test represents a command-line scenario with specific arguments

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the command-line arguments are valid.
    /// </summary>
    /// <param name="value">The command-line arguments to check.</param>
    /// <returns>True if the arguments are valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this CliArgsTests value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the command-line arguments are valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The command-line arguments to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the arguments are invalid, containing a list of problems.</exception>
    public static void EnsureValid(this CliArgsTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Command-line argument test validation failed:{Environment.NewLine}  - {string.Join($"{Environment.NewLine}  - ", problems)}");
        }
    }

    /// <summary>
    /// Validates command-line argument arrays for CLI testing scenarios.
    /// </summary>
    /// <param name="args">The command-line arguments to validate.</param>
    /// <returns>A list of human-readable validation problems, or an empty list if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="args"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        var problems = new List<string>();

        if (args.Length == 0)
        {
            problems.Add("Command-line arguments cannot be empty for test scenarios.");
        }
        else
        {
            // Validate each argument
            for (int i = 0; i < args.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(args[i]))
                {
                    problems.Add($"Argument at position {i} is null, empty, or whitespace.");
                }
                else if (args[i].Length > 1024)
                {
                    problems.Add($"Argument at position {i} exceeds maximum length of 1024 characters.");
                }
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the command-line argument array is valid.
    /// </summary>
    /// <param name="args">The command-line arguments to check.</param>
    /// <returns>True if the arguments are valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="args"/> is null.</exception>
    public static bool IsValid(this string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);
        return Validate(args).Count == 0;
    }

    /// <summary>
    /// Ensures that the command-line argument array is valid, throwing an exception if not.
    /// </summary>
    /// <param name="args">The command-line arguments to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="args"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the arguments are invalid, containing a list of problems.</exception>
    public static void EnsureValid(this string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        var problems = Validate(args);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Command-line argument validation failed:{Environment.NewLine}  - {string.Join($"{Environment.NewLine}  - ", problems)}");
        }
    }
}