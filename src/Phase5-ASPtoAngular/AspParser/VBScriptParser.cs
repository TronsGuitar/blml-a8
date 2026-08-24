using BLML.Phase1Foundation.AST;

namespace BLML.Phase5ASPtoAngular.AspParser
{
    public enum AspStreamItemKind { Code, Leaf }

    /// <summary>
    /// One item in the combined per-page token stream: either a VBScript code token,
    /// or a pre-built leaf statement (an Html or `&lt;%= %&gt;` output chunk) sitting where it
    /// was physically written in the .asp file. Feeding both kinds through the same
    /// statement parser is what lets `&lt;% If x Then %&gt;html&lt;% End If %&gt;` nest the html
    /// correctly inside the If's body instead of after it.
    /// </summary>
    public class AspStreamItem
    {
        public AspStreamItemKind Kind { get; set; }
        public VbsToken? Token { get; set; }
        public StatementNode? Leaf { get; set; }

        public static AspStreamItem ForToken(VbsToken t) => new() { Kind = AspStreamItemKind.Code, Token = t };
        public static AspStreamItem ForLeaf(StatementNode n) => new() { Kind = AspStreamItemKind.Leaf, Leaf = n };
    }

    /// <summary>
    /// Recursive-descent parser for classic-ASP VBScript. Scoped to the patterns that
    /// actually show up in real classic ASP pages rather than the full VBScript grammar:
    ///  - no `Class ... End Class`, no user-defined `Type`, no `Property` blocks
    ///    (uncommon inline in .asp pages; when Global.asa needs Sub bodies it reuses
    ///    the same statement parser used here)
    ///  - `Select Case` supports comma-separated value lists and `Case Else` only -
    ///    VBScript itself doesn't support `Case 1 To 10` / `Case Is > 5` (those are
    ///    VBA/VB6-only), so there's nothing being left out here
    ///  - parenless statement calls (`Response.Write "x", y` with no parens around the
    ///    argument list - a real VB-family ambiguity) are supported heuristically: the
    ///    parser always consumes an adjacent `(...)` as a call's arguments, then treats
    ///    any further comma-separated expressions up to the next line break as
    ///    additional arguments
    /// </summary>
    public class VBScriptParser
    {
        public List<string> Warnings { get; } = new();

        private List<AspStreamItem> _items = new();
        private int _pos;

        public List<StatementNode> ParseProgram(List<AspStreamItem> stream)
        {
            _items = stream;
            _pos = 0;
            var statements = new List<StatementNode>();
            SkipNewLines();
            while (!AtCodeEnd())
            {
                var stmt = ParseStatement();
                if (stmt != null) statements.Add(stmt);
                SkipNewLines();
            }
            return statements;
        }

        public ExpressionNode ParseExpressionText(string text)
        {
            var tokens = new VbScriptTokenizer().Tokenize(text);
            _items = tokens.Where(t => t.Kind != VbsTokenKind.NewLine).Select(AspStreamItem.ForToken).ToList();
            _pos = 0;
            return ParseExpression();
        }

        public List<StatementNode> ParseCodeText(string code)
        {
            var tokens = new VbScriptTokenizer().Tokenize(code);
            return ParseProgram(tokens.Select(AspStreamItem.ForToken).ToList());
        }

        // ----- stream helpers -----

        private AspStreamItem Current => _pos < _items.Count ? _items[_pos] : AspStreamItem.ForToken(new VbsToken { Kind = VbsTokenKind.EndOfFile });
        private bool AtCodeEnd() => Current.Kind == AspStreamItemKind.Code && Current.Token!.Kind == VbsTokenKind.EndOfFile;
        private bool IsLeaf() => Current.Kind == AspStreamItemKind.Leaf;
        private bool IsNewLine() => Current.Kind == AspStreamItemKind.Code && Current.Token!.Kind == VbsTokenKind.NewLine;

        private bool IsKeyword(string word) => Current.Kind == AspStreamItemKind.Code && (Current.Token!.IsKeyword(word));
        private bool IsOperator(string op) => Current.Kind == AspStreamItemKind.Code && Current.Token!.IsOperator(op);

        private VbsToken Advance()
        {
            var t = Current.Kind == AspStreamItemKind.Code ? Current.Token! : new VbsToken { Kind = VbsTokenKind.Identifier, Value = string.Empty };
            _pos++;
            return t;
        }

        private void SkipNewLines()
        {
            while (IsNewLine()) _pos++;
        }

        private void ExpectKeyword(string word)
        {
            if (!IsKeyword(word)) Warnings.Add($"Expected '{word}' at token '{(Current.Kind == AspStreamItemKind.Code ? Current.Token!.Value : "<html>")}'.");
            else _pos++;
        }

