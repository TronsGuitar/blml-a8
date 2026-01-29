using System;

namespace BLML.Phase1Foundation.ProjectModel
{
    public class ProjectFileParser
    {
        /* TODO: Implementation Logic
         * 1. Load and parse .vbp (Visual Basic Project) files.
         * 2. Extract references (DLLs/OCXs) and their GUIDs.
         * 3. List all Modules (.bas), Classes (.cls), and Forms (.frm).
         * 4. Identify the Startup Object (Sub Main or Form).
         * 5. Map conditional compilation constants and project settings.
         */
<<<<<<< HEAD
        public VB6Project Parse(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
                throw new System.IO.FileNotFoundException("Project file not found", filePath);

            var project = new VB6Project();
            var lines = System.IO.File.ReadAllLines(filePath);

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                int eqIndex = line.IndexOf('=');
                if (eqIndex <= 0) continue;

                string key = line.Substring(0, eqIndex).Trim();
                string value = line.Substring(eqIndex + 1).Trim();

                ParseLine(project, key, value);
            }

            return project;
        }

        private void ParseLine(VB6Project project, string key, string value)
        {
            switch (key.ToLowerInvariant())
            {
                case "name":
                    project.Name = value.Trim('"');
                    break;
                case "type":
                    project.Type = value;
                    break;
                case "startup":
                    project.Startup = value.Trim('"');
                    break;
                case "form":
                    project.Forms.Add(value);
                    break;
                case "module":
                    // Format: ModuleName; FileName.bas
                    var modParts = value.Split(';');
                    project.Modules.Add(modParts.Length > 1 ? modParts[1].Trim() : modParts[0].Trim());
                    break;
                case "class":
                    // Format: ClassName; FileName.cls
                    var clsParts = value.Split(';');
                    project.Classes.Add(clsParts.Length > 1 ? clsParts[1].Trim() : clsParts[0].Trim());
                    break;
                case "usercontrol":
                    project.UserControls.Add(value);
                    break;
                case "reference":
                    project.References.Add(ParseReference(value));
                    break;
                case "object":
                    project.Objects.Add(ParseObject(value));
                    break;
                case "versioncompanyname":
                case "versionproductname":
                case "versioncopyright":
                case "versiondescription":
                    project.VersionInfo[key] = value.Trim('"');
                    break;
                default:
                    project.Settings[key] = value;
                    break;
            }
        }

        private VB6Reference ParseReference(string value)
        {
            // Format: *\G{GUID}#Ver#Lcid#Path#Description
            var reference = new VB6Reference();
            var parts = value.Split('#');
            if (parts.Length >= 5)
            {
                reference.Guid = parts[0].Replace("*\\G", "").Trim();
                reference.Version = parts[1];
                reference.Lcid = parts[2];
                reference.Path = parts[3];
                reference.Description = parts[4];
            }
            return reference;
        }

        private VB6ObjectReference ParseObject(string value)
        {
            // Format: {GUID}#Ver#Lcid; FileName.ocx
            var objRef = new VB6ObjectReference();
            var semiIndex = value.IndexOf(';');
            if (semiIndex > 0)
            {
                objRef.Name = value.Substring(semiIndex + 1).Trim();
                var parts = value.Substring(0, semiIndex).Split('#');
                if (parts.Length >= 3)
                {
                    objRef.Guid = parts[0];
                    objRef.Version = parts[1];
                    objRef.Lcid = parts[2];
                }
            }
            return objRef;
=======
        public ProjectFileParser()
        {
>>>>>>> 2e0740d (The prototype files)
        }

        private void ParseLine(VB6Project project, string key, string value)
        {
            switch (key.ToLowerInvariant())
            {
                case "name":
                    project.Name = value.Trim('"');
                    break;
                case "type":
                    project.Type = value;
                    break;
                case "startup":
                    project.Startup = value.Trim('"');
                    break;
                case "form":
                    project.Forms.Add(value);
                    break;
                case "module":
                    // Format: ModuleName; FileName.bas
                    var modParts = value.Split(';');
                    project.Modules.Add(modParts.Length > 1 ? modParts[1].Trim() : modParts[0].Trim());
                    break;
                case "class":
                    // Format: ClassName; FileName.cls
                    var clsParts = value.Split(';');
                    project.Classes.Add(clsParts.Length > 1 ? clsParts[1].Trim() : clsParts[0].Trim());
                    break;
                case "usercontrol":
                    project.UserControls.Add(value);
                    break;
                case "reference":
                    project.References.Add(ParseReference(value));
                    break;
                case "object":
                    project.Objects.Add(ParseObject(value));
                    break;
                case "versioncompanyname":
                case "versionproductname":
                case "versioncopyright":
                case "versiondescription":
                    project.VersionInfo[key] = value.Trim('"');
                    break;
                default:
                    project.Settings[key] = value;
                    break;
            }
        }

        private VB6Reference ParseReference(string value)
        {
            // Format: *\G{GUID}#Ver#Lcid#Path#Description
            var reference = new VB6Reference();
            var parts = value.Split('#');
            if (parts.Length >= 5)
            {
                reference.Guid = parts[0].Replace("*\\G", "").Trim();
                reference.Version = parts[1];
                reference.Lcid = parts[2];
                reference.Path = parts[3];
                reference.Description = parts[4];
            }
            return reference;
        }

        private VB6ObjectReference ParseObject(string value)
        {
            // Format: {GUID}#Ver#Lcid; FileName.ocx
            var objRef = new VB6ObjectReference();
            var semiIndex = value.IndexOf(';');
            if (semiIndex > 0)
            {
                objRef.Name = value.Substring(semiIndex + 1).Trim();
                var parts = value.Substring(0, semiIndex).Split('#');
                if (parts.Length >= 3)
                {
                    objRef.Guid = parts[0];
                    objRef.Version = parts[1];
                    objRef.Lcid = parts[2];
                }
            }
            return objRef;
        }
    }
}
