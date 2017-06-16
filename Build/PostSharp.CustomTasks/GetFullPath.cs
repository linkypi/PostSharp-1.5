using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.IO;

namespace PostSharp.CustomTasks
{
    public class GetFullPath : Task
    {

        private string relativePath;

        [Required]
        public string RelativePath
        {
            get { return relativePath; }
            set { relativePath = value; }
        }

        private string fullPath;

        [Output]
        public string FullPath
        {
            get { return fullPath; }
        }

       


        public override bool Execute()
        {
            this.fullPath = System.IO.Path.GetFullPath(this.relativePath);
            return true;
        }

    }
}
