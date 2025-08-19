using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Semver;

namespace Dnvm;

public sealed class PruneCommand
{
    public sealed record Options
    {
        public bool Verbose { get; init; } = false;
        public bool DryRun { get; init; } = false;
    }

    public static Task<int> Run(DnvmEnv env, Logger logger, DnvmSubCommand.PruneArgs args)
    {
        return Run(env, logger, new Options
        {
            Verbose = args.Verbose ?? false,
            DryRun = args.DryRun ?? false
        });
    }

    public static async Task<int> Run(DnvmEnv env, Logger logger, Options options)
    {
        using var @lock = await ManifestLock.Acquire(env);
        Manifest manifest;
        try
        {
            manifest = await @lock.ReadManifest(env);
        }
        catch (Exception e)
        {
            Environment.FailFast("Error reading manifest: ", e);
            // unreachable
            return 1;
        }

        var sdksToRemove = GetOutOfDateSdks(manifest);
        foreach (var sdk in sdksToRemove)
        {
            if (options.DryRun)
            {
                Console.WriteLine($"Would remove {sdk}");
            }
            else
            {
                Console.WriteLine($"Removing {sdk}");
                int result = await UninstallCommand.Run(@lock, env, logger, sdk.Version, sdk.Dir);
                if (result != 0)
                {
                    return result;
                }
            }
        }
        return 0;
    }

    public static List<(SemVersion Version, SdkDirName Dir)> GetOutOfDateSdks(Manifest manifest)
    {
        var sdksToRemove = new List<(SemVersion, SdkDirName)>();
        foreach (var sdk in manifest.InstalledSdks)
        {
            // Find all SDKs that could be replaced by this one
            foreach (var other in manifest.InstalledSdks)
            {
                if (sdk == other)
                {
                    continue;
                }

                if (VersionUtils.IsUpdate(other.SdkVersion, sdk.SdkVersion, other.RollForward))
                {
                    sdksToRemove.Add((other.SdkVersion, other.SdkDirName));
                }
            }
        }
        return sdksToRemove;
    }
}