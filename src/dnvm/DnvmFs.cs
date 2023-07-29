
using System;
using System.IO;
using System.Threading.Tasks;
using Serde.Json;
using Zio;
using Zio.FileSystems;

namespace Dnvm;

public sealed class DnvmFs : IDisposable
{
    public const string ManifestFileName = "dnvmManifest.json";
    public static UPath ManifestPath => UPath.Root / ManifestFileName;
    public static UPath EnvPath => UPath.Root / "env";

    public static DnvmFs CreatePhysical(string realPath)
    {
        var physicalFs = new PhysicalFileSystem();
        return new DnvmFs(new SubFileSystem(physicalFs, physicalFs.ConvertPathFromInternal(realPath)));
    }

    public readonly IFileSystem Vfs;
    public string RealPath => Vfs.ConvertPathToInternal(UPath.Root);
    public SubFileSystem TempFs { get; }

    public DnvmFs(IFileSystem vfs)
    {
        Vfs = vfs;
        // TempFs must be a physical file system because we pass the path to external
        // commands that will not be able to write to shared memory
        TempFs = new SubFileSystem(
            new PhysicalFileSystem(),
            Path.GetTempPath(),
            owned: true);
    }

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
        var tmpPath = ManifestPath + ".tmp";
        Vfs.WriteAllText(tmpPath, text);
        Vfs.MoveFile(tmpPath, ManifestPath);
    }

    public void Dispose()
    {
        Vfs.Dispose();
        TempFs.Dispose();
    }
}