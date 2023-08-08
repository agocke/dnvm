﻿
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Semver;
using Spectre.Console;
using Zio.FileSystems;

namespace Dnvm;

public static class Program
{
    public static SemVersion SemVer = SemVersion.Parse(GitVersionInformation.MajorMinorPatch, SemVersionStyles.Strict);

    internal static readonly HttpClient HttpClient = new();

    static async Task<int> Main(string[] args)
    {
        Console.WriteLine("dnvm " + SemVer + " " + GitVersionInformation.Sha);
        Console.WriteLine();
        var options = CommandLineArguments.Parse(args);
        var logger = new Logger(AnsiConsole.Console);
        var globalOptions = GetGlobalConfig();
        Directory.CreateDirectory(globalOptions.DnvmHome);
        using var env = DnvmEnv.CreateDefault(globalOptions.DnvmHome);

        return options.Command switch
        {
            CommandArguments.InstallArguments o => (int)await InstallCommand.Run(env, globalOptions, logger, o),
            CommandArguments.UpdateArguments o => (int)await UpdateCommand.Run(env, globalOptions, logger, o),
            CommandArguments.ListArguments => (int)await ListCommand.Run(logger, env),
            CommandArguments.SelectArguments o => (int)await SelectCommand.Run(env, globalOptions, logger, o),
            CommandArguments.SelfInstallArguments o => (int)await SelfInstallCommand.Run(env, globalOptions, logger, o),
            _ => throw ExceptionUtilities.Unreachable
        };
    }

    /// <summary>
    /// Get the path to DNVM_HOME, which is the location of the dnvm manifest
    /// and the installed SDKs. If the environment variable is not set, uses
    /// <see cref="DefaultConfig.DnvmHome" /> as the default.
    /// </summar>
    private static GlobalOptions GetGlobalConfig()
    {
        var config = GlobalOptions.Default;

        var home = Environment.GetEnvironmentVariable("DNVM_HOME");
        if (!string.IsNullOrWhiteSpace(home))
        {
            return config with {
                DnvmHome = home
            };
        }
        return config;
    }
}