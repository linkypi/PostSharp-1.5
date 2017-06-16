using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace PostSharp.CustomTasks
{
    public sealed class AdjustSampleProjects : Task
    {
        private static readonly List<string> correctedDependencies = new List<string>(new string[]
                                                                                          {
                                                                                              "postsharp.core",
                                                                                              "postsharp.public",
                                                                                              "postsharp.laos",
                                                                                              "postsharp.laos.weaver",
                                                                                              "postsharp.public.cf",
                                                                                              "postsharp.laos.cf",
                                                                                              "postsharp.public.sl",
                                                                                              "postsharp.laos.sl"
                                                                                          });

        private ITaskItem[] projects;


        public ITaskItem[] Projects
        {
            get { return projects; }
            set { projects = value; }
        }

        public override bool Execute()
        {
            foreach (ITaskItem project in this.projects)
            {
                Modify(project.ItemSpec);
            }

            return true;
        }

        private void Modify(string file)
        {
            const string msbuildNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";

#if !DEBUG
            this.Log.LogMessage("Adjusting the reference in {0}.", file);
#endif

            XmlDocument document = new XmlDocument();
            XmlNamespaceManager nsManager = new XmlNamespaceManager(document.NameTable);
            nsManager.AddNamespace("m", msbuildNamespace);
            document.Load(file);

            // Replace <ProjectReference> by <Reference> when appropriate.
            int count = 0;
            foreach (XmlElement oldReferenceNode in
                document.SelectNodes("/m:Project/m:ItemGroup/m:ProjectReference | /m:Project/m:ItemGroup/m:Reference",
                                     nsManager))
            {
                string include = oldReferenceNode.GetAttribute("Include");

#if !DEBUG
            this.Log.LogMessage("Considering the reference {0} '{1}'", oldReferenceNode.Name, include);
#endif

                string project;

                if (include.Contains("\\"))
                {
                    project = Path.GetFileNameWithoutExtension(include);
                }
                else
                {
                    project = new AssemblyName(include).Name;
                }
          

                if (correctedDependencies.Contains(project.ToLowerInvariant()))
                {
                    count++;
#if !DEBUG
                    this.Log.LogMessage("Adjusting the reference '{0}'.", project);
#endif

                    XmlElement newReferenceNode = document.CreateElement("Reference", msbuildNamespace);
                    newReferenceNode.SetAttribute("Include", project);
                    oldReferenceNode.ParentNode.ReplaceChild(newReferenceNode, oldReferenceNode);

                    // Add a hint path. Use the environment variable.
                    XmlElement hintNode = document.CreateElement("HintPath", msbuildNamespace);
                    hintNode.AppendChild(document.CreateTextNode("$(POSTSHARP15)\\" + project + ".dll"));
                    newReferenceNode.AppendChild(hintNode);
                }
            }

            /*
            // Set the PostSharpCoreDirectory property.
            foreach (XmlElement postSharpDirectoryNode in document.SelectNodes("/m:Project/m:PropertyGroup/m:PostSharpCoreDirectory", nsManager))
            {
                this.Log.LogMessage("Adjusting the PostSharpCoreDirectory property.");
                postSharpDirectoryNode.RemoveAll();
                postSharpDirectoryNode.AppendChild(document.CreateTextNode(basePath));
                count++;
            }
             */

            // Set the PostSharpDeployed property.
            foreach (
                XmlElement postSharpDirectoryNode in
                    document.SelectNodes("/m:Project/m:PropertyGroup/m:PostSharpDeployed", nsManager))
            {
#if !DEBUG
                this.Log.LogMessage("Adjusting the PostSharpDeployed property.");
#endif
                postSharpDirectoryNode.RemoveAll();
                postSharpDirectoryNode.AppendChild(document.CreateTextNode("True"));
                count++;
            }

            if (count > 0)
            {
#if !DEBUG
                this.Log.LogMessage("Saving back {0}. {1} correction(s) were made.", file, count);
#endif
                document.Save(file);
            }
            else
            {
#if !DEBUG
                this.Log.LogMessage("No reference corrected in {0}.", file);
#endif
            }
        }
    }
}