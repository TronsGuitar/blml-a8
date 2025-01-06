using System;

namespace Vb6FormParser.Models
{
    public class Vb6ControlInfo
    {
        public string FormFileName { get; set; }     // e.g., "MyForm.frm"
        public string ControlType { get; set; }      // e.g., "VB.CommandButton"
        public string ControlName { get; set; }      // e.g., "Command1"
        public string Guid { get; set; }             // e.g., "{0002E55F-0000-0000-C000-000000000046}"
    }
}