        private void ExpectOperator(string op)
        {
            if (!IsOperator(op)) Warnings.Add($"Expected '{op}' at token '{(Current.Kind == AspStreamItemKind.Code ? Current.Token!.Value : "<html>")}'.");
            else _pos++;
        }

        // ----- statements -----

        public StatementNode? ParseStatement()
        {
            if (IsLeaf())
            {
                var leaf = Current.Leaf!;
                _pos++;
                return leaf;
            }

            if (IsKeyword("Dim") || IsKeyword("Public") || IsKeyword("Private")) return ParseDeclaration();
            if (IsKeyword("Const")) return ParseConst();
            if (IsKeyword("ReDim")) return ParseReDim();
            if (IsKeyword("If")) return ParseIf();
            if (IsKeyword("For")) return ParseFor();
            if (IsKeyword("While")) return ParseWhile();
            if (IsKeyword("Do")) return ParseDoLoop();
            if (IsKeyword("Select")) return ParseSelectCase();
            if (IsKeyword("Sub")) return ParseSubOrFunction(isFunction: false);
            if (IsKeyword("Function")) return ParseSubOrFunction(isFunction: true);
            if (IsKeyword("Exit")) return ParseExit();
            if (IsKeyword("On")) return ParseOnError();
            if (IsKeyword("Call")) return ParseCall();
            if (IsKeyword("Set")) return ParseAssignmentOrCall(isSet: true);

            return ParseAssignmentOrCall(isSet: false);
        }

        private BlockNode ParseBlockUntil(params string[] terminators)
        {
            var block = new BlockNode();
            SkipNewLines();
            while (!AtCodeEnd() && !NextIsAnyKeyword(terminators))
            {
                var stmt = ParseStatement();
                if (stmt != null) block.Statements.Add(stmt);
                SkipNewLines();
            }
            return block;
        }

        private bool NextIsAnyKeyword(string[] words)
        {
            foreach (var w in words) if (IsKeyword(w)) return true;
            return false;
        }

        private StatementNode ParseDeclaration()
        {
            var accessibility = IsKeyword("Public") ? VB6Accessibility.Public : IsKeyword("Private") ? VB6Accessibility.Private : VB6Accessibility.Private;
            Advance(); // Dim/Public/Private
            var group = new VariableDeclarationGroupNode();
            do
            {
                if (IsOperator(",")) Advance();
                if (Current.Kind != AspStreamItemKind.Code || Current.Token!.Kind != VbsTokenKind.Identifier)
                {
                    Warnings.Add("Expected variable name in declaration.");
                    break;
                }
                var name = Advance().Value;
                bool isArray = false;
                if (IsOperator("("))
                {
                    isArray = true;
                    Advance();
                    while (!IsOperator(")") && !AtCodeEnd()) Advance();
                    if (IsOperator(")")) Advance();
                }
                group.Declarations.Add(new VariableDeclarationNode { Name = name, Accessibility = accessibility, IsArray = isArray });
            } while (IsOperator(","));
            return group;
        }

        private StatementNode ParseConst()
        {
            Advance(); // Const
            var group = new VariableDeclarationGroupNode();
            do
            {
                if (IsOperator(",")) Advance();
                var name = Advance().Value;
                ExpectOperator("=");
                var value = ParseExpression();
                group.Declarations.Add(new VariableDeclarationNode { Name = name, InitialValue = ExpressionToText(value) });
            } while (IsOperator(","));
            return group;
        }

        private StatementNode ParseReDim()
        {
            Advance(); // ReDim
            bool preserve = false;
            if (IsKeyword("Preserve")) { preserve = true; Advance(); }
            var name = Advance().Value;
            var node = new ReDimStatementNode { VariableName = name, Preserve = preserve };
            ExpectOperator("(");
            while (!IsOperator(")") && !AtCodeEnd())
            {
                node.NewDimensions.Add(ParseExpression());
                if (IsOperator(",")) Advance(); else break;
            }
            ExpectOperator(")");
            return node;
        }

