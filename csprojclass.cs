using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

[XmlRoot("Project")]
public class DataClass
{
    [XmlAttribute("Sdk")]
    public string Sdk { get; set; } = "Microsoft.NET.Sdk";

    [XmlElement("PropertyGroup")]
    public PropertyGroup MainPropertyGroup { get; set; } = new PropertyGroup();

    [XmlElement("ItemGroup")]
    public List<ItemGroup> ItemGroups { get; set; } = new List<ItemGroup>();

    public static DataClass CreateSample()
    {
        return new DataClass
        {
            MainPropertyGroup = new PropertyGroup
            {
                OutputType = "Exe",
                TargetFramework = "net8.0",
                Nullable = "enable",
                ImplicitUsings = "enable"
            },
            ItemGroups = new List<ItemGroup>
            {
                new ItemGroup
                {
                    CompileItems = new List<Compile>
                    {
                        new Compile { Include = "Program.cs" }
                    },
                    PackageReferences = new List<PackageReference>
                    {
                        new PackageReference { Include = "Newtonsoft.Json", Version = "13.0.3" }
                    }
                }
            }
        };
    }

    public void SaveToCsproj(string filePath)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(DataClass));
        var namespaces = new XmlSerializerNamespaces();
        namespaces.Add("", ""); // Avoid adding xmlns="" to elements

        using (FileStream fs = new FileStream(filePath, FileMode.Create))
        {
            serializer.Serialize(fs, this, namespaces);
        }
    }

    public static DataClass LoadFromCsproj(string filePath)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(DataClass));
        using (FileStream fs = new FileStream(filePath, FileMode.Open))
        {
            return (DataClass)serializer.Deserialize(fs);
        }
    }
}

public class PropertyGroup
{
    [XmlElement("OutputType")]
    public string OutputType { get; set; }

    [XmlElement("TargetFramework")]
    public string TargetFramework { get; set; }

    [XmlElement("Nullable")]
    public string Nullable { get; set; }

    [XmlElement("ImplicitUsings")]
    public string ImplicitUsings { get; set; }
}

public class ItemGroup
{
    [XmlElement("Compile")]
    public List<Compile> CompileItems { get; set; } = new List<Compile>();

    [XmlElement("PackageReference")]
    public List<PackageReference> PackageReferences { get; set; } = new List<PackageReference>();
}

public class Compile
{
    [XmlAttribute("Include")]
    public string Include { get; set; }
}

public class PackageReference
{
    [XmlAttribute("Include")]
    public string Include { get; set; }

    [XmlAttribute("Version")]
    public string Version { get; set; }
}

// Program.cs
class Program
{
    static void Main(string[] args)
    {
        // Create sample data
        DataClass project = DataClass.CreateSample();

        // Save to a .csproj file
        string filePath = "SampleProject.csproj";
        project.SaveToCsproj(filePath);
        Console.WriteLine($"Saved to {filePath}");

        // Load the .csproj file back
        DataClass loadedProject = DataClass.LoadFromCsproj(filePath);
        Console.WriteLine("Loaded project:");
        Console.WriteLine($"OutputType: {loadedProject.MainPropertyGroup.OutputType}");
        Console.WriteLine($"TargetFramework: {loadedProject.MainPropertyGroup.TargetFramework}");
        foreach (var item in loadedProject.ItemGroups)
        {
            foreach (var compile in item.CompileItems)
            {
                Console.WriteLine($"Compile Include: {compile.Include}");
            }
            foreach (var reference in item.PackageReferences)
            {
                Console.WriteLine($"PackageReference Include: {reference.Include}, Version: {reference.Version}");
            }
        }
    }
}


---

Explanation

1. Structure:

DataClass: Represents the top-level <Project> element.

PropertyGroup: Contains core project properties like TargetFramework and Nullable.

ItemGroup: Contains compile-time items (<Compile>) and dependencies (<PackageReference>).

Compile and PackageReference: Represent specific elements under ItemGroup.



2. Serialization:

Prevents xmlns="" using XmlSerializerNamespaces.

Outputs a clean XML structure.



3. Deserialization:

Reconstructs the .csproj structure from the XML file.





---

Sample .csproj File Output

Running the program generates a SampleProject.csproj file with the following content:

<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include="Program.cs" />
    <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
  </ItemGroup>
</Project>


---

This approach aligns perfectly with .NET 8 SDK-style .csproj file standards and allows seamless creation, saving, and loading of such files programmatically. Let me know if you need any adjustments!

