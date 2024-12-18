
using System.IO;
using System.Threading.Tasks;
using Serde.Json;
using Spectre.Console;
using Zio;

namespace Dnvm;

public static class RestoreCommand
{
    public static Task<int> Run(DnvmEnv env, Logger logger)
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
            return Task.FromResult(1);
        }

        try
        {
            var text = env.CwdFs.ReadAllText(globalJsonPath);
            var json = JsonSerializer.Deserialize<JsonValue>(text);
        }
        catch (IOException)
        {
        }
        return Task.FromResult(0);
    }
}