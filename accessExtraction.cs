
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Office.Interop.Access;

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

            Application accessApp = null;

            try
            {
                // Create Access application COM object
                accessApp = new Application();
                
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

        private static void ExtractForms(Application accessApp, string outputDir)
        {
            Console.WriteLine("Extracting forms...");
            
            // Get all forms in the database
            foreach (AccessObject form in accessApp.CurrentProject.AllForms)
            {
                string formName = form.Name;
                Console.WriteLine($"Processing form: {formName}");
                
                try
                {
                    // Export form as XML
                    string xmlFilePath = Path.Combine(outputDir, $"{formName}.xml");
                    accessApp.DoCmd.ExportXML(
                        ObjectType: AcExportXMLObjectType.acExportForm,
                        DataSource: formName,
                        DataTarget: xmlFilePath);
                    
                    Console.WriteLine($"Exported form XML: {xmlFilePath}");
                    
                    // Also try to export as HTML if possible
                    try
                    {
                        string htmlFilePath = Path.Combine(outputDir, $"{formName}.html");
                        accessApp.DoCmd.OutputTo(
                            ObjectType: AcOutputObjectType.acOutputForm,
                            ObjectName: formName,
                            OutputFormat: AcFormat.acFormatHTML,
                            OutputFile: htmlFilePath);
                        
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
            }
            
            Console.WriteLine($"Form extraction complete. Forms saved to: {outputDir}");
        }

        private static void ExtractReports(Application accessApp, string outputDir)
        {
            Console.WriteLine("Extracting reports...");
            
            // Get all reports in the database
            foreach (AccessObject report in accessApp.CurrentProject.AllReports)
            {
                string reportName = report.Name;
                Console.WriteLine($"Processing report: {reportName}");
                
                try
                {
                    // Export report as XML
                    string xmlFilePath = Path.Combine(outputDir, $"{reportName}.xml");
                    accessApp.DoCmd.ExportXML(
                        ObjectType: AcExportXMLObjectType.acExportReport,
                        DataSource: reportName,
                        DataTarget: xmlFilePath);
                    
                    Console.WriteLine($"Exported report XML: {xmlFilePath}");
                    
                    // Also try to export as PDF and other formats
                    try
                    {
                        string pdfFilePath = Path.Combine(outputDir, $"{reportName}.pdf");
                        accessApp.DoCmd.OutputTo(
                            ObjectType: AcOutputObjectType.acOutputReport,
                            ObjectName: reportName,
                            OutputFormat: AcFormat.acFormatPDF,
                            OutputFile: pdfFilePath);
                        
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
                        accessApp.DoCmd.OutputTo(
                            ObjectType: AcOutputObjectType.acOutputReport,
                            ObjectName: reportName,
                            OutputFormat: AcFormat.acFormatHTML,
                            OutputFile: htmlFilePath);
                        
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
            }
            
            Console.WriteLine($"Report extraction complete. Reports saved to: {outputDir}");
        }
    }
}