        /// <summary>
        /// Handles the single-line-vs-block ambiguity: `If cond Then` immediately
        /// followed by a NewLine is the block form (terminated by ElseIf/Else/End If);
        /// followed by anything else on the same line is the single-line form, which
        /// has no End If and whose Else branch (if any) is also single-statement.
        /// </summary>
        private StatementNode ParseIf()
        {
            Advance(); // If
            var condition = ParseExpression();
            ExpectKeyword("Then");

            if (IsNewLine())
            {
                var ifNode = new IfStatementNode { Condition = condition };
                ifNode.TrueBlock = ParseBlockUntil("ElseIf", "Else", "End");

                if (IsKeyword("ElseIf"))
                {
                    var elseBlock = new BlockNode();
                    elseBlock.Statements.Add(ParseIf());
                    ifNode.ElseBlock = elseBlock;
                    return ifNode;
                }

                if (IsKeyword("Else"))
                {
                    Advance();
                    ifNode.ElseBlock = ParseBlockUntil("End");
                }

                ExpectKeyword("End");
                ExpectKeyword("If");
                return ifNode;
            }

            // Single-line form: If cond Then stmt [Else stmt]
            var single = new SingleLineIfStatementNode { Condition = condition };
            single.ThenStatement = ParseSingleLineBody();
            if (IsKeyword("Else"))
            {
                Advance();
                single.ElseStatement = ParseSingleLineBody();
            }
            return single;
        }

        private StatementNode ParseSingleLineBody()
        {
            // A single-line If/Else body may itself contain multiple `:`-separated
            // statements; ParseStatement/SkipNewLines already treat `:` as a line break,
            // so collecting statements until the real end-of-line (or Else/EOF) covers it.
            var stmt = ParseStatement();
            return stmt ?? new ExpressionStatementNode { Expression = new LiteralExpressionNode { Value = string.Empty } };
        }

        private StatementNode ParseFor()
        {
            Advance(); // For
            if (IsKeyword("Each"))
            {
                Advance();
                var varName = Advance().Value;
                ExpectKeyword("In");
                var collection = ParseExpression();
                // `For Each` has no single-line form in VBScript - the body always runs to a matching Next.
                var node = new ForEachStatementNode { LoopVariable = varName, Collection = collection };
                node.Body = ParseBlockUntil("Next");
                ExpectKeyword("Next");
                if (Current.Kind == AspStreamItemKind.Code && Current.Token!.Kind == VbsTokenKind.Identifier) Advance();
                return node;
            }
            else
            {
                var loopVar = Advance().Value;
                ExpectOperator("=");
                var start = ParseExpression();
                ExpectKeyword("To");
                var end = ParseExpression();
                ExpressionNode? step = null;
                if (IsKeyword("Step")) { Advance(); step = ParseExpression(); }

                var node = new ForStatementNode { LoopVariable = loopVar, StartValue = start, EndValue = end, StepValue = step };
                node.Body = ParseBlockUntil("Next");
                ExpectKeyword("Next");
                if (Current.Kind == AspStreamItemKind.Code && Current.Token!.Kind == VbsTokenKind.Identifier) Advance();
                return node;
            }
        }

        private StatementNode ParseWhile()
        {
            Advance(); // While
            var cond = ParseExpression();
            var node = new WhileStatementNode { Condition = cond };
            node.Body = ParseBlockUntil("Wend");
            ExpectKeyword("Wend");
            return node;
        }

        private StatementNode ParseDoLoop()
        {
            Advance(); // Do
            bool isDoWhile = false, isUntil = false;
            ExpressionNode? condition = null;

            if (IsKeyword("While") || IsKeyword("Until"))
            {
                isDoWhile = true;
                isUntil = IsKeyword("Until");
                Advance();
                condition = ParseExpression();
            }

            var node = new DoLoopStatementNode { IsDoWhile = isDoWhile, IsUntil = isUntil, Condition = condition };
            node.Body = ParseBlockUntil("Loop");
            ExpectKeyword("Loop");

            if (!isDoWhile && (IsKeyword("While") || IsKeyword("Until")))
            {
                node.IsUntil = IsKeyword("Until");
                Advance();
                node.Condition = ParseExpression();
            }
            return node;
        }

        private StatementNode ParseSelectCase()
        {
            Advance(); // Select
            ExpectKeyword("Case");
            var testExpr = ParseExpression();
            var node = new SelectCaseStatementNode { TestExpression = testExpr };
            SkipNewLines();

            while (IsKeyword("Case"))
            {
                Advance();
                if (IsKeyword("Else"))
                {
                    Advance();
                    node.CaseElseBlock = ParseBlockUntil("Case", "End");
                    continue;
                }

                var clause = new CaseClauseNode();
                clause.Values.Add(ParseExpression());
                while (IsOperator(","))
                {
                    Advance();
                    clause.Values.Add(ParseExpression());
                }
                clause.Body = ParseBlockUntil("Case", "End");
                node.Cases.Add(clause);
            }

            ExpectKeyword("End");
            ExpectKeyword("Select");
            return node;
        }

        private StatementNode ParseSubOrFunction(bool isFunction)
        {
            Advance(); // Sub/Function
            var name = Advance().Value;
            var method = new MethodDeclarationNode { Name = name, IsFunction = isFunction };

            if (IsOperator("("))
            {
                Advance();
                while (!IsOperator(")") && !AtCodeEnd())
                {
                    bool byRef = true;
                    if (IsKeyword("Byval")) { byRef = false; Advance(); }
                    else if (IsKeyword("Byref")) { Advance(); }
                    var pname = Advance().Value;
                    method.Parameters.Add(new ParameterNode { Name = pname, IsByRef = byRef });
                    if (IsOperator(",")) Advance(); else break;
                }
                ExpectOperator(")");
            }

            var terminator = isFunction ? "Function" : "Sub";
            var block = ParseBlockUntil("End");
            method.Body.AddRange(block.Statements);
            ExpectKeyword("End");
            ExpectKeyword(terminator);
            return method;
        }

        private StatementNode ParseExit()
        {
            Advance(); // Exit
            var kind = Advance().Value; // For/Do/Sub/Function
            return new ExitStatementNode { ExitKind = kind };
        }

        private StatementNode ParseOnError()
        {
            Advance(); // On
            ExpectKeyword("Error");
            if (IsKeyword("Resume"))
            {
                Advance();
                ExpectKeyword("Next");
                return new OnErrorStatementNode { IsResumeNext = true };
            }
            ExpectKeyword("GoTo");
            var target = Advance().Value; // usually "0"
            return new OnErrorStatementNode { IsGoTo0 = target == "0", LabelName = target };
        }

        private StatementNode ParseCall()
        {
            Advance(); // Call
            var expr = ParsePostfix(ParsePrimary());
            return new CallStatementNode { Invocation = expr };
        }

        /// <summary>
        /// Handles `target = expr`, `Set target = expr`, and bare statement invocations
        /// (`rs.MoveNext`, `Response.Write "x", y` with no enclosing parens). See the
        /// class-level remarks on the parenless-call heuristic.
        /// </summary>
        private StatementNode ParseAssignmentOrCall(bool isSet)
        {
            if (isSet) Advance(); // Set

            var expr = ParsePostfix(ParsePrimary());

            if (IsOperator("="))
            {
                Advance();
                var value = ParseExpression();
                return new AssignmentNode { Target = expr, Value = value };
            }

            var invocation = expr as InvocationExpressionNode ?? new InvocationExpressionNode { Target = expr };
            if (!IsNewLine() && !AtCodeEnd() && !NextStartsBlockKeyword())
            {
                invocation.Arguments.Add(ParseExpression());
                while (IsOperator(","))
                {
                    Advance();
                    invocation.Arguments.Add(ParseExpression());
                }
            }
            return new CallStatementNode { Invocation = invocation };
        }

        private bool NextStartsBlockKeyword()
        {
            string[] keywords = { "Else", "ElseIf", "End", "Next", "Loop", "Wend", "Case" };
            return NextIsAnyKeyword(keywords);
        }

        // ----- expressions (precedence climbing) -----

        public ExpressionNode ParseExpression() => ParseOr();

        private ExpressionNode ParseOr()
        {
            var left = ParseAnd();
            while (IsKeyword("Or") || IsKeyword("Xor") || IsKeyword("Eqv") || IsKeyword("Imp"))
            {
                var op = Advance().Value;
                left = new BinaryExpressionNode { Left = left, Operator = op, Right = ParseAnd() };
            }
            return left;
        }

        private ExpressionNode ParseAnd()
        {
            var left = ParseNot();
            while (IsKeyword("And"))
            {
                Advance();
                left = new BinaryExpressionNode { Left = left, Operator = "And", Right = ParseNot() };
            }
            return left;
        }

        /// <summary>
        /// `Not` is unary, but the shared AST only models <see cref="BinaryExpressionNode"/>.
        /// By convention, unary operators are encoded with the operand in <c>Right</c> and
        /// <c>Left</c> left null - callers generating code from this tree must check for
        /// that (see AspExpressionToCSharp/AspExpressionToTypeScript's "Not" case).
        /// </summary>
        private ExpressionNode ParseNot()
        {
            if (IsKeyword("Not"))
            {
                Advance();
                return new BinaryExpressionNode { Left = null!, Operator = "Not", Right = ParseNot() };
            }
            return ParseComparison();
        }

        private static readonly string[] ComparisonOps = { "=", "<>", "<", ">", "<=", ">=" };

