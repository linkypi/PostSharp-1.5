using System;

namespace PostSharp.MSBuild
{
    [Serializable]
    internal class PostSharpTaskParameters
    {
        public string Project;
        public string Parameters;
        public bool NoLogo;
        public bool Verbose;
        public string Input;
        public bool AutoUpdateDisabled;
        public string InputReferenceDirectory;
        public bool AttachDebugger;
        public bool DisableReflection;
    }
}