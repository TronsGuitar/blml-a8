namespace BLML.Phase1Foundation.ProjectModel
{
    public class VB6Project
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Startup { get; set; }
        public string HelpFile { get; set; }
        public string Title { get; set; }
        public string ExeName32 { get; set; }
        public string Command32 { get; set; }

        public List<string> Forms { get; } = new List<string>();
        public List<string> Modules { get; } = new List<string>();
        public List<string> Classes { get; } = new List<string>();
        public List<string> UserControls { get; } = new List<string>();

        public List<VB6Reference> References { get; } = new List<VB6Reference>();
        public List<VB6ObjectReference> Objects { get; } = new List<VB6ObjectReference>();

        public Dictionary<string, string> VersionInfo { get; } = new Dictionary<string, string>();
        public Dictionary<string, string> Settings { get; } = new Dictionary<string, string>();
    }

    public class VB6Reference
    {
        public string Guid { get; set; }
        public string Version { get; set; }
        public string Lcid { get; set; }
        public string Path { get; set; }
        public string Description { get; set; }
    }

    public class VB6ObjectReference
    {
        public string Guid { get; set; }
        public string Version { get; set; }
        public string Lcid { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }
    }
}
