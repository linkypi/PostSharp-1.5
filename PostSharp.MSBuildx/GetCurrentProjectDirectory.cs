using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace PostSharp.MSBuild
{
    /// <summary>
    /// <b>[MSBuild Task]</b> Gets the directory containing the current project 
    /// or targets file.
    /// </summary>
    public sealed class PostSharp15GetCurrentProjectDirectory : Task
    {
        private ITaskItem directory;

        /// <summary>
        /// After task execution, gets the directory containing the project or
        /// target file that invoked the task.
        /// </summary>
        [Output]
        public ITaskItem Directory
        {
            get { return directory; }
            set { directory = value; }
        }

        /// <inheritdoc />
        public override bool Execute()
        {
            FileInfo projFile = new FileInfo(this.BuildEngine.ProjectFileOfTaskNode);

            this.directory = new TaskItem(projFile.Directory.FullName);


            return true;
        }
    }
}