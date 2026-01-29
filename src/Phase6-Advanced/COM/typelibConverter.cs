using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace TlbToAssembly
{
    class Program
    {
        static void Main(string[] args)
        {
            string tlbPath = @"C:\Path\To\Your\ComLibrary.tlb";
            string assemblyPath = @"C:\Path\To\Your\ComLibrary.Interop.dll";

            // Create an instance of TypeLibConverter
            TypeLibConverter converter = new TypeLibConverter();

            // Implement ITypeLibImporterNotifySink to handle events during conversion
            ITypeLibImporterNotifySink notifySink = new TypeLibImporterNotifySink();

            // Load the type library
            object typeLib;
            LoadTypeLib(tlbPath, out typeLib);

            // Convert the type library to an interop assembly
            AssemblyBuilder asmBuilder = converter.ConvertTypeLibToAssembly(
                typeLib,
                assemblyPath,
                TypeLibImporterFlags.None,
                notifySink,
                null,
                null,
                null,
                null);

            // Save the assembly to disk
            asmBuilder.Save(System.IO.Path.GetFileName(assemblyPath));

            Console.WriteLine("Interop assembly generated successfully.");
        }

        // Helper method to load the type library
        [DllImport("oleaut32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        private static extern void LoadTypeLib(string fileName, out object typeLib);

        // Implementation of ITypeLibImporterNotifySink
        public class TypeLibImporterNotifySink : ITypeLibImporterNotifySink
        {
            public void ReportEvent(ImporterEventKind eventKind, int eventCode, string eventMsg)
            {
                Console.WriteLine($"Event: {eventMsg}");
            }

            public Assembly ResolveRef(object typeLib)
            {
                // Handle references to other type libraries if necessary
                return null;
            }
        }
    }
}
