using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.Xml;
using System.IO;
using System.Text.RegularExpressions;

namespace PostSharp.CustomTasks
{
    public class GenerateWixTree : Task
    {
        private const string wixNamespace = "http://schemas.microsoft.com/wix/2003/01/wi";
        private string structureFile;
        private string wixDirectoryFile;
        private ITaskItem[] files;
        private Dictionary<string, string> indexedFiles;
        private XmlDocument wixDirectoryDocument;
        private XmlDocument wixFeatureDocument;
        private XmlElement wixFeatureRoot;
        private string outputWixDirectory;
        private string filesBaseDirectory;
        private string wixFeatureFile;
        private  static readonly Regex identifierRegex = new Regex("[^a-zA-Z0-9]");

        public GenerateWixTree()
        {
        }

        public string WixFeatureFile
        {
            get { return wixFeatureFile; }
            set { wixFeatureFile = value; }
        }
	
        public string FilesBaseDirectory
        {
            get { return filesBaseDirectory; }
            set { filesBaseDirectory = value; }
        }
	

        public string StructureFile
        {
            get { return structureFile; }
            set { structureFile = value; }
        }

        public string WixDirectoryFile
        {
            get { return wixDirectoryFile; }
            set { wixDirectoryFile = value; }
        }

        public ITaskItem[] Files
        {
            get { return files; }
            set { files = value; }
        }

     
	    public string OutputWixDirectory
	    {
		    get { return outputWixDirectory;}
		    set { outputWixDirectory = value;}
	    }
	

        public override bool Execute()
        {
            // Open the XML documents.
            XmlDocument structureDocument = new XmlDocument();
            structureDocument.Load(this.structureFile);

            this.wixDirectoryDocument = new XmlDocument();
            XmlElement wixDirectoryRoot = this.wixDirectoryDocument.CreateElement("Include", wixNamespace);
            this.wixDirectoryDocument.AppendChild(wixDirectoryRoot);


            this.wixFeatureDocument = new XmlDocument();
            this.wixFeatureRoot = this.wixFeatureDocument.CreateElement("Include", wixNamespace);
            this.wixFeatureDocument.AppendChild(wixFeatureRoot);


            // Canonize input files and index them.
            this.indexedFiles = new Dictionary<string, string>(this.files.Length, StringComparer.InvariantCultureIgnoreCase);
            foreach (ITaskItem file in this.files)
            {
                this.indexedFiles.Add(Path.GetFullPath(file.ItemSpec), file.ItemSpec);
            }

            // Process the root directory
            this.ProcessDirectory(structureDocument.DocumentElement, wixDirectoryRoot, ".", ".");

            using (XmlTextWriter writer = new XmlTextWriter(this.wixDirectoryFile, Encoding.UTF8))
            {
                writer.Formatting = Formatting.Indented;
                this.wixDirectoryDocument.Save(writer);
            }


            using (XmlTextWriter writer = new XmlTextWriter(this.wixFeatureFile, Encoding.UTF8))
            {
                writer.Formatting = Formatting.Indented;
                this.wixFeatureDocument.Save(writer);
            }


            return true;


        }

        private void ProcessDirectory(XmlElement input, XmlElement outputParent, string sourceParentRelativeDirectory, string targetParentRelativeDirectory )
        {
            string directoryLongName = input.GetAttribute("LongName");
            string directoryGuid = input.GetAttribute("Guid");
          
            string sourceRelativeDirectory = Path.Combine( sourceParentRelativeDirectory, directoryLongName );
            string targetRelativeDirectory = Path.Combine(sourceParentRelativeDirectory, directoryLongName);
#if !DEBUG
            this.Log.LogMessage("Processing directory {0}.", sourceRelativeDirectory); 
#endif

            // Create the directory element.
            XmlElement wixDirectory = wixDirectoryDocument.CreateElement("Directory", wixNamespace);
            outputParent.AppendChild(wixDirectory);
            wixDirectory.SetAttribute("Id", FormatIdentifier( directoryGuid ));
            wixDirectory.SetAttribute("Name", directoryLongName);

            // Create the component element.
            XmlElement wixComponent = wixDirectoryDocument.CreateElement("Component", wixNamespace);
            wixDirectory.AppendChild(wixComponent);
            wixComponent.SetAttribute("Id", FormatIdentifier(directoryGuid));
            wixComponent.SetAttribute("Guid", directoryGuid);
            wixComponent.SetAttribute("KeyPath", "yes");

        
            // Create the create/remove folder elements.
            XmlElement element = wixDirectoryDocument.CreateElement("CreateFolder", wixNamespace);
            wixComponent.AppendChild(element);

            // Create the permission element under the folder element
            foreach (XmlElement permission in input.SelectNodes("*[local-name(.)='Permission']"))
            {
#if !DEBUG
                this.Log.LogMessage("Adding permission element");
#endif
                element.AppendChild(wixDirectoryDocument.ImportNode(permission, true));
            }

            /*   element = wixDirectoryDocument.CreateElement("RemoveFolder", wixNamespace);
               wixComponent.AppendChild(element);
               element.SetAttribute("Id", FormatIdentifier(directoryGuid));
               element.SetAttribute( "On", "uninstall" );
            */

            // Reference the component in the feature.
            element = wixFeatureDocument.CreateElement("ComponentRef", wixNamespace);
            this.wixFeatureRoot.AppendChild(element);
            element.SetAttribute("Id", FormatIdentifier(directoryGuid));

            // Create the target directory
            string sourceDirectory = Path.Combine(this.filesBaseDirectory, sourceRelativeDirectory);
            string targetDirectory = Path.Combine(this.outputWixDirectory, targetRelativeDirectory);
            if (!Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }

            // Process files.
            foreach (string relativeFile in Directory.GetFiles(sourceDirectory))
            {
                string file = Path.GetFileName(relativeFile);
                string canonicalFile = Path.GetFullPath(Path.Combine(sourceDirectory, file));
                if (this.indexedFiles.ContainsKey(canonicalFile))
                {
                    element = wixDirectoryDocument.CreateElement("File", wixNamespace);
                    wixComponent.AppendChild(element);
                    element.SetAttribute("Id", FormatIdentifier( directoryGuid + "_" + file.ToLowerInvariant().GetHashCode().ToString("X") ));
                    //element.SetAttribute("Name", file.GetHashCode().ToString("X"));
                    element.SetAttribute("Name", file);
                    element.SetAttribute("Vital", "no");
                    element.SetAttribute("DiskId", "1");

                    // Copy the file
                    File.Copy( canonicalFile, Path.Combine( targetDirectory, file ), true );
                }
            }

            // Process subdirectories
            foreach (XmlElement directoryElement in input.SelectNodes("Directory"))
            {
                this.ProcessDirectory(directoryElement, wixDirectory, sourceRelativeDirectory, targetRelativeDirectory);
            }

        }

        private static string FormatIdentifier(string str)
        {
            return "_" + identifierRegex.Replace(str, "_");
        }



    }
}
