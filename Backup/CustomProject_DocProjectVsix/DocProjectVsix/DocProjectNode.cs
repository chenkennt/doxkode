using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Project;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.VisualStudio.OLE.Interop;

namespace MicrosoftIT.DocProject
{
    public class DocProjectNode : ProjectNode
    {
        private DocProjectPackage package;
        private static ImageList imageList;

        static DocProjectNode()
        {
            imageList = Utilities.GetImageList(typeof(DocProjectNode).Assembly.GetManifestResourceStream("MicrosoftIT.DocProject.Resources.DocProjectNode.bmp"));
        }

        public DocProjectNode(DocProjectPackage package)
        {
            this.package = package;

            imageIndex = this.ImageHandler.ImageList.Images.Count;

            foreach (Image img in imageList.Images)
            {
                this.ImageHandler.AddImage(img);
            }
        }

        public override Guid ProjectGuid
        {
            get { return GuidList.guidDocProjectFactory; }
        }

        internal static int imageIndex;
        public override int ImageIndex
        {
            get { return imageIndex; }
        }

        public override string ProjectType
        {
            get { return "DocProjectType"; }
        }

        public override void AddFileFromTemplate(
            string source, string target)
        {
            this.FileTemplateProcessor.UntokenFile(source, target);
            this.FileTemplateProcessor.Reset();
        }

        protected override Guid[] GetConfigurationIndependentPropertyPages()
        {
            Guid[] result = new Guid[1];
            result[0] = typeof(GeneralPropertyPage).GUID;
            return result;
        }

        protected override Guid[] GetPriorityProjectDesignerPages()
        {
            Guid[] result = new Guid[1];
            result[0] = typeof(GeneralPropertyPage).GUID;
            return result;
        }
    }
}