        private ExpressionNode ParseComparison()
        {
            var left = ParseConcat();
            while ((Current.Kind == AspStreamItemKind.Code && Current.Token!.Kind == VbsTokenKind.Operator && ComparisonOps.Contains(Current.Token!.Value))
                   || IsKeyword("Is") || IsKeyword("Like"))
            {
                var op = Advance().Value;
                left = new BinaryExpressionNode { Left = left, Operator = op, Right = ParseConcat() };
            }
            return left;
        }

        private ExpressionNode ParseConcat()
        {
            var left = ParseAdditive();
            while (IsOperator("&"))
            {
                Advance();
                left = new BinaryExpressionNode { Left = left, Operator = "&", Right = ParseAdditive() };
            }
            return left;
        }

        private ExpressionNode ParseAdditive()
        {
            var left = ParseMultiplicative();
            while (IsOperator("+") || IsOperator("-"))
            {
                var op = Advance().Value;
                left = new BinaryExpressionNode { Left = left, Operator = op, Right = ParseMultiplicative() };
            }
            return left;
        }

        private ExpressionNode ParseMultiplicative()
        {
            var left = ParseUnary();
            while (IsOperator("*") || IsOperator("/") || IsOperator("\\") || IsKeyword("Mod"))
            {
                var op = Advance().Value;
                left = new BinaryExpressionNode { Left = left, Operator = op, Right = ParseUnary() };
            }
            return left;
        }

        private ExpressionNode ParseUnary()
        {
            if (IsOperator("-") || IsOperator("+"))
            {
                var op = Advance().Value;
                return new BinaryExpressionNode { Left = new LiteralExpressionNode { Value = 0 }, Operator = op, Right = ParsePower() };
            }
            return ParsePower();
        }

        private ExpressionNode ParsePower()
        {
            var left = ParsePostfix(ParsePrimary());
            if (IsOperator("^"))
            {
                Advance();
                left = new BinaryExpressionNode { Left = left, Operator = "^", Right = ParseUnary() };
            }
            return left;
        }

        private ExpressionNode ParsePostfix(ExpressionNode expr)
        {
            while (true)
            {
                if (IsOperator("."))
                {
                    Advance();
                    var member = Advance().Value;
                    expr = new BinaryExpressionNode { Left = expr, Operator = ".", Right = new IdentifierExpressionNode { Name = member } };
                    continue;
                }

                if (IsOperator("("))
                {
                    Advance();
                    var invocation = new InvocationExpressionNode { Target = expr };
                    while (!IsOperator(")") && !AtCodeEnd())
                    {
                        invocation.Arguments.Add(ParseExpression());
                        if (IsOperator(",")) Advance(); else break;
                    }
                    ExpectOperator(")");
                    expr = invocation;
                    continue;
                }

                break;
            }
            return expr;
        }

        private ExpressionNode ParsePrimary()
        {
            if (Current.Kind != AspStreamItemKind.Code)
            {
                Warnings.Add("Expected expression but found embedded HTML/output.");
                return new LiteralExpressionNode { Value = string.Empty };
            }

            var token = Current.Token!;

            if (token.Kind == VbsTokenKind.String) { Advance(); return new LiteralExpressionNode { Value = token.Value }; }
            if (token.Kind == VbsTokenKind.Number)
            {
                Advance();
                return new LiteralExpressionNode { Value = token.Value.Contains('.') ? double.Parse(token.Value) : (object)long.Parse(token.Value) };
            }
            if (token.IsKeyword("True")) { Advance(); return new LiteralExpressionNode { Value = true }; }
            if (token.IsKeyword("False")) { Advance(); return new LiteralExpressionNode { Value = false }; }
            if (token.IsKeyword("Nothing") || token.IsKeyword("Null") || token.IsKeyword("Empty")) { Advance(); return new LiteralExpressionNode { Value = null! }; }

            if (token.IsOperator("("))
            {
                Advance();
                var inner = ParseExpression();
                ExpectOperator(")");
                return inner;
            }

            if (token.IsKeyword("Not"))
            {
                return ParseNot();
            }

            if (token.Kind == VbsTokenKind.Identifier || token.Kind == VbsTokenKind.Keyword)
            {
                Advance();
                return new IdentifierExpressionNode { Name = token.Value };
            }

            Warnings.Add($"Unexpected token '{token.Value}' while parsing an expression.");
            Advance();
            return new LiteralExpressionNode { Value = string.Empty };
        }

        private static string ExpressionToText(ExpressionNode expr) => expr switch
        {
            LiteralExpressionNode lit => lit.Value?.ToString() ?? "",
            IdentifierExpressionNode id => id.Name,
            _ => expr.GetType().Name
        };
    }
}
