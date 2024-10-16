using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Diagnostics;

public class ReferencedLibrary
{
    public List<string> Types { get; private set; } = new List<string>();
    public List<string> Properties { get; private set; } = new List<string>();
    public List<string> Methods { get; private set; } = new List<string>();
    public string LibraryName { get; set; }

    public void LoadExistingData(string txtFilePath)
    {
        try
        {
            string[] lines = System.IO.File.ReadAllLines(txtFilePath);
            foreach (var line in lines)
            {
                if (line.StartsWith("Type: "))
                {
                    Types.Add(line.Substring(6));
                }
                else if (line.StartsWith("Property: "))
                {
                    Properties.Add(line.Substring(10));
                }
                else if (line.StartsWith("Method: "))
                {
                    Methods.Add(line.Substring(8));
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error while loading existing data: {ex.Message}");
        }
    }

    public void SaveAssemblyData(string txtFilePath)
    {
        try
        {
            using (var writer = new System.IO.StreamWriter(txtFilePath))
            {
                foreach (var type in Types)
                {
                    writer.WriteLine($"Type: {type}");
                }
                foreach (var property in Properties)
                {
                    writer.WriteLine($"Property: {property}");
                }
                foreach (var method in Methods)
                {
                    writer.WriteLine($"Method: {method}");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error while saving assembly data: {ex.Message}");
        }
    }
}

public class LibraryInspector
{
    private ReferencedLibrary referencedLibrary = new ReferencedLibrary();

    public void InspectLibrary(string filePath)
    {
        referencedLibrary.LibraryName = System.IO.Path.GetFileName(filePath);
        string extension = System.IO.Path.GetExtension(filePath).ToLower();
        string interopDllPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, System.IO.Path.GetFileNameWithoutExtension(filePath) + $".Interop{extension}.dll");

        switch (extension)
        {
            case ".ocx":
            case ".tlb":
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
        string txtFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, System.IO.Path.GetFileNameWithoutExtension(inputFilePath) + ".txt");
        
        // Check if the information file already exists
        if (System.IO.File.Exists(txtFilePath))
        {
            Debug.WriteLine("Information file already exists. Loading existing data.");
            referencedLibrary.LoadExistingData(txtFilePath);
            return;
        }
        // Check if library has already been inspected
        if (System.IO.File.Exists(outputDllPath))
        {
            Debug.WriteLine("Interop assembly already exists. Loading existing assembly.");
            Assembly existingAssembly = Assembly.LoadFile(outputDllPath);
            InspectAndSaveAssembly(existingAssembly, txtFilePath);
            return;
        }
        // List of known libraries to skip
        List<string> exceptionLibraries = new List<string>
        {
            "stdole2.tlb",
            "sccrun.tlb",
            "msbind.dll",
            "msdatsrc.tlb",
            "msadodc.ocx",
            "mscomctl.ocx",
            "mscomct2.ocx",
            "comdlg32.ocx",   // Common Dialog Control
            "richtx32.ocx",   // Rich Textbox Control
            "msinet.ocx",     // Microsoft Internet Transfer Control
            "tabctl32.ocx",   // Microsoft Tabbed Dialog Control
            "comctl32.ocx",   // Common Controls ActiveX Control
            "sysinfo.ocx",    // System Information Control
            "mci32.ocx",      // MCI Control
            "msmask32.ocx",   // Masked Edit Control
            "picclp32.ocx"    // Picture Clip Control
        };

        if (exceptionLibraries.Contains(System.IO.Path.GetFileName(inputFilePath).ToLower()))
        {
            Debug.WriteLine("Skipping generation for known exception library as it is already provided by the system or is known to cause issues.");
            return;
        }

        try
        {
            // Generate the .NET interop assembly using tlbimp
            Process tlbimpProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "tlbimp",
                    Arguments = $"\"{inputFilePath}\" /out:\"{outputDllPath}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            tlbimpProcess.Start();
            string output = tlbimpProcess.StandardOutput.ReadToEnd();
            Debug.WriteLine($"tlbimp output: {output}");
            tlbimpProcess.WaitForExit();

            if (tlbimpProcess.ExitCode != 0)
            {
                Debug.WriteLine("Failed to generate interop assembly.");
                return;
            }

            Debug.WriteLine("Interop assembly generated successfully.\n");

            // Load the COM Type Library
            Assembly assembly = Assembly.LoadFile(outputDllPath);

            InspectAndSaveAssembly(assembly, txtFilePath);
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

                bool is64BitProcess = Environment.Is64BitProcess;
                if (is64BitProcess && Is32BitLibrary(dllFilePath))
                {
                    Debug.WriteLine("Bitness mismatch: The process is 64-bit, but the library is 32-bit.");
                    return;
                }
                else if (!is64BitProcess && Is64BitLibrary(dllFilePath))
                {
                    Debug.WriteLine("Bitness mismatch: The process is 32-bit, but the library is 64-bit.");
                    return;
                }

                // If not a .NET assembly, try to generate an interop assembly using tlbimp
                string interopDllPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, System.IO.Path.GetFileNameWithoutExtension(dllFilePath) + ".Interop.dll");
                Process tlbimpProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "tlbimp",
                        Arguments = $"\"{dllFilePath}\" /out:\"{interopDllPath}\"",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                tlbimpProcess.Start();
                string output = tlbimpProcess.StandardOutput.ReadToEnd();
                Debug.WriteLine($"tlbimp output: {output}");
                tlbimpProcess.WaitForExit();

                if (tlbimpProcess.ExitCode != 0)
                {
                    Debug.WriteLine("Failed to generate interop assembly for COM object.");
                    return;
                }

                Debug.WriteLine("Interop assembly for COM object generated successfully.\n");
                assembly = Assembly.LoadFile(interopDllPath);
            }

            InspectAndSaveAssembly(assembly, System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, System.IO.Path.GetFileNameWithoutExtension(dllFilePath) + ".txt"));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"General error: {ex.Message}");
        }
    }

    private bool Is32BitLibrary(string filePath)
    {
        try
        {
            using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                byte[] data = new byte[4];
                stream.Seek(0x3C, System.IO.SeekOrigin.Begin);
                stream.Read(data, 0, 4);
                int peHeaderOffset = BitConverter.ToInt32(data, 0);
                stream.Seek(peHeaderOffset + 0x4, System.IO.SeekOrigin.Begin);
                stream.Read(data, 0, 2);
                return BitConverter.ToUInt16(data, 0) == 0x10B;
            }
        }
        catch
        {
            return false;
        }
    }

    private bool Is64BitLibrary(string filePath)
    {
        try
        {
            using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                byte[] data = new byte[4];
                stream.Seek(0x3C, System.IO.SeekOrigin.Begin);
                stream.Read(data, 0, 4);
                int peHeaderOffset = BitConverter.ToInt32(data, 0);
                stream.Seek(peHeaderOffset + 0x4, System.IO.SeekOrigin.Begin);
                stream.Read(data, 0, 2);
                return BitConverter.ToUInt16(data, 0) == 0x20B;
            }
        }
        catch
        {
            return false;
        }
    }

}
