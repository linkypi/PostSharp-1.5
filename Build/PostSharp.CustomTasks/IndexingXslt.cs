#region Original copyright and license
// Based on MSBuildCommunityTasks, released under BSD
// $Id: Xslt.cs 171 2006-05-19 08:15:49Z iko $
// Copyright © 2006 Ignaz Kohlbecker

/*
 BSD License

Copyright © 2006 Ignaz Kohlbecker, all rights reserved.

Redistribution and use in source and binary forms, with or without modification, 
are permitted provided that the following conditions are met:

·	Redistributions of source code must retain the above copyright notice, 
    this list of conditions and the following disclaimer.
·	Redistributions in binary form must reproduce the above copyright notice, 
    this list of conditions and the following disclaimer in the documentation and/or other 
    materials provided with the distribution.
·	Neither the name of the organization nor the names of its contributors may be used 
    to endorse or promote products derived from this software without specific 
    prior written permission.

    THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY 
    EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF 
    MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL
    THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, 
    EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF 
    SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) 
    HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, 
    OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
    EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*/
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Xml.Schema;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace PostSharp.CustomTasks
{

    public class IndexingXslt : Task
    {
        #region Constants

        /// <summary>
        /// The name of the default attribute
        /// of the <see cref="RootTag"/>.
        /// The value is <c>"created"</c>,
        /// and the attribute will contain a local time stamp.
        /// </summary>
        public const string CREATED_ATTRIBUTE = @"created";

        #endregion Constants

        #region Fields
        private ITaskItem[] indexFiles;
        private ITaskItem[] inputs;
        private string xsl;
        private string arguments;
        #endregion Fields

        #region Input Parameters

        /// <summary>
        /// Gets or sets the xml input files.
        /// </summary>
        [Required]
        public ITaskItem[] Inputs
        {
            get { return inputs; }
            set { inputs = value; }
        }

        public ITaskItem[] IndexedFiles
        {
            get { return this.indexFiles; }
            set { this.indexFiles = value; }
        }

        /// <summary>
        /// Gets or sets the path of the
        /// xsl transformation file to apply.
        /// </summary>
        /// <remarks>
        /// The property can be given any number of metadata,
        /// which will be handed to the xsl transformation
        /// as parameters.
        /// </remarks>
        public string Xsl
        {
            get { return xsl; }
            set { xsl = value; }
        }

        public string Arguments
        {
            get { return this.arguments; }
            set { this.arguments = value; }
        }

        #endregion Input Parameters

        #region Task overrides
        /// <summary>
        /// When overridden in a derived class, executes the task.
        /// </summary>
        /// <returns>
        /// Returns <c>true</c> if the task successfully executed; otherwise, <c>false</c>.
        /// </returns>
        public override bool Execute()
        {
            #region Sanity checks
            if ((this.inputs == null) || (this.inputs.Length == 0))
            {
                if (this.BuildEngine != null) Log.LogError("No input files.");
                return false;
            }

            #endregion Sanity checks

            #region Load indexed files
            // The working document
            XmlDocument indexedFilesDoc = new XmlDocument();
            XmlElement indexedFilesRootElement = indexedFilesDoc.CreateElement("files");
            indexedFilesDoc.AppendChild(indexedFilesRootElement);
            if (this.indexFiles != null)
            {
                foreach (ITaskItem indexItem in this.indexFiles)
                {
                    if (this.BuildEngine != null) Log.LogMessage("Loading the indexed file {0}.", indexItem.ItemSpec);

                    XmlDocument indexFile = new XmlDocument();
                    indexFile.Load(indexItem.ItemSpec);
                    XmlElement indexElement = indexedFilesDoc.CreateElement("file");
                    indexElement.SetAttribute("href", indexItem.ItemSpec);
                    indexedFilesRootElement.AppendChild(indexElement);
                    indexElement.AppendChild(indexedFilesDoc.ImportNode(indexFile.DocumentElement, true));

                }
            }
            #endregion


            #region Load the XSLT and create the indexes
            IndexManager indexManager = new IndexManager();

            XmlDocument xsltDocument = new XmlDocument();
            xsltDocument.Load(this.xsl);
            foreach (XmlProcessingInstruction indexInstruction in
                xsltDocument.SelectNodes("//processing-instruction()[local-name()='index']"))
            {

                string indexName;
                string keyExpression;
                string nodesExpression;

                try
                {
                    XmlReaderSettings readerSettings = new XmlReaderSettings();
                    readerSettings.ConformanceLevel = ConformanceLevel.Fragment;
                    readerSettings.ValidationFlags = XmlSchemaValidationFlags.None;
                    readerSettings.ValidationType = ValidationType.None;
                    string dummyElementText = "<index " + indexInstruction.Value + "/>";

                    XmlReader reader = XmlTextReader.Create(new StringReader(dummyElementText),
                        readerSettings);
                    reader.Read();
                    indexName = reader.GetAttribute("name");
                    keyExpression = reader.GetAttribute("key");
                    nodesExpression = reader.GetAttribute("nodes");
                }
                catch (XmlException e)
                {
                    if (this.BuildEngine != null) this.Log.LogError("Cannot parse the <?index {0} ?> processing instruction: {1}", indexInstruction.Value, e.Message);
                    return false;
                }

                if (string.IsNullOrEmpty(indexName))
                {
                    if (this.BuildEngine != null) this.Log.LogError("Missing 'name' attribute on an <?index ?> processing instruction.");
                    return false;
                }

                if (string.IsNullOrEmpty(keyExpression))
                {
                    if (this.BuildEngine != null) this.Log.LogError("Missing 'key' attribute on an <?index ?> processing instruction.");
                    return false;
                }

                if (string.IsNullOrEmpty(nodesExpression))
                {
                    if (this.BuildEngine != null) this.Log.LogError("Missing 'nodes' attribute on an <?index ?> processing instruction.");
                    return false;
                }

                if (this.BuildEngine != null) this.Log.LogMessage(MessageImportance.Normal, "Creating the index {0}.", indexName);
                int count = indexManager.CreateIndex(indexName, nodesExpression, keyExpression, indexedFilesDoc);
                if (this.BuildEngine != null) this.Log.LogMessage(MessageImportance.Normal, "{0} entries indexed.", count);
            }
            #endregion

            #region Create the transform
            XsltSettings xsltSettings = new XsltSettings(true, true);
            XslCompiledTransform transform = new XslCompiledTransform();
            transform.Load(xsl, xsltSettings, new XmlUrlResolver());
            XsltArgumentList argumentList = new XsltArgumentList();
            if (!string.IsNullOrEmpty(this.arguments))
            {
                foreach (string argument in this.arguments.Split(';'))
                {
                    string[] keyValuePair = argument.Split('=');

                    Log.LogMessage(MessageImportance.Normal, "Setting argument {0}=\"{1}\".",
                        keyValuePair[0], keyValuePair[1]);

                    argumentList.AddParam(keyValuePair[0], "", keyValuePair[1]);
                }
            }
            argumentList.XsltMessageEncountered += new XsltMessageEncounteredEventHandler(this.OnXsltMessage);
            // Add the index manager.
            argumentList.AddExtensionObject("urn:indexManager", indexManager);

            #endregion


            #region Create and execute the transform
            for (int i = 0; i < this.inputs.Length; i++)
            {

                bool copyBack = false;
                string outputFile = this.inputs[i].GetMetadata("Output");
                if (string.IsNullOrEmpty(outputFile))
                {
                    outputFile = Path.GetTempFileName();
                    copyBack = true;
                    if (this.BuildEngine != null) Log.LogMessage("Transforming {{{0}}} -> {{{0}}}.", this.inputs[i].ItemSpec);
                }
                else
                {
                    string directory = Path.GetDirectoryName(outputFile);
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                    if (this.BuildEngine != null) Log.LogMessage("Transforming {{{0}}} -> {{{1}}}.", this.inputs[i].ItemSpec, outputFile);
                }

                XmlDocument inputDocument = new XmlDocument();
                using (TextReader reader = new StreamReader(this.inputs[i].ItemSpec, Encoding.UTF8))
                {
                    inputDocument.Load(reader);
                }


                XmlWriter xmlWriter = null;
                List<string> newArguments = new List<string>();
                foreach (string metadataName in this.inputs[i].MetadataNames)
                {

                    if (metadataName.ToLowerInvariant().StartsWith("xsltparam_"))
                    {
                        string name = metadataName.Substring("XsltParam_".Length);
                        string value = this.inputs[i].GetMetadata(metadataName);
                        newArguments.Add(name);
                        argumentList.AddParam(name, "", value);
                        this.Log.LogMessage(MessageImportance.Low, "  with {0}={1}", name, value);
                    }
                }


                try
                {
                    xmlWriter = XmlWriter.Create(outputFile, transform.OutputSettings);
                    transform.Transform(inputDocument, argumentList, xmlWriter);

                }
                finally
                {
                    if (xmlWriter != null) xmlWriter.Close();
                }

                foreach ( string argument in newArguments )
                {
                    argumentList.RemoveParam(argument, "");
                }

                if (copyBack)
                {
                    File.Copy(outputFile, this.inputs[i].ItemSpec, true);
                    File.Delete(outputFile);
                }
            }

            #endregion Create and execute the transform

            return true;
        }

        void OnXsltMessage(object sender, XsltMessageEncounteredEventArgs e)
        {
            if (this.BuildEngine != null) Log.LogMessage("[xslt] " + e.Message);

        }

        #endregion Task overrides

        public class IndexManager
        {
            Dictionary<string, Dictionary<string, IndexedNodeList>> indexes = new Dictionary<string, Dictionary<string, IndexedNodeList>>();

            internal int CreateIndex(string indexName, string indexedNodesExpression, string indexKeyExpression, XmlDocument document)
            {
                int count = 0;
                Dictionary<string, IndexedNodeList> index = new Dictionary<string, IndexedNodeList>();
                XPathExpression indexKeyExpressionCompiled = XPathExpression.Compile(indexKeyExpression);
                XPathNavigator navigator = document.CreateNavigator();

                foreach (XmlNode node in document.SelectNodes(indexedNodesExpression))
                {
                    XPathNavigator nodeNavigator = node.CreateNavigator();
                    XPathNavigator keyNode = nodeNavigator.SelectSingleNode(indexKeyExpression);
                    if (keyNode != null)
                    {
                        string keyValue = keyNode.Value;
                        if (!string.IsNullOrEmpty(keyValue))
                        {
                            IndexedNodeList nodeList;
                            if (!index.TryGetValue(keyValue, out nodeList))
                            {
                                nodeList = new IndexedNodeList();
                                index.Add(keyValue, nodeList);
                            }
                            nodeList.Add(nodeNavigator);

                        }
                    }

                    count++;

                }

                this.indexes.Add(indexName, index);

                return count;
            }

            static IndexedNodeList emptyNodeList = new IndexedNodeList();
            public XPathNodeIterator getIndexedNode(string indexName, string key)
            {
                if (string.IsNullOrEmpty(indexName)) throw new ArgumentNullException("indexName");
                if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("key");

                Dictionary<string, IndexedNodeList> index;

                if (!this.indexes.TryGetValue(indexName, out index))
                {
                    throw new ArgumentException("Invalid index name: " + indexName + ".");
                }

                IndexedNodeList nodeList;
                if (!index.TryGetValue(key, out nodeList))
                {
                    return emptyNodeList;
                }

                return nodeList;

            }

            private class IndexedNodeList : XPathNodeIterator
            {
                List<XPathNavigator> list;
                int current = -1;

                internal void Add(XPathNavigator element)
                {
                    this.list.Add(element);
                }

                private IndexedNodeList(IndexedNodeList original)
                {
                    this.list = original.list;
                }

                public IndexedNodeList()
                {
                    this.list = new List<XPathNavigator>();
                }

                public override XPathNodeIterator Clone()
                {
                    return new IndexedNodeList(this);

                }

                public override XPathNavigator Current
                {
                    get
                    {
                        if (this.current == -1)
                        {
                            return null;
                        }
                        return this.list[this.current];
                    }
                }

                public override int CurrentPosition
                {
                    get { return this.current; }
                }

                public override bool MoveNext()
                {
                    if (this.current < this.list.Count - 1)
                    {
                        this.current++;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

        }

    }
}