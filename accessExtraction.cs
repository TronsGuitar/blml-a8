using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;

namespace AccessExtractor
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: AccessExtractor.exe <path_to_accdb> <output_directory>");
                return;
            }

            string databasePath = Path.GetFullPath(args[0]);
            string outputDir = Path.GetFullPath(args[1]);

            if (!File.Exists(databasePath))
            {
                Console.WriteLine($"Error: Database file '{databasePath}' does not exist.");
                return;
            }

            if (!Directory.Exists(outputDir))
            {
                try
                {
                    Directory.CreateDirectory(outputDir);
                    Console.WriteLine($"Created output directory: {outputDir}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating output directory: {ex.Message}");
                    return;
                }
            }

            Console.WriteLine($"Processing Access database: {databasePath}");
            Console.WriteLine($"Output directory: {outputDir}");

            // Create output subdirectories
            string formsDir = Path.Combine(outputDir, "Forms");
            string reportsDir = Path.Combine(outputDir, "Reports");
            
            Directory.CreateDirectory(formsDir);
            Directory.CreateDirectory(reportsDir);

            dynamic accessApp = null;

            try
            {
                // Create Access application COM object through late binding
                Type accessType = Type.GetTypeFromProgID("Access.Application");
                accessApp = Activator.CreateInstance(accessType);
                
                // Open the database
                accessApp.Visible = false;
                accessApp.OpenCurrentDatabase(databasePath, false);
                
                // Extract forms
                ExtractForms(accessApp, formsDir);
                
                // Extract reports
                ExtractReports(accessApp, reportsDir);
                
                Console.WriteLine("Extraction completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                // Clean up COM objects
                if (accessApp != null)
                {
                    try
                    {
                        accessApp.CloseCurrentDatabase();
                        accessApp.Quit();
                        Marshal.ReleaseComObject(accessApp);
                    }
                    catch 
                    {
                        // Ignore errors during cleanup
                    }
                    finally
                    {
                        accessApp = null;
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                    }
                }
            }
        }

        private static void ExtractForms(dynamic accessApp, string outputDir)
        {
            Console.WriteLine("Extracting forms...");
            
            try
            {
                // Get all forms in the database using dynamic (late binding) to avoid AccessObject type issues
                dynamic allForms = accessApp.CurrentProject.AllForms;
                int count = allForms.Count;
                
                Console.WriteLine($"Found {count} forms in the database.");
                
                for (int i = 0; i < count; i++)
                {
                    dynamic form = allForms[i];
                    string formName = form.Name;
                    Console.WriteLine($"Processing form: {formName}");
                    
                    try
                    {
                        // Save form definition using SaveAsText
                        string txtFilePath = Path.Combine(outputDir, $"{formName}.txt");
                        
                        try {
                            // acForm = 2
                            accessApp.Application.SaveAsText(2, formName, txtFilePath);
                            Console.WriteLine($"Exported form definition: {txtFilePath}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"  Warning: Could not export {formName} definition: {ex.Message}");
                            
                            // Fall back to OutputTo
                            try {
                                // Use dynamic to access AcOutputObjectType and AcFormat enums
                                dynamic acOutputForm = 2; // 2 = acOutputForm
                                dynamic acFormatTXT = 0; // 0 = acFormatTXT
                                
                                accessApp.DoCmd.OutputTo(
                                    acOutputForm,
                                    formName,
                                    acFormatTXT,
                                    txtFilePath);
                                
                                Console.WriteLine($"Exported form TXT: {txtFilePath}");
                            }
                            catch (Exception innerEx)
                            {
                                Console.WriteLine($"  Warning: Could not export {formName} as TXT: {innerEx.Message}");
                            }
                        }
                        
                        // Also try to export as HTML if possible
                        try
                        {
                            string htmlFilePath = Path.Combine(outputDir, $"{formName}.html");
                            
                            // Use dynamic to access AcOutputObjectType and AcFormat enums
                            dynamic acOutputForm = 2; // 2 = acOutputForm
                            dynamic acFormatHTML = 2; // 2 = acFormatHTML
                            
                            accessApp.DoCmd.OutputTo(
                                acOutputForm,
                                formName,
                                acFormatHTML,
                                htmlFilePath);
                            
                            Console.WriteLine($"Exported form HTML: {htmlFilePath}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"  Warning: Could not export {formName} as HTML: {ex.Message}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"  Error exporting form {formName}: {ex.Message}");
                    }
                    
                    // Release COM object
                    if (form != null)
                    {
                        Marshal.ReleaseComObject(form);
                    }
                }
                
                // Release COM object
                if (allForms != null)
                {
                    Marshal.ReleaseComObject(allForms);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing forms: {ex.Message}");
            }
            
            Console.WriteLine($"Form extraction complete. Forms saved to: {outputDir}");
        }

        private static void ExtractReports(dynamic accessApp, string outputDir)
        {
            Console.WriteLine("Extracting reports...");
            
            try
            {
                // Get all reports in the database using dynamic (late binding) to avoid AccessObject type issues
                dynamic allReports = accessApp.CurrentProject.AllReports;
                int count = allReports.Count;
                
                Console.WriteLine($"Found {count} reports in the database.");
                
                for (int i = 0; i < count; i++)
                {
                    dynamic report = allReports[i];
                    string reportName = report.Name;
                    Console.WriteLine($"Processing report: {reportName}");
                    
                    try
                    {
                        // Save report definition using SaveAsText
                        string txtFilePath = Path.Combine(outputDir, $"{reportName}.txt");
                        
                        try {
                            // acReport = 3
                            accessApp.Application.SaveAsText(3, reportName, txtFilePath);
                            Console.WriteLine($"Exported report definition: {txtFilePath}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"  Warning: Could not export {reportName} definition: {ex.Message}");
                            
                            // Fall back to OutputTo
                            try {
                                // Use dynamic to access AcOutputObjectType and AcFormat enums
                                dynamic acOutputReport = 3; // 3 = acOutputReport
                                dynamic acFormatTXT = 0; // 0 = acFormatTXT
                                
                                accessApp.DoCmd.OutputTo(
                                    acOutputReport,
                                    reportName,
                                    acFormatTXT,
                                    txtFilePath);
                                
                                Console.WriteLine($"Exported report TXT: {txtFilePath}");
                            }
                            catch (Exception innerEx)
                            {
                                Console.WriteLine($"  Warning: Could not export {reportName} as TXT: {innerEx.Message}");
                            }
                        }
                        
                        // Also try to export as PDF
                        try
                        {
                            string pdfFilePath = Path.Combine(outputDir, $"{reportName}.pdf");
                            
                            // Use dynamic to access AcOutputObjectType and AcFormat enums
                            dynamic acOutputReport = 3; // 3 = acOutputReport
                            dynamic acFormatPDF = 1; // 1 = acFormatPDF
                            
                            accessApp.DoCmd.OutputTo(
                                acOutputReport,
                                reportName,
                                acFormatPDF,
                                pdfFilePath);
                            
                            Console.WriteLine($"Exported report PDF: {pdfFilePath}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"  Warning: Could not export {reportName} as PDF: {ex.Message}");
                        }
                        
                        // Try to export as HTML as well
                        try
                        {
                            string htmlFilePath = Path.Combine(outputDir, $"{reportName}.html");
                            
                            // Use dynamic to access AcOutputObjectType and AcFormat enums
                            dynamic acOutputReport = 3; // 3 = acOutputReport
                            dynamic acFormatHTML = 2; // 2 = acFormatHTML
                            
                            accessApp.DoCmd.OutputTo(
                                acOutputReport,
                                reportName,
                                acFormatHTML,
                                htmlFilePath);
                            
                            Console.WriteLine($"Exported report HTML: {htmlFilePath}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"  Warning: Could not export {reportName} as HTML: {ex.Message}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"  Error exporting report {reportName}: {ex.Message}");
                    }
                    
                    // Release COM object
                    if (report != null)
                    {
                        Marshal.ReleaseComObject(report);
                    }
                }
                
                // Release COM object
                if (allReports != null)
                {
                    Marshal.ReleaseComObject(allReports);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing reports: {ex.Message}");
            }
            
            Console.WriteLine($"Report extraction complete. Reports saved to: {outputDir}");
        }
    }
}
