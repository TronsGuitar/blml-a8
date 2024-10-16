using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Diagnostics;

public class LibraryInspector
{
    public List<string> Types { get; private set; } = new List<string>();
    public List<string> Properties { get; private set; } = new List<string>();
    public List<string> Methods { get; private set; } = new List<string>();
    public string LibraryName { get; private set; }

    public void InspectLibrary(string filePath)
    {
        LibraryName = System.IO.Path.GetFileName(filePath);
        string extension = System.IO.Path.GetExtension(filePath).ToLower();
        switch (extension)
        {
            case ".ocx":
            case ".tlb":
                string interopDllPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, System.IO.Path.GetFileNameWithoutExtension(filePath) + $".Interop{extension}.dll");
                GenerateInteropAndInspect(filePath, interopDllPath);
                break;
            case ".dll":
                InspectDllFile(filePath);
                break;
            default:
                Debug.WriteLine("Unsupported file type.");
                break;
        }
    }

    private void GenerateInteropAndInspect(string inputFilePath, string outputDllPath)
    {
        // List of known libraries to skip
        List<string> exceptionLibraries = new List<string>
        {
            "stdole2.tlb",
            "sccrun.tlb",
            "msbind.dll",
            "msdatsrc.tlb",
            "msadodc.ocx",
            "mscomctl.ocx",
            "mscomct2.ocx"
        };
        // Skip generation for stdole2.tlb as it is already provided by the system
        if (exceptionLibraries.Contains(System.IO.Path.GetFileName(inputFilePath).ToLower())
        {
            Debug.WriteLine("Skipping generation for known exception library as it is already provided by the system or is known to cause issues.");
            return;
        }
        try
        {
            // Generate the .NET interop assembly using tlbimp
            Process tlbimpProcess = new Process();
            tlbimpProcess.StartInfo.FileName = "tlbimp";
            tlbimpProcess.StartInfo.Arguments = $"\"{inputFilePath}\" /out:\"{outputDllPath}\"";
            tlbimpProcess.StartInfo.RedirectStandardOutput = true;
            tlbimpProcess.StartInfo.UseShellExecute = false;
            tlbimpProcess.StartInfo.CreateNoWindow = true;
            tlbimpProcess.Start();
            tlbimpProcess.WaitForExit();

            if (tlbimpProcess.ExitCode != 0)
            {
                Debug.WriteLine("Failed to generate interop assembly.");
                return;
            }

            Debug.WriteLine("Interop assembly generated successfully.\n");

            // Load the COM Type Library
            Assembly assembly = Assembly.LoadFile(outputDllPath);

            InspectAssembly(assembly);
        }
        catch (COMException comEx)
        {
            Debug.WriteLine($"COM error: {comEx.Message}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"General error: {ex.Message}");
        }
    }

    private void InspectDllFile(string dllFilePath)
    {
        try
        {
            Assembly assembly = null;

            // Try to load as a .NET assembly
            try
            {
                assembly = Assembly.LoadFile(dllFilePath);
                Debug.WriteLine("Loaded as a .NET assembly.\n");
            }
            catch (BadImageFormatException)
            {
                Debug.WriteLine("Not a .NET assembly. Attempting to load as a COM object...\n");

                // If not a .NET assembly, try to generate an interop assembly using tlbimp
                string interopDllPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, System.IO.Path.GetFileNameWithoutExtension(dllFilePath) + ".Interop.dll");
                Process tlbimpProcess = new Process();
                tlbimpProcess.StartInfo.FileName = "tlbimp";
                tlbimpProcess.StartInfo.Arguments = $"\"{dllFilePath}\" /out:\"{interopDllPath}\"";
                tlbimpProcess.StartInfo.RedirectStandardOutput = true;
                tlbimpProcess.StartInfo.UseShellExecute = false;
                tlbimpProcess.StartInfo.CreateNoWindow = true;
                tlbimpProcess.Start();
                tlbimpProcess.WaitForExit();

                if (tlbimpProcess.ExitCode != 0)
                {
                    Debug.WriteLine("Failed to generate interop assembly for COM object.");
                    return;
                }

                Debug.WriteLine("Interop assembly for COM object generated successfully.\n");
                assembly = Assembly.LoadFile(interopDllPath);
            }

            InspectAssembly(assembly);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"General error: {ex.Message}");
        }
    }

    private void InspectAssembly(Assembly assembly)
    {
        try
        {
            // Get all types in the assembly
            Type[] types = assembly.GetTypes();

            foreach (Type type in types)
            {
                Types.Add(type.Name);
                Debug.WriteLine($"Inspecting type: {type.Name}\n");

                // Get the properties of the type
                foreach (PropertyInfo prop in type.GetProperties())
                {
                    Properties.Add(prop.Name);
                    Debug.WriteLine($"Property: {prop.Name}");
                }

                // Get the methods of the type
                foreach (MethodInfo method in type.GetMethods())
                {
                    Methods.Add(method.Name);
                    Debug.WriteLine($"Method: {method.Name}");
                }
            }

            // Display the results
            Debug.WriteLine("\nList of Types:");
            Types.ForEach(type => Debug.WriteLine(type));

            Debug.WriteLine("\nList of Properties:");
            Properties.ForEach(property => Debug.WriteLine(property));

            Debug.WriteLine("\nList of Methods:");
            Methods.ForEach(method => Debug.WriteLine(method));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"General error while inspecting assembly: {ex.Message}");
        }
    }
}

// Note:
// You need to have tlbimp.exe available in your system PATH or specify the full path to tlbimp.
// Replace "C:\path\to\your\control.ocx", "C:\path\to\your\library.tlb", and "C:\path\to\your\library.dll" with the actual paths to your files.
