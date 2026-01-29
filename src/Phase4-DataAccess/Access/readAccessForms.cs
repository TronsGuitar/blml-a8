using System;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace AccessFormReader
{
    class Program
    {
        static void Main(string[] args)
        {
            // Get the COM type for Microsoft Access.
            Type accessType = Type.GetTypeFromProgID("Access.Application");
            if (accessType == null)
            {
                Console.WriteLine("Microsoft Access is not installed or the ProgID is unavailable.");
                return;
            }

            // Create an instance of Access.
            dynamic accessApp = Activator.CreateInstance(accessType);

            try
            {
                // Update these paths and names as needed.
                string dbPath = @"C:\Path\YourDatabase.mdb";
                string formName = "YourFormName";

                Console.WriteLine("Opening database: " + dbPath);
                accessApp.OpenCurrentDatabase(dbPath);

                // Open the form in design view (1 = design view).
                Console.WriteLine("Opening form in design view: " + formName);
                accessApp.DoCmd.OpenForm(formName, 1);

                // Retrieve the form object.
                dynamic form = accessApp.Forms[formName];

                Console.WriteLine($"Extracting controls from form: {formName}");
                foreach (dynamic ctrl in form.Controls)
                {
                    Console.WriteLine("--------------------------------------------------");
                    Console.WriteLine($"Control Name: {ctrl.Name}");
                    Console.WriteLine($"Control Type: {ctrl.ControlType}");

                    // Use TypeDescriptor to attempt to retrieve all accessible properties.
                    PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(ctrl);
                    Console.WriteLine("Properties:");
                    foreach (PropertyDescriptor prop in properties)
                    {
                        try
                        {
                            object value = prop.GetValue(ctrl);
                            Console.WriteLine($"{prop.Name}: {value}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"{prop.Name}: <Error retrieving value: {ex.Message}>");
                        }
                    }
                }

                // Close the form (2 = acForm) and quit Access.
                Console.WriteLine("Closing form and quitting Access.");
                accessApp.DoCmd.Close(2, formName);
                accessApp.Quit();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                // Clean up the COM object.
                if (accessApp != null)
                {
                    Marshal.ReleaseComObject(accessApp);
                }
            }

            Console.WriteLine("Done. Press any key to exit.");
            Console.ReadKey();
        }
    }
}
