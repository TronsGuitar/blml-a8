using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;

namespace CodeDomExample
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create a CodeTypeDeclaration representing a class named "Person"
            CodeTypeDeclaration personClass = new CodeTypeDeclaration("Person")
            {
                IsClass = true,
                TypeAttributes = System.Reflection.TypeAttributes.Public
            };

            // Add a private field "_name" of type string
            CodeMemberField nameField = new CodeMemberField(typeof(string), "_name")
            {
                Attributes = MemberAttributes.Private
            };
            personClass.Members.Add(nameField);

            // Add a public property "Name" with get and set accessors
            CodeMemberProperty nameProperty = new CodeMemberProperty
            {
                Name = "Name",
                Type = new CodeTypeReference(typeof(string)),
                Attributes = MemberAttributes.Public | MemberAttributes.Final
            };
            nameProperty.GetStatements.Add(new CodeMethodReturnStatement(
                new CodeFieldReferenceExpression(
                    new CodeThisReferenceExpression(), "_name")));
            nameProperty.SetStatements.Add(new CodeAssignStatement(
                new CodeFieldReferenceExpression(
                    new CodeThisReferenceExpression(), "_name"),
                new CodePropertySetValueReferenceExpression()));
            personClass.Members.Add(nameProperty);

            // Add a method "Greet" that prints a greeting message
            CodeMemberMethod greetMethod = new CodeMemberMethod
            {
                Name = "Greet",
                Attributes = MemberAttributes.Public | MemberAttributes.Final,
                ReturnType = new CodeTypeReference(typeof(void))
            };
            greetMethod.Statements.Add(new CodeMethodInvokeExpression(
                new CodeTypeReferenceExpression("Console"),
                "WriteLine",
                new CodeBinaryOperatorExpression(
                    new CodePrimitiveExpression("Hello, my name is "),
                    CodeBinaryOperatorType.Add,
                    new CodeFieldReferenceExpression(
                        new CodeThisReferenceExpression(), "_name"))));
            personClass.Members.Add(greetMethod);

            // Generate the code using GenerateCodeFromType
            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
            CodeGeneratorOptions options = new CodeGeneratorOptions
            {
                BracingStyle = "C",
                IndentString = "    "
            };

            using (StringWriter writer = new StringWriter())
            {
                provider.GenerateCodeFromType(personClass, writer, options);
                string generatedCode = writer.ToString();
                Console.WriteLine("Generated Code:");
                Console.WriteLine(generatedCode);
            }
        }
    }
}
