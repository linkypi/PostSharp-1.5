using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace PostSharp.CustomTasks
{
    public class RegexMatch : Task
    {
        private ITaskItem[] files;
        private string expression;
        private ITaskItem[] matches;

        [Output]
        public ITaskItem[] Matches { get { return matches; } set { matches = value; } }

        [Required]
        public string Expression { get { return expression; } set { expression = value; } }

        [Required]
        public ITaskItem[] Files { get { return files; } set { files = value; } }

        public override bool Execute()
        {
            Regex regex = new Regex( this.expression, RegexOptions.Multiline );

            foreach ( ITaskItem file in this.files )
            {
                string content = File.ReadAllText( file.ItemSpec );
                MatchCollection regexMatches = regex.Matches( content );
                List<ITaskItem> matchesList = new List<ITaskItem>();

                foreach ( Match match in regexMatches )
                {
                    matchesList.Add( new TaskItem( match.Groups[1].Value ) );
                }

                this.matches = matchesList.ToArray();
            }

            return true;
        }
    }
}