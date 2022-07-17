using JetBrains.Annotations;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common;

namespace DataFilters.ContinuousIntegration
{
    [PublicAPI]
    public interface IHaveGitVersion : INukeBuild
    {
        [GitVersion(Framework = "net5.0", NoFetch = true)]
        [Required]
        public GitVersion Versioning => TryGetValue(() => Versioning);
    }
}