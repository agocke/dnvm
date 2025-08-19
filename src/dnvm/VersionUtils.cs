
using Semver;
using static Dnvm.GlobalJsonSubset.SdkSubset.RollForwardOptions;

namespace Dnvm;

public static class VersionUtils
{
    public static bool IsUpdate(SemVersion installed, SemVersion available, GlobalJsonSubset.SdkSubset.RollForwardOptions rollForward)
    {
        return rollForward switch
        {
            Major => available.Major > installed.Major,
            Minor => available.Major == installed.Major && available.Minor > installed.Minor,
            Feature => available.Major == installed.Major && available.Minor == installed.Minor && available.Patch / 100 > installed.Patch / 100,
            LatestPatch => available.Major == installed.Major && available.Minor == installed.Minor && available.Patch > installed.Patch,
            _ => false
        };
    }
}
