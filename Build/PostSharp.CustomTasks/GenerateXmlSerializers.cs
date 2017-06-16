using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.Xml;
using System.Xml.Serialization;
using System.Reflection;
using System.CodeDom.Compiler;
using System.IO;
using System.Web.Services.Protocols;

namespace PostSharp.CustomTasks
{

    public sealed class GenerateXmlSerializers : AppDomainIsolatedTask
    {
        private string assemblyName;
        private string output;
        private ITaskItem[] types;
        private ITaskItem[] references;
        private bool debug;
        private string compilerOptions;



        public string CompilerOptions
        {
            get { return compilerOptions; }
            set { compilerOptions = value; }
        }

        [Required]
        public string AssemblyName
        {
            get { return assemblyName; }
            set { assemblyName = value; }
        }

        [Required]
        public string Output
        {
            get { return output; }
            set { output = value; }
        }

        [Required]
        public ITaskItem[] Types
        {
            get { return types; }
            set { types = value; }
        }

        public ITaskItem[] References
        {
            get { return references; }
            set { references = value; }
        }

        public bool Debug
        {
            get { return debug; }
            set { debug = value; }
        }

        private static string[] ToStringArray(ITaskItem[] items)
        {
            if (items == null) return null;
            string[] array = new string[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                array[i] = items[i].ItemSpec;
            }

            return array;
        }

        public override bool Execute()
        {
            try
            {
                // Load the input assembly.
                string assemblyFullName = Path.GetFullPath(this.assemblyName);
                if (!File.Exists(assemblyFullName))
                {
                    this.Log.LogError("Assembly {0} not found.", assemblyFullName);
                    return false;
                }

                Assembly sourceAssembly;
                sourceAssembly = Assembly.LoadFrom(assemblyFullName);

                // Get the types.
                XmlReflectionImporter importer = new XmlReflectionImporter();
                ArrayList mappings = new ArrayList();

                Type[] types = new Type[this.types.Length];

                for (int i = 0; i < types.Length; i++)
                {
                    string typeName = this.types[i].ItemSpec;

                    types[i] = sourceAssembly.GetType(typeName, false);
                    if (types[i] == null)
                    {
                        Log.LogError("Cannot find type {0}.", typeName);
                        return false;
                    }

                    importer.IncludeType(types[i]);
                    XmlMapping mapping = importer.ImportTypeMapping(types[i]);
                    if (mapping == null)
                    {
                        Log.LogWarning("Could not get a mapping for type {0}.", typeName);
                    }
                    else
                    {
                        mappings.Add(mapping);
                    }


                }

                HttpGetClientProtocol.GenerateXmlMappings(types, mappings);


                // Get the serialization assembly.
                CompilerParameters compilerParameters = new CompilerParameters();
                compilerParameters.CompilerOptions = this.compilerOptions;
                compilerParameters.GenerateExecutable = false;
                compilerParameters.GenerateInMemory = false;
                compilerParameters.IncludeDebugInformation = this.debug;
                compilerParameters.OutputAssembly = this.output;

                if (this.references != null)
                {
                    foreach (ITaskItem reference in this.references)
                    {
                        compilerParameters.ReferencedAssemblies.Add(reference.ItemSpec);
                    }
                }

                Assembly targetAssembly = XmlSerializer.GenerateSerializer(types, (XmlMapping[])mappings.ToArray(typeof(XmlMapping)), compilerParameters);

                if (targetAssembly == null)
                {
                    this.Log.LogError("The generation failed.");
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                this.Log.LogErrorFromException(e);
                return false;
            }

        }

    }
}
