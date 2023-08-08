using Spectre.Console;
using Spectre.Console.Testing;
using Xunit;
using Zio.FileSystems;

namespace Dnvm.Test;

public sealed class ListTests
{
    private readonly TestConsole _console = new();
    private readonly Logger _logger;
    private readonly DnvmEnv _memoryEnv;

    public ListTests()
    {
       _logger = new Logger(_console);
        var envVars = new Dictionary<string, string>();
        _memoryEnv = new DnvmEnv(
            new MemoryFileSystem(),
            getUserEnvVar: s => envVars[s],
            setUserEnvVar: (name, val) => envVars[name] = val);
    }

    [Fact]
    public void BasicList()
    {
        var manifest = Manifest.Empty
            .AddSdk(new InstalledSdk("1.0.0"), Channel.Latest)
            .AddSdk(new InstalledSdk("4.0.0-preview1") { SdkDirName = new("preview") }, Channel.Preview);

        var newline = Text.NewLine;
        ListCommand.PrintSdks(_logger, manifest);
        var output = """
Installed SDKs:

┌───┬─────────┬────────────────┬──────────┐
│   │ Channel │ Version        │ Location │
├───┼─────────┼────────────────┼──────────┤
│ * │ latest  │ 1.0.0          │ dn       │
│   │ preview │ 4.0.0-preview1 │ preview  │
└───┴─────────┴────────────────┴──────────┘
""";

        Assert.Equal(output, string.Join(Environment.NewLine, _console.Lines));
    }

    [Fact]
    public async Task ListFromFile()
    {
        var manifest = Manifest.Empty
            .AddSdk(new InstalledSdk("42.42.42"), Channel.Latest);

        var home = _memoryEnv;
        home.WriteManifest(manifest);

        var ret = await ListCommand.Run(_logger, home);
        Assert.Equal(0, ret);
        var output = """
Installed SDKs:

┌───┬─────────┬──────────┬──────────┐
│   │ Channel │ Version  │ Location │
├───┼─────────┼──────────┼──────────┤
│ * │ latest  │ 42.42.42 │ dn       │
└───┴─────────┴──────────┴──────────┘
""";

        Assert.Equal(output, string.Join(Environment.NewLine, _console.Lines));
    }
}