using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using BLML.Phase1Foundation.AST;

namespace BLML.Phase1Foundation.Parser
{
    public class VB6CodeGenerator
    {
<<<<<<< HEAD
        public string GenerateCSharpCode(ModuleNode module)
        {
            if (module == null) return string.Empty;

=======
        public string GenerateCSharpCode(VB6SyntaxNode node)
        {
>>>>>>> 2e0740d (The prototype files)
            var compilation = CSharpCompilation.Create("VB6Converted")
                .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));

            var syntaxTree = CSharpSyntaxTree.Create(
<<<<<<< HEAD
                GenerateCompilationUnit(module).NormalizeWhitespace()
=======
                GenerateCompilationUnit(node)
>>>>>>> 2e0740d (The prototype files)
            );

            return syntaxTree.ToString();
        }

<<<<<<< HEAD
        private CompilationUnitSyntax GenerateCompilationUnit(ModuleNode module)
        {
            var usings = new List<UsingDirectiveSyntax>
            {
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System"))
            };

            var members = new List<MemberDeclarationSyntax>();
            
            // In C#, we usually wrap logic in a class
            var classDecl = SyntaxFactory.ClassDeclaration(module.Name)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword));

            foreach (var decl in module.Declarations)
            {
                var member = GenerateMember(decl);
                if (member != null)
                {
                    classDecl = classDecl.AddMembers(member);
                }
            }

            members.Add(classDecl);

=======
        private CompilationUnitSyntax GenerateCompilationUnit(VB6SyntaxNode node)
        {
            var usings = new List<UsingDirectiveSyntax>
            {
                SyntaxFactory.UsingDirective(
                    SyntaxFactory.ParseName("System")
                )
            };

            var members = new List<MemberDeclarationSyntax>();
            foreach (var child in node.Children)
            {
                var member = GenerateMember(child);
                if (member != null)
                {
                    members.Add(member);
                }
            }

>>>>>>> 2e0740d (The prototype files)
            return SyntaxFactory.CompilationUnit()
                .AddUsings(usings.ToArray())
                .AddMembers(members.ToArray());
        }

