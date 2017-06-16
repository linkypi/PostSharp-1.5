using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.IO;

namespace PostSharp.CustomTasks
{
    public class SplitPath : Task
    {

        private string path;

        [Required]
        public string Path
        {
            get { return path; }
            set { path = value; }
        }

        private string directory;

        [Output]
        public string Directory
        {
            get { return directory; }
        }

        private string fileName;

        [Output]
        public string FileName
        {
            get { return fileName; }
        }

        private string fileNameWithoutExtension;

        [Output]
        public string FileNameWithoutExtension
        {
            get { return fileNameWithoutExtension; }
        }

        private string extension;

        [Output]
        public string Extension
        {
            get { return extension; }
        }
	
	

        public override bool Execute()
        {
            this.directory = System.IO.Path.GetDirectoryName(this.path);
            this.fileName = System.IO.Path.GetFileName(this.path);
            this.fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(this.path);
            this.extension = System.IO.Path.GetExtension( this.path ).Trim( '.' );
            return true;
        }
	
    }
}
