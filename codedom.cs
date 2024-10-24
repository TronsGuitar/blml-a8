using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;

namespace CodeDomTest
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create a new CodeCompileUnit to contain the program graph
            CodeCompileUnit compileUnit = new CodeCompileUnit();

            // Declare a new namespace called CodeDomTestNamespace
            CodeNamespace codeNamespace = new CodeNamespace("CodeDomTestNamespace");
            // Add the new namespace to the compile unit
            compileUnit.Namespaces.Add(codeNamespace);

            // Import System namespace
            codeNamespace.Imports.Add(new CodeNamespaceImport("System"));

            // Declare a new type called TestClass
            CodeTypeDeclaration testClass = new CodeTypeDeclaration("TestClass");
            // Add the class to the namespace
            codeNamespace.Types.Add(testClass);

            // Add a private field
            CodeMemberField field = new CodeMemberField(typeof(int), "_number");
            field.Attributes = MemberAttributes.Private;
            testClass.Members.Add(field);

            // Add a property
            CodeMemberProperty property = new CodeMemberProperty();
            property.Name = "Number";
            property.Type = new CodeTypeReference(typeof(int));
            property.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            property.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_number")));
            property.SetStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_number"), new CodePropertySetValueReferenceExpression()));
            testClass.Members.Add(property);

            // Add a method
            CodeMemberMethod method = new CodeMemberMethod();
            method.Name = "PrintNumber";
            method.Attributes = MemberAttributes.Public;
            method.ReturnType = new CodeTypeReference(typeof(void));

            // Add a variable declaration
            CodeVariableDeclarationStatement variable = new CodeVariableDeclarationStatement(typeof(string), "message", new CodePrimitiveExpression("The number is:"));
            method.Statements.Add(variable);

            // Add a Console.WriteLine statement
            CodeMethodInvokeExpression consoleWriteLine = new CodeMethodInvokeExpression(
                new CodeTypeReferenceExpression("Console"),
                "WriteLine",
                new CodeBinaryOperatorExpression(
                    new CodeVariableReferenceExpression("message"),
                    CodeBinaryOperatorType.Add,
                    new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_number")
                )
            );
            method.Statements.Add(consoleWriteLine);

            // Add an if statement
            CodeConditionStatement condition = new CodeConditionStatement(
                new CodeBinaryOperatorExpression(
                    new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_number"),
                    CodeBinaryOperatorType.GreaterThan,
                    new CodePrimitiveExpression(10)
                ),
                new CodeStatement[] {
                    new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("Console"), "WriteLine", new CodePrimitiveExpression("Number is greater than 10"))
                },
                new CodeStatement[] {
                    new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("Console"), "WriteLine", new CodePrimitiveExpression("Number is less than or equal to 10"))
                }
            );
            method.Statements.Add(condition);

            // Add a for loop
            CodeIterationStatement forLoop = new CodeIterationStatement(
                // initialization
                new CodeAssignStatement(new CodeVariableReferenceExpression("i"), new CodePrimitiveExpression(0)),
                // test expression
                new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("i"), CodeBinaryOperatorType.LessThan, new CodePrimitiveExpression(5)),
                // increment statement
                new CodeAssignStatement(new CodeVariableReferenceExpression("i"), new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("i"), CodeBinaryOperatorType.Add, new CodePrimitiveExpression(1))),
                // statements inside the loop
                new CodeStatement[] {
                    new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("Console"), "WriteLine", new CodeVariableReferenceExpression("i"))
                }
            );

            // Declare the loop variable
            method.Statements.Add(new CodeVariableDeclarationStatement(typeof(int), "i", new CodePrimitiveExpression(0)));
            method.Statements.Add(forLoop);

            // Add the method to the class
            testClass.Members.Add(method);

            // Add a constructor
            CodeConstructor constructor = new CodeConstructor();
            constructor.Attributes = MemberAttributes.Public;
            constructor.Parameters.Add(new CodeParameterDeclarationExpression(typeof(int), "initialNumber"));
            constructor.Statements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "_number"), new CodeArgumentReferenceExpression("initialNumber")));
            testClass.Members.Add(constructor);

            // Generate the C# code
            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");

            // Create a StringWriter to hold the generated code
            StringWriter sw = new StringWriter();

            // Set code generation options
            CodeGeneratorOptions options = new CodeGeneratorOptions();
            options.BracingStyle = "C";
            options.IndentString = "    ";

            // Generate the code
            provider.GenerateCodeFromCompileUnit(compileUnit, sw, options);

            // Output the code
            Console.WriteLine(sw.ToString());
        }
    }
}
