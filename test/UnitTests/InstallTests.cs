using System.Collections.Immutable;
using Serde.Json;
using Xunit;
using Xunit.Abstractions;
using static Dnvm.Install;

namespace Dnvm.Test;

public sealed class InstallTests : IDisposable
{
    private readonly Logger _logger;
    private readonly TempDirectory _userHome = TestUtils.CreateTempDirectory();
    private readonly TempDirectory _dnvmHome = TestUtils.CreateTempDirectory();
    private readonly Dictionary<string, string> _envVars = new();
    private readonly GlobalOptions _globalOptions;

    public InstallTests(ITestOutputHelper output)
    {
        var wrapper = new OutputWrapper(output);
        _logger = new Logger(wrapper, wrapper);
        _globalOptions = new GlobalOptions {
            DnvmHome = _dnvmHome.Path,
            UserHome = _userHome.Path,
            GetUserEnvVar = s => _envVars[s],
            SetUserEnvVar = (name, val) => _envVars[name] = val,
        };
    }

    public void Dispose()
    {
        _userHome.Dispose();
        _dnvmHome.Dispose();
    }

    [Fact]
    public async Task LtsInstall()
    {
        await using var server = new MockServer();
        const Channel channel = Channel.Lts;
        var options = new CommandArguments.InstallArguments()
        {
            Channel = channel,
            FeedUrl = server.PrefixString,
            UpdateUserEnvironment = false,
        };
        var installCmd = new Install(_globalOptions, _logger, options);
        var task = installCmd.Run();
        Result retVal = await task;
        Assert.Equal(Result.Success, retVal);
        var sdkInstallDir = Path.Combine(_dnvmHome.Path, GlobalOptions.DefaultSdkDirName.Name);
        var dotnetFile = Path.Combine(sdkInstallDir, "dotnet" + Utilities.ExeSuffix);
        Assert.True(File.Exists(dotnetFile));
        Assert.Contains(Assets.ArchiveToken, File.ReadAllText(dotnetFile));

        var manifest = File.ReadAllText(_globalOptions.ManifestPath);
        var installedVersion = server.ReleasesIndexJson.Releases[0].LatestSdk;
        var installedVersions = ImmutableArray.Create(new InstalledSdk { Version = installedVersion, SdkDirName = GlobalOptions.DefaultSdkDirName });
        Assert.Equal(new Manifest
        {
            InstalledSdkVersions = installedVersions,
            TrackedChannels = ImmutableArray.Create(new[] { new TrackedChannel {
                ChannelName = channel,
                SdkDirName = GlobalOptions.DefaultSdkDirName,
                InstalledSdkVersions = ImmutableArray.Create(installedVersion)
            }})
        }, JsonSerializer.Deserialize<Manifest>(manifest));
    }

    [Fact]
    public async Task SdkInstallDirMissing()
    {
        await using var server = new MockServer();
        var args = new CommandArguments.InstallArguments()
        {
            Channel = Channel.Lts,
            FeedUrl = server.PrefixString,
            UpdateUserEnvironment = false,
            Verbose = true,
        };
        var sdkInstallDir = Path.Combine(_globalOptions.DnvmHome, GlobalOptions.DefaultSdkDirName.Name);
        Assert.False(Directory.Exists(sdkInstallDir));
        Assert.True(Directory.Exists(_globalOptions.DnvmHome));
        Assert.Equal(Result.Success, await Install.Run(_globalOptions, _logger, args));
        var dotnetFile = Path.Combine(sdkInstallDir, "dotnet" + Utilities.ExeSuffix);
        Assert.True(File.Exists(dotnetFile));
        Assert.Contains(Assets.ArchiveToken, File.ReadAllText(dotnetFile));
    }

    [Fact]
    public async Task PreviewIsolated()
    {
        await using var server = new MockServer();
        server.ReleasesIndexJson = server.ReleasesIndexJson with {
            Releases = server.ReleasesIndexJson.Releases.Select(r => r with { SupportPhase = "preview" }).ToImmutableArray()
        };

        var args = new CommandArguments.InstallArguments()
        {
            Channel = Channel.Preview,
            FeedUrl = server.PrefixString,
            UpdateUserEnvironment = false,
        };
        // Check that the preview install is isolated into a "preview" subdirectory
        var sdkInstallDir = Path.Combine(_globalOptions.DnvmHome, Channel.Preview.ToString().ToLowerInvariant());
        Assert.False(Directory.Exists(sdkInstallDir));
        Assert.True(Directory.Exists(_globalOptions.DnvmHome));
        Assert.Equal(Result.Success, await Install.Run(_globalOptions, _logger, args));
        var dotnetFile = Path.Combine(sdkInstallDir, "dotnet" + Utilities.ExeSuffix);
        Assert.True(File.Exists(dotnetFile));
        Assert.Contains(Assets.ArchiveToken, File.ReadAllText(dotnetFile));
    }

    [Fact]
    public async Task InstallStsToSubdir()
    {
        await using var server = new MockServer();
        server.ReleasesIndexJson = server.ReleasesIndexJson with {
            Releases = server.ReleasesIndexJson.Releases.Select(r => r with { ReleaseType = "sts" }).ToImmutableArray()
        };
        const string dirName = "sts";
        var args = new CommandArguments.InstallArguments()
        {
            Channel = Channel.Sts,
            FeedUrl = server.PrefixString,
            UpdateUserEnvironment = false,
            SdkDir = dirName
        };
        // Check that the SDK is installed is isolated into the "sts" subdirectory
        var sdkInstallDir = Path.Combine(_globalOptions.DnvmHome, dirName);
        Assert.False(Directory.Exists(sdkInstallDir));
        Assert.True(Directory.Exists(_globalOptions.DnvmHome));
        Assert.Equal(Result.Success, await Install.Run(_globalOptions, _logger, args));
        var dotnetFile = Path.Combine(sdkInstallDir, "dotnet" + Utilities.ExeSuffix);
        Assert.True(File.Exists(dotnetFile));
        Assert.Contains(Assets.ArchiveToken, File.ReadAllText(dotnetFile));
    }
}