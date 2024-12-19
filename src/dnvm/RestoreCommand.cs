
using System.IO;
using System.Threading.Tasks;
using Serde;
using Serde.Json;
using Spectre.Console;
using Zio;

namespace Dnvm;

public static partial class RestoreCommand
{
    public enum Result
    {
        Success = 0,
        NoGlobalJson = 1,
        IoError = 2,
        NoSdkSection = 3,
        NoVersion = 4,
    }

    [GenerateDeserialize]
    private sealed partial record GlobalJsonSubset
    {
        public SdkSubset? Sdk { get; init; }

        [GenerateDeserialize]
        public sealed partial record SdkSubset
        {
            public string? Version { get; init; }
            public RollForwardOptions? RollForward { get; init; }

            [GenerateDeserialize]
            public enum RollForwardOptions
            {
                Patch,
                Feature,
                Minor,
                Major,
                LatestPatch,
                LatestFeature,
                LatestMinor,
                LatestMajor,
                Disable,
            }
        }
    }

    public static Task<Result> Run(DnvmEnv env, Logger logger)
    {
        UPath? globalJsonPathOpt = null;
        UPath cwd = env.Cwd;
        while (cwd != UPath.Root)
        {
            var testPath = cwd / "global.json";
            if (!env.CwdFs.FileExists(testPath))
            {
                break;
            }
            cwd = cwd.GetDirectory();
        }

        if (globalJsonPathOpt is not {} globalJsonPath)
        {
            logger.Error("No global.json found in the current directory or any of its parents.");
            return Task.FromResult(Result.NoGlobalJson);
        }

        GlobalJsonSubset json;
        try
        {
            var text = env.CwdFs.ReadAllText(globalJsonPath);
            json = JsonSerializer.Deserialize<GlobalJsonSubset>(text);
        }
        catch (IOException e)
        {
            logger.Error("Failed to read global.json: " + e.Message);
            return Task.FromResult(Result.IoError);
        }

        if (json.Sdk is not {} sdk)
        {
            logger.Error("global.json does not contain an SDK section.");
            return Task.FromResult(Result.NoSdkSection);
        }

        if (sdk.Version is not {} version)
        {
            logger.Error("SDK section in global.json does not contain a version.");
            return Task.FromResult(Result.NoVersion);
        }

        return Task.FromResult(Result.Success);
    }
}