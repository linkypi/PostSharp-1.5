using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;

namespace PostSharp.CustomTasks
{
    public sealed class GenerateItemsXml : Task
    {
        private ITaskItem[] items;

        [Required]
        public ITaskItem[] Items
        {
            get { return items; }
            set { items = value; }
        }

        private string fileName;

        [Required]
        public string FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }
	
	
        public override bool Execute()
        {
            using (XmlTextWriter writer = new XmlTextWriter(this.fileName, Encoding.UTF8))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("Items");
                writer.WriteAttributeString("Count", XmlConvert.ToString( this.items.Length));

                foreach (ITaskItem item in this.items)
                {
                    writer.WriteStartElement("Item");
                    foreach (string metaDataName in item.MetadataNames)
                    {
                        writer.WriteAttributeString(metaDataName, item.GetMetadata(metaDataName));
                    }
                    writer.WriteString(item.ItemSpec);
                    writer.WriteEndElement();

                }

                writer.WriteEndElement();
                writer.Close();
            }

            return true;
        }
    }
}
