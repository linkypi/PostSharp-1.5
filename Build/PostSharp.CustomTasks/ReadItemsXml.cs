using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.Xml.XPath;

namespace PostSharp.CustomTasks
{
    public class ReadItemsXml : Task
    {
        private ITaskItem fileName;

        [Required]
        public ITaskItem FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }

        private ITaskItem[] items;

        [Output]
        public ITaskItem[] Items
        {
            get { return items; }
            set { items = value; }
        }

        public override bool Execute()
        {
            List<ITaskItem> items = new List<ITaskItem>();

            XPathDocument document = new XPathDocument(this.fileName.ItemSpec);
            XPathNavigator navigator = document.CreateNavigator();
            foreach (XPathNavigator node in navigator.Select("/Items/Item"))
            {
                TaskItem item = new TaskItem(node.Value);
                foreach (XPathNavigator attribute in node.Select("@*"))
                {
                    item.SetMetadata(attribute.LocalName, attribute.Value);
                }
                items.Add(item);
            }

            this.items = items.ToArray();

            return true;
        }
	
	
    }
}
