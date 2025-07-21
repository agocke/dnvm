
using Semver;

namespace Dnvm;

internal static class RollForwardComparisons
{
    /// <summary>
    /// Compares equal if the major, minor, and feature band versions are equal, and the patch
    /// version is greater or equal.
    /// </summary>
    public static int LatestPatchComparison(SemVersion a, SemVersion b)
    {
        if (a.Major != b.Major)
        {
            return a.Major.CompareTo(b.Major);
        }
        if (a.Minor != b.Minor)
        {
            return a.Minor.CompareTo(b.Minor);
        }
        // This is where dotnet differs from semver. The semver patch version is the latest
        // number in the version string, but dotnet expects SDKs versions to end in xnn, where
        // x is the feature band and nn is the patch level.
        if ((a.Patch / 100) != (b.Patch / 100))
        {
            return (a.Patch / 100).CompareTo(b.Patch / 100);
        }
        var aPatch = a.Patch % 100;
        var bPatch = b.Patch % 100;
        if (aPatch != bPatch)
        {
            // If the patch version is greater than, we will consider the versions 'equal', meaning
            // that they are compatible.
            return aPatch >= bPatch ? 0 : -1;
        }
        if (a.Prerelease != b.Prerelease)
        {
            return a.Prerelease.CompareTo(b.Prerelease) >= 0 ? 0 : -1;
        }
        return 0;
    }

    /// <summary>
    /// Compares equal if the major and minor versions are equal, and the feature band and patch
    /// version are greater or equal.
    /// </summary>
    public static int LatestFeatureComparison(SemVersion a, SemVersion b)
    {
        if (a.Major != b.Major)
        {
            return a.Major.CompareTo(b.Major);
        }
        if (a.Minor != b.Minor)
        {
            return a.Minor.CompareTo(b.Minor);
        }
        if (a.Patch != b.Patch)
        {
            return a.Patch >= b.Patch ? 0 : -1;
        }
        if (a.Prerelease != b.Prerelease)
        {
            return a.Prerelease.CompareTo(b.Prerelease) >= 0 ? 0 : -1;
        }
        return 0;
    }

    /// <summary>
    /// Compares equal if the major versions are equal, and the minor versions are greater or equal.
    /// </summary>
    public static int LatestMinorComparison(SemVersion a, SemVersion b)
    {
        if (a.Major != b.Major)
        {
            return a.Major.CompareTo(b.Major);
        }
        if (a.Minor != b.Minor)
        {
            return a.Minor >= b.Minor ? 0 : -1;
        }
        if (a.Patch != b.Patch)
        {
            return a.Patch >= b.Patch ? 0 : -1;
        }
        if (a.Prerelease != b.Prerelease)
        {
            return a.Prerelease.CompareTo(b.Prerelease) >= 0 ? 0 : -1;
        }
        return 0;
    }

    /// <summary>
    /// Compares equal if the major versions are greater than or equal.
    /// </summary>
    public static int LatestMajorComparison(SemVersion a, SemVersion b)
    {
        if (a.Major != b.Major)
        {
            return a.Major >= b.Major ? 0 : -1;
        }
        if (a.Minor != b.Minor)
        {
            return a.Minor >= b.Minor ? 0 : -1;
        }
        if (a.Patch != b.Patch)
        {
            return a.Patch >= b.Patch ? 0 : -1;
        }
        if (a.Prerelease != b.Prerelease)
        {
            return a.Prerelease.CompareTo(b.Prerelease) >= 0 ? 0 : -1;
        }
        return 0;
    }
}