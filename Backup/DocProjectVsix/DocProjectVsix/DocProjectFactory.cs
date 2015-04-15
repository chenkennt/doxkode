using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Project;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace MicrosoftIT.DocProject.Templates.Projects.DocProject
{
    [Guid(GuidList.guidDocProjectFactoryString)]
    class DocProjectFactory : ProjectFactory
    {
        private DocProjectPackage package;

        public DocProjectFactory(DocProjectPackage package)
            : base(package)
        {
            this.package = package;
        }

        protected override ProjectNode CreateProject()
        {
            DocProjectNode project = new DocProjectNode(this.package);

            project.SetSite((IOleServiceProvider)((IServiceProvider)this.package).GetService(typeof(IOleServiceProvider)));
            return project;
        }

    }
}
