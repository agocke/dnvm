using Semver;
using Serde;
using StaticCs;

namespace Dnvm;

/// <summary>
/// Serializes the <see cref="SdkDirName.Name"/> directly as a string.
/// </summary>
[GenerateSerde(With = typeof(SdkDirNameV10._SerdeObj))]
public sealed partial record SdkDirNameV10(string Name)
{
    public string Name { get; init; } = Name.ToLower();

    private sealed class _SerdeObj : ISerde<SdkDirNameV10>
    {
        public ISerdeInfo SerdeInfo => StringProxy.SerdeInfo;
        public SdkDirNameV10 Deserialize(IDeserializer deserializer)
            => new SdkDirNameV10(StringProxy.Instance.Deserialize(deserializer));
        public void Serialize(SdkDirNameV10 value, ISerializer serializer)
        {
            serializer.WriteString(value.Name);
        }
    }

    public static implicit operator SdkDirNameV10(SdkDirNameV9 dirName) => new SdkDirNameV10(dirName.Name);
}

[GenerateSerde]
public sealed partial record ManifestV10
{
    // Serde doesn't serialize consts, so we have a separate property below for serialization.
    public const int VersionField = 10;

    [SerdeMemberOptions(SkipDeserialize = true)]
    public int Version => VersionField;

    public required bool PreviewsEnabled { get; init; }
    public required SdkDirNameV10 CurrentSdkDir { get; init; }
    public required EqArray<InstalledSdkV10> InstalledSdks { get; init; }
    public required EqArray<RegisteredChannelV10> RegisteredChannels { get; init; }
}

[GenerateSerde]
public partial record RegisteredChannelV10
{
    public required Channel ChannelName { get; init; }
    public required SdkDirNameV10 SdkDirName { get; init; }
    [SerdeMemberOptions(
        SerializeProxy = typeof(EqArrayProxy.Ser<SemVersion, SemVersionProxy>),
        DeserializeProxy = typeof(EqArrayProxy.De<SemVersion, SemVersionProxy>))]
    public EqArray<SemVersion> InstalledSdkVersions { get; init; } = EqArray<SemVersion>.Empty;
    public bool Untracked { get; init; } = false;
}

[GenerateSerde]
public partial record InstalledSdkV10
{
    [SerdeMemberOptions(Proxy = typeof(SemVersionProxy))]
    public required SemVersion ReleaseVersion { get; init; }
    [SerdeMemberOptions(Proxy = typeof(SemVersionProxy))]
    public required SemVersion SdkVersion { get; init; }
    [SerdeMemberOptions(Proxy = typeof(SemVersionProxy))]
    public required SemVersion RuntimeVersion { get; init; }
    [SerdeMemberOptions(Proxy = typeof(SemVersionProxy))]
    public required SemVersion AspNetVersion { get; init; }
    public required SdkDirNameV10 SdkDirName { get; init; }
    public required RollForwardOptions RollForward { get; init; } = RollForwardOptions.Disable;

    [Closed]
    [GenerateSerde]
    public enum RollForwardOptions
    {
        Disable,
        Patch,
        Minor,
        Major
    }
}

public static partial class ManifestV10Convert
{
    public static ManifestV10 Convert(this ManifestV9 v9) => new ManifestV10
    {
        PreviewsEnabled = false,
        CurrentSdkDir = v9.CurrentSdkDir,
        InstalledSdks = v9.InstalledSdks.SelectAsArray(v => v.Convert()),
        RegisteredChannels = v9.RegisteredChannels.SelectAsArray(c => c.Convert())
    };

    public static InstalledSdkV10 Convert(this InstalledSdkV9 v9) => new InstalledSdkV10
    {
        ReleaseVersion = v9.ReleaseVersion,
        SdkVersion = v9.SdkVersion,
        RuntimeVersion = v9.RuntimeVersion,
        AspNetVersion = v9.AspNetVersion,
        SdkDirName = v9.SdkDirName,
        RollForward = InstalledSdkV10.RollForwardOptions.Patch,
    };

    public static RegisteredChannelV10 Convert(this RegisteredChannelV9 v9) => new RegisteredChannelV10
    {
        ChannelName = v9.ChannelName,
        SdkDirName = v9.SdkDirName,
        InstalledSdkVersions = v9.InstalledSdkVersions,
        Untracked = v9.Untracked,
    };
}