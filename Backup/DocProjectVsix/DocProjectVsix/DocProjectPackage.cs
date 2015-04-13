using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using MicrosoftIT.DocProject.Templates.Projects.DocProject;
using Microsoft.VisualStudio.Project;
using EnvDTE;

namespace MicrosoftIT.DocProject
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the information needed to show this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideProjectFactory(typeof(DocProjectFactory),null,
    "Doc Project Files (*.docproj);*.docproj", "docproj", "docproj",
    ".\\NullPath", LanguageVsTemplate = "DocProject")]
    [Guid(GuidList.guidDocProjectPkgString)]
    [ProvideObject(typeof(GeneralPropertyPage))]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class DocProjectPackage : ProjectPackage
    {
        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public DocProjectPackage()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }

        /////////////////////////////////////////////////////////////////////////////
        // Overridden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Debug.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();
            this.RegisterProjectFactory(new DocProjectFactory(this));

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                // Create the 'publish' command for the menu item.
                CommandID publishToGithubMenuCommandID = new CommandID(GuidList.guidDocProjectCmdSet, (int)PkgCmdIDList.PublishToGithubCommand);
                MenuCommand publishToGithubMenuItem = new MenuCommand(PublishToGithubMenuItemCallback, publishToGithubMenuCommandID);
                mcs.AddCommand(publishToGithubMenuItem);
            }
        }

        public override string ProductUserContext
        {
            get { return ""; }
        }

        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void PublishToGithubMenuItemCallback(object sender, EventArgs e)
        {
            //Generate the htmls
            DTE dte = (DTE)GetService(typeof(DTE));
            Project activeProject = null;
            Array selectedProjects = (Array)dte.ActiveSolutionProjects;
            if (selectedProjects.Length == 1)
            {
                activeProject = (Project)selectedProjects.GetValue(0);
                if (activeProject.FileName.EndsWith(".docproj"))
                {
                    System.Threading.Tasks.Task.Run(() => {
                        PublishToGithubOperation.operate(activeProject);
                    });

                    //PublishToGithubOperation.operate(activeProject);
                }
            }
        }
        #endregion

    }
}
