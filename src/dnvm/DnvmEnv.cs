
using System;
using System.IO;
using System.Threading.Tasks;
using Serde.Json;
using Zio;
using Zio.FileSystems;

namespace Dnvm;

/// <summary>
/// Represents the external environment of the dnvm tool.
/// </summary>
public sealed class DnvmEnv : IDisposable
{
    public const string ManifestFileName = "dnvmManifest.json";

    public readonly IFileSystem Vfs;

    public static UPath ManifestPath => UPath.Root / ManifestFileName;
    public static UPath EnvPath => UPath.Root / "env";
    public static UPath DnvmExePath => UPath.Root / Utilities.DnvmExeName;

    public string RealPath => Vfs.ConvertPathToInternal(UPath.Root);

    public static DnvmEnv CreateDefault(string dnvmHome)
    {
        var physicalFs = new PhysicalFileSystem();
        return new DnvmEnv(new SubFileSystem(physicalFs, physicalFs.ConvertPathFromInternal(dnvmHome)),
            getUserEnvVar: s => Environment.GetEnvironmentVariable(s, EnvironmentVariableTarget.User),
            setUserEnvVar: (name, val) => Environment.SetEnvironmentVariable(name, val, EnvironmentVariableTarget.User));
    }

    public DnvmEnv(
        IFileSystem vfs,
        Func<string, string?> getUserEnvVar,
        Action<string, string> setUserEnvVar)
    {
        Vfs = vfs;
        GetUserEnvVar = getUserEnvVar;
        SetUserEnvVar = setUserEnvVar;
    }

    public Func<string, string?> GetUserEnvVar { get; }
    public Action<string, string> SetUserEnvVar { get; }

    /// <summary>
    /// Reads a manifest (any version) from the given path and returns
    /// an up-to-date <see cref="Manifest" /> (latest version).
    /// Throws <see cref="InvalidDataException" /> if the manifest is invalid.
    /// </summary>
    public Manifest ReadManifest()
    {
        var text = Vfs.ReadAllText(ManifestPath);
        return ManifestUtils.DeserializeNewOrOldManifest(text) ?? throw new InvalidDataException();
    }

    public void WriteManifest(Manifest manifest)
    {
        var text = JsonSerializer.Serialize(manifest);
        Vfs.WriteAllText(ManifestPath, text);
    }

    public void Dispose()
    {
        Vfs.Dispose();
    }
}