using System;
using System.Collections.Generic;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI.AzurePipelines;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.Publish);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("Packs version postfix")]
    readonly string PackVersionPostfix;
    
    [Solution] 
    readonly Solution Solution;

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            EnsureCleanDirectory("deploy/ITLab-Back");
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoRestore());
        });
    Target Publish => _ => _
        .DependsOn(Compile, Clean)
        .Executes(() =>
        {
            DotNetPublish(s => s
                .SetProject("BackEnd/BackEnd.csproj")
                .SetOutput("deploy/ITLab-Back")
                .SetConfiguration(Configuration)
                .EnableNoRestore());
        });

    Target PackDb => _ => _
        .Requires(() => PackVersionPostfix)
        .Executes(() =>
        {
            Solution.GetProjects("Database$|Models$|Extensions$")
                .ForEach(project =>
                {
                    DotNetPack(p => p
                        .SetProject(project.Path)
                        .SetConfiguration(Configuration)
                        .SetOutputDirectory("packs")
                        .SetVersion($"1.0.0-CI-{PackVersionPostfix}"));
                });
        });
}
