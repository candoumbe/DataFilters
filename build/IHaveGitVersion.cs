using JetBrains.Annotations;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common;

namespace DataFilters.ContinuousIntegration
{
    [PublicAPI]
    public interface IHaveGitVersion : INukeBuild
    {
        [GitVersion(Framework = "net6.0", NoFetch = true)]
        [Required]
        GitVersion Versionning => TryGetValue(() => Versionning);
    }
}