using PostSharp.Extensibility.Configuration;
using System;
using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.Xml.Serialization.GeneratedAssembly
{
    public class XmlSerializationReader1 : XmlSerializationReader
    {
        private string id20_Select;

        private string id12_Property;

        private string id33_Parameters;

        private string id1_Configuration;

        private string id37_Required;

        private string id23_oldVersion;

        private string id38_Position;

        private string id19_File;

        private string id25_newPublicKeyToken;

        private string id35_Phase;

        private string id14_StrongName;

        private string id42_ApplicationConfiguration;

        private string id22_bindingRedirect;

        private string id6_ReferenceDirectory;

        private string id7_Item;

        private string id11_Platform;

        private string id16_dependentAssembly;

        private string id41_PlugInConfiguration;

        private string id31_Name;

        private string id36_Dependency;

        private string id3_PlugIn;

        private string id10_TaskType;

        private string id40_Directory;

        private string id39_PlugInFile;

        private string id28_publicKeyToken;

        private string id5_ProjectConfiguration;

        private string id9_Using;

        private string id8_SearchPath;

        private string id18_Import;

        private string id4_Project;

        private string id32_Implementation;

        private string id17_Item;

        private string id43_Phases;

        private string id26_newName;

        private string id30_Value;

        private string id2_Item;

        private string id44_Ordinal;

        private string id27_name;

        private string id29_culture;

        private string id21_assemblyIdentity;

        private string id34_Parameter;

        private string id13_AssemblyBinding;

        private string id24_newVersion;

        private string id15_Tasks;

        public object Read21_Configuration()
        {
            object result = null;
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if (base.Reader.LocalName != this.id1_Configuration || base.Reader.NamespaceURI != this.id2_Item)
                {
                    throw base.CreateUnknownNodeException();
                }
                result = this.Read18_ApplicationConfiguration(true, true);
            }
            else
            {
                base.UnknownNode(null, "http://schemas.postsharp.org/1.0/configuration:Configuration");
            }
            return result;
        }

        public object Read22_PlugIn()
        {
            object result = null;
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if (base.Reader.LocalName != this.id3_PlugIn || base.Reader.NamespaceURI != this.id2_Item)
                {
                    throw base.CreateUnknownNodeException();
                }
                result = this.Read19_PlugInConfiguration(true, true);
            }
            else
            {
                base.UnknownNode(null, "http://schemas.postsharp.org/1.0/configuration:PlugIn");
            }
            return result;
        }

        public object Read23_Project()
        {
            object result = null;
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if (base.Reader.LocalName != this.id4_Project || base.Reader.NamespaceURI != this.id2_Item)
                {
                    throw base.CreateUnknownNodeException();
                }
                result = this.Read20_ProjectConfiguration(true, true);
            }
            else
            {
                base.UnknownNode(null, "http://schemas.postsharp.org/1.0/configuration:Project");
            }
            return result;
        }

        private ProjectConfiguration Read20_ProjectConfiguration(bool isNullable, bool checkType)
        {
            XmlQualifiedName xmlQualifiedName = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if (checkType && !(xmlQualifiedName == null) && (xmlQualifiedName.Name != this.id5_ProjectConfiguration || xmlQualifiedName.Namespace != this.id2_Item))
            {
                throw base.CreateUnknownTypeException(xmlQualifiedName);
            }
            if (flag)
            {
                return null;
            }
            ProjectConfiguration projectConfiguration = new ProjectConfiguration();
            if (projectConfiguration.SearchPath == null)
            {
                projectConfiguration.SearchPath = new SearchPathConfigurationCollection();
            }
            SearchPathConfigurationCollection searchPath = projectConfiguration.SearchPath;
            if (projectConfiguration.UsedPlugIns == null)
            {
                projectConfiguration.UsedPlugIns = new UsingConfigurationCollection();
            }
            UsingConfigurationCollection usedPlugIns = projectConfiguration.UsedPlugIns;
            if (projectConfiguration.TaskTypes == null)
            {
                projectConfiguration.TaskTypes = new TaskTypeConfigurationCollection();
            }
            TaskTypeConfigurationCollection taskTypes = projectConfiguration.TaskTypes;
            if (projectConfiguration.Platforms == null)
            {
                projectConfiguration.Platforms = new PlatformConfigurationCollection();
            }
            PlatformConfigurationCollection platforms = projectConfiguration.Platforms;
            if (projectConfiguration.Properties == null)
            {
                projectConfiguration.Properties = new PropertyConfigurationCollection();
            }
            PropertyConfigurationCollection properties = projectConfiguration.Properties;
            string[] array = null;
            int num = 0;
            bool[] array2 = new bool[9];
            while (base.Reader.MoveToNextAttribute())
            {
                if (!array2[7] && base.Reader.LocalName == this.id6_ReferenceDirectory && base.Reader.NamespaceURI == this.id7_Item)
                {
                    projectConfiguration.ReferenceDirectory = base.Reader.Value;
                    array2[7] = true;
                }
                else if (!base.IsXmlnsAttribute(base.Reader.Name))
                {
                    base.UnknownNode(projectConfiguration, ":ReferenceDirectory");
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                projectConfiguration.StrongNames = (string[])base.ShrinkArray(array, num, typeof(string), true);
                return projectConfiguration;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int num2 = 0;
            int readerCount = base.ReaderCount;
            while (base.Reader.NodeType != XmlNodeType.EndElement && base.Reader.NodeType != XmlNodeType.None)
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    if (base.Reader.LocalName == this.id8_SearchPath && base.Reader.NamespaceURI == this.id2_Item)
                    {
                        if (searchPath == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            searchPath.Add(this.Read3_SearchPathConfiguration(false, true));
                        }
                    }
                    else if (base.Reader.LocalName == this.id9_Using && base.Reader.NamespaceURI == this.id2_Item)
                    {
                        if (usedPlugIns == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            usedPlugIns.Add(this.Read4_UsingConfiguration(false, true));
                        }
                    }
                    else if (base.Reader.LocalName == this.id10_TaskType && base.Reader.NamespaceURI == this.id2_Item)
                    {
                        if (taskTypes == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            taskTypes.Add(this.Read8_TaskTypeConfiguration(false, true));
                        }
                    }
                    else if (base.Reader.LocalName == this.id11_Platform && base.Reader.NamespaceURI == this.id2_Item)
                    {
                        if (platforms == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            platforms.Add(this.Read9_PlatformConfiguration(false, true));
                        }
                    }
                    else if (base.Reader.LocalName == this.id12_Property && base.Reader.NamespaceURI == this.id2_Item)
                    {
                        if (properties == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            properties.Add(this.Read10_PropertyConfiguration(false, true));
                        }
                    }
                    else if (!array2[5] && base.Reader.LocalName == this.id13_AssemblyBinding && base.Reader.NamespaceURI == this.id2_Item)
                    {
                        projectConfiguration.AssemblyBinding = this.Read15_AssemblyBindingConfiguration(false, true);
                        array2[5] = true;
                    }
                    else if (base.Reader.LocalName == this.id14_StrongName && base.Reader.NamespaceURI == this.id2_Item)
                    {
                        array = (string[])base.EnsureArrayIndex(array, num, typeof(string));
                        array[num++] = base.Reader.ReadElementString();
                    }
                    else if (!array2[8] && base.Reader.LocalName == this.id15_Tasks && base.Reader.NamespaceURI == this.id2_Item)
                    {
                        projectConfiguration.TasksElement = (XmlElement)base.ReadXmlNode(false);
                        array2[8] = true;
                    }
                    else
                    {
                        base.UnknownNode(projectConfiguration, "http://schemas.postsharp.org/1.0/configuration:SearchPath, http://schemas.postsharp.org/1.0/configuration:Using, http://schemas.postsharp.org/1.0/configuration:TaskType, http://schemas.postsharp.org/1.0/configuration:Platform, http://schemas.postsharp.org/1.0/configuration:Property, http://schemas.postsharp.org/1.0/configuration:AssemblyBinding, http://schemas.postsharp.org/1.0/configuration:StrongName, http://schemas.postsharp.org/1.0/configuration:Tasks");
                    }
                }
                else
                {
                    base.UnknownNode(projectConfiguration, "http://schemas.postsharp.org/1.0/configuration:SearchPath, http://schemas.postsharp.org/1.0/configuration:Using, http://schemas.postsharp.org/1.0/configuration:TaskType, http://schemas.postsharp.org/1.0/configuration:Platform, http://schemas.postsharp.org/1.0/configuration:Property, http://schemas.postsharp.org/1.0/configuration:AssemblyBinding, http://schemas.postsharp.org/1.0/configuration:StrongName, http://schemas.postsharp.org/1.0/configuration:Tasks");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref num2, ref readerCount);
            }
            projectConfiguration.StrongNames = (string[])base.ShrinkArray(array, num, typeof(string), true);
            base.ReadEndElement();
            return projectConfiguration;
        }

        private AssemblyBindingConfiguration Read15_AssemblyBindingConfiguration(bool isNullable, bool checkType)
        {
            XmlQualifiedName xmlQualifiedName = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if (checkType && !(xmlQualifiedName == null) && (xmlQualifiedName.Name != this.id7_Item || xmlQualifiedName.Namespace != this.id2_Item))
            {
                throw base.CreateUnknownTypeException(xmlQualifiedName);
            }
            if (flag)
            {
                return null;
            }
            AssemblyBindingConfiguration assemblyBindingConfiguration = new AssemblyBindingConfiguration();
            DependentAssemblyConfigurationCollection dependentAssemblies = assemblyBindingConfiguration.DependentAssemblies;
            ImportAssemblyBindingsConfigurationCollection importAssemblyBindings = assemblyBindingConfiguration.ImportAssemblyBindings;
            while (base.Reader.MoveToNextAttribute())
            {
                if (!base.IsXmlnsAttribute(base.Reader.Name))
                {
                    base.UnknownNode(assemblyBindingConfiguration);
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                return assemblyBindingConfiguration;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int num = 0;
            int readerCount = base.ReaderCount;
            while (base.Reader.NodeType != XmlNodeType.EndElement && base.Reader.NodeType != XmlNodeType.None)
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    if (base.Reader.LocalName == this.id16_dependentAssembly && base.Reader.NamespaceURI == this.id17_Item)
                    {
                        if (dependentAssemblies == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            dependentAssemblies.Add(this.Read13_DependentAssemblyConfiguration(false, true));
                        }
                    }
                    else if (base.Reader.LocalName == this.id18_Import && base.Reader.NamespaceURI == this.id2_Item)
                    {
                        if (importAssemblyBindings == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            importAssemblyBindings.Add(this.Read14_Item(false, true));
                        }
                    }
                    else
                    {
                        base.UnknownNode(assemblyBindingConfiguration, "urn:schemas-microsoft-com:asm.v1:dependentAssembly, http://schemas.postsharp.org/1.0/configuration:Import");
                    }
                }
                else
                {
                    base.UnknownNode(assemblyBindingConfiguration, "urn:schemas-microsoft-com:asm.v1:dependentAssembly, http://schemas.postsharp.org/1.0/configuration:Import");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref num, ref readerCount);
            }
            base.ReadEndElement();
            return assemblyBindingConfiguration;
        }

        private ImportAssemblyBindingsConfiguration Read14_Item(bool isNullable, bool checkType)
        {
            XmlQualifiedName xmlQualifiedName = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if (checkType && !(xmlQualifiedName == null) && (xmlQualifiedName.Name != this.id7_Item || xmlQualifiedName.Namespace != this.id2_Item))
            {
                throw base.CreateUnknownTypeException(xmlQualifiedName);
            }
            if (flag)
            {
                return null;
            }
            ImportAssemblyBindingsConfiguration importAssemblyBindingsConfiguration = new ImportAssemblyBindingsConfiguration();
            bool[] array = new bool[2];
            while (base.Reader.MoveToNextAttribute())
            {
                if (!array[0] && base.Reader.LocalName == this.id19_File && base.Reader.NamespaceURI == this.id7_Item)
                {
                    importAssemblyBindingsConfiguration.File = base.Reader.Value;
                    array[0] = true;
                }
                else if (!array[1] && base.Reader.LocalName == this.id20_Select && base.Reader.NamespaceURI == this.id7_Item)
                {
                    importAssemblyBindingsConfiguration.Select = base.Reader.Value;
                    array[1] = true;
                }
                else if (!base.IsXmlnsAttribute(base.Reader.Name))
                {
                    base.UnknownNode(importAssemblyBindingsConfiguration, ":File, :Select");
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                return importAssemblyBindingsConfiguration;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int num = 0;
            int readerCount = base.ReaderCount;
            while (base.Reader.NodeType != XmlNodeType.EndElement && base.Reader.NodeType != XmlNodeType.None)
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    base.UnknownNode(importAssemblyBindingsConfiguration, "");
                }
                else
                {
                    base.UnknownNode(importAssemblyBindingsConfiguration, "");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref num, ref readerCount);
            }
            base.ReadEndElement();
            return importAssemblyBindingsConfiguration;
        }

        private DependentAssemblyConfiguration Read13_DependentAssemblyConfiguration(bool isNullable, bool checkType)
        {
            XmlQualifiedName xmlQualifiedName = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if (checkType && !(xmlQualifiedName == null) && (xmlQualifiedName.Name != this.id7_Item || xmlQualifiedName.Namespace != this.id17_Item))
            {
                throw base.CreateUnknownTypeException(xmlQualifiedName);
            }
            if (flag)
            {
                return null;
            }
            DependentAssemblyConfiguration dependentAssemblyConfiguration = new DependentAssemblyConfiguration();
            bool[] array = new bool[2];
            while (base.Reader.MoveToNextAttribute())
            {
                if (!base.IsXmlnsAttribute(base.Reader.Name))
                {
                    base.UnknownNode(dependentAssemblyConfiguration);
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                return dependentAssemblyConfiguration;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int num = 0;
            int readerCount = base.ReaderCount;
            while (base.Reader.NodeType != XmlNodeType.EndElement && base.Reader.NodeType != XmlNodeType.None)
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    if (!array[0] && base.Reader.LocalName == this.id21_assemblyIdentity && base.Reader.NamespaceURI == this.id17_Item)
                    {
                        dependentAssemblyConfiguration.AssemblyIdentity = this.Read11_AssemblyIdentityConfiguration(false, true);
                        array[0] = true;
                    }
                    else if (!array[1] && base.Reader.LocalName == this.id22_bindingRedirect && base.Reader.NamespaceURI == this.id17_Item)
                    {
                        dependentAssemblyConfiguration.BindingRedirect = this.Read12_BindingRedirectConfiguration(false, true);
                        array[1] = true;
                    }
                    else
                    {
                        base.UnknownNode(dependentAssemblyConfiguration, "urn:schemas-microsoft-com:asm.v1:assemblyIdentity, urn:schemas-microsoft-com:asm.v1:bindingRedirect");
                    }
                }
                else
                {
                    base.UnknownNode(dependentAssemblyConfiguration, "urn:schemas-microsoft-com:asm.v1:assemblyIdentity, urn:schemas-microsoft-com:asm.v1:bindingRedirect");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref num, ref readerCount);
            }
            base.ReadEndElement();
            return dependentAssemblyConfiguration;
        }

        private BindingRedirectConfiguration Read12_BindingRedirectConfiguration(bool isNullable, bool checkType)
        {
            XmlQualifiedName xmlQualifiedName = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if (checkType && !(xmlQualifiedName == null) && (xmlQualifiedName.Name != this.id7_Item || xmlQualifiedName.Namespace != this.id17_Item))
            {
                throw base.CreateUnknownTypeException(xmlQualifiedName);
            }
            if (flag)
            {
                return null;
            }
            BindingRedirectConfiguration bindingRedirectConfiguration = new BindingRedirectConfiguration();
            bool[] array = new bool[4];
            while (base.Reader.MoveToNextAttribute())
            {
                if (!array[0] && base.Reader.LocalName == this.id23_oldVersion && base.Reader.NamespaceURI == this.id7_Item)
                {
                    bindingRedirectConfiguration.OldVersion = base.Reader.Value;
                    array[0] = true;
                }
                else if (!array[1] && base.Reader.LocalName == this.id24_newVersion && base.Reader.NamespaceURI == this.id7_Item)
                {
                    bindingRedirectConfiguration.NewVersion = base.Reader.Value;
                    array[1] = true;
                }
                else if (!array[2] && base.Reader.LocalName == this.id25_newPublicKeyToken && base.Reader.NamespaceURI == this.id7_Item)
                {
                    bindingRedirectConfiguration.NewPublicKeyToken = base.Reader.Value;
                    array[2] = true;
                }
                else if (!array[3] && base.Reader.LocalName == this.id26_newName && base.Reader.NamespaceURI == this.id7_Item)
                {
                    bindingRedirectConfiguration.NewName = base.Reader.Value;
                    array[3] = true;
                }
                else if (!base.IsXmlnsAttribute(base.Reader.Name))
                {
                    base.UnknownNode(bindingRedirectConfiguration, ":oldVersion, :newVersion, :newPublicKeyToken, :newName");
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                return bindingRedirectConfiguration;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int num = 0;
            int readerCount = base.ReaderCount;
            while (base.Reader.NodeType != XmlNodeType.EndElement && base.Reader.NodeType != XmlNodeType.None)
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    base.UnknownNode(bindingRedirectConfiguration, "");
                }
                else
                {
                    base.UnknownNode(bindingRedirectConfiguration, "");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref num, ref readerCount);
            }
            base.ReadEndElement();
            return bindingRedirectConfiguration;
        }

        private AssemblyIdentityConfiguration Read11_AssemblyIdentityConfiguration(bool isNullable, bool checkType)
        {
            XmlQualifiedName xmlQualifiedName = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if (checkType && !(xmlQualifiedName == null) && (xmlQualifiedName.Name != this.id7_Item || xmlQualifiedName.Namespace != this.id17_Item))
            {
                throw base.CreateUnknownTypeException(xmlQualifiedName);
            }
            if (flag)
            {
                return null;
            }
            AssemblyIdentityConfiguration assemblyIdentityConfiguration = new AssemblyIdentityConfiguration();
            bool[] array = new bool[3];
            while (base.Reader.MoveToNextAttribute())
            {
                if (!array[0] && base.Reader.LocalName == this.id27_name && base.Reader.NamespaceURI == this.id7_Item)
                {
                    assemblyIdentityConfiguration.Name = base.Reader.Value;
                    array[0] = true;
                }
                else if (!array[1] && base.Reader.LocalName == this.id28_publicKeyToken && base.Reader.NamespaceURI == this.id7_Item)
                {
                    assemblyIdentityConfiguration.PublicKeyToken = base.Reader.Value;
                    array[1] = true;
                }
                else if (!array[2] && base.Reader.LocalName == this.id29_culture && base.Reader.NamespaceURI == this.id7_Item)
                {
                    assemblyIdentityConfiguration.Culture = base.Reader.Value;
                    array[2] = true;
                }
                else if (!base.IsXmlnsAttribute(base.Reader.Name))
                {
                    base.UnknownNode(assemblyIdentityConfiguration, ":name, :publicKeyToken, :culture");
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                return assemblyIdentityConfiguration;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int num = 0;
            int readerCount = base.ReaderCount;
            while (base.Reader.NodeType != XmlNodeType.EndElement && base.Reader.NodeType != XmlNodeType.None)
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    base.UnknownNode(assemblyIdentityConfiguration, "");
                }
                else
                {
                    base.UnknownNode(assemblyIdentityConfiguration, "");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref num, ref readerCount);
            }
            base.ReadEndElement();
            return assemblyIdentityConfiguration;
        }

        private PropertyConfiguration Read10_PropertyConfiguration(bool isNullable, bool checkType)
        {
            XmlQualifiedName xmlQualifiedName = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if (checkType && !(xmlQualifiedName == null) && (xmlQualifiedName.Name != this.id7_Item || xmlQualifiedName.Namespace != this.id2_Item))
            {
                throw base.CreateUnknownTypeException(xmlQualifiedName);
            }
            if (flag)
            {
                return null;
            }
            PropertyConfiguration propertyConfiguration = new PropertyConfiguration();
            bool[] array = new bool[2];
            while (base.Reader.MoveToNextAttribute())
            {
                if (!array[0] && base.Reader.LocalName == this.id30_Value && base.Reader.NamespaceURI == this.id7_Item)
                {
                    propertyConfiguration.Value = base.Reader.Value;
                    array[0] = true;
                }
                else if (!array[1] && base.Reader.LocalName == this.id31_Name && base.Reader.NamespaceURI == this.id7_Item)
                {
                    propertyConfiguration.Name = base.Reader.Value;
                    array[1] = true;
                }
                else if (!base.IsXmlnsAttribute(base.Reader.Name))
                {
                    base.UnknownNode(propertyConfiguration, ":Value, :Name");
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                return propertyConfiguration;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int num = 0;
            int readerCount = base.ReaderCount;
            while (base.Reader.NodeType != XmlNodeType.EndElement && base.Reader.NodeType != XmlNodeType.None)
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    base.UnknownNode(propertyConfiguration, "");
                }
                else
                {
                    base.UnknownNode(propertyConfiguration, "");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref num, ref readerCount);
            }
            base.ReadEndElement();
            return propertyConfiguration;
        }

        private PlatformConfiguration Read9_PlatformConfiguration(bool isNullable, bool checkType)
        {
            XmlQualifiedName xmlQualifiedName = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if (checkType && !(xmlQualifiedName == null) && (xmlQualifiedName.Name != this.id7_Item || xmlQualifiedName.Namespace != this.id2_Item))
            {
                throw base.CreateUnknownTypeException(xmlQualifiedName);
            }
            if (flag)
            {
                return null;
            }
            PlatformConfiguration platformConfiguration = new PlatformConfiguration();
            if (platformConfiguration.Parameters == null)
            {
                platformConfiguration.Parameters = new NameValuePairCollection();
            }
            NameValuePairCollection arg_6D_0 = platformConfiguration.Parameters;
            bool[] array = new bool[3];
            while (base.Reader.MoveToNextAttribute())
            {
                if (!array[0] && base.Reader.LocalName == this.id32_Implementation && base.Reader.NamespaceURI == this.id7_Item)
                {
                    platformConfiguration.Implementation = base.Reader.Value;
                    array[0] = true;
                }
                else if (!array[1] && base.Reader.LocalName == this.id31_Name && base.Reader.NamespaceURI == this.id7_Item)
                {
                    platformConfiguration.Name = base.Reader.Value;
                    array[1] = true;
                }
                else if (!base.IsXmlnsAttribute(base.Reader.Name))
                {
                    base.UnknownNode(platformConfiguration, ":Implementation, :Name");
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                return platformConfiguration;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int num = 0;
            int readerCount = base.ReaderCount;
            while (base.Reader.NodeType != XmlNodeType.EndElement && base.Reader.NodeType != XmlNodeType.None)
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    if (base.Reader.LocalName == this.id33_Parameters && base.Reader.NamespaceURI == this.id2_Item)
                    {
                        if (!base.ReadNull())
                        {
                            if (platformConfiguration.Parameters == null)
                            {
                                platformConfiguration.Parameters = new NameValuePairCollection();
                            }
                            NameValuePairCollection parameters = platformConfiguration.Parameters;
                            if (base.Reader.IsEmptyElement)
                            {
                                base.Reader.Skip();
                            }
                            else
                            {
                                base.Reader.ReadStartElement();
                                base.Reader.MoveToContent();
                                int num2 = 0;
                                int readerCount2 = base.ReaderCount;
                                while (base.Reader.NodeType != XmlNodeType.EndElement && base.Reader.NodeType != XmlNodeType.None)
                                {
                                    if (base.Reader.NodeType == XmlNodeType.Element)
                                    {
                                        if (base.Reader.LocalName == this.id34_Parameter && base.Reader.NamespaceURI == this.id2_Item)
                                        {
                                            if (parameters == null)
                                            {
                                                base.Reader.Skip();
                                            }
                                            else
                                            {
                                                parameters.Add(this.Read7_NameValuePair(true, true));
                                            }
                                        }
                                        else
                                        {
                                            base.UnknownNode(null, "http://schemas.postsharp.org/1.0/configuration:Parameter");
                                        }
                                    }
                                    else
                                    {
                                        base.UnknownNode(null, "http://schemas.postsharp.org/1.0/configuration:Parameter");
                                    }
                                    base.Reader.MoveToContent();
                                    base.CheckReaderCount(ref num2, ref readerCount2);
                                }
                                base.ReadEndElement();
                            }
                        }
                    }
                    else
                    {
                        base.UnknownNode(platformConfiguration, "http://schemas.postsharp.org/1.0/configuration:Parameters");
                    }
                }
                else
                {
                    base.UnknownNode(platformConfiguration, "http://schemas.postsharp.org/1.0/configuration:Parameters");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref num, ref readerCount);
            }
            base.ReadEndElement();
            return platformConfiguration;
        }

        private NameValuePair Read7_NameValuePair(bool isNullable, bool checkType)
        {
            XmlQualifiedName xmlQualifiedName = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if (checkType && !(xmlQualifiedName == null) && (xmlQualifiedName.Name != this.id7_Item || xmlQualifiedName.Namespace != this.id2_Item))
            {
                throw base.CreateUnknownTypeException(xmlQualifiedName);
            }
            if (flag)
            {
                return null;
            }
            NameValuePair nameValuePair = new NameValuePair();
            bool[] array = new bool[2];
            while (base.Reader.MoveToNextAttribute())
            {
                if (!array[0] && base.Reader.LocalName == this.id30_Value && base.Reader.NamespaceURI == this.id7_Item)
                {
                    nameValuePair.Value = base.Reader.Value;
                    array[0] = true;
                }
                else if (!array[1] && base.Reader.LocalName == this.id31_Name && base.Reader.NamespaceURI == this.id7_Item)
                {
                    nameValuePair.Name = base.Reader.Value;
                    array[1] = true;
                }
                else if (!base.IsXmlnsAttribute(base.Reader.Name))
                {
                    base.UnknownNode(nameValuePair, ":Value, :Name");
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                return nameValuePair;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int num = 0;
            int readerCount = base.ReaderCount;
            while (base.Reader.NodeType != XmlNodeType.EndElement && base.Reader.NodeType != XmlNodeType.None)
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    base.UnknownNode(nameValuePair, "");
                }
                else
                {
                    base.UnknownNode(nameValuePair, "");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref num, ref readerCount);
            }
            base.ReadEndElement();
            return nameValuePair;
        }

        private TaskTypeConfiguration Read8_TaskTypeConfiguration(bool isNullable, bool checkType)
        {
            XmlQualifiedName xmlQualifiedName = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if (checkType && !(xmlQualifiedName == null) && (xmlQualifiedName.Name != this.id7_Item || xmlQualifiedName.Namespace != this.id2_Item))
            {
                throw base.CreateUnknownTypeException(xmlQualifiedName);
            }
            if (flag)
            {
                return null;
            }
            TaskTypeConfiguration taskTypeConfiguration = new TaskTypeConfiguration();
            if (taskTypeConfiguration.Dependencies == null)
            {
                taskTypeConfiguration.Dependencies = new DependencyConfigurationCollection();
            }
            DependencyConfigurationCollection dependencies = taskTypeConfiguration.Dependencies;
            if (taskTypeConfiguration.Parameters == null)
            {
                taskTypeConfiguration.Parameters = new NameValuePairCollection();
            }
            NameValuePairCollection arg_87_0 = taskTypeConfiguration.Parameters;
            bool[] array = new bool[5];
            while (base.Reader.MoveToNextAttribute())
            {
                if (!array[0] && base.Reader.LocalName == this.id35_Phase && base.Reader.NamespaceURI == this.id7_Item)
                {
                    taskTypeConfiguration.Phase = base.Reader.Value;
                    array[0] = true;
                }
                else if (!array[1] && base.Reader.LocalName == this.id31_Name && base.Reader.NamespaceURI == this.id7_Item)
                {
                    taskTypeConfiguration.Name = base.Reader.Value;
                    array[1] = true;
                }
                else if (!array[2] && base.Reader.LocalName == this.id32_Implementation && base.Reader.NamespaceURI == this.id7_Item)
                {
                    taskTypeConfiguration.Implementation = base.Reader.Value;
                    array[2] = true;
                }
                else if (!base.IsXmlnsAttribute(base.Reader.Name))
                {
                    base.UnknownNode(taskTypeConfiguration, ":Phase, :Name, :Implementation");
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                return taskTypeConfiguration;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int num = 0;
            int readerCount = base.ReaderCount;
            while (base.Reader.NodeType != XmlNodeType.EndElement && base.Reader.NodeType != XmlNodeType.None)
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    if (base.Reader.LocalName == this.id36_Dependency && base.Reader.NamespaceURI == this.id2_Item)
                    {
                        if (dependencies == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            dependencies.Add(this.Read6_DependencyConfiguration(false, true));
                        }
                    }
                    else if (base.Reader.LocalName == this.id33_Parameters && base.Reader.NamespaceURI == this.id2_Item)
                    {
                        if (!base.ReadNull())
                        {
                            if (taskTypeConfiguration.Parameters == null)
                            {
                                taskTypeConfiguration.Parameters = new NameValuePairCollection();
                            }
                            NameValuePairCollection parameters = taskTypeConfiguration.Parameters;
                            if (base.Reader.IsEmptyElement)
                            {
                                base.Reader.Skip();
                            }
                            else
                            {
                                base.Reader.ReadStartElement();
                                base.Reader.MoveToContent();
                                int num2 = 0;
                                int readerCount2 = base.ReaderCount;
                                while (base.Reader.NodeType != XmlNodeType.EndElement && base.Reader.NodeType != XmlNodeType.None)
                                {
                                    if (base.Reader.NodeType == XmlNodeType.Element)
                                    {
                                        if (base.Reader.LocalName == this.id34_Parameter && base.Reader.NamespaceURI == this.id2_Item)
                                        {
                                            if (parameters == null)
                                            {
                                                base.Reader.Skip();
                                            }
                                            else
                                            {
                                                parameters.Add(this.Read7_NameValuePair(true, true));
                                            }
                                        }
                                        else
                                        {
                                            base.UnknownNode(null, "http://schemas.postsharp.org/1.0/configuration:Parameter");
                                        }
                                    }
                                    else
                                    {
                                        base.UnknownNode(null, "http://schemas.postsharp.org/1.0/configuration:Parameter");
                                    }
                                    base.Reader.MoveToContent();
                                    base.CheckReaderCount(ref num2, ref readerCount2);
                                }
                                base.ReadEndElement();
                            }
                        }
                    }
                    else
                    {
                        base.UnknownNode(taskTypeConfiguration, "http://schemas.postsharp.org/1.0/configuration:Dependency, http://schemas.postsharp.org/1.0/configuration:Parameters");
                    }
                }
                else
                {
                    base.UnknownNode(taskTypeConfiguration, "http://schemas.postsharp.org/1.0/configuration:Dependency, http://schemas.postsharp.org/1.0/configuration:Parameters");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref num, ref readerCount);
            }
            base.ReadEndElement();
            return taskTypeConfiguration;
        }

        private DependencyConfiguration Read6_DependencyConfiguration(bool isNullable, bool checkType)
        {
            XmlQualifiedName xmlQualifiedName = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if (checkType && !(xmlQualifiedName == null) && (xmlQualifiedName.Name != this.id7_Item || xmlQualifiedName.Namespace != this.id2_Item))
            {
                throw base.CreateUnknownTypeException(xmlQualifiedName);
            }
            if (flag)
            {
                return null;
            }
            DependencyConfiguration dependencyConfiguration = new DependencyConfiguration();
            bool[] array = new bool[3];
            while (base.Reader.MoveToNextAttribute())
            {
                if (!array[0] && base.Reader.LocalName == this.id37_Required && base.Reader.NamespaceURI == this.id7_Item)
                {
                    dependencyConfiguration.Required = XmlConvert.ToBoolean(base.Reader.Value);
                    array[0] = true;
                }
                else if (!array[1] && base.Reader.LocalName == this.id10_TaskType && base.Reader.NamespaceURI == this.id7_Item)
                {
                    dependencyConfiguration.TaskType = base.Reader.Value;
                    array[1] = true;
                }
                else if (!array[2] && base.Reader.LocalName == this.id38_Position && base.Reader.NamespaceURI == this.id7_Item)
                {
                    dependencyConfiguration.Position = this.Read5_DependencyPosition(base.Reader.Value);
                    array[2] = true;
                }
                else if (!base.IsXmlnsAttribute(base.Reader.Name))
                {
                    base.UnknownNode(dependencyConfiguration, ":Required, :TaskType, :Position");
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                return dependencyConfiguration;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int num = 0;
            int readerCount = base.ReaderCount;
            while (base.Reader.NodeType != XmlNodeType.EndElement && base.Reader.NodeType != XmlNodeType.None)
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    base.UnknownNode(dependencyConfiguration, "");
                }
                else
                {
                    base.UnknownNode(dependencyConfiguration, "");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref num, ref readerCount);
            }
            base.ReadEndElement();
            return dependencyConfiguration;
        }

        private DependencyPosition Read5_DependencyPosition(string s)
        {
            if (s != null)
            {
                if (s == "Default")
                {
                    return DependencyPosition.Default;
                }
                if (s == "Before")
                {
                    return DependencyPosition.Default;
                }
                if (s == "After")
                {
                    return DependencyPosition.After;
                }
            }
            throw base.CreateUnknownConstantException(s, typeof(DependencyPosition));
        }

        private UsingConfiguration Read4_UsingConfiguration(bool isNullable, bool checkType)
        {
            XmlQualifiedName xmlQualifiedName = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if (checkType && !(xmlQualifiedName == null) && (xmlQualifiedName.Name != this.id7_Item || xmlQualifiedName.Namespace != this.id2_Item))
            {
                throw base.CreateUnknownTypeException(xmlQualifiedName);
            }
            if (flag)
            {
                return null;
            }
            UsingConfiguration usingConfiguration = new UsingConfiguration();
            if (usingConfiguration.SearchPath == null)
            {
                usingConfiguration.SearchPath = new SearchPathConfigurationCollection();
            }
            SearchPathConfigurationCollection searchPath = usingConfiguration.SearchPath;
            if (usingConfiguration.UsedPlugIns == null)
            {
                usingConfiguration.UsedPlugIns = new UsingConfigurationCollection();
            }
            UsingConfigurationCollection usedPlugIns = usingConfiguration.UsedPlugIns;
            if (usingConfiguration.TaskTypes == null)
            {
                usingConfiguration.TaskTypes = new TaskTypeConfigurationCollection();
            }
            TaskTypeConfigurationCollection taskTypes = usingConfiguration.TaskTypes;
            if (usingConfiguration.Platforms == null)
            {
                usingConfiguration.Platforms = new PlatformConfigurationCollection();
            }
            PlatformConfigurationCollection platforms = usingConfiguration.Platforms;
            if (usingConfiguration.Properties == null)
            {
                usingConfiguration.Properties = new PropertyConfigurationCollection();
            }
            PropertyConfigurationCollection properties = usingConfiguration.Properties;
            string[] array = null;
            int num = 0;
            bool[] array2 = new bool[8];
            while (base.Reader.MoveToNextAttribute())
            {
                if (!array2[7] && base.Reader.LocalName == this.id39_PlugInFile && base.Reader.NamespaceURI == this.id7_Item)
                {
                    usingConfiguration.PlugInFile = base.Reader.Value;
                    array2[7] = true;
                }
                else if (!base.IsXmlnsAttribute(base.Reader.Name))
                {
                    base.UnknownNode(usingConfiguration, ":PlugInFile");
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                usingConfiguration.StrongNames = (string[])base.ShrinkArray(array, num, typeof(string), true);
                return usingConfiguration;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int num2 = 0;
            int readerCount = base.ReaderCount;
            while (base.Reader.NodeType != XmlNodeType.EndElement && base.Reader.NodeType != XmlNodeType.None)
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    if (base.Reader.LocalName == this.id8_SearchPath && base.Reader.NamespaceURI == this.id2_Item)
                    {
                        if (searchPath == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            searchPath.Add(this.Read3_SearchPathConfiguration(false, true));
                        }
                    }
                    else if (base.Reader.LocalName == this.id9_Using && base.Reader.NamespaceURI == this.id2_Item)
                    {
                        if (usedPlugIns == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            usedPlugIns.Add(this.Read4_UsingConfiguration(false, true));
                        }
                    }
                    else if (base.Reader.LocalName == this.id10_TaskType && base.Reader.NamespaceURI == this.id2_Item)
                    {
                        if (taskTypes == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            taskTypes.Add(this.Read8_TaskTypeConfiguration(false, true));
                        }
                    }
                    else if (base.Reader.LocalName == this.id11_Platform && base.Reader.NamespaceURI == this.id2_Item)
                    {
                        if (platforms == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            platforms.Add(this.Read9_PlatformConfiguration(false, true));
                        }
                    }
                    else if (base.Reader.LocalName == this.id12_Property && base.Reader.NamespaceURI == this.id2_Item)
                    {
                        if (properties == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            properties.Add(this.Read10_PropertyConfiguration(false, true));
                        }
                    }
                    else if (!array2[5] && base.Reader.LocalName == this.id13_AssemblyBinding && base.Reader.NamespaceURI == this.id2_Item)
                    {
                        usingConfiguration.AssemblyBinding = this.Read15_AssemblyBindingConfiguration(false, true);
                        array2[5] = true;
                    }
                    else if (base.Reader.LocalName == this.id14_StrongName && base.Reader.NamespaceURI == this.id2_Item)
                    {
                        array = (string[])base.EnsureArrayIndex(array, num, typeof(string));
                        array[num++] = base.Reader.ReadElementString();
                    }
                    else
                    {
                        base.UnknownNode(usingConfiguration, "http://schemas.postsharp.org/1.0/configuration:SearchPath, http://schemas.postsharp.org/1.0/configuration:Using, http://schemas.postsharp.org/1.0/configuration:TaskType, http://schemas.postsharp.org/1.0/configuration:Platform, http://schemas.postsharp.org/1.0/configuration:Property, http://schemas.postsharp.org/1.0/configuration:AssemblyBinding, http://schemas.postsharp.org/1.0/configuration:StrongName");
                    }
                }
                else
                {
                    base.UnknownNode(usingConfiguration, "http://schemas.postsharp.org/1.0/configuration:SearchPath, http://schemas.postsharp.org/1.0/configuration:Using, http://schemas.postsharp.org/1.0/configuration:TaskType, http://schemas.postsharp.org/1.0/configuration:Platform, http://schemas.postsharp.org/1.0/configuration:Property, http://schemas.postsharp.org/1.0/configuration:AssemblyBinding, http://schemas.postsharp.org/1.0/configuration:StrongName");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref num2, ref readerCount);
            }
            usingConfiguration.StrongNames = (string[])base.ShrinkArray(array, num, typeof(string), true);
            base.ReadEndElement();
            return usingConfiguration;
        }

        private SearchPathConfiguration Read3_SearchPathConfiguration(bool isNullable, bool checkType)
        {
            XmlQualifiedName xmlQualifiedName = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if (checkType && !(xmlQualifiedName == null) && (xmlQualifiedName.Name != this.id7_Item || xmlQualifiedName.Namespace != this.id2_Item))
            {
                throw base.CreateUnknownTypeException(xmlQualifiedName);
            }
            if (flag)
            {
                return null;
            }
            SearchPathConfiguration searchPathConfiguration = new SearchPathConfiguration();
            bool[] array = new bool[2];
            while (base.Reader.MoveToNextAttribute())
            {
                if (!array[0] && base.Reader.LocalName == this.id40_Directory && base.Reader.NamespaceURI == this.id7_Item)
                {
                    searchPathConfiguration.Directory = base.Reader.Value;
                    array[0] = true;
                }
                else if (!array[1] && base.Reader.LocalName == this.id19_File && base.Reader.NamespaceURI == this.id7_Item)
                {
                    searchPathConfiguration.File = base.Reader.Value;
                    array[1] = true;
                }
                else if (!base.IsXmlnsAttribute(base.Reader.Name))
                {
                    base.UnknownNode(searchPathConfiguration, ":Directory, :File");
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                return searchPathConfiguration;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int num = 0;
            int readerCount = base.ReaderCount;
            while (base.Reader.NodeType != XmlNodeType.EndElement && base.Reader.NodeType != XmlNodeType.None)
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    base.UnknownNode(searchPathConfiguration, "");
                }
                else
                {
                    base.UnknownNode(searchPathConfiguration, "");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref num, ref readerCount);
            }
            base.ReadEndElement();
            return searchPathConfiguration;
        }

        private PlugInConfiguration Read19_PlugInConfiguration(bool isNullable, bool checkType)
        {
            XmlQualifiedName xmlQualifiedName = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if (checkType && !(xmlQualifiedName == null) && (xmlQualifiedName.Name != this.id41_PlugInConfiguration || xmlQualifiedName.Namespace != this.id2_Item))
            {
                throw base.CreateUnknownTypeException(xmlQualifiedName);
            }
            if (flag)
            {
                return null;
            }
            PlugInConfiguration plugInConfiguration = new PlugInConfiguration();
            if (plugInConfiguration.SearchPath == null)
            {
                plugInConfiguration.SearchPath = new SearchPathConfigurationCollection();
            }
            SearchPathConfigurationCollection searchPath = plugInConfiguration.SearchPath;
            if (plugInConfiguration.UsedPlugIns == null)
            {
                plugInConfiguration.UsedPlugIns = new UsingConfigurationCollection();
            }
            UsingConfigurationCollection usedPlugIns = plugInConfiguration.UsedPlugIns;
            if (plugInConfiguration.TaskTypes == null)
            {
                plugInConfiguration.TaskTypes = new TaskTypeConfigurationCollection();
            }
            TaskTypeConfigurationCollection taskTypes = plugInConfiguration.TaskTypes;
            if (plugInConfiguration.Platforms == null)
            {
                plugInConfiguration.Platforms = new PlatformConfigurationCollection();
            }
            PlatformConfigurationCollection platforms = plugInConfiguration.Platforms;
            if (plugInConfiguration.Properties == null)
            {
                plugInConfiguration.Properties = new PropertyConfigurationCollection();
            }
            PropertyConfigurationCollection properties = plugInConfiguration.Properties;
            string[] array = null;
            int num = 0;
            bool[] array2 = new bool[7];
            while (base.Reader.MoveToNextAttribute())
            {
                if (!base.IsXmlnsAttribute(base.Reader.Name))
                {
                    base.UnknownNode(plugInConfiguration);
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                plugInConfiguration.StrongNames = (string[])base.ShrinkArray(array, num, typeof(string), true);
                return plugInConfiguration;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int num2 = 0;
            int readerCount = base.ReaderCount;
            while (base.Reader.NodeType != XmlNodeType.EndElement && base.Reader.NodeType != XmlNodeType.None)
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    if (base.Reader.LocalName == this.id8_SearchPath && base.Reader.NamespaceURI == this.id2_Item)
                    {
                        if (searchPath == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            searchPath.Add(this.Read3_SearchPathConfiguration(false, true));
                        }
                    }
                    else if (base.Reader.LocalName == this.id9_Using && base.Reader.NamespaceURI == this.id2_Item)
                    {
                        if (usedPlugIns == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            usedPlugIns.Add(this.Read4_UsingConfiguration(false, true));
                        }
                    }
                    else if (base.Reader.LocalName == this.id10_TaskType && base.Reader.NamespaceURI == this.id2_Item)
                    {
                        if (taskTypes == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            taskTypes.Add(this.Read8_TaskTypeConfiguration(false, true));
                        }
                    }
                    else if (base.Reader.LocalName == this.id11_Platform && base.Reader.NamespaceURI == this.id2_Item)
                    {
                        if (platforms == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            platforms.Add(this.Read9_PlatformConfiguration(false, true));
                        }
                    }
                    else if (base.Reader.LocalName == this.id12_Property && base.Reader.NamespaceURI == this.id2_Item)
                    {
                        if (properties == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            properties.Add(this.Read10_PropertyConfiguration(false, true));
                        }
                    }
                    else if (!array2[5] && base.Reader.LocalName == this.id13_AssemblyBinding && base.Reader.NamespaceURI == this.id2_Item)
                    {
                        plugInConfiguration.AssemblyBinding = this.Read15_AssemblyBindingConfiguration(false, true);
                        array2[5] = true;
                    }
                    else if (base.Reader.LocalName == this.id14_StrongName && base.Reader.NamespaceURI == this.id2_Item)
                    {
                        array = (string[])base.EnsureArrayIndex(array, num, typeof(string));
                        array[num++] = base.Reader.ReadElementString();
                    }
                    else
                    {
                        base.UnknownNode(plugInConfiguration, "http://schemas.postsharp.org/1.0/configuration:SearchPath, http://schemas.postsharp.org/1.0/configuration:Using, http://schemas.postsharp.org/1.0/configuration:TaskType, http://schemas.postsharp.org/1.0/configuration:Platform, http://schemas.postsharp.org/1.0/configuration:Property, http://schemas.postsharp.org/1.0/configuration:AssemblyBinding, http://schemas.postsharp.org/1.0/configuration:StrongName");
                    }
                }
                else
                {
                    base.UnknownNode(plugInConfiguration, "http://schemas.postsharp.org/1.0/configuration:SearchPath, http://schemas.postsharp.org/1.0/configuration:Using, http://schemas.postsharp.org/1.0/configuration:TaskType, http://schemas.postsharp.org/1.0/configuration:Platform, http://schemas.postsharp.org/1.0/configuration:Property, http://schemas.postsharp.org/1.0/configuration:AssemblyBinding, http://schemas.postsharp.org/1.0/configuration:StrongName");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref num2, ref readerCount);
            }
            plugInConfiguration.StrongNames = (string[])base.ShrinkArray(array, num, typeof(string), true);
            base.ReadEndElement();
            return plugInConfiguration;
        }

        private ApplicationConfiguration Read18_ApplicationConfiguration(bool isNullable, bool checkType)
        {
            XmlQualifiedName xmlQualifiedName = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if (checkType && !(xmlQualifiedName == null) && (xmlQualifiedName.Name != this.id42_ApplicationConfiguration || xmlQualifiedName.Namespace != this.id2_Item))
            {
                throw base.CreateUnknownTypeException(xmlQualifiedName);
            }
            if (flag)
            {
                return null;
            }
            ApplicationConfiguration applicationConfiguration = new ApplicationConfiguration();
            if (applicationConfiguration.SearchPath == null)
            {
                applicationConfiguration.SearchPath = new SearchPathConfigurationCollection();
            }
            SearchPathConfigurationCollection searchPath = applicationConfiguration.SearchPath;
            if (applicationConfiguration.UsedPlugIns == null)
            {
                applicationConfiguration.UsedPlugIns = new UsingConfigurationCollection();
            }
            UsingConfigurationCollection usedPlugIns = applicationConfiguration.UsedPlugIns;
            if (applicationConfiguration.TaskTypes == null)
            {
                applicationConfiguration.TaskTypes = new TaskTypeConfigurationCollection();
            }
            TaskTypeConfigurationCollection taskTypes = applicationConfiguration.TaskTypes;
            if (applicationConfiguration.Platforms == null)
            {
                applicationConfiguration.Platforms = new PlatformConfigurationCollection();
            }
            PlatformConfigurationCollection platforms = applicationConfiguration.Platforms;
            if (applicationConfiguration.Properties == null)
            {
                applicationConfiguration.Properties = new PropertyConfigurationCollection();
            }
            PropertyConfigurationCollection properties = applicationConfiguration.Properties;
            string[] array = null;
            int num = 0;
            if (applicationConfiguration.Phases == null)
            {
                applicationConfiguration.Phases = new PhaseConfigurationCollection();
            }
            PhaseConfigurationCollection arg_F9_0 = applicationConfiguration.Phases;
            bool[] array2 = new bool[8];
            while (base.Reader.MoveToNextAttribute())
            {
                if (!base.IsXmlnsAttribute(base.Reader.Name))
                {
                    base.UnknownNode(applicationConfiguration);
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                applicationConfiguration.StrongNames = (string[])base.ShrinkArray(array, num, typeof(string), true);
                return applicationConfiguration;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int num2 = 0;
            int readerCount = base.ReaderCount;
            while (base.Reader.NodeType != XmlNodeType.EndElement && base.Reader.NodeType != XmlNodeType.None)
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    if (base.Reader.LocalName == this.id8_SearchPath && base.Reader.NamespaceURI == this.id2_Item)
                    {
                        if (searchPath == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            searchPath.Add(this.Read3_SearchPathConfiguration(false, true));
                        }
                    }
                    else if (base.Reader.LocalName == this.id9_Using && base.Reader.NamespaceURI == this.id2_Item)
                    {
                        if (usedPlugIns == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            usedPlugIns.Add(this.Read4_UsingConfiguration(false, true));
                        }
                    }
                    else if (base.Reader.LocalName == this.id10_TaskType && base.Reader.NamespaceURI == this.id2_Item)
                    {
                        if (taskTypes == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            taskTypes.Add(this.Read8_TaskTypeConfiguration(false, true));
                        }
                    }
                    else if (base.Reader.LocalName == this.id11_Platform && base.Reader.NamespaceURI == this.id2_Item)
                    {
                        if (platforms == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            platforms.Add(this.Read9_PlatformConfiguration(false, true));
                        }
                    }
                    else if (base.Reader.LocalName == this.id12_Property && base.Reader.NamespaceURI == this.id2_Item)
                    {
                        if (properties == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            properties.Add(this.Read10_PropertyConfiguration(false, true));
                        }
                    }
                    else if (!array2[5] && base.Reader.LocalName == this.id13_AssemblyBinding && base.Reader.NamespaceURI == this.id2_Item)
                    {
                        applicationConfiguration.AssemblyBinding = this.Read15_AssemblyBindingConfiguration(false, true);
                        array2[5] = true;
                    }
                    else if (base.Reader.LocalName == this.id14_StrongName && base.Reader.NamespaceURI == this.id2_Item)
                    {
                        array = (string[])base.EnsureArrayIndex(array, num, typeof(string));
                        array[num++] = base.Reader.ReadElementString();
                    }
                    else if (base.Reader.LocalName == this.id43_Phases && base.Reader.NamespaceURI == this.id2_Item)
                    {
                        if (!base.ReadNull())
                        {
                            if (applicationConfiguration.Phases == null)
                            {
                                applicationConfiguration.Phases = new PhaseConfigurationCollection();
                            }
                            PhaseConfigurationCollection phases = applicationConfiguration.Phases;
                            if (base.Reader.IsEmptyElement)
                            {
                                base.Reader.Skip();
                            }
                            else
                            {
                                base.Reader.ReadStartElement();
                                base.Reader.MoveToContent();
                                int num3 = 0;
                                int readerCount2 = base.ReaderCount;
                                while (base.Reader.NodeType != XmlNodeType.EndElement && base.Reader.NodeType != XmlNodeType.None)
                                {
                                    if (base.Reader.NodeType == XmlNodeType.Element)
                                    {
                                        if (base.Reader.LocalName == this.id35_Phase && base.Reader.NamespaceURI == this.id2_Item)
                                        {
                                            if (phases == null)
                                            {
                                                base.Reader.Skip();
                                            }
                                            else
                                            {
                                                phases.Add(this.Read17_PhaseConfiguration(true, true));
                                            }
                                        }
                                        else
                                        {
                                            base.UnknownNode(null, "http://schemas.postsharp.org/1.0/configuration:Phase");
                                        }
                                    }
                                    else
                                    {
                                        base.UnknownNode(null, "http://schemas.postsharp.org/1.0/configuration:Phase");
                                    }
                                    base.Reader.MoveToContent();
                                    base.CheckReaderCount(ref num3, ref readerCount2);
                                }
                                base.ReadEndElement();
                            }
                        }
                    }
                    else
                    {
                        base.UnknownNode(applicationConfiguration, "http://schemas.postsharp.org/1.0/configuration:SearchPath, http://schemas.postsharp.org/1.0/configuration:Using, http://schemas.postsharp.org/1.0/configuration:TaskType, http://schemas.postsharp.org/1.0/configuration:Platform, http://schemas.postsharp.org/1.0/configuration:Property, http://schemas.postsharp.org/1.0/configuration:AssemblyBinding, http://schemas.postsharp.org/1.0/configuration:StrongName, http://schemas.postsharp.org/1.0/configuration:Phases");
                    }
                }
                else
                {
                    base.UnknownNode(applicationConfiguration, "http://schemas.postsharp.org/1.0/configuration:SearchPath, http://schemas.postsharp.org/1.0/configuration:Using, http://schemas.postsharp.org/1.0/configuration:TaskType, http://schemas.postsharp.org/1.0/configuration:Platform, http://schemas.postsharp.org/1.0/configuration:Property, http://schemas.postsharp.org/1.0/configuration:AssemblyBinding, http://schemas.postsharp.org/1.0/configuration:StrongName, http://schemas.postsharp.org/1.0/configuration:Phases");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref num2, ref readerCount);
            }
            applicationConfiguration.StrongNames = (string[])base.ShrinkArray(array, num, typeof(string), true);
            base.ReadEndElement();
            return applicationConfiguration;
        }

        private PhaseConfiguration Read17_PhaseConfiguration(bool isNullable, bool checkType)
        {
            XmlQualifiedName xmlQualifiedName = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if (checkType && !(xmlQualifiedName == null) && (xmlQualifiedName.Name != this.id7_Item || xmlQualifiedName.Namespace != this.id2_Item))
            {
                throw base.CreateUnknownTypeException(xmlQualifiedName);
            }
            if (flag)
            {
                return null;
            }
            PhaseConfiguration phaseConfiguration = new PhaseConfiguration();
            bool[] array = new bool[2];
            while (base.Reader.MoveToNextAttribute())
            {
                if (!array[0] && base.Reader.LocalName == this.id44_Ordinal && base.Reader.NamespaceURI == this.id7_Item)
                {
                    phaseConfiguration.Ordinal = XmlConvert.ToInt32(base.Reader.Value);
                    array[0] = true;
                }
                else if (!array[1] && base.Reader.LocalName == this.id31_Name && base.Reader.NamespaceURI == this.id7_Item)
                {
                    phaseConfiguration.Name = base.Reader.Value;
                    array[1] = true;
                }
                else if (!base.IsXmlnsAttribute(base.Reader.Name))
                {
                    base.UnknownNode(phaseConfiguration, ":Ordinal, :Name");
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                return phaseConfiguration;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int num = 0;
            int readerCount = base.ReaderCount;
            while (base.Reader.NodeType != XmlNodeType.EndElement && base.Reader.NodeType != XmlNodeType.None)
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    base.UnknownNode(phaseConfiguration, "");
                }
                else
                {
                    base.UnknownNode(phaseConfiguration, "");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref num, ref readerCount);
            }
            base.ReadEndElement();
            return phaseConfiguration;
        }

        protected override void InitCallbacks()
        {
        }

        protected override void InitIDs()
        {
            this.id20_Select = base.Reader.NameTable.Add("Select");
            this.id12_Property = base.Reader.NameTable.Add("Property");
            this.id33_Parameters = base.Reader.NameTable.Add("Parameters");
            this.id1_Configuration = base.Reader.NameTable.Add("Configuration");
            this.id37_Required = base.Reader.NameTable.Add("Required");
            this.id23_oldVersion = base.Reader.NameTable.Add("oldVersion");
            this.id38_Position = base.Reader.NameTable.Add("Position");
            this.id19_File = base.Reader.NameTable.Add("File");
            this.id25_newPublicKeyToken = base.Reader.NameTable.Add("newPublicKeyToken");
            this.id35_Phase = base.Reader.NameTable.Add("Phase");
            this.id14_StrongName = base.Reader.NameTable.Add("StrongName");
            this.id42_ApplicationConfiguration = base.Reader.NameTable.Add("ApplicationConfiguration");
            this.id22_bindingRedirect = base.Reader.NameTable.Add("bindingRedirect");
            this.id6_ReferenceDirectory = base.Reader.NameTable.Add("ReferenceDirectory");
            this.id7_Item = base.Reader.NameTable.Add("");
            this.id11_Platform = base.Reader.NameTable.Add("Platform");
            this.id16_dependentAssembly = base.Reader.NameTable.Add("dependentAssembly");
            this.id41_PlugInConfiguration = base.Reader.NameTable.Add("PlugInConfiguration");
            this.id31_Name = base.Reader.NameTable.Add("Name");
            this.id36_Dependency = base.Reader.NameTable.Add("Dependency");
            this.id3_PlugIn = base.Reader.NameTable.Add("PlugIn");
            this.id10_TaskType = base.Reader.NameTable.Add("TaskType");
            this.id40_Directory = base.Reader.NameTable.Add("Directory");
            this.id39_PlugInFile = base.Reader.NameTable.Add("PlugInFile");
            this.id28_publicKeyToken = base.Reader.NameTable.Add("publicKeyToken");
            this.id5_ProjectConfiguration = base.Reader.NameTable.Add("ProjectConfiguration");
            this.id9_Using = base.Reader.NameTable.Add("Using");
            this.id8_SearchPath = base.Reader.NameTable.Add("SearchPath");
            this.id18_Import = base.Reader.NameTable.Add("Import");
            this.id4_Project = base.Reader.NameTable.Add("Project");
            this.id32_Implementation = base.Reader.NameTable.Add("Implementation");
            this.id17_Item = base.Reader.NameTable.Add("urn:schemas-microsoft-com:asm.v1");
            this.id43_Phases = base.Reader.NameTable.Add("Phases");
            this.id26_newName = base.Reader.NameTable.Add("newName");
            this.id30_Value = base.Reader.NameTable.Add("Value");
            this.id2_Item = base.Reader.NameTable.Add("http://schemas.postsharp.org/1.0/configuration");
            this.id44_Ordinal = base.Reader.NameTable.Add("Ordinal");
            this.id27_name = base.Reader.NameTable.Add("name");
            this.id29_culture = base.Reader.NameTable.Add("culture");
            this.id21_assemblyIdentity = base.Reader.NameTable.Add("assemblyIdentity");
            this.id34_Parameter = base.Reader.NameTable.Add("Parameter");
            this.id13_AssemblyBinding = base.Reader.NameTable.Add("AssemblyBinding");
            this.id24_newVersion = base.Reader.NameTable.Add("newVersion");
            this.id15_Tasks = base.Reader.NameTable.Add("Tasks");
        }
    }
}