<<<<<<< HEAD
        private MemberDeclarationSyntax GenerateMember(DeclarationNode node)
        {
            return node switch
            {
                MethodDeclarationNode method => GenerateMethod(method),
                VariableDeclarationNode variable => GenerateField(variable),
                _ => null
            };
        }

        private MethodDeclarationSyntax GenerateMethod(MethodDeclarationNode node)
        {
            var returnType = ParseVB6Type(node.ReturnType);
            
            var parameters = node.Parameters.Select(p => 
                SyntaxFactory.Parameter(SyntaxFactory.Identifier(p.Name))
                    .WithType(ParseVB6Type(p.Type))
                    .AddModifiers(p.IsByRef ? SyntaxFactory.Token(SyntaxKind.RefKeyword) : default)
            ).ToArray();

            var bodyStatements = node.Body.Select(GenerateStatement).Where(s => s != null).ToArray();
            var body = SyntaxFactory.Block(bodyStatements);

            var method = SyntaxFactory.MethodDeclaration(returnType, node.Name)
                .WithParameterList(SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(parameters)))
                .WithBody(body);

            // Add modifiers
            if (node.Accessibility == VB6Accessibility.Public)
                method = method.AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
            else if (node.Accessibility == VB6Accessibility.Friend)
                method = method.AddModifiers(SyntaxFactory.Token(SyntaxKind.InternalKeyword));
            else
                method = method.AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));

            if (node.Accessibility == VB6Accessibility.Static)
                method = method.AddModifiers(SyntaxFactory.Token(SyntaxKind.StaticKeyword));

            return method;
        }

        private FieldDeclarationSyntax GenerateField(VariableDeclarationNode node)
        {
            var variable = SyntaxFactory.VariableDeclaration(ParseVB6Type(node.Type))
                .AddVariables(SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(node.Name)));

            var field = SyntaxFactory.FieldDeclaration(variable);

            if (node.Accessibility == VB6Accessibility.Public)
                field = field.AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
            else if (node.Accessibility == VB6Accessibility.Friend)
                field = field.AddModifiers(SyntaxFactory.Token(SyntaxKind.InternalKeyword));
            else
                field = field.AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));

            return field;
        }

        private StatementSyntax GenerateStatement(StatementNode node)
        {
            if (node is ExpressionStatementNode exprStmt)
            {
                var expr = GenerateExpression(exprStmt.Expression);
                return SyntaxFactory.ExpressionStatement(expr);
            }
            if (node is AssignmentNode assign)
            {
                var target = GenerateExpression(assign.Target);
                var value = GenerateExpression(assign.Value);
                var kind = SyntaxKind.SimpleAssignmentExpression;
                return SyntaxFactory.ExpressionStatement(SyntaxFactory.AssignmentExpression(kind, target, value));
            }
            if (node is IfStatementNode ifStmt)
            {
                var condition = GenerateExpression(ifStmt.Condition);
                var trueBlock = SyntaxFactory.Block(ifStmt.TrueBlock.Statements.Select(GenerateStatement));
                var elseClause = ifStmt.ElseBlock != null 
                    ? SyntaxFactory.ElseClause(SyntaxFactory.Block(ifStmt.ElseBlock.Statements.Select(GenerateStatement))) 
                    : null;
                
                return SyntaxFactory.IfStatement(condition, trueBlock, elseClause);
            }
            if (node is VariableDeclarationNode varDecl)
            {
                // Local variable declaration
                var variable = SyntaxFactory.VariableDeclaration(ParseVB6Type(varDecl.Type))
                    .AddVariables(SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(varDecl.Name)));
                return SyntaxFactory.LocalDeclarationStatement(variable);
            }
            if (node is ForStatementNode forStmt)
            {
                return GenerateForStatement(forStmt);
            }
            if (node is WhileStatementNode whileStmt)
            {
                return GenerateWhileStatement(whileStmt);
            }
            if (node is DoLoopStatementNode doStmt)
            {
                return GenerateDoLoopStatement(doStmt);
            }
            if (node is SelectCaseStatementNode selectStmt)
            {
                return GenerateSelectCaseStatement(selectStmt);
            }

            return SyntaxFactory.EmptyStatement();
        }

        private StatementSyntax GenerateSelectCaseStatement(SelectCaseStatementNode node)
        {
            // VB6 Select Case is similar to C# switch, but more flexible
            // For simple cases, we generate a switch. For complex cases (ranges, Is comparisons), we use if-else chains.

            var testExpr = GenerateExpression(node.TestExpression);
            var hasComplexCases = node.Cases.Any(c => c.IsRange || c.IsComparison || c.Values.Count > 1);

            if (hasComplexCases)
            {
                // Generate if-else chain for complex cases
                return GenerateSelectCaseAsIfElse(node, testExpr);
            }
            else
            {
                // Generate switch statement for simple cases
                return GenerateSelectCaseAsSwitch(node, testExpr);
            }
        }

        private StatementSyntax GenerateSelectCaseAsSwitch(SelectCaseStatementNode node, ExpressionSyntax testExpr)
        {
            var sections = new List<SwitchSectionSyntax>();

            foreach (var caseClause in node.Cases)
            {
                if (caseClause.Values.Count > 0)
                {
                    var labels = caseClause.Values.Select(v =>
                        (SwitchLabelSyntax)SyntaxFactory.CaseSwitchLabel(GenerateExpression(v))).ToList();

                    var statements = caseClause.Body.Statements
                        .Select(GenerateStatement)
                        .Where(s => s != null)
                        .ToList();
                    statements.Add(SyntaxFactory.BreakStatement());

                    sections.Add(SyntaxFactory.SwitchSection(
                        SyntaxFactory.List(labels),
                        SyntaxFactory.List(statements)));
                }
            }

            // Add default case if there's a Case Else
            if (node.CaseElseBlock != null)
            {
                var defaultStatements = node.CaseElseBlock.Statements
                    .Select(GenerateStatement)
                    .Where(s => s != null)
                    .ToList();
                defaultStatements.Add(SyntaxFactory.BreakStatement());

                sections.Add(SyntaxFactory.SwitchSection(
                    SyntaxFactory.SingletonList<SwitchLabelSyntax>(SyntaxFactory.DefaultSwitchLabel()),
                    SyntaxFactory.List(defaultStatements)));
            }

            return SyntaxFactory.SwitchStatement(testExpr, SyntaxFactory.List(sections));
        }

        private StatementSyntax GenerateSelectCaseAsIfElse(SelectCaseStatementNode node, ExpressionSyntax testExpr)
        {
            StatementSyntax? result = null;
            StatementSyntax? current = null;

            foreach (var caseClause in node.Cases.AsEnumerable().Reverse())
            {
                ExpressionSyntax condition;

                if (caseClause.IsComparison)
                {
                    // Case Is > 5 => testExpr > 5
                    var op = caseClause.ComparisonOperator switch
                    {
                        ">" => SyntaxKind.GreaterThanExpression,
                        "<" => SyntaxKind.LessThanExpression,
                        ">=" => SyntaxKind.GreaterThanOrEqualExpression,
                        "<=" => SyntaxKind.LessThanOrEqualExpression,
                        "<>" => SyntaxKind.NotEqualsExpression,
                        _ => SyntaxKind.EqualsExpression
                    };
                    condition = SyntaxFactory.BinaryExpression(op, testExpr, GenerateExpression(caseClause.Values[0]));
                }
                else if (caseClause.IsRange)
                {
                    // Case 1 To 10 => testExpr >= 1 && testExpr <= 10
                    var startExpr = GenerateExpression(caseClause.Values[0]);
                    var endExpr = GenerateExpression(caseClause.RangeEnd!);
                    condition = SyntaxFactory.BinaryExpression(
                        SyntaxKind.LogicalAndExpression,
                        SyntaxFactory.BinaryExpression(SyntaxKind.GreaterThanOrEqualExpression, testExpr, startExpr),
                        SyntaxFactory.BinaryExpression(SyntaxKind.LessThanOrEqualExpression, testExpr, endExpr));
                }
                else if (caseClause.Values.Count > 1)
                {
                    // Case 1, 2, 3 => testExpr == 1 || testExpr == 2 || testExpr == 3
                    condition = caseClause.Values
                        .Select(v => (ExpressionSyntax)SyntaxFactory.BinaryExpression(
                            SyntaxKind.EqualsExpression, testExpr, GenerateExpression(v)))
                        .Aggregate((left, right) => SyntaxFactory.BinaryExpression(SyntaxKind.LogicalOrExpression, left, right));
                }
                else
                {
                    // Case 1 => testExpr == 1
                    condition = SyntaxFactory.BinaryExpression(
                        SyntaxKind.EqualsExpression, testExpr, GenerateExpression(caseClause.Values[0]));
                }

                var bodyStatements = caseClause.Body.Statements.Select(GenerateStatement).Where(s => s != null);
                var body = SyntaxFactory.Block(bodyStatements);

                var elseClause = result != null ? SyntaxFactory.ElseClause(result) : null;
                result = SyntaxFactory.IfStatement(condition, body, elseClause);
            }

            // Add Case Else as final else
            if (node.CaseElseBlock != null && result is IfStatementSyntax ifResult)
            {
                var elseStatements = node.CaseElseBlock.Statements.Select(GenerateStatement).Where(s => s != null);
                var elseBody = SyntaxFactory.Block(elseStatements);

                // Find the deepest else clause and add Case Else there
                result = AddElseToIfChain(ifResult, SyntaxFactory.ElseClause(elseBody));
            }

            return result ?? SyntaxFactory.EmptyStatement();
        }

        private IfStatementSyntax AddElseToIfChain(IfStatementSyntax ifStmt, ElseClauseSyntax elseClause)
        {
            if (ifStmt.Else == null)
            {
                return ifStmt.WithElse(elseClause);
            }
            else if (ifStmt.Else.Statement is IfStatementSyntax nestedIf)
            {
                return ifStmt.WithElse(SyntaxFactory.ElseClause(AddElseToIfChain(nestedIf, elseClause)));
            }
            return ifStmt;
        }

        private StatementSyntax GenerateForStatement(ForStatementNode node)
        {
            // For i = 1 To 10 Step 2  =>  for (int i = 1; i <= 10; i += 2)
            var loopVar = node.LoopVariable;
            var startExpr = GenerateExpression(node.StartValue);
            var endExpr = GenerateExpression(node.EndValue);

            // Declare and initialize loop variable
            var declaration = SyntaxFactory.VariableDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)))
                .AddVariables(SyntaxFactory.VariableDeclarator(loopVar)
                    .WithInitializer(SyntaxFactory.EqualsValueClause(startExpr)));

            // Condition: i <= end (or i >= end for negative step)
            var condition = SyntaxFactory.BinaryExpression(
                SyntaxKind.LessThanOrEqualExpression,
                SyntaxFactory.IdentifierName(loopVar),
                endExpr);

            // Increment: i++ or i += step
            ExpressionSyntax incrementExpr;
            if (node.StepValue != null)
            {
                var stepExpr = GenerateExpression(node.StepValue);
                incrementExpr = SyntaxFactory.AssignmentExpression(
                    SyntaxKind.AddAssignmentExpression,
                    SyntaxFactory.IdentifierName(loopVar),
                    stepExpr);
            }
            else
            {
                incrementExpr = SyntaxFactory.PostfixUnaryExpression(
                    SyntaxKind.PostIncrementExpression,
                    SyntaxFactory.IdentifierName(loopVar));
            }

            // Body
            var bodyStatements = node.Body.Statements.Select(GenerateStatement).Where(s => s != null);
            var body = SyntaxFactory.Block(bodyStatements);

            return SyntaxFactory.ForStatement(declaration, default, condition,
                SyntaxFactory.SingletonSeparatedList(incrementExpr), body);
        }

        private StatementSyntax GenerateWhileStatement(WhileStatementNode node)
        {
            // While condition => while (condition)
            var condition = GenerateExpression(node.Condition);
            var bodyStatements = node.Body.Statements.Select(GenerateStatement).Where(s => s != null);
            var body = SyntaxFactory.Block(bodyStatements);

            return SyntaxFactory.WhileStatement(condition, body);
        }

        private StatementSyntax GenerateDoLoopStatement(DoLoopStatementNode node)
        {
            var bodyStatements = node.Body.Statements.Select(GenerateStatement).Where(s => s != null);
            var body = SyntaxFactory.Block(bodyStatements);

            // Handle condition
            ExpressionSyntax condition;
            if (node.Condition != null)
            {
                condition = GenerateExpression(node.Condition);
                if (node.IsUntil)
                {
                    // Until => negate the condition
                    condition = SyntaxFactory.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression,
                        SyntaxFactory.ParenthesizedExpression(condition));
                }
            }
            else
            {
                // Infinite loop: Do ... Loop => while (true)
                condition = SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression);
            }

            if (node.IsDoWhile)
            {
                // Do While/Until ... Loop => while (condition) { }
                return SyntaxFactory.WhileStatement(condition, body);
            }
            else
            {
                // Do ... Loop While/Until => do { } while (condition)
                return SyntaxFactory.DoStatement(body, condition);
            }
        }

        private ExpressionSyntax GenerateExpression(ExpressionNode node)
        {
            if (node is LiteralExpressionNode literal)
            {
                if (literal.Value is string s)
                    return SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(s));
                if (literal.Value is int i)
                    return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(i));
                if (literal.Value is double d)
                    return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(d));
                return SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);
            }
            if (node is IdentifierExpressionNode ident)
            {
                return SyntaxFactory.IdentifierName(ident.Name);
            }
            if (node is BinaryExpressionNode binary)
            {
                var kind = binary.Operator switch
                {
                    "+" => SyntaxKind.AddExpression,
                    "-" => SyntaxKind.SubtractExpression,
                    "*" => SyntaxKind.MultiplyExpression,
                    "/" => SyntaxKind.DivideExpression,
                    "&" => SyntaxKind.AddExpression, // Concatenation
                    "=" => SyntaxKind.EqualsExpression,
                    "<>" => SyntaxKind.NotEqualsExpression,
                    "<" => SyntaxKind.LessThanExpression,
                    ">" => SyntaxKind.GreaterThanExpression,
                    "<=" => SyntaxKind.LessThanOrEqualExpression,
                    ">=" => SyntaxKind.GreaterThanOrEqualExpression,
                    _ => SyntaxKind.None
                };

                if (kind == SyntaxKind.None) return SyntaxFactory.IdentifierName($"/* Unsupported Op: {binary.Operator} */");

                return SyntaxFactory.BinaryExpression(kind, GenerateExpression(binary.Left), GenerateExpression(binary.Right));
            }
            if (node is InvocationExpressionNode invoke)
            {
                // Check if this is a built-in VB6 function that needs special handling
                if (invoke.Target is IdentifierExpressionNode targetIdent &&
                    BuiltInFunctionHandler.IsBuiltInFunction(targetIdent.Name))
                {
                    // Generate argument strings for the built-in function handler
                    var argStrings = invoke.Arguments.Select(a => GenerateExpression(a).ToFullString()).ToArray();
                    var csharpCode = BuiltInFunctionHandler.GenerateCShrapCall(targetIdent.Name, argStrings);
                    return SyntaxFactory.ParseExpression(csharpCode);
                }

                var args = invoke.Arguments.Select(a => SyntaxFactory.Argument(GenerateExpression(a))).ToArray();
                return SyntaxFactory.InvocationExpression(GenerateExpression(invoke.Target))
                    .WithArgumentList(SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(args)));
            }
            return SyntaxFactory.IdentifierName("/* Unsupported Expression */");
=======
        private MemberDeclarationSyntax GenerateMember(VB6SyntaxNode node)
        {
            switch (node.Type)
            {
                case NodeType.Class:
                    return GenerateClass(node);
                case NodeType.Function:
                    return GenerateMethod(node, true);
                case NodeType.Sub:
                    return GenerateMethod(node, false);
                case NodeType.Property:
                    return GenerateProperty(node);
                default:
                    return null;
            }
        }

        private string GenerateExpression(VB6SyntaxNode node)
        {
            if (node.Type == NodeType.Expression)
            {
                if (BuiltInFunctionHandler.IsBuiltInFunction(node.Value))
                {
                    var args = node.Children.Select(GenerateExpression).ToArray();
                    return BuiltInFunctionHandler.GenerateCShrapCall(node.Value, args);
                }
                // Handle other expressions...
                return node.Value;
            }
            return node.Value;
        }

        private ClassDeclarationSyntax GenerateClass(VB6SyntaxNode node)
        {
            var members = new List<MemberDeclarationSyntax>();
            foreach (var child in node.Children)
            {
                var member = GenerateMember(child);
                if (member != null)
                {
                    members.Add(member);
                }
            }

            return SyntaxFactory.ClassDeclaration(node.Value)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddMembers(members.ToArray());
        }

        private MethodDeclarationSyntax GenerateMethod(VB6SyntaxNode node, bool isFunction)
        {
            var returnType = isFunction ? 
                ParseVB6Type(node.Attributes["ReturnType"]) : 
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword));

            var parameters = new List<ParameterSyntax>();
            foreach (var param in node.Children.Where(c => c.Type == NodeType.Variable))
            {
                parameters.Add(SyntaxFactory.Parameter(
                    SyntaxFactory.Identifier(param.Value))
                    .WithType(ParseVB6Type(param.Attributes["Type"]))
                );
            }

            var body = SyntaxFactory.Block();
            // TODO: Generate method body statements

            return SyntaxFactory.MethodDeclaration(returnType, node.Value)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddParameterListParameters(parameters.ToArray())
                .WithBody(body);
        }

        private PropertyDeclarationSyntax GenerateProperty(VB6SyntaxNode node)
        {
            var propertyType = ParseVB6Type(node.Attributes["Type"]);
            
            var accessors = new List<AccessorDeclarationSyntax>
            {
                SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithBody(SyntaxFactory.Block()),
                SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                    .WithBody(SyntaxFactory.Block())
            };

            return SyntaxFactory.PropertyDeclaration(propertyType, node.Value)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddAccessorListAccessors(accessors.ToArray());
>>>>>>> 2e0740d (The prototype files)
        }

        private TypeSyntax ParseVB6Type(string vb6Type)
        {
            switch (vb6Type?.ToLowerInvariant())
            {
<<<<<<< HEAD
                case "string": return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword));
                case "integer": return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword));
                case "long": return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.LongKeyword));
                case "single": return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.FloatKeyword));
                case "double": return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.DoubleKeyword));
                case "boolean": return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword));
                case "date": return SyntaxFactory.IdentifierName("DateTime");
                case "variant":
                case "object": return SyntaxFactory.IdentifierName("object");
                case "void": return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword));
                default: return SyntaxFactory.IdentifierName(vb6Type ?? "object");
=======
                case "string":
                    return SyntaxFactory.PredefinedType(
                        SyntaxFactory.Token(SyntaxKind.StringKeyword));
                case "integer":
                    return SyntaxFactory.PredefinedType(
                        SyntaxFactory.Token(SyntaxKind.IntKeyword));
                case "long":
                    return SyntaxFactory.PredefinedType(
                        SyntaxFactory.Token(SyntaxKind.LongKeyword));
                case "single":
                    return SyntaxFactory.PredefinedType(
                        SyntaxFactory.Token(SyntaxKind.FloatKeyword));
                case "double":
                    return SyntaxFactory.PredefinedType(
                        SyntaxFactory.Token(SyntaxKind.DoubleKeyword));
                case "boolean":
                    return SyntaxFactory.PredefinedType(
                        SyntaxFactory.Token(SyntaxKind.BoolKeyword));
                case "date":
                    return SyntaxFactory.IdentifierName("DateTime");
                case "variant":
                    return SyntaxFactory.IdentifierName("object");
                case "object":
                    return SyntaxFactory.IdentifierName("object");
                default:
                    // Handle custom types
                    return SyntaxFactory.IdentifierName(vb6Type ?? "object");
>>>>>>> 2e0740d (The prototype files)
            }
        }
    }
}
