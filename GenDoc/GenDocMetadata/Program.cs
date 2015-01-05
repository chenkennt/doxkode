using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.IO;
using Microsoft.CodeAnalysis.MSBuild;
using Newtonsoft.Json;
using System.Collections;
using DocAsCode.EntityModel;
using DocAsCode.Utility;

namespace DocAsCode.GenDocMetadata
{
    [Flags]
    enum OutputType
    {
        Metadata = 0x1,
        Markdown = 0x2,
        Both = 0x3,
    }

    public class Program
    {
        private static DelimitedStringArrayConverter _delimitedArrayConverter = new DelimitedStringArrayConverter();

        static void Main(string[] args)
        {
            string slnPath = null;
            string outputDirectory = null;
            string delimitedProjectFilenames = null;
            OutputType outputType = OutputType.Both;

            var options = new Option[]
                {
                    new Option(null, s => slnPath = s, helpName: "solutionPath", required: true, helpText: @"The path of the solution whose metadata is to be generated"),
                    new Option("o", s => outputDirectory = s, defaultValue: null, helpName: "outputDirectory", helpText: "The output metadata files will be generated into this folder. If not set, the default output directory would be under the current folder with the sln name"),
                    new Option("p", s => delimitedProjectFilenames = s, defaultValue: null, helpName: "delimitedProjectFiles", helpText: "Specqifiy the project names whose metadata file will be generated, delimits files with comma, only file names with .csproj extension will be recognized"),
                    new Option("t", s => outputType = (OutputType)Enum.Parse(typeof(OutputType), s, true), defaultValue: outputType.ToString(), helpName: "outputType", helpText: "Specifiy if the docmta or the markdown file will be generated, by default both the docmta and the markdown file will be generated"),
                };

            if (!ConsoleParameterParser.ParseParameters(options, args))
            {
                return;
            }

            if (string.IsNullOrEmpty(outputDirectory))
            {
                // use the sln name as the default output directory
                outputDirectory = Path.GetFileNameWithoutExtension(slnPath);
            }

            if (Directory.Exists(outputDirectory))
            {
                Console.Error.WriteLine("Warning: {0} directory already exists.", outputDirectory);
            }

            Directory.CreateDirectory(outputDirectory);
            try
            {
                var solution = MSBuildWorkspace.Create().OpenSolutionAsync(slnPath).Result;
                var projects = solution.Projects;
                string[] specifiedProjectFilenames = delimitedProjectFilenames == null ? null : (string[])_delimitedArrayConverter.ConvertFrom(delimitedProjectFilenames);

                foreach (var project in projects)
                {
                    if (specifiedProjectFilenames != null && !specifiedProjectFilenames.Contains(Path.GetFileName(project.FilePath), StringComparer.OrdinalIgnoreCase))
                    {
                        Console.Error.WriteLine("Project {0} is not in the specified file list, will be ignored.", project.Name);
                        continue;
                    }

                    var assemblyDocMetadata = GenerateAssemblyDocMetadata(project);

                    if (outputType.HasFlag(OutputType.Metadata))
                    {
                        ExportMetadataFile(assemblyDocMetadata, Path.Combine(outputDirectory, "mta"));
                    }

                    if (outputType.HasFlag(OutputType.Markdown))
                    {
                        ExportMarkdownToc(assemblyDocMetadata, Path.Combine(outputDirectory, "mdtoc"));
                    }
                }

                Console.WriteLine("Metadata files successfully generated under {0}", outputDirectory);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Failing in generating metadata from {0}: {1}", slnPath, e);
            }
        }

        static void PrintUsage()
        {
            Console.Error.WriteLine("Usage: GenDocMetadata <SolutionPath> (<OutputDirectory>)");
        }

        private static void ExportMetadataFile(AssemblyDocMetadata assemblyDocMetadata, string baseDirectory)
        {
            if (Directory.Exists(baseDirectory))
            {
                Console.Error.WriteLine("Warning:" + string.Format("Directory {0} already exists!", baseDirectory));
            }

            Directory.CreateDirectory(baseDirectory);
            // For metadata file, per assembly per file
            var metadataFilePath = Path.Combine(baseDirectory, (assemblyDocMetadata.Id + ".docmta").ToValidFilePath());
            
            Console.WriteLine("Generating metadata file {0}", metadataFilePath);
            using (StreamWriter streamWriter = new StreamWriter(metadataFilePath))
            {
                assemblyDocMetadata.WriteMetadata(streamWriter);
            }
        }

