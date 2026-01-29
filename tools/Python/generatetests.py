import os
import re
import shutil

# MSTest Test Template
TEST_TEMPLATE = """
using Microsoft.VisualStudio.TestTools.UnitTesting;
using {namespace};

namespace {namespace}.Tests
{{
    [TestClass]
    public class {class_name}Tests
    {{
        private {class_name} _testClass;

        [TestInitialize]
        public void Setup()
        {{
            _testClass = new {class_name}();
        }}

{methods_tests}
    }}
}}
"""

# TestMethod Template
METHOD_TEST_TEMPLATE = """
        [TestMethod]
        public void {method_name}_Test()
        {{
            // Arrange

            // Act
            // Uncomment the next line and replace with actual invocation:
            // var result = _testClass.{method_name}();

            // Assert
            // Add assertions here.
        }}
"""

def extract_class_namespace_and_methods(file_path):
    """
    Extracts the namespace, class name, and public method names from a C# file.
    """
    with open(file_path, 'r', encoding='utf-8') as file:
        content = file.read()

    # Extract namespace
    namespace_match = re.search(r'namespace\s+([\w\.]+)', content)
    namespace = namespace_match.group(1) if namespace_match else "UnknownNamespace"

    # Extract class name
    class_match = re.search(r'class\s+(\w+)', content)
    class_name = class_match.group(1) if class_match else None

    # Extract public methods
    method_matches = re.findall(r'public\s+[\w<>\[\]]+\s+(\w+)\s*\(', content)
    
    return namespace, class_name, method_matches

def generate_test_file(input_file, output_file):
    """
    Generates a test file based on the class, namespace, and methods extracted.
    """
    namespace, class_name, method_names = extract_class_namespace_and_methods(input_file)
    if not class_name:
        print(f"Skipping {input_file}: No class found.")
        return

    # Generate test methods
    methods_tests = ""
    for method_name in method_names:
        methods_tests += METHOD_TEST_TEMPLATE.format(method_name=method_name)

    # Fill the test template
    test_content = TEST_TEMPLATE.format(
        namespace=namespace,
        class_name=class_name,
        methods_tests=methods_tests
    )

    # Write to output file
    with open(output_file, 'w', encoding='utf-8') as test_file:
        test_file.write(test_content)
    print(f"Test file created: {output_file}")

def recreate_folder_structure_and_generate_tests(input_folder, output_folder):
    """
    Recursively iterates through input_folder, recreates structure in output_folder,
    and generates test files for each C# file.
    """
    if os.path.exists(output_folder):
        shutil.rmtree(output_folder)
    os.makedirs(output_folder)

    for root, _, files in os.walk(input_folder):
        for file in files:
            if file.endswith('.cs'):  # Only process C# files
                input_file = os.path.join(root, file)

                # Extract namespace to determine test file path
                namespace, class_name, _ = extract_class_namespace_and_methods(input_file)
                if not class_name:
                    continue  # Skip files without a class

                # Create a folder structure based on the namespace
                namespace_path = namespace.replace('.', os.sep)
                test_folder = os.path.join(output_folder, namespace_path)
                os.makedirs(test_folder, exist_ok=True)

                # Generate test file
                test_file_name = f"{class_name}Tests.cs"
                output_file = os.path.join(test_folder, test_file_name)
                generate_test_file(input_file, output_file)

def main(input_folder, output_folder):
    """
    Main function to generate tests for all C# files in the input folder.
    """
    if not os.path.exists(input_folder):
        print(f"Input folder not found: {input_folder}")
        return

    recreate_folder_structure_and_generate_tests(input_folder, output_folder)
    print("\nTest generation complete. Check the output folder for generated test files.")

if __name__ == "__main__":
    import sys
    if len(sys.argv) != 3:
        print("Usage: python generate_tests.py <input_folder> <output_folder>")
        sys.exit(1)

    input_folder = sys.argv[1]
    output_folder = sys.argv[2]

    main(input_folder, output_folder)
