using System;
using System.IO;

class ZipFileSplitter
{
    public static void SplitZipFile(string inputFile, string outputDirectory)
    {
        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        byte[] fileBytes = File.ReadAllBytes(inputFile);
        int partSize = fileBytes.Length / 4;
        int remainder = fileBytes.Length % 4;

        for (int i = 0; i < 4; i++)
        {
            string partPath = Path.Combine(outputDirectory, $"part{i + 1}.zip.part");
            int currentPartSize = partSize + (i == 3 ? remainder : 0);
            File.WriteAllBytes(partPath, fileBytes[(i * partSize)..((i * partSize) + currentPartSize)]);
            Console.WriteLine($"Created {partPath}, Size: {currentPartSize} bytes");
        }
    }

    public static void MergeZipFiles(string outputFile, string inputDirectory)
    {
        string[] partFiles = Directory.GetFiles(inputDirectory, "part*.zip.part");
        Array.Sort(partFiles);
        
        using (FileStream output = new FileStream(outputFile, FileMode.Create))
        {
            foreach (string partFile in partFiles)
            {
                byte[] partBytes = File.ReadAllBytes(partFile);
                output.Write(partBytes, 0, partBytes.Length);
                Console.WriteLine($"Merged {partFile}, Size: {partBytes.Length} bytes");
            }
        }
        Console.WriteLine($"Successfully merged to {outputFile}");
    }

    public static void Main(string[] args)
    {
        string zipFilePath = "sample.zip";
        string splitDirectory = "split_parts";
        string mergedZipPath = "merged.zip";
        
        SplitZipFile(zipFilePath, splitDirectory);
        MergeZipFiles(mergedZipPath, splitDirectory);
    }
}