        /// <summary>
        /// |--AssemblyName
        ///         |--NamespaceId1
        ///                 |--NamespaceId1.md
        ///                 |--ClassId1.md
        ///                 |--ClassId2.md
        ///         |--NamepsaceId2
        ///                 |--NamespaceId2.md
        ///                 |--ClassId3.md
        /// </summary>
        /// <param name="directory"></param>
        private static void ExportMarkdownToc(AssemblyDocMetadata assemblyDocMetadata, string baseDirectory)
        {
            baseDirectory = Path.Combine(baseDirectory, assemblyDocMetadata.Id);
            if (Directory.Exists(baseDirectory))
            {
                Console.Error.WriteLine("Warning:" + string.Format("Directory {0} already exists!", baseDirectory));
            }

            if (!assemblyDocMetadata.Namespaces.Any())
            {
                Console.Error.WriteLine("Warning:" + string.Format("No namespace is found inside current assembly {0}", assemblyDocMetadata.Id));
            }

            Directory.CreateDirectory(baseDirectory);

            foreach (var ns in assemblyDocMetadata.Namespaces)
            {
                string directory = Path.Combine(baseDirectory, ns.Id.ToString().ToValidFilePath());
                if (Directory.Exists(directory))
                {
                    Console.Error.WriteLine("Warning:" + string.Format("Directory {0} already exists!", directory));
                }

                if (!ns.Members.Any())
                {
                    Console.Error.WriteLine("Warning:" + string.Format("No member is found inside current assembly {0}", ns.Id));
                }

                Directory.CreateDirectory(directory);
                var namespaceFile = Path.Combine(directory, ns.Id.ToString().ToValidFilePath()) + ".md";
                using (StreamWriter writer = new StreamWriter(namespaceFile))
                {
                    ns.WriteMarkdownSkeleton(writer);
                }

                foreach (var member in ns.Members)
                {
                    var memberFile = Path.Combine(directory, member.Id.ToString().ToValidFilePath()) + ".md";
                    using (StreamWriter writer = new StreamWriter(memberFile))
                    {
                        member.WriteMarkdownSkeleton(writer);
                    }
                }
            }
        }

        private static AssemblyDocMetadata GenerateAssemblyDocMetadata(Project project)
        {
            var compilation = project.GetCompilationAsync().Result;

            // essentially you get the containing assembly of SpecialType.System_Object and get the assembly��s version
            var mscolibAssembly = compilation.GetSpecialType(SpecialType.System_Object).ContainingAssembly.Identity;
            var version = mscolibAssembly.Version;

            // Get AssemblySymbols for above compilation and the assembly (mscorlib) referenced by it.
            IAssemblySymbol compilationAssembly = compilation.Assembly;

            AssemblyDocMetadata assemblyDocMetadata = new AssemblyDocMetadata(compilationAssembly.Name)
            {
                MscorlibVersion = version,
            };

            var namespaceMembers = compilation.Assembly.GlobalNamespace.GetNamespaceMembers();
            var namespaceQueue = new Queue<INamespaceSymbol>();

            foreach (var ns in namespaceMembers)
            {
                namespaceQueue.Enqueue(ns);
            }

            while (namespaceQueue.Count > 0)
            {
                var ns = namespaceQueue.Dequeue();
                foreach (var namespaceMember in ns.GetNamespaceMembers())
                {
                    namespaceQueue.Enqueue(namespaceMember);
                }

                var types = ns.GetTypeMembers();

                if (!types.Any())
                {
                    continue;
                }

                // var identifier = syntaxRoot.DescendantNodes().OfType<NamespaceDeclarationSyntax>();
                var namespaceDocMetadata = DocMetadataConverterFactory.Convert(ns) as NamespaceDocMetadata;
                if (namespaceDocMetadata == null)
                {
                    continue;
                }

                assemblyDocMetadata.TryAddNamespace(namespaceDocMetadata);

                // Namespace:
                foreach (var type in types)
                {
                    var classMetadata = DocMetadataConverterFactory.Convert(type) as ClassDocMetadata;
                    if (classMetadata != null)
                    {
                        namespaceDocMetadata.TryAdd(classMetadata, MemberType.Class);
                        classMetadata = DocMetadataConverterFactory.ExpandSymbolMembers(type, (CompositeDocMetadata)classMetadata) as ClassDocMetadata;
                        continue;
                    }

                    var enumMetadata = DocMetadataConverterFactory.Convert(type) as EnumDocMetadata;
                    if (enumMetadata != null)
                    {
                        namespaceDocMetadata.TryAdd(enumMetadata, MemberType.Enum);
                        continue;
                    }
                    var structMetadata = DocMetadataConverterFactory.Convert(type) as StructDocMetadata;
                    if (structMetadata != null)
                    {
                        namespaceDocMetadata.TryAdd(structMetadata, MemberType.Struct);
                        structMetadata = DocMetadataConverterFactory.ExpandSymbolMembers(type, (CompositeDocMetadata)structMetadata) as StructDocMetadata;
                        continue;
                    }
                    var interfaceMetadata = DocMetadataConverterFactory.Convert(type) as InterfaceDocMetadata;
                    if (interfaceMetadata != null)
                    {
                        namespaceDocMetadata.TryAdd(interfaceMetadata, MemberType.Interface);
                        interfaceMetadata = DocMetadataConverterFactory.ExpandSymbolMembers(type, (CompositeDocMetadata)interfaceMetadata) as InterfaceDocMetadata;
                        continue;
                    }
                    var delegateMetadata = DocMetadataConverterFactory.Convert(type) as DelegateDocMetadata;
                    if (delegateMetadata != null)
                    {
                        namespaceDocMetadata.TryAdd(delegateMetadata, MemberType.Delegate);
                    }
                }
            }
            return assemblyDocMetadata;
        }
    }
}
