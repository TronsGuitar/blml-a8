using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Diagnostics;

class Program
{
    static void Main(string[] args)
    {
        string ocxFilePath = @"C:\path\to\your\control.ocx";
        string interopDllPath = @"C:\path\to\your\Interop.YourControl.dll";

        GenerateInteropAndInspect(ocxFilePath, interopDllPath);

        string tlbFilePath = @"C:\path\to\your\library.tlb";
        string interopTlbPath = @"C:\path\to\your\Interop.YourLibrary.dll";

        GenerateInteropAndInspect(tlbFilePath, interopTlbPath);

        string dllFilePath = @"C:\path\to\your\library.dll";
        InspectDllFile(dllFilePath);
    }

    static void GenerateInteropAndInspect(string inputFilePath, string outputDllPath)
    {
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
                Console.WriteLine("Failed to generate interop assembly.");
                return;
            }

            Console.WriteLine("Interop assembly generated successfully.\n");

            // Load the COM Type Library
            Assembly assembly = Assembly.LoadFile(outputDllPath);

            // Get all types in the assembly
            Type[] types = assembly.GetTypes();

            // Create lists to hold properties and methods
            List<string> properties = new List<string>();
            List<string> methods = new List<string>();

            foreach (Type type in types)
            {
                Console.WriteLine($"Inspecting type: {type.Name}\n");

                // Get the properties of the type
                foreach (PropertyInfo prop in type.GetProperties())
                {
                    properties.Add(prop.Name);
                    Console.WriteLine($"Property: {prop.Name}");
                }

                // Get the methods of the type
                foreach (MethodInfo method in type.GetMethods())
                {
                    methods.Add(method.Name);
                    Console.WriteLine($"Method: {method.Name}");
                }
            }

            // Display the results
            Console.WriteLine("\nList of Properties:");
            properties.ForEach(Console.WriteLine);

            Console.WriteLine("\nList of Methods:");
            methods.ForEach(Console.WriteLine);
        }
        catch (COMException comEx)
        {
            Console.WriteLine($"COM error: {comEx.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"General error: {ex.Message}");
        }
    }

    static void InspectDllFile(string dllFilePath)
    {
        try
        {
            Assembly assembly = null;

            // Try to load as a .NET assembly
            try
            {
                assembly = Assembly.LoadFile(dllFilePath);
                Console.WriteLine("Loaded as a .NET assembly.\n");
            }
            catch (BadImageFormatException)
            {
                Console.WriteLine("Not a .NET assembly. Attempting to load as a COM object...\n");

                // If not a .NET assembly, try to generate an interop assembly using tlbimp
                string interopDllPath = dllFilePath.Replace(".dll", ".Interop.dll");
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
                    Console.WriteLine("Failed to generate interop assembly for COM object.");
                    return;
                }

                Console.WriteLine("Interop assembly for COM object generated successfully.\n");
                assembly = Assembly.LoadFile(interopDllPath);
            }

            // Get all types in the assembly
            Type[] types = assembly.GetTypes();

            // Create lists to hold properties and methods
            List<string> properties = new List<string>();
            List<string> methods = new List<string>();

            foreach (Type type in types)
            {
                Console.WriteLine($"Inspecting type: {type.Name}\n");

                // Get the properties of the type
                foreach (PropertyInfo prop in type.GetProperties())
                {
                    properties.Add(prop.Name);
                    Console.WriteLine($"Property: {prop.Name}");
                }

                // Get the methods of the type
                foreach (MethodInfo method in type.GetMethods())
                {
                    methods.Add(method.Name);
                    Console.WriteLine($"Method: {method.Name}");
                }
            }

            // Display the results
            Console.WriteLine("\nList of Properties:");
            properties.ForEach(Console.WriteLine);

            Console.WriteLine("\nList of Methods:");
            methods.ForEach(Console.WriteLine);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"General error: {ex.Message}");
        }
    }
}

// Note:
// You need to have tlbimp.exe available in your system PATH or specify the full path to tlbimp.
// Replace "C:\path\to\your\control.ocx", "C:\path\to\your\library.tlb", and "C:\path\to\your\library.dll" with the actual paths to your files.
