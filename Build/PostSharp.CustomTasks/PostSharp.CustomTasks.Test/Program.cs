using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;

namespace PostSharp.CustomTasks.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
            GetRelativeDirectory getRelativeDirectory = new GetRelativeDirectory();
            getRelativeDirectory.SourceDirectory = @"C:\Windows\System32\drivers\";
            getRelativeDirectory.TargetDirectory = @"C:\WINDOWS\Wow64\";
            getRelativeDirectory.Execute();
            Console.WriteLine(getRelativeDirectory.RelativeDirectory);
             */

            /*
            TaskItem inputFile = new TaskItem("input.xml");
            inputFile.SetMetadata("Output", "output.xml");

            IndexingXslt indexingXsltTask = new IndexingXslt();
            indexingXsltTask.Inputs = new ITaskItem[] { inputFile };
            indexingXsltTask.Xsl = "stylesheet.xslt";
            indexingXsltTask.IndexedFiles = new ITaskItem[] { new TaskItem("indexedFile1.xml"),
                new TaskItem( "indexedFile2.xml" ) };

            indexingXsltTask.Execute();
             */

            AdjustSampleProjects task = new AdjustSampleProjects();
            TaskItem inputFile = new TaskItem(@"p:\branches\1.5\build\intermediate\release\wix\samples\postsharp.samples.Compact\PostSharp.Samples.Compact.csproj");
            task.Projects = new ITaskItem[] { inputFile};
            task.Execute();
            
            /*
            GenerateWixTree wixTree = new GenerateWixTree();
            wixTree.StructureFile = @"P:\branches\1.5\Private\Distribution\SamplesWix.xml";
            wixTree.OutputWixDirectory = @"P:\branches\1.5\Build\intermediate\Release\wix";
            wixTree.FilesBaseDirectory = @"P:\branches\1.5";
            wixTree.Files = new ITaskItem[0];
            wixTree.Execute();
            */

            
        }
    }
}
