using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.Text.RegularExpressions;
using System.IO;

namespace PostSharp.CustomTasks
{
    public sealed class ReplaceInFiles : Task
    {
        private ITaskItem[] files;

        [Required]
        public ITaskItem[] Files
        {
            get { return files; }
            set { files = value; }
        }

        private string pattern;

        [Required]
        public string Pattern
        {
            get { return pattern; }
            set { pattern = value; }
        }

        private string replace;

        public string Replace
        {
            get { return replace; }
            set { replace = value; }
        }


        private string encoding;

        public string Encoding
        {
            get { return encoding; }
            set { encoding = value; }
        }
	
	
	
	
        public override bool Execute()
        {
          
            Regex regex = new Regex(this.pattern);
            string replaceNotNull = this.replace == null ? "" : this.replace;

            for (int i = 0; i < this.files.Length; i++)
            {
                string outputFile = this.files[i].GetMetadata("Output");
                bool copyBack;
                if ( string.IsNullOrEmpty( outputFile ))
                {
                    outputFile = Path.GetTempFileName();
                    copyBack = true;
                }
                else
                {
                    copyBack = false;
                }

                Encoding encoding;
                if (string.IsNullOrEmpty(this.encoding))
                {
                    encoding = System.Text.Encoding.Default;
                }
                else
                {
                    encoding = System.Text.Encoding.GetEncoding(this.encoding);
                }


                using (StreamReader reader = new StreamReader(this.files[i].ItemSpec, encoding))
                using (StreamWriter writer = new StreamWriter(outputFile, false, encoding))
                {
                    string inputLine;
                    int count = 0;

                    while ((inputLine = reader.ReadLine()) != null)
                    {
                        string outputLine = regex.Replace(inputLine, replaceNotNull);
                        if (inputLine != outputLine) count++;
                        writer.WriteLine(outputLine);
                    }

                    this.Log.LogMessage("{0} line(s) replaced in {1}.", count, this.files[i].ItemSpec);

                    writer.Flush();
                }

                if (copyBack)
                {
                    File.Copy(outputFile, this.files[i].ItemSpec, true);
                    File.Delete(outputFile);
                }
            }

            return true;
        }
    }
}
