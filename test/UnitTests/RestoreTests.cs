
using Spectre.Console.Testing;
using Xunit;

namespace Dnvm.Test;

public sealed class RestoreTests
{
    [Fact]
    public async Task NoGlobalJson() => await TestUtils.RunWithServer(async (server, env) =>
    {
        var logger = new Logger(new TestConsole());
        var restoreResult = await RestoreCommand.Run(env, logger);
        Assert.Equal(RestoreCommand.Result.NoGlobalJson, restoreResult);
    });
}