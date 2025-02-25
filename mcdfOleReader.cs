using System;
using System.Text;
using System.Runtime.InteropServices;
using OpenMcdf;  // Install OpenMcdf from NuGet

namespace OLEPropertySearch
{
    class Program
    {
        static void Main(string[] args)
        {
            // Update this path to your Access database (MDB/ACCDB) file.
            string mdbPath = @"C:\Path\YourDatabase.mdb";

            try
            {
                using (CompoundFile cf = new CompoundFile(mdbPath))
                {
                    Console.WriteLine("Enumerating storages and streams in the compound file:");
                    EnumerateStorage(cf.RootStorage, "");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error opening file: " + ex.Message);
            }

            Console.WriteLine("Done. Press any key to exit.");
            Console.ReadKey();
        }

        /// <summary>
        /// Recursively enumerates the storages and streams in the compound file.
        /// For each stream, it checks if it appears to be a property stream.
        /// </summary>
        static void EnumerateStorage(CFStorage storage, string indent)
        {
            foreach (CFItem item in storage)
            {
                if (item.IsStorage)
                {
                    Console.WriteLine($"{indent}Storage: {item.Name}");
                    EnumerateStorage(item as CFStorage, indent + "  ");
                }
                else
                {
                    Console.WriteLine($"{indent}Stream: {item.Name} (Size: {item.Size} bytes)");
                    try
                    {
                        var stream = item as CFStream;
                        byte[] data = stream.GetData();
                        // Check if the stream contains enough data for a header.
                        if (data.Length >= 2)
                        {
                            // In OLE property sets, the first 2 bytes represent the byte order.
                            // A valid property stream in little-endian should have a value of 0xFFFE.
                            short byteOrder = BitConverter.ToInt16(data, 0);
                            if (byteOrder == 0xFFFE)
                            {
                                Console.WriteLine($"{indent}--> This stream appears to be a property stream.");
                                // Optionally decode further.
                                DecodePropertyStream(data, indent + "  ");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{indent}Error reading stream: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Rudimentary method to display the beginning of the property stream.
        /// A full implementation would parse the property set according to [MS-OLEPS].
        /// </summary>
        static void DecodePropertyStream(byte[] data, string indent)
        {
            int bytesToDisplay = Math.Min(32, data.Length);
            string hexDump = BitConverter.ToString(data, 0, bytesToDisplay);
            Console.WriteLine($"{indent}Property stream raw data (first {bytesToDisplay} bytes): {hexDump}...");
        }
    }
}
