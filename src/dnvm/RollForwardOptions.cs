
using StaticCs;

namespace Dnvm;

[Closed]
public enum RollForwardOptions
{
    /// <summary>
    /// Uses the specified version.
    ///
    /// If not found, rolls forward to the latest patch level.
    ///
    /// If not found, fails.
    /// </summary>
    Patch,
    /// <summary>
    /// Uses the latest patch level for the specified major, minor, and feature band.
    ///
    /// If not found, rolls forward to the next higher feature band within the same
    /// major/minor and uses the latest patch level for that feature band.
    ///
    /// If not found, fails.
    /// </summary>
    Feature,
    /// <summary>
    /// Uses the latest patch level for the specified major, minor, and feature band.
    ///
    /// If not found, rolls forward to the next higher feature band within the same
    /// major/minor version and uses the latest patch level for that feature band.
    ///
    /// If not found, rolls forward to the next higher minor and feature band within the
    /// same major and uses the latest patch level for that feature band.
    ///
    /// If not found, fails.
    /// </summary>
    Minor,
    /// <summary>
    /// Uses the latest patch level for the specified major, minor, and feature band.
    ///
    /// If not found, rolls forward to the next higher feature band within the same
    /// major/minor version and uses the latest patch level for that feature band.
    ///
    /// If not found, rolls forward to the next higher minor and feature band within the
    /// same major and uses the latest patch level for that feature band.
    ///
    /// If not found, rolls forward to the next higher major, minor, and feature band
    /// and uses the latest patch level for that feature band.
    ///
    /// If not found, fails.
    /// </summary>
    Major,
    /// <summary>
    /// Uses the latest installed patch level that matches the requested major, minor,
    /// and feature band with a patch level that's greater than or equal to the
    /// specified value.
    ///
    /// If not found, fails.
    /// </summary>
    LatestPatch,
    /// <summary>
    /// Uses the highest installed feature band and patch level that matches the
    /// requested major and minor with a feature band and patch level that's greater
    /// than or equal to the specified value.
    ///
    /// If not found, fails.
    /// </summary>
    LatestFeature,
    /// <summary>
    /// Uses the highest installed minor, feature band, and patch level that matches the
    /// requested major with a minor, feature band, and patch level that's greater than
    /// or equal to the specified value.
    ///
    /// If not found, fails.
    /// </summary>
    LatestMinor,
    /// <summary>
    /// Uses the highest installed .NET SDK with a version that's greater than or equal
    /// to the specified value.
    ///
    /// If not found, fail.
    /// </summary>
    LatestMajor,
    /// <summary>
    /// Doesn't roll forward. An exact match is required.
    /// </summary>
    Disable,
}
