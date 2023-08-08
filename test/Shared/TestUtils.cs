
using System.Runtime.CompilerServices;
using Zio.FileSystems;

namespace Dnvm.Test;

public static class TestUtils
{
    private static DirectoryInfo ThisDir([CallerFilePath]string path = "") => Directory.GetParent(path)!;

    public static readonly string AssetsDir = Path.Combine(ThisDir().FullName, "assets");

    public static readonly string ArtifactsDir = Path.Combine(ThisDir().Parent!.Parent!.FullName, "artifacts");

    public static readonly DirectoryInfo ArtifactsTestDir = Directory.CreateDirectory(
        Path.Combine(ArtifactsDir, "test"));

    public static readonly DirectoryInfo ArtifactsTmpDir = Directory.CreateDirectory(
        Path.Combine(ArtifactsTestDir.FullName, "tmp"));

    public static TempDirectory CreateTempDirectory() => TempDirectory.CreateSubDirectory(ArtifactsTmpDir.FullName);

    public static DnvmEnv CreatePhysicalTestEnv(out TempDirectory dnvmHome, out Dictionary<string, string> envVars)
    {
        dnvmHome = CreateTempDirectory();
        envVars = new Dictionary<string, string>();
        var envCopy = envVars;
        var physicalFs = new PhysicalFileSystem();
        return new DnvmEnv(
            new SubFileSystem(physicalFs, physicalFs.ConvertPathFromInternal(dnvmHome.Path)),
            s => envCopy[s],
            (name, val) => envCopy[name] = val);
    }
}