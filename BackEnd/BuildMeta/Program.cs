using System;
using DocAsCode.Utility;
using DocAsCode.EntityModel;

namespace DocAsCode.EntityModel
{
    [Flags]
    enum BuildTarget
    {
        Build = 0x0001,
        Merge = 0x0010,
        Markdown = 0x0100,
        All = 0x0111,
    }

    public class Program
    {
        static int Main(string[] args)
        {
            const string DefaultTocFileName = "toc.yml";
            const string DefaultIndexFileName = "index.yml";
            const string DefaultMarkdownFileName = "md.yml";
            const string DefaultMdFolderName = "md";
            const string DefaultReferenceFolderName = "reference";
            const string DefaultApiFolderName = "api";
            const string DefaultOutputFolderName = "output";
            const string DefaultIntermediateOutputListFile = "obj/output.list";
            string inputProjectList = null;
            string outputFolder = null;
            string inputMarkdownList = null;
            string outputMarkdownIndexFile = null;
            string outputTocFile = null;
            string outputIndexFile = null;
            string intermediateOutputListFile = null;
            string outputApiFolder = null;
            string outputMarkdownFolder = null;
            string outputReferenceFolder = null;
            BuildTarget target = BuildTarget.All;

            try
            {
                var options = new Option[]
                    {
                    new Option(null, p => inputProjectList = p, helpName: "inputProjectList", required: false, helpText: "Required. Specify the list of project files to generate documentation. Supported input types are: [1. the file path with extension .list | 2. the file paths seperated by comma(,)]."),
                    new Option("m", s => inputMarkdownList = s, defaultValue: null, helpName: "inputMarkdownList", helpText: "Specify the list of markdown files that contains all the additional documentations to the members in projects files. Supported input types are: [1. the file path with extension .list | 2. the file paths seperated by comma(,)]."),
                    new Option("o", s => outputFolder = s, defaultValue: DefaultOutputFolderName, helpName: "outputFolder", helpText: "Specify the output folder that contains all the generated metadata files. (default: " + DefaultOutputFolderName + ")."),
                    new Option("i", s => intermediateOutputListFile = s, defaultValue: DefaultIntermediateOutputListFile, helpName: "intermediateOutputListFile", helpText: "Specify the intermediate output file name for the metadata file generated by each project. (default: " + DefaultIntermediateOutputListFile + ")."),
                    new Option("toc", s => outputTocFile = s, defaultValue: DefaultTocFileName, helpName: "outputTocFile", helpText: "Specify the file name containing all the namespace yaml file paths. (default: " + DefaultTocFileName + ")."),
                    new Option("index", s => outputIndexFile = s, defaultValue: DefaultIndexFileName, helpName: "outputIndexFile", helpText: "Specify the file name containing all the available yaml files. (default: " + DefaultIndexFileName + ")."),
                    new Option("md", s => outputMarkdownIndexFile = s, defaultValue: DefaultMarkdownFileName, helpName: "outputMarkdownIndexFile", helpText: "Specify the file name containing all the markdown index. (default: " + DefaultMarkdownFileName + ")."),
                    new Option("folder", s => outputApiFolder = s, defaultValue: DefaultApiFolderName, helpName: "outputApiFolder", helpText: "Specify the output subfolder name containing all the member's yaml file. (default: " + DefaultApiFolderName + ")."),
                    new Option("mdFolder", s => outputMarkdownFolder = s, defaultValue: DefaultMdFolderName, helpName: "outputMarkdownFolder", helpText: "Specify the output subfolder name containing all the markdown files copied. (default: " + DefaultMdFolderName+ ")."),
                    new Option("referenceFolder", s => outputReferenceFolder = s, defaultValue: DefaultReferenceFolderName, helpName: "outputReferenceFolder", helpText: "Specify the output subfolder name containing all the files copied referenced by markdown files. (default: " + DefaultReferenceFolderName+ ")."),
                    new Option("target", s => target = (BuildTarget)Enum.Parse(typeof(BuildTarget), s, true), defaultValue: BuildTarget.All.ToString(), helpName: "buildTarget", helpText: "Specify the output target. (default: All)."),
                    };

                if (!ConsoleParameterParser.ParseParameters(options, args))
                {
                    return 1;
                }

                if (target.HasFlag(BuildTarget.Build))
                {

                    if (inputProjectList == null || inputProjectList.Length == 0)
                    {
                        ParseResult.WriteToConsole(ResultLevel.Warn, "No source project files are referenced. No documentation will be generated.");
                        return 2;
                    }

                    BuildMetaHelper.GenerateMetadataFromProjectListAsync(inputProjectList, intermediateOutputListFile).Wait();
                }

                if (target.HasFlag(BuildTarget.Merge))
                    BuildMetaHelper.MergeMetadataFromMetadataListAsync(intermediateOutputListFile, outputFolder, outputIndexFile, outputTocFile, outputApiFolder, BuildMetaHelper.MetadataType.Yaml).Wait();
                if (target.HasFlag(BuildTarget.Markdown))
                    BuildMetaHelper.GenerateIndexForMarkdownListAsync(outputFolder, outputIndexFile, inputMarkdownList, outputMarkdownIndexFile, outputMarkdownFolder, outputReferenceFolder).Wait();
                return 0;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Failing in generating metadata from {0}: {1}", inputProjectList, e);
                return 1;
            }
        }
    }
}
