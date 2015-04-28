using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using Microsoft.Framework.Runtime;
using DnxProject = Microsoft.Framework.Runtime.Project;
using Microsoft.Framework.Runtime.Roslyn;
using System.Runtime.Versioning;

namespace Microsoft.CodeAnalysis.Workspaces.Dnx
{
    // Represents a project for a particular target framework
    public class ProjectInformation
    {
        // Path to the project.json
        public string Path { get; set; }

        public string Configuration { get; set; }

        public FrameworkName Framework { get; set; }

        public CompilationSettings CompilationSettings { get; set; }

        // List of source files
        public IList<string> SourceFiles { get; set; }

        // Dependency information
        public DependencyInformation DependencyInfo { get; set; }

        public DnxProject Project { get; set; }
    }
}