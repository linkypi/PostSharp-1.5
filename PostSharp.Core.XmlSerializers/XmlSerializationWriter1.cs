using PostSharp.Extensibility.Configuration;
using System;
using System.Collections;
using System.Globalization;
using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.Xml.Serialization.GeneratedAssembly
{
    public class XmlSerializationWriter1 : XmlSerializationWriter
    {
        public void Write21_Configuration(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteNullTagLiteral("Configuration", "http://schemas.postsharp.org/1.0/configuration");
                return;
            }
            base.TopLevelElement();
            this.Write18_ApplicationConfiguration("Configuration", "http://schemas.postsharp.org/1.0/configuration", (ApplicationConfiguration)o, true, false);
        }

        public void Write22_PlugIn(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteNullTagLiteral("PlugIn", "http://schemas.postsharp.org/1.0/configuration");
                return;
            }
            base.TopLevelElement();
            this.Write19_PlugInConfiguration("PlugIn", "http://schemas.postsharp.org/1.0/configuration", (PlugInConfiguration)o, true, false);
        }

        public void Write23_Project(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteNullTagLiteral("Project", "http://schemas.postsharp.org/1.0/configuration");
                return;
            }
            base.TopLevelElement();
            this.Write20_ProjectConfiguration("Project", "http://schemas.postsharp.org/1.0/configuration", (ProjectConfiguration)o, true, false);
        }

        private void Write20_ProjectConfiguration(string n, string ns, ProjectConfiguration o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
                return;
            }
            if (!needType)
            {
                Type type = o.GetType();
                if (type != typeof(ProjectConfiguration))
                {
                    throw base.CreateUnknownTypeException(o);
                }
            }
            base.WriteStartElement(n, ns, o, false, null);
            if (needType)
            {
                base.WriteXsiType("ProjectConfiguration", "http://schemas.postsharp.org/1.0/configuration");
            }
            base.WriteAttribute("ReferenceDirectory", "", o.ReferenceDirectory);
            SearchPathConfigurationCollection searchPath = o.SearchPath;
            if (searchPath != null)
            {
                IEnumerator enumerator = searchPath.GetEnumerator();
                if (enumerator != null)
                {
                    while (enumerator.MoveNext())
                    {
                        SearchPathConfiguration o2 = (SearchPathConfiguration)enumerator.Current;
                        this.Write3_SearchPathConfiguration("SearchPath", "http://schemas.postsharp.org/1.0/configuration", o2, false, false);
                    }
                }
            }
            UsingConfigurationCollection usedPlugIns = o.UsedPlugIns;
            if (usedPlugIns != null)
            {
                IEnumerator enumerator2 = usedPlugIns.GetEnumerator();
                if (enumerator2 != null)
                {
                    while (enumerator2.MoveNext())
                    {
                        UsingConfiguration o3 = (UsingConfiguration)enumerator2.Current;
                        this.Write4_UsingConfiguration("Using", "http://schemas.postsharp.org/1.0/configuration", o3, false, false);
                    }
                }
            }
            TaskTypeConfigurationCollection taskTypes = o.TaskTypes;
            if (taskTypes != null)
            {
                for (int i = 0; i < ((ICollection)taskTypes).Count; i++)
                {
                    this.Write8_TaskTypeConfiguration("TaskType", "http://schemas.postsharp.org/1.0/configuration", taskTypes[i], false, false);
                }
            }
            PlatformConfigurationCollection platforms = o.Platforms;
            if (platforms != null)
            {
                IEnumerator enumerator3 = platforms.GetEnumerator();
                if (enumerator3 != null)
                {
                    while (enumerator3.MoveNext())
                    {
                        PlatformConfiguration o4 = (PlatformConfiguration)enumerator3.Current;
                        this.Write9_PlatformConfiguration("Platform", "http://schemas.postsharp.org/1.0/configuration", o4, false, false);
                    }
                }
            }
            PropertyConfigurationCollection properties = o.Properties;
            if (properties != null)
            {
                IEnumerator enumerator4 = properties.GetEnumerator();
                if (enumerator4 != null)
                {
                    while (enumerator4.MoveNext())
                    {
                        PropertyConfiguration o5 = (PropertyConfiguration)enumerator4.Current;
                        this.Write10_PropertyConfiguration("Property", "http://schemas.postsharp.org/1.0/configuration", o5, false, false);
                    }
                }
            }
            this.Write15_AssemblyBindingConfiguration("AssemblyBinding", "http://schemas.postsharp.org/1.0/configuration", o.AssemblyBinding, false, false);
            string[] strongNames = o.StrongNames;
            if (strongNames != null)
            {
                for (int j = 0; j < strongNames.Length; j++)
                {
                    base.WriteElementString("StrongName", "http://schemas.postsharp.org/1.0/configuration", strongNames[j]);
                }
            }
            if (o.TasksElement != null || o.TasksElement == null)
            {
                base.WriteElementLiteral(o.TasksElement, "Tasks", "http://schemas.postsharp.org/1.0/configuration", false, true);
                base.WriteEndElement(o);
                return;
            }
            throw base.CreateInvalidAnyTypeException(o.TasksElement);
        }

        private void Write15_AssemblyBindingConfiguration(string n, string ns, AssemblyBindingConfiguration o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
                return;
            }
            if (!needType)
            {
                Type type = o.GetType();
                if (type != typeof(AssemblyBindingConfiguration))
                {
                    throw base.CreateUnknownTypeException(o);
                }
            }
            base.WriteStartElement(n, ns, o, false, null);
            if (needType)
            {
                base.WriteXsiType(null, "http://schemas.postsharp.org/1.0/configuration");
            }
            DependentAssemblyConfigurationCollection dependentAssemblies = o.DependentAssemblies;
            if (dependentAssemblies != null)
            {
                for (int i = 0; i < ((ICollection)dependentAssemblies).Count; i++)
                {
                    this.Write13_DependentAssemblyConfiguration("dependentAssembly", "urn:schemas-microsoft-com:asm.v1", dependentAssemblies[i], false, false);
                }
            }
            ImportAssemblyBindingsConfigurationCollection importAssemblyBindings = o.ImportAssemblyBindings;
            if (importAssemblyBindings != null)
            {
                for (int j = 0; j < ((ICollection)importAssemblyBindings).Count; j++)
                {
                    this.Write14_Item("Import", "http://schemas.postsharp.org/1.0/configuration", importAssemblyBindings[j], false, false);
                }
            }
            base.WriteEndElement(o);
        }

        private void Write14_Item(string n, string ns, ImportAssemblyBindingsConfiguration o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
                return;
            }
            if (!needType)
            {
                Type type = o.GetType();
                if (type != typeof(ImportAssemblyBindingsConfiguration))
                {
                    throw base.CreateUnknownTypeException(o);
                }
            }
            base.WriteStartElement(n, ns, o, false, null);
            if (needType)
            {
                base.WriteXsiType(null, "http://schemas.postsharp.org/1.0/configuration");
            }
            base.WriteAttribute("File", "", o.File);
            base.WriteAttribute("Select", "", o.Select);
            base.WriteEndElement(o);
        }

        private void Write13_DependentAssemblyConfiguration(string n, string ns, DependentAssemblyConfiguration o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
                return;
            }
            if (!needType)
            {
                Type type = o.GetType();
                if (type != typeof(DependentAssemblyConfiguration))
                {
                    throw base.CreateUnknownTypeException(o);
                }
            }
            base.WriteStartElement(n, ns, o, false, null);
            if (needType)
            {
                base.WriteXsiType(null, "urn:schemas-microsoft-com:asm.v1");
            }
            this.Write11_AssemblyIdentityConfiguration("assemblyIdentity", "urn:schemas-microsoft-com:asm.v1", o.AssemblyIdentity, false, false);
            this.Write12_BindingRedirectConfiguration("bindingRedirect", "urn:schemas-microsoft-com:asm.v1", o.BindingRedirect, false, false);
            base.WriteEndElement(o);
        }

        private void Write12_BindingRedirectConfiguration(string n, string ns, BindingRedirectConfiguration o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
                return;
            }
            if (!needType)
            {
                Type type = o.GetType();
                if (type != typeof(BindingRedirectConfiguration))
                {
                    throw base.CreateUnknownTypeException(o);
                }
            }
            base.WriteStartElement(n, ns, o, false, null);
            if (needType)
            {
                base.WriteXsiType(null, "urn:schemas-microsoft-com:asm.v1");
            }
            base.WriteAttribute("oldVersion", "", o.OldVersion);
            base.WriteAttribute("newVersion", "", o.NewVersion);
            base.WriteAttribute("newPublicKeyToken", "", o.NewPublicKeyToken);
            base.WriteAttribute("newName", "", o.NewName);
            base.WriteEndElement(o);
        }

        private void Write11_AssemblyIdentityConfiguration(string n, string ns, AssemblyIdentityConfiguration o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
                return;
            }
            if (!needType)
            {
                Type type = o.GetType();
                if (type != typeof(AssemblyIdentityConfiguration))
                {
                    throw base.CreateUnknownTypeException(o);
                }
            }
            base.WriteStartElement(n, ns, o, false, null);
            if (needType)
            {
                base.WriteXsiType(null, "urn:schemas-microsoft-com:asm.v1");
            }
            base.WriteAttribute("name", "", o.Name);
            base.WriteAttribute("publicKeyToken", "", o.PublicKeyToken);
            base.WriteAttribute("culture", "", o.Culture);
            base.WriteEndElement(o);
        }

        private void Write10_PropertyConfiguration(string n, string ns, PropertyConfiguration o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
                return;
            }
            if (!needType)
            {
                Type type = o.GetType();
                if (type != typeof(PropertyConfiguration))
                {
                    throw base.CreateUnknownTypeException(o);
                }
            }
            base.WriteStartElement(n, ns, o, false, null);
            if (needType)
            {
                base.WriteXsiType(null, "http://schemas.postsharp.org/1.0/configuration");
            }
            base.WriteAttribute("Value", "", o.Value);
            base.WriteAttribute("Name", "", o.Name);
            base.WriteEndElement(o);
        }

        private void Write9_PlatformConfiguration(string n, string ns, PlatformConfiguration o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
                return;
            }
            if (!needType)
            {
                Type type = o.GetType();
                if (type != typeof(PlatformConfiguration))
                {
                    throw base.CreateUnknownTypeException(o);
                }
            }
            base.WriteStartElement(n, ns, o, false, null);
            if (needType)
            {
                base.WriteXsiType(null, "http://schemas.postsharp.org/1.0/configuration");
            }
            base.WriteAttribute("Implementation", "", o.Implementation);
            base.WriteAttribute("Name", "", o.Name);
            NameValuePairCollection parameters = o.Parameters;
            if (parameters != null)
            {
                base.WriteStartElement("Parameters", "http://schemas.postsharp.org/1.0/configuration", null, false);
                IEnumerator enumerator = parameters.GetEnumerator();
                if (enumerator != null)
                {
                    while (enumerator.MoveNext())
                    {
                        NameValuePair o2 = (NameValuePair)enumerator.Current;
                        this.Write7_NameValuePair("Parameter", "http://schemas.postsharp.org/1.0/configuration", o2, true, false);
                    }
                }
                base.WriteEndElement();
            }
            base.WriteEndElement(o);
        }

        private void Write7_NameValuePair(string n, string ns, NameValuePair o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
                return;
            }
            if (!needType)
            {
                Type type = o.GetType();
                if (type != typeof(NameValuePair))
                {
                    throw base.CreateUnknownTypeException(o);
                }
            }
            base.WriteStartElement(n, ns, o, false, null);
            if (needType)
            {
                base.WriteXsiType(null, "http://schemas.postsharp.org/1.0/configuration");
            }
            base.WriteAttribute("Value", "", o.Value);
            base.WriteAttribute("Name", "", o.Name);
            base.WriteEndElement(o);
        }

        private void Write8_TaskTypeConfiguration(string n, string ns, TaskTypeConfiguration o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
                return;
            }
            if (!needType)
            {
                Type type = o.GetType();
                if (type != typeof(TaskTypeConfiguration))
                {
                    throw base.CreateUnknownTypeException(o);
                }
            }
            base.WriteStartElement(n, ns, o, false, null);
            if (needType)
            {
                base.WriteXsiType(null, "http://schemas.postsharp.org/1.0/configuration");
            }
            base.WriteAttribute("Phase", "", o.Phase);
            base.WriteAttribute("Name", "", o.Name);
            base.WriteAttribute("Implementation", "", o.Implementation);
            DependencyConfigurationCollection dependencies = o.Dependencies;
            if (dependencies != null)
            {
                IEnumerator enumerator = dependencies.GetEnumerator();
                if (enumerator != null)
                {
                    while (enumerator.MoveNext())
                    {
                        DependencyConfiguration o2 = (DependencyConfiguration)enumerator.Current;
                        this.Write6_DependencyConfiguration("Dependency", "http://schemas.postsharp.org/1.0/configuration", o2, false, false);
                    }
                }
            }
            NameValuePairCollection parameters = o.Parameters;
            if (parameters != null)
            {
                base.WriteStartElement("Parameters", "http://schemas.postsharp.org/1.0/configuration", null, false);
                IEnumerator enumerator2 = parameters.GetEnumerator();
                if (enumerator2 != null)
                {
                    while (enumerator2.MoveNext())
                    {
                        NameValuePair o3 = (NameValuePair)enumerator2.Current;
                        this.Write7_NameValuePair("Parameter", "http://schemas.postsharp.org/1.0/configuration", o3, true, false);
                    }
                }
                base.WriteEndElement();
            }
            base.WriteEndElement(o);
        }

        private void Write6_DependencyConfiguration(string n, string ns, DependencyConfiguration o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
                return;
            }
            if (!needType)
            {
                Type type = o.GetType();
                if (type != typeof(DependencyConfiguration))
                {
                    throw base.CreateUnknownTypeException(o);
                }
            }
            base.WriteStartElement(n, ns, o, false, null);
            if (needType)
            {
                base.WriteXsiType(null, "http://schemas.postsharp.org/1.0/configuration");
            }
            if (!o.Required)
            {
                base.WriteAttribute("Required", "", XmlConvert.ToString(o.Required));
            }
            base.WriteAttribute("TaskType", "", o.TaskType);
            if (o.Position != DependencyPosition.Default)
            {
                base.WriteAttribute("Position", "", this.Write5_DependencyPosition(o.Position));
            }
            base.WriteEndElement(o);
        }

        private string Write5_DependencyPosition(DependencyPosition v)
        {
            string result;
            switch (v)
            {
                case DependencyPosition.Default:
                    result = "Default";
                    break;
                case DependencyPosition.After:
                    result = "After";
                    break;
                default:
                    throw base.CreateInvalidEnumValueException(((long)v).ToString(CultureInfo.InvariantCulture), "PostSharp.Extensibility.Configuration.DependencyPosition");
            }
            return result;
        }

        private void Write4_UsingConfiguration(string n, string ns, UsingConfiguration o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
                return;
            }
            if (!needType)
            {
                Type type = o.GetType();
                if (type != typeof(UsingConfiguration))
                {
                    throw base.CreateUnknownTypeException(o);
                }
            }
            base.WriteStartElement(n, ns, o, false, null);
            if (needType)
            {
                base.WriteXsiType(null, "http://schemas.postsharp.org/1.0/configuration");
            }
            base.WriteAttribute("PlugInFile", "", o.PlugInFile);
            SearchPathConfigurationCollection searchPath = o.SearchPath;
            if (searchPath != null)
            {
                IEnumerator enumerator = searchPath.GetEnumerator();
                if (enumerator != null)
                {
                    while (enumerator.MoveNext())
                    {
                        SearchPathConfiguration o2 = (SearchPathConfiguration)enumerator.Current;
                        this.Write3_SearchPathConfiguration("SearchPath", "http://schemas.postsharp.org/1.0/configuration", o2, false, false);
                    }
                }
            }
            UsingConfigurationCollection usedPlugIns = o.UsedPlugIns;
            if (usedPlugIns != null)
            {
                IEnumerator enumerator2 = usedPlugIns.GetEnumerator();
                if (enumerator2 != null)
                {
                    while (enumerator2.MoveNext())
                    {
                        UsingConfiguration o3 = (UsingConfiguration)enumerator2.Current;
                        this.Write4_UsingConfiguration("Using", "http://schemas.postsharp.org/1.0/configuration", o3, false, false);
                    }
                }
            }
            TaskTypeConfigurationCollection taskTypes = o.TaskTypes;
            if (taskTypes != null)
            {
                for (int i = 0; i < ((ICollection)taskTypes).Count; i++)
                {
                    this.Write8_TaskTypeConfiguration("TaskType", "http://schemas.postsharp.org/1.0/configuration", taskTypes[i], false, false);
                }
            }
            PlatformConfigurationCollection platforms = o.Platforms;
            if (platforms != null)
            {
                IEnumerator enumerator3 = platforms.GetEnumerator();
                if (enumerator3 != null)
                {
                    while (enumerator3.MoveNext())
                    {
                        PlatformConfiguration o4 = (PlatformConfiguration)enumerator3.Current;
                        this.Write9_PlatformConfiguration("Platform", "http://schemas.postsharp.org/1.0/configuration", o4, false, false);
                    }
                }
            }
            PropertyConfigurationCollection properties = o.Properties;
            if (properties != null)
            {
                IEnumerator enumerator4 = properties.GetEnumerator();
                if (enumerator4 != null)
                {
                    while (enumerator4.MoveNext())
                    {
                        PropertyConfiguration o5 = (PropertyConfiguration)enumerator4.Current;
                        this.Write10_PropertyConfiguration("Property", "http://schemas.postsharp.org/1.0/configuration", o5, false, false);
                    }
                }
            }
            this.Write15_AssemblyBindingConfiguration("AssemblyBinding", "http://schemas.postsharp.org/1.0/configuration", o.AssemblyBinding, false, false);
            string[] strongNames = o.StrongNames;
            if (strongNames != null)
            {
                for (int j = 0; j < strongNames.Length; j++)
                {
                    base.WriteElementString("StrongName", "http://schemas.postsharp.org/1.0/configuration", strongNames[j]);
                }
            }
            base.WriteEndElement(o);
        }

        private void Write3_SearchPathConfiguration(string n, string ns, SearchPathConfiguration o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
                return;
            }
            if (!needType)
            {
                Type type = o.GetType();
                if (type != typeof(SearchPathConfiguration))
                {
                    throw base.CreateUnknownTypeException(o);
                }
            }
            base.WriteStartElement(n, ns, o, false, null);
            if (needType)
            {
                base.WriteXsiType(null, "http://schemas.postsharp.org/1.0/configuration");
            }
            base.WriteAttribute("Directory", "", o.Directory);
            base.WriteAttribute("File", "", o.File);
            base.WriteEndElement(o);
        }

        private void Write19_PlugInConfiguration(string n, string ns, PlugInConfiguration o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
                return;
            }
            if (!needType)
            {
                Type type = o.GetType();
                if (type != typeof(PlugInConfiguration))
                {
                    throw base.CreateUnknownTypeException(o);
                }
            }
            base.WriteStartElement(n, ns, o, false, null);
            if (needType)
            {
                base.WriteXsiType("PlugInConfiguration", "http://schemas.postsharp.org/1.0/configuration");
            }
            SearchPathConfigurationCollection searchPath = o.SearchPath;
            if (searchPath != null)
            {
                IEnumerator enumerator = searchPath.GetEnumerator();
                if (enumerator != null)
                {
                    while (enumerator.MoveNext())
                    {
                        SearchPathConfiguration o2 = (SearchPathConfiguration)enumerator.Current;
                        this.Write3_SearchPathConfiguration("SearchPath", "http://schemas.postsharp.org/1.0/configuration", o2, false, false);
                    }
                }
            }
            UsingConfigurationCollection usedPlugIns = o.UsedPlugIns;
            if (usedPlugIns != null)
            {
                IEnumerator enumerator2 = usedPlugIns.GetEnumerator();
                if (enumerator2 != null)
                {
                    while (enumerator2.MoveNext())
                    {
                        UsingConfiguration o3 = (UsingConfiguration)enumerator2.Current;
                        this.Write4_UsingConfiguration("Using", "http://schemas.postsharp.org/1.0/configuration", o3, false, false);
                    }
                }
            }
            TaskTypeConfigurationCollection taskTypes = o.TaskTypes;
            if (taskTypes != null)
            {
                for (int i = 0; i < ((ICollection)taskTypes).Count; i++)
                {
                    this.Write8_TaskTypeConfiguration("TaskType", "http://schemas.postsharp.org/1.0/configuration", taskTypes[i], false, false);
                }
            }
            PlatformConfigurationCollection platforms = o.Platforms;
            if (platforms != null)
            {
                IEnumerator enumerator3 = platforms.GetEnumerator();
                if (enumerator3 != null)
                {
                    while (enumerator3.MoveNext())
                    {
                        PlatformConfiguration o4 = (PlatformConfiguration)enumerator3.Current;
                        this.Write9_PlatformConfiguration("Platform", "http://schemas.postsharp.org/1.0/configuration", o4, false, false);
                    }
                }
            }
            PropertyConfigurationCollection properties = o.Properties;
            if (properties != null)
            {
                IEnumerator enumerator4 = properties.GetEnumerator();
                if (enumerator4 != null)
                {
                    while (enumerator4.MoveNext())
                    {
                        PropertyConfiguration o5 = (PropertyConfiguration)enumerator4.Current;
                        this.Write10_PropertyConfiguration("Property", "http://schemas.postsharp.org/1.0/configuration", o5, false, false);
                    }
                }
            }
            this.Write15_AssemblyBindingConfiguration("AssemblyBinding", "http://schemas.postsharp.org/1.0/configuration", o.AssemblyBinding, false, false);
            string[] strongNames = o.StrongNames;
            if (strongNames != null)
            {
                for (int j = 0; j < strongNames.Length; j++)
                {
                    base.WriteElementString("StrongName", "http://schemas.postsharp.org/1.0/configuration", strongNames[j]);
                }
            }
            base.WriteEndElement(o);
        }

        private void Write18_ApplicationConfiguration(string n, string ns, ApplicationConfiguration o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
                return;
            }
            if (!needType)
            {
                Type type = o.GetType();
                if (type != typeof(ApplicationConfiguration))
                {
                    throw base.CreateUnknownTypeException(o);
                }
            }
            base.WriteStartElement(n, ns, o, false, null);
            if (needType)
            {
                base.WriteXsiType("ApplicationConfiguration", "http://schemas.postsharp.org/1.0/configuration");
            }
            SearchPathConfigurationCollection searchPath = o.SearchPath;
            if (searchPath != null)
            {
                IEnumerator enumerator = searchPath.GetEnumerator();
                if (enumerator != null)
                {
                    while (enumerator.MoveNext())
                    {
                        SearchPathConfiguration o2 = (SearchPathConfiguration)enumerator.Current;
                        this.Write3_SearchPathConfiguration("SearchPath", "http://schemas.postsharp.org/1.0/configuration", o2, false, false);
                    }
                }
            }
            UsingConfigurationCollection usedPlugIns = o.UsedPlugIns;
            if (usedPlugIns != null)
            {
                IEnumerator enumerator2 = usedPlugIns.GetEnumerator();
                if (enumerator2 != null)
                {
                    while (enumerator2.MoveNext())
                    {
                        UsingConfiguration o3 = (UsingConfiguration)enumerator2.Current;
                        this.Write4_UsingConfiguration("Using", "http://schemas.postsharp.org/1.0/configuration", o3, false, false);
                    }
                }
            }
            TaskTypeConfigurationCollection taskTypes = o.TaskTypes;
            if (taskTypes != null)
            {
                for (int i = 0; i < ((ICollection)taskTypes).Count; i++)
                {
                    this.Write8_TaskTypeConfiguration("TaskType", "http://schemas.postsharp.org/1.0/configuration", taskTypes[i], false, false);
                }
            }
            PlatformConfigurationCollection platforms = o.Platforms;
            if (platforms != null)
            {
                IEnumerator enumerator3 = platforms.GetEnumerator();
                if (enumerator3 != null)
                {
                    while (enumerator3.MoveNext())
                    {
                        PlatformConfiguration o4 = (PlatformConfiguration)enumerator3.Current;
                        this.Write9_PlatformConfiguration("Platform", "http://schemas.postsharp.org/1.0/configuration", o4, false, false);
                    }
                }
            }
            PropertyConfigurationCollection properties = o.Properties;
            if (properties != null)
            {
                IEnumerator enumerator4 = properties.GetEnumerator();
                if (enumerator4 != null)
                {
                    while (enumerator4.MoveNext())
                    {
                        PropertyConfiguration o5 = (PropertyConfiguration)enumerator4.Current;
                        this.Write10_PropertyConfiguration("Property", "http://schemas.postsharp.org/1.0/configuration", o5, false, false);
                    }
                }
            }
            this.Write15_AssemblyBindingConfiguration("AssemblyBinding", "http://schemas.postsharp.org/1.0/configuration", o.AssemblyBinding, false, false);
            string[] strongNames = o.StrongNames;
            if (strongNames != null)
            {
                for (int j = 0; j < strongNames.Length; j++)
                {
                    base.WriteElementString("StrongName", "http://schemas.postsharp.org/1.0/configuration", strongNames[j]);
                }
            }
            PhaseConfigurationCollection phases = o.Phases;
            if (phases != null)
            {
                base.WriteStartElement("Phases", "http://schemas.postsharp.org/1.0/configuration", null, false);
                IEnumerator enumerator5 = phases.GetEnumerator();
                if (enumerator5 != null)
                {
                    while (enumerator5.MoveNext())
                    {
                        PhaseConfiguration o6 = (PhaseConfiguration)enumerator5.Current;
                        this.Write17_PhaseConfiguration("Phase", "http://schemas.postsharp.org/1.0/configuration", o6, true, false);
                    }
                }
                base.WriteEndElement();
            }
            base.WriteEndElement(o);
        }

        private void Write17_PhaseConfiguration(string n, string ns, PhaseConfiguration o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
                return;
            }
            if (!needType)
            {
                Type type = o.GetType();
                if (type != typeof(PhaseConfiguration))
                {
                    throw base.CreateUnknownTypeException(o);
                }
            }
            base.WriteStartElement(n, ns, o, false, null);
            if (needType)
            {
                base.WriteXsiType(null, "http://schemas.postsharp.org/1.0/configuration");
            }
            base.WriteAttribute("Ordinal", "", XmlConvert.ToString(o.Ordinal));
            base.WriteAttribute("Name", "", o.Name);
            base.WriteEndElement(o);
        }

        protected override void InitCallbacks()
        {
        }
    }
}
