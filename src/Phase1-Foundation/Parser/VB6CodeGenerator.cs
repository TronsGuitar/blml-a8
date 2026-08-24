using BLML.Phase1Foundation.AST;
using BLML.Phase1Foundation.SymbolTable;
using BLML.Phase6AdvancedFeatures;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BLML.Phase1Foundation.Parser
{
    public class VB6CodeGenerator
    {
        // Tracks the expression each currently-open `With` block resolves a bare `.Member`
        // reference against - a stack because VB6 allows nested With blocks.
        private readonly Stack<ExpressionSyntax> _withTargetStack = new Stack<ExpressionSyntax>();
        private int _withTempCounter;

        public string GenerateCSharpCode(ModuleNode module, BLML.Phase1Foundation.ProjectModel.VB6Project project = null)
        {
            if (module == null) return string.Empty;

            var compilation = CSharpCompilation.Create("VB6Converted")
                .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));

            var syntaxTree = CSharpSyntaxTree.Create(
                GenerateCompilationUnit(module, project).NormalizeWhitespace()
            );

            return syntaxTree.ToString();
        }

        private CompilationUnitSyntax GenerateCompilationUnit(ModuleNode module, BLML.Phase1Foundation.ProjectModel.VB6Project project = null)
        {
            var usings = BLML.Phase1Foundation.ProjectModel.NamespaceAndUsingGenerator.GenerateUsings(project, true);

            // In C#, we usually wrap logic in a class
            var classDecl = SyntaxFactory.ClassDeclaration(string.IsNullOrWhiteSpace(module.Name) ? "GeneratedModule" : module.Name)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword));

            foreach (var member in GenerateMembers(module.Declarations))
            {
                classDecl = classDecl.AddMembers(member);
            }

            var namespaceDecl = BLML.Phase1Foundation.ProjectModel.NamespaceAndUsingGenerator.WrapInNamespace(project?.Name, classDecl);

            return SyntaxFactory.CompilationUnit()
                .AddUsings(usings.ToArray())
                .AddMembers(namespaceDecl);
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateMembers(IReadOnlyList<DeclarationNode> declarations)
        {
            var handledPropertyGroups = new HashSet<string>(StringComparer.Ordinal);

            foreach (var declaration in declarations)
            {
                if (declaration is PropertyDeclarationNode propertyProcedure)
                {
                    var groupKey = GetPropertyGroupKey(propertyProcedure);
                    if (!handledPropertyGroups.Add(groupKey))
                    {
                        continue;
                    }

                    var propertyProcedures = declarations
                        .OfType<PropertyDeclarationNode>()
                        .Where(candidate => GetPropertyGroupKey(candidate) == groupKey)
                        .ToList();

                    var property = PropertyProcedureGenerator.TryGenerateProperty(
                        propertyProcedures,
                        ParseVB6Type,
                        GenerateStatement,
                        GenerateExpression);

                    if (property is not null)
                    {
                        yield return AddAccessibilityModifiers(property, propertyProcedure.Accessibility);
                    }
                    else
                    {
                        foreach (var procedure in propertyProcedures)
                        {
                            yield return GeneratePropertyProcedureFallbackMethod(procedure);
                        }
                    }

                    continue;
                }

                var member = GenerateMember(declaration);
                if (member != null)
                {
                    yield return member;
                }
            }
        }

        private MemberDeclarationSyntax GenerateMember(DeclarationNode node)
        {
#pragma warning disable CS8603 // Possible null reference return.
            return node switch
            {
                MethodDeclarationNode method => GenerateMethod(method),
                VariableDeclarationNode variable => GenerateField(variable),
                EnumDeclarationNode enumDecl => GenerateEnum(enumDecl),
                DeclareStatementNode declare => GenerateDeclare(declare),
                _ => null
            };
#pragma warning restore CS8603 // Possible null reference return.
        }

        private MethodDeclarationSyntax GenerateMethod(MethodDeclarationNode node)
        {
            var returnType = ParseVB6Type(node.ReturnType);

            var parameters = node.Parameters.Select(GenerateParameter).ToArray();

            var bodyStatements = node.Body.Select(GenerateStatement).Where(s => s != null).ToArray();
            var body = SyntaxFactory.Block(bodyStatements);

            var method = SyntaxFactory.MethodDeclaration(returnType, node.Name)
                .WithParameterList(SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(parameters)))
                .WithBody(body);

            return AddAccessibilityModifiers(method, node.Accessibility);
        }

        private FieldDeclarationSyntax GenerateField(VariableDeclarationNode node)
        {
            var variable = SyntaxFactory.VariableDeclaration(ParseVB6Type(node.Type))
                .AddVariables(SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(node.Name)));

            var field = SyntaxFactory.FieldDeclaration(variable);

            return AddAccessibilityModifiers(field, node.Accessibility);
        }

        private EnumDeclarationSyntax GenerateEnum(EnumDeclarationNode node)
        {
            var members = node.Members.Select(m =>
            {
                var member = SyntaxFactory.EnumMemberDeclaration(m.Name);
                if (m.Value != null)
                {
                    member = member.WithEqualsValue(SyntaxFactory.EqualsValueClause(GenerateExpression(m.Value)));
                }
                return member;
            });

            var enumDecl = SyntaxFactory.EnumDeclaration(node.Name).AddMembers(members.ToArray());
            return AddAccessibilityModifiers(enumDecl, node.Accessibility);
        }

        /// <summary>VB6 `Declare Function/Sub ... Lib "x.dll" [Alias "y"] (...)` -> a C# `[DllImport]` extern method.</summary>
        private MethodDeclarationSyntax GenerateDeclare(DeclareStatementNode node)
        {
            var returnType = node.IsFunction
                ? ParseVB6Type(node.ReturnType)
                : SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword));

            var parameters = node.Parameters.Select(GenerateParameter).ToArray();

            var dllImportArgs = new List<AttributeArgumentSyntax>
            {
                SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(node.LibraryName)))
            };
            if (!string.IsNullOrEmpty(node.Alias))
            {
                dllImportArgs.Add(SyntaxFactory.AttributeArgument(
                    SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(node.Alias)))
                    .WithNameEquals(SyntaxFactory.NameEquals("EntryPoint")));
            }

            var dllImportAttribute = SyntaxFactory.Attribute(SyntaxFactory.ParseName("System.Runtime.InteropServices.DllImport"))
                .WithArgumentList(SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList(dllImportArgs)));

            var method = SyntaxFactory.MethodDeclaration(returnType, node.Name)
                .WithParameterList(SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(parameters)))
                .AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(dllImportAttribute)))
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

            // Accessibility must be added before static/extern - AddAccessibilityModifiers
            // appends to whatever modifiers already exist, and C# convention (and the
            // compiler's own preferred ordering) puts the access modifier first.
            method = AddAccessibilityModifiers(method, node.Accessibility);
            return method.AddModifiers(SyntaxFactory.Token(SyntaxKind.StaticKeyword), SyntaxFactory.Token(SyntaxKind.ExternKeyword));
        }

        private MethodDeclarationSyntax GeneratePropertyProcedureFallbackMethod(PropertyDeclarationNode node)
        {
            var method = new MethodDeclarationNode
            {
                Name = node.Name,
                Accessibility = node.Accessibility,
                IsFunction = node.PropertyKind == PropertyProcedureKind.Get,
                ReturnType = node.PropertyKind == PropertyProcedureKind.Get ? node.Type : "void"
            };

            foreach (var parameter in node.Parameters)
            {
                method.Parameters.Add(parameter);
            }

            foreach (var statement in node.Body)
            {
                method.Body.Add(statement);
            }

            return GenerateMethod(method);
        }

        private ParameterSyntax GenerateParameter(ParameterNode parameter)
        {
            if (parameter.IsParamArray)
            {
                // ParamArray -> C# params array. params can't combine with ref or a default
                // value in C#, and VB6 disallows ParamArray from being ByRef or Optional
                // anyway, so there is nothing else to layer on here.
                var arrayType = SyntaxFactory.ArrayType(ParseVB6Type(parameter.Type))
                    .AddRankSpecifiers(SyntaxFactory.ArrayRankSpecifier(SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(SyntaxFactory.OmittedArraySizeExpression())));

                return SyntaxFactory.Parameter(SyntaxFactory.Identifier(parameter.Name))
                    .WithType(arrayType)
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.ParamsKeyword));
            }

            var generatedParameter = SyntaxFactory.Parameter(SyntaxFactory.Identifier(parameter.Name))
                .WithType(ParseVB6Type(parameter.Type));

            if (parameter.IsByRef)
            {
                generatedParameter = generatedParameter.AddModifiers(SyntaxFactory.Token(SyntaxKind.RefKeyword));
            }

            if (parameter.IsOptional && !parameter.IsByRef)
            {
                var defaultValueExpression = GenerateOptionalDefaultValue(parameter);
                if (defaultValueExpression != null)
                {
                    generatedParameter = generatedParameter.WithDefault(
                        SyntaxFactory.EqualsValueClause(defaultValueExpression));
                }
            }

            return generatedParameter;
        }

        private ExpressionSyntax? GenerateOptionalDefaultValue(ParameterNode parameter)
        {
            if (parameter.DefaultValueExpression != null)
            {
                return GenerateExpression(parameter.DefaultValueExpression);
            }

            if (string.IsNullOrWhiteSpace(parameter.DefaultValue))
            {
                return null;
            }

            if (int.TryParse(parameter.DefaultValue, out var integerValue))
            {
                return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(integerValue));
            }

            if (double.TryParse(parameter.DefaultValue, out var doubleValue))
            {
                return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(doubleValue));
            }

            if (bool.TryParse(parameter.DefaultValue, out var boolValue))
            {
                return boolValue
                    ? SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression)
                    : SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression);
            }

            return SyntaxFactory.IdentifierName(parameter.DefaultValue);
        }

        private MethodDeclarationSyntax AddAccessibilityModifiers(MethodDeclarationSyntax member, VB6Accessibility accessibility)
        {
            return accessibility switch
            {
                VB6Accessibility.Public => member.AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword)),
                VB6Accessibility.Friend => member.AddModifiers(SyntaxFactory.Token(SyntaxKind.InternalKeyword)),
                VB6Accessibility.Static => member.AddModifiers(
                    SyntaxFactory.Token(SyntaxKind.PrivateKeyword),
                    SyntaxFactory.Token(SyntaxKind.StaticKeyword)),
                _ => member.AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword))
            };
        }

        private FieldDeclarationSyntax AddAccessibilityModifiers(FieldDeclarationSyntax member, VB6Accessibility accessibility)
        {
            return accessibility switch
            {
                VB6Accessibility.Public => member.AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword)),
                VB6Accessibility.Friend => member.AddModifiers(SyntaxFactory.Token(SyntaxKind.InternalKeyword)),
                VB6Accessibility.Static => member.AddModifiers(
                    SyntaxFactory.Token(SyntaxKind.PrivateKeyword),
                    SyntaxFactory.Token(SyntaxKind.StaticKeyword)),
                _ => member.AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword))
            };
        }

        private PropertyDeclarationSyntax AddAccessibilityModifiers(PropertyDeclarationSyntax member, VB6Accessibility accessibility)
        {
            return accessibility switch
            {
                VB6Accessibility.Public => member.AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword)),
                VB6Accessibility.Friend => member.AddModifiers(SyntaxFactory.Token(SyntaxKind.InternalKeyword)),
                VB6Accessibility.Static => member.AddModifiers(
                    SyntaxFactory.Token(SyntaxKind.PrivateKeyword),
                    SyntaxFactory.Token(SyntaxKind.StaticKeyword)),
                _ => member.AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword))
            };
        }

        private EnumDeclarationSyntax AddAccessibilityModifiers(EnumDeclarationSyntax member, VB6Accessibility accessibility)
        {
            return accessibility switch
            {
                VB6Accessibility.Public => member.AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword)),
                VB6Accessibility.Friend => member.AddModifiers(SyntaxFactory.Token(SyntaxKind.InternalKeyword)),
                // C# has no "static enum" concept - VB6's Static accessibility on an Enum has no real meaning; fall back to private like other non-Public/Friend cases.
                _ => member.AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword))
            };
        }

        private static string GetPropertyGroupKey(PropertyDeclarationNode propertyDeclaration)
        {
            return $"{propertyDeclaration.Accessibility}:{propertyDeclaration.Name}";
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
            if (node is ExitStatementNode)
            {
                return SyntaxFactory.BreakStatement();
            }
            if (node is WithStatementNode withStmt)
            {
                return GenerateWithStatement(withStmt);
            }

            return SyntaxFactory.EmptyStatement();
        }

        /// <summary>
        /// `With target ... End With` has no direct C# equivalent construct - it's
        /// purely a VB6 parsing convenience for resolving bare `.Member` references, so
        /// this just inlines the body statements (with `_withTargetStack` set so nested
        /// GenerateExpression calls can resolve those references) into a block.
        ///
        /// When the target itself is more than a simple identifier (e.g. `With
        /// GetEmployee()`), VB6 evaluates it exactly once and every `.Member` reuses
        /// that same instance - naively substituting the raw target expression at every
        /// reference site would instead re-evaluate it each time, which is wrong if the
        /// target expression has side effects. To preserve "evaluate once" semantics,
        /// such targets are captured into a compiler-generated local first.
        /// </summary>
        private StatementSyntax GenerateWithStatement(WithStatementNode node)
        {
            var targetExpr = GenerateExpression(node.Target);
            var statements = new List<StatementSyntax>();

            ExpressionSyntax referenceExpr;
            if (node.Target is IdentifierExpressionNode)
            {
                referenceExpr = targetExpr;
            }
            else
            {
                var tempName = $"__with{_withTempCounter++}";
                referenceExpr = SyntaxFactory.IdentifierName(tempName);
                var declaration = SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName("var"))
                    .AddVariables(SyntaxFactory.VariableDeclarator(tempName).WithInitializer(SyntaxFactory.EqualsValueClause(targetExpr)));
                statements.Add(SyntaxFactory.LocalDeclarationStatement(declaration));
            }

            _withTargetStack.Push(referenceExpr);
            try
            {
                statements.AddRange(node.Body.Statements.Select(GenerateStatement));
            }
            finally
            {
                _withTargetStack.Pop();
            }

            return SyntaxFactory.Block(statements);
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

        /// <summary>
        /// A VB6 named argument (`Foo(bar:=1)`) can't be represented as an
        /// ExpressionSyntax on its own - C#'s named-argument syntax
        /// (`Foo(bar: 1)`) lives on the ArgumentSyntax, not the expression - so this is
        /// handled at the call-argument-building call site rather than inside
        /// GenerateExpression.
        /// </summary>
        private ArgumentSyntax GenerateArgument(ExpressionNode node)
        {
            if (node is NamedArgumentExpressionNode named)
            {
                return SyntaxFactory.Argument(GenerateExpression(named.Value)).WithNameColon(SyntaxFactory.NameColon(named.Name));
            }
            return SyntaxFactory.Argument(GenerateExpression(node));
        }

        private ExpressionSyntax GenerateExpression(ExpressionNode node)
        {
            if (node is LiteralExpressionNode literal)
            {
                if (literal.Value is string s)
                    return SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(s));
                if (literal.Value is char c)
                    return SyntaxFactory.LiteralExpression(SyntaxKind.CharacterLiteralExpression, SyntaxFactory.Literal(c));
                if (literal.Value is int i)
                    return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(i));
                if (literal.Value is double d)
                    return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(d));
                if (literal.Value is bool b)
                    return b ? SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression) : SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression);
                return SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);
            }
            if (node is IdentifierExpressionNode ident)
            {
                if (SymbolTableBuilder.PredefinedConstants.TryGetValue(ident.Name, out var constantValue))
                {
                    return GenerateExpression(new LiteralExpressionNode { Value = constantValue! });
                }

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
            if (node is WithMemberAccessExpressionNode withMember)
            {
                var target = _withTargetStack.Count > 0
                    ? _withTargetStack.Peek()
                    : SyntaxFactory.IdentifierName("/* Unresolved With target */");
                return SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, target, SyntaxFactory.IdentifierName(withMember.MemberName));
            }
            if (node is InvocationExpressionNode invoke)
            {
                if (invoke.Target is IdentifierExpressionNode targetIdentifier && BuiltInFunctionHandler.IsBuiltInFunction(targetIdentifier.Name))
                {
                    var generatedArguments = invoke.Arguments.Select(a => GenerateExpression(a).ToString()).ToArray();
                    return SyntaxFactory.ParseExpression(BuiltInFunctionHandler.GenerateCShrapCall(targetIdentifier.Name, generatedArguments));
                }

                var args = invoke.Arguments.Select(GenerateArgument).ToArray();
                return SyntaxFactory.InvocationExpression(GenerateExpression(invoke.Target))
                    .WithArgumentList(SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(args)));
            }
            return SyntaxFactory.IdentifierName("/* Unsupported Expression */");
        }

        private TypeSyntax ParseVB6Type(string vb6Type)
        {
            switch (vb6Type?.ToLowerInvariant())
            {
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
            }
        }
    }
}
