using Xunit;
using BLML.Phase1Foundation.AST;
using BLML.Phase5ASPtoAngular.AspParser;
using BLML.Phase5ASPtoAngular.Analysis;
using BLML.Phase5ASPtoAngular.Backend.ApiGenerator;
using BLML.Phase5ASPtoAngular.Backend.Infrastructure;
using BLML.Phase5ASPtoAngular.Frontend;

namespace BLML.Tests
{
    public class Phase5AspToAngularTests
    {
        [Fact]
        public void TemplateConverter_RendersClassicRecordsetLoopAsForWithTrackAndFieldAccess()
        {
            var asp = "<% While Not rs.EOF %><tr><td><%=rs(\"Name\")%></td></tr><% rs.MoveNext %><% Wend %>";
            var page = new AspParser().Parse(asp);

            var html = new TemplateConverter().Convert(page.Statements);

            Assert.Contains("@for (item of rsItems(); track item.id)", html);
            Assert.Contains("{{ item.name }}", html);
            Assert.DoesNotContain("MoveNext", html); // loop mechanics dropped, not mistranslated
        }

        [Fact]
        public void TemplateConverter_RendersElseIfChainAsAtElseIf()
        {
            var asp = "<% If x = 1 Then %>A<% ElseIf x = 2 Then %>B<% Else %>C<% End If %>";
            var page = new AspParser().Parse(asp);

            var html = new TemplateConverter().Convert(page.Statements);

            Assert.Contains("@if (", html);
            Assert.Contains("@else if (", html);
            Assert.Contains("@else {", html);
            Assert.Contains("A", html);
            Assert.Contains("B", html);
            Assert.Contains("C", html);
        }

        [Fact]
        public void TemplateConverter_TranslatesResponseWriteToInterpolation()
        {
            var asp = "<% Response.Write name %>";
            var page = new AspParser().Parse(asp);

            var html = new TemplateConverter().Convert(page.Statements);

            Assert.Contains("{{ name }}", html);
        }

        [Fact]
        public void AngularAntiPatternChecker_FlagsEveryTargetedLegacyPattern()
        {
            var checker = new AngularAntiPatternChecker();

            var templateFindings = checker.CheckTemplate("<div *ngIf=\"x\"></div>@for (x of items) { {{x}} }<input [(ngModel)]=\"y\">");
            Assert.Contains(templateFindings, f => f.Rule == "no-legacy-structural-directives");
            Assert.Contains(templateFindings, f => f.Rule == "for-requires-track");
            Assert.Contains(templateFindings, f => f.Rule == "no-ngmodel-two-way-binding");

            var badComponent = "@NgModule({})\nexport class X {\n  constructor(private http: HttpClient) {}\n  load(): any { this.http.get('/x').subscribe(r => r); }\n  el = document.getElementById('x');\n}";
            var componentFindings = checker.CheckComponent(badComponent);
            Assert.Contains(componentFindings, f => f.Rule == "no-ngmodule");
            Assert.Contains(componentFindings, f => f.Rule == "no-any-type");
            Assert.Contains(componentFindings, f => f.Rule == "no-unmanaged-subscribe");
            Assert.Contains(componentFindings, f => f.Rule == "prefer-inject-function");
            Assert.Contains(componentFindings, f => f.Rule == "no-direct-dom-access");
        }

        [Fact]
        public void ComponentGenerator_OwnOutputPassesTheAntiPatternChecker()
        {
            var generator = new ComponentGenerator();
            var template = "@for (item of rsItems(); track item.id) { <li>{{ item.name }}</li> }";

            var component = generator.GenerateComponent("ProductsComponent", "app-products", "/api/products", "ProductDto", template, new[] { "rsItems" });

            Assert.Empty(component.Findings);
            Assert.Contains("standalone: true", component.TypeScript);
            Assert.Contains("inject(HttpClient)", component.TypeScript);
            Assert.Contains("toSignal(", component.TypeScript);
            Assert.DoesNotContain("constructor(", component.TypeScript);
        }

        [Fact]
        public void FormConverter_InfersRequiredFromEmptyCheckAndFlagsAmbiguousBareRequestAccess()
        {
            var asp = "<% If Request.Form(\"Email\") = \"\" Then %>Missing<% End If %><% x = Request(\"Promo\") %>";
            var page = new AspParser().Parse(asp);

            var result = new FormConverter().Convert(page.Statements, "checkoutForm");

            var email = result.Fields.Single(f => f.Name == "Email");
            Assert.True(email.Required);
            Assert.True(email.LooksLikeEmail);
            Assert.Contains(result.Fields, f => f.Name == "Promo");
            Assert.Contains(result.Warnings, w => w.Contains("ambiguous"));
            Assert.Contains("Validators.required", result.TypeScript);
            Assert.Contains("FormBuilder", result.TypeScript); // uses typed Reactive Forms, not template-driven ngModel binding
        }

        [Fact]
        public void RoutingGenerator_PutsHomePageAtEmptyPathAndOthersAtKebabRoutes()
        {
            var edges = new List<PageFlowEdge>
            {
                new() { FromPage = "index.asp", ToPage = "productDetails.asp", Trigger = PageFlowTrigger.Link }
            };

            var routes = new RoutingGenerator().GenerateRoutes(edges, "index.asp");

            Assert.Contains("{ path: '', loadComponent:", routes);
            Assert.Contains("product-details", routes);
            Assert.Contains("ProductDetailsComponent", routes);
            Assert.Contains("{ path: '**', redirectTo: '' }", routes);
        }
        [Fact]
        public void DtoGenerator_EmitsUntypedPropertiesWithVerifyTodoForEachAspField()
        {
            var output = new DtoGenerator().GenerateDto("ProductDto", new[] { "Id", "Name", "Price" });

            Assert.Contains("public class ProductDto", output);
            Assert.Contains("public object? Id { get; set; }", output);
            Assert.Contains("public object? Name { get; set; }", output);
            Assert.Contains("// TODO: verify type", output);
        }

        [Fact]
        public void ServiceGenerator_AlwaysParameterizesEvenWhenSourceSqlWasConcatenated()
        {
            var asp = "<% Set rs = Server.CreateObject(\"ADODB.Recordset\") %>"
                     + "<% rs.Open \"SELECT * FROM Products WHERE Id=\" & id, conn %>";
            var page = new AspParser().Parse(asp);
            var adoObjects = new DatabaseCallAnalyzer().Analyze(page.Statements);
            var site = adoObjects.Single().CallSites.Single();
            Assert.True(site.BuiltByUnsafeConcatenation); // confirm the source really was unsafe

            var spec = new ServiceMethodSpec { MethodName = "GetProducts", Site = site, ResultFields = new[] { "Id", "Name" } };
            var output = new ServiceGenerator().GenerateServiceClass("ProductService", "ProductDto", new[] { spec });

            Assert.Contains("new SqlCommand(\"SELECT * FROM Products WHERE Id=@id\", connection)", output);
            Assert.Contains("command.Parameters.AddWithValue(\"@id\", id)", output);
            Assert.DoesNotContain("\" & id", output); // no leftover string concatenation of user input into SQL
        }

        [Fact]
        public void ControllerGenerator_DerivesRestVerbFromSqlStatement()
        {
            Assert.Equal(("GET", false), ControllerGenerator.DeriveVerbFromSql("SELECT * FROM Products"));
            Assert.Equal(("POST", true), ControllerGenerator.DeriveVerbFromSql("INSERT INTO Products (Name) VALUES (?)"));
            Assert.Equal(("PUT", true), ControllerGenerator.DeriveVerbFromSql("UPDATE Products SET Name=?"));
            Assert.Equal(("DELETE", true), ControllerGenerator.DeriveVerbFromSql("DELETE FROM Products WHERE Id=?"));
        }

        [Fact]
        public void ControllerGenerator_EmitsRestConventionActionsWithCorrectStatusCodes()
        {
            var actions = new[]
            {
                new ControllerActionSpec { MethodName = "GetProducts", ServiceMethodName = "GetProducts", HttpVerb = "GET" },
                new ControllerActionSpec { MethodName = "CreateProduct", ServiceMethodName = "CreateProduct", HttpVerb = "POST" }
            };
            var output = new ControllerGenerator().GenerateController("products", "ProductsController", "ProductService", "ProductDto", actions);

            Assert.Contains("[Route(\"api/products\")]", output);
            Assert.Contains("[HttpGet(", output);
            Assert.Contains("[HttpPost]", output);
            Assert.Contains("StatusCode(201)", output);
        }

        [Fact]
        public void AuthConverter_NormalizesAllThreeNullCheckIdiomsToOneClaimsCheck()
        {
            var info = new SessionVariableInfo { Name = "UserId" };
            info.NullCheckIdiomsObserved.Add(SessionNullCheckIdiom.EqualsEmptyString);
            info.NullCheckIdiomsObserved.Add(SessionNullCheckIdiom.IsEmptyCall);
            info.NullCheckIdiomsObserved.Add(SessionNullCheckIdiom.IsNothingComparison);

            var check = new AuthConverter().GenerateNormalizedNullCheck(info);

            Assert.Contains("User.HasClaim(c => c.Type == \"UserId\")", check);
            // exactly one generated check regardless of how many idioms the source mixed
            Assert.Equal(1, System.Text.RegularExpressions.Regex.Matches(check, "User\\.HasClaim").Count);
        }

        [Fact]
        public void MiddlewareGenerator_WiresCorsJwtAndServiceRegistrationsIntoProgramCs()
        {
            var output = new MiddlewareGenerator().GenerateProgramCs(new[] { "ProductService" }, "https://localhost:4200");

            Assert.Contains("AddScoped<ProductService>", output);
            Assert.Contains("WithOrigins(\"https://localhost:4200\")", output);
            Assert.Contains("AddJwtBearer", output);
            Assert.Contains("app.UseAuthentication();", output);
            Assert.Contains("app.UseAuthorization();", output);
        }
        [Fact]
        public void SessionVariableTracker_CatalogsReadsWritesAndAllThreeNullCheckIdioms()
        {
            var asp = "<% Session(\"UserId\") = 42 %>"
                     + "<% If Session(\"UserId\") = \"\" Then %>A<% End If %>"
                     + "<% If IsEmpty(Session(\"UserId\")) Then %>B<% End If %>"
                     + "<% If Session(\"UserId\") Is Nothing Then %>C<% End If %>";
            var page = new AspParser().Parse(asp);

            var catalog = new SessionVariableTracker().Catalog(page.Statements);

            var info = catalog["UserId"];
            Assert.Single(info.WriteSites);
            Assert.Equal(3, info.ReadSites.Count);
            Assert.Contains(SessionNullCheckIdiom.EqualsEmptyString, info.NullCheckIdiomsObserved);
            Assert.Contains(SessionNullCheckIdiom.IsEmptyCall, info.NullCheckIdiomsObserved);
            Assert.Contains(SessionNullCheckIdiom.IsNothingComparison, info.NullCheckIdiomsObserved);
        }

        [Fact]
        public void DatabaseCallAnalyzer_FlagsSqlBuiltByUnsafeConcatenationAndExtractsTableName()
        {
            var asp = "<% Set rs = Server.CreateObject(\"ADODB.Recordset\") %>"
                     + "<% rs.Open \"SELECT * FROM Users WHERE Id=\" & userId, conn %>";
            var page = new AspParser().Parse(asp);

            var results = new DatabaseCallAnalyzer().Analyze(page.Statements);

            var rsInfo = Assert.Single(results);
            Assert.Equal(AdoObjectKind.Recordset, rsInfo.Kind);
            var callSite = Assert.Single(rsInfo.CallSites);
            Assert.True(callSite.BuiltByUnsafeConcatenation);
            Assert.Contains("Users", callSite.TablesReferenced);
        }

        [Fact]
        public void DatabaseCallAnalyzer_DoesNotFlagPureLiteralSqlAsUnsafe()
        {
            var asp = "<% Set rs = Server.CreateObject(\"ADODB.Recordset\") %>"
                     + "<% rs.Open \"SELECT * FROM Products\", conn %>";
            var page = new AspParser().Parse(asp);

            var results = new DatabaseCallAnalyzer().Analyze(page.Statements);

            var callSite = Assert.Single(Assert.Single(results).CallSites);
            Assert.False(callSite.BuiltByUnsafeConcatenation);
            Assert.Contains("Products", callSite.TablesReferenced);
        }

        [Fact]
        public void PageFlowAnalyzer_FindsServerRedirectFormAndLinkTargets()
        {
            var asp = "<% If Not loggedIn Then Response.Redirect \"login.asp\" %>"
                     + "<form action=\"save.asp\" method=\"post\"></form>"
                     + "<a href=\"details.asp?id=1\">details</a>";
            var page = new AspParser().Parse(asp);

            var edges = new PageFlowAnalyzer().Analyze(page.Statements, "index.asp");

            Assert.Contains(edges, e => e.ToPage == "login.asp" && e.Trigger == PageFlowTrigger.Redirect);
            Assert.Contains(edges, e => e.ToPage == "save.asp" && e.Trigger == PageFlowTrigger.FormSubmit && e.HttpMethod == "POST");
            Assert.Contains(edges, e => e.ToPage == "details.asp" && e.Trigger == PageFlowTrigger.Link);
        }

        [Fact]
        public void GlobalAsaParser_ExtractsLifecycleEventsAndObjectDeclarations()
        {
            var content = @"
<OBJECT RUNAT=Server SCOPE=Application ID=DbConn PROGID=""ADODB.Connection"">
</OBJECT>
<SCRIPT LANGUAGE=""VBScript"" RUNAT=""Server"">
Sub Application_OnStart
    Application(""Name"") = ""MyApp""
End Sub

Sub Session_OnStart
    Session.Timeout = 20
End Sub
</SCRIPT>";

            var page = new GlobalAsaParser().Parse(content);

            Assert.NotNull(page.ApplicationOnStart);
            Assert.NotNull(page.SessionOnStart);
            Assert.Null(page.ApplicationOnEnd);
            var obj = Assert.Single(page.Objects);
            Assert.Equal("DbConn", obj.Id);
            Assert.Equal("ADODB.Connection", obj.ProgId);
            Assert.Equal("Application", obj.Scope);
        }

        [Fact]
        public void BusinessLogicExtractor_SeparatesAdoWorkFromHtmlOutput()
        {
            var asp = "<% Set rs = Server.CreateObject(\"ADODB.Recordset\") %><h1>Title</h1><%= name %>";
            var page = new AspParser().Parse(asp);

            var classified = new BusinessLogicExtractor().Classify(page.Statements);

            var adoStatement = classified.First(c => c.Statement is AssignmentNode);
            Assert.Equal(StatementKind.BusinessLogic, adoStatement.Kind);

            var htmlStatement = classified.First(c => c.Statement is HtmlOutputStatementNode);
            Assert.Equal(StatementKind.Presentation, htmlStatement.Kind);

            var outputStatement = classified.First(c => c.Statement is AspOutputExpressionStatementNode);
            Assert.Equal(StatementKind.Presentation, outputStatement.Kind);
        }

        [Fact]
        public void BusinessLogicExtractor_PropagatesClassificationThroughSimpleDefUseChain()
        {
            // userId is assigned from a Session read (business logic); a later
            // assignment that only reads userId (no Session/Request/ADO marker of its
            // own) should still inherit that classification, since the computation it
            // performs belongs in the service layer alongside where userId came from.
            var asp = "<% userId = Session(\"UserId\") %><% displayName = userId & \" (VIP)\" %>";
            var page = new AspParser().Parse(asp);

            var classified = new BusinessLogicExtractor().Classify(page.Statements);
            var assignments = classified.Where(c => c.Statement is AssignmentNode).ToList();

            Assert.Equal(2, assignments.Count);
            Assert.All(assignments, a => Assert.Equal(StatementKind.BusinessLogic, a.Kind));
        }
        [Fact]
        public void AspLexer_SplitsHtmlCodeAndOutputExpressionRegions()
        {
            var lexer = new AspLexer();
            var regions = lexer.Tokenize("<html><% x = 1 %><%= x %></html>");

            Assert.Equal(4, regions.Count);
            Assert.Equal(AspRegionType.Html, regions[0].Type);
            Assert.Equal("<html>", regions[0].Text);
            Assert.Equal(AspRegionType.CodeBlock, regions[1].Type);
            Assert.Equal("x = 1", regions[1].Text);
            Assert.Equal(AspRegionType.OutputExpression, regions[2].Type);
            Assert.Equal(AspRegionType.Html, regions[3].Type);
            Assert.Equal("</html>", regions[3].Text);
        }

        [Fact]
        public void AspLexer_DoesNotTreatPercentGreaterThanInsideStringAsBlockTerminator()
        {
            var lexer = new AspLexer();
            var regions = lexer.Tokenize("<% s = \"50%>done\" %>");

            var code = Assert.Single(regions.Where(r => r.Type == AspRegionType.CodeBlock));
            Assert.Equal("s = \"50%>done\"", code.Text);
        }

        [Fact]
        public void AspLexer_DetectsDirectiveAndServerComment()
        {
            var lexer = new AspLexer();
            var regions = lexer.Tokenize("<%@ Language=\"VBScript\" %><%-- a comment --%>");

            Assert.Equal(AspRegionType.Directive, regions[0].Type);
            Assert.Equal(AspRegionType.ServerComment, regions[1].Type);
        }

        [Fact]
        public void AspLexer_DistinguishesIncludeDirectiveFromOrdinaryHtmlComment()
        {
            var lexer = new AspLexer();
            var regions = lexer.Tokenize("<!--#include file=\"header.asp\"--><!-- just a comment -->");

            Assert.Equal(2, regions.Count);
            Assert.Equal(AspRegionType.Include, regions[0].Type);
            Assert.Equal("header.asp", regions[0].IncludePath);
            Assert.False(regions[0].IncludeIsVirtual);
            Assert.Equal(AspRegionType.Html, regions[1].Type);
        }

        [Fact]
        public void AspParser_NestsHtmlInsideIfElseBlocksInsteadOfFlatteningAfterThem()
        {
            var page = new AspParser().Parse("<% If x = 1 Then %>A<% Else %>B<% End If %>");

            var ifStmt = Assert.IsType<IfStatementNode>(Assert.Single(page.Statements));
            var trueHtml = Assert.IsType<HtmlOutputStatementNode>(Assert.Single(ifStmt.TrueBlock.Statements));
            Assert.Equal("A", trueHtml.Html);
            var elseHtml = Assert.IsType<HtmlOutputStatementNode>(Assert.Single(ifStmt.ElseBlock!.Statements));
            Assert.Equal("B", elseHtml.Html);
        }

        [Fact]
        public void AspParser_ParsesSingleLineIfWithNoEndIf()
        {
            var page = new AspParser().Parse("<% If loggedIn Then Response.Write \"hi\" %>");

            var single = Assert.IsType<SingleLineIfStatementNode>(Assert.Single(page.Statements));
            var call = Assert.IsType<CallStatementNode>(single.ThenStatement);
            var invocation = Assert.IsType<InvocationExpressionNode>(call.Invocation);
            Assert.Single(invocation.Arguments);
        }

        [Fact]
        public void AspParser_ParsesClassicRecordsetLoopWithHtmlNestedInWhileBody()
        {
            // The exact pattern documented in ProjectPlan.md's "Data Binding Patterns" section.
            var asp = "<% While Not rs.EOF %><tr><td><%=rs(\"Name\")%></td></tr><% rs.MoveNext %><% Wend %>";
            var page = new AspParser().Parse(asp);

            var whileStmt = Assert.IsType<WhileStatementNode>(Assert.Single(page.Statements));
            Assert.Contains(whileStmt.Body.Statements, s => s is HtmlOutputStatementNode html && html.Html.Contains("<tr>"));
            Assert.Contains(whileStmt.Body.Statements, s => s is AspOutputExpressionStatementNode);
            Assert.Contains(whileStmt.Body.Statements, s => s is CallStatementNode);
        }

        [Fact]
        public void AspParser_ParsesParenlessStatementCallWithMultipleArguments()
        {
            var page = new AspParser().Parse("<% Response.Write \"hello\", \"world\" %>");

            var call = Assert.IsType<CallStatementNode>(Assert.Single(page.Statements));
            var invocation = Assert.IsType<InvocationExpressionNode>(call.Invocation);
            Assert.Equal(2, invocation.Arguments.Count);
        }

        [Fact]
        public void AspParser_ParsesForEachOverServerVariablesStyleCollection()
        {
            var page = new AspParser().Parse("<% For Each item In items %><%=item%><% Next %>");

            var forEach = Assert.IsType<ForEachStatementNode>(Assert.Single(page.Statements));
            Assert.Equal("item", forEach.LoopVariable);
            Assert.Single(forEach.Body.Statements);
        }

        [Fact]
        public void AspParser_DoesNotRunTogetherStatementsFromSeparateCodeBlocks()
        {
            // Without a synthetic break between adjacent <% %> tags, this would misparse
            // as a parenless call `rs.MoveNext(x)` followed by a stray `= 1`.
            var page = new AspParser().Parse("<% rs.MoveNext %><% x = 1 %>");

            Assert.Equal(2, page.Statements.Count);
            Assert.IsType<CallStatementNode>(page.Statements[0]);
            Assert.IsType<AssignmentNode>(page.Statements[1]);
        }

        [Fact]
        public void AspParser_ParsesDimWithMultipleNames()
        {
            var page = new AspParser().Parse("<% Dim a, b, c %>");

            var group = Assert.IsType<VariableDeclarationGroupNode>(Assert.Single(page.Statements));
            Assert.Equal(new[] { "a", "b", "c" }, group.Declarations.Select(d => d.Name));
        }

        [Fact]
        public void AspParser_ExtractsDirectiveAttributes()
        {
            var page = new AspParser().Parse("<%@ Language=\"VBScript\" CodePage=\"65001\" %>");

            var directive = Assert.Single(page.Directives);
            Assert.Equal("VBScript", directive.Attributes["Language"]);
            Assert.Equal("65001", directive.Attributes["CodePage"]);
        }

        [Fact]
        public void IncludeFileResolver_SplicesFileRelativeInclude()
        {
            var root = Path.Combine(Path.GetTempPath(), "BLML.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            try
            {
                var headerPath = Path.Combine(root, "header.asp");
                File.WriteAllText(headerPath, "<b>Header</b>");
                var mainPath = Path.Combine(root, "main.asp");
                File.WriteAllText(mainPath, "<!--#include file=\"header.asp\"-->Body");

                var page = new AspParser().Parse(File.ReadAllText(mainPath), mainPath, root);

                var html = Assert.IsType<HtmlOutputStatementNode>(Assert.Single(page.Statements));
                Assert.Equal("<b>Header</b>Body", html.Html);
                Assert.Single(page.ResolvedIncludePaths);
            }
            finally
            {
                Directory.Delete(root, recursive: true);
            }
        }

        [Fact]
        public void IncludeFileResolver_DetectsCircularIncludes()
        {
            var root = Path.Combine(Path.GetTempPath(), "BLML.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            try
            {
                var aPath = Path.Combine(root, "a.asp");
                var bPath = Path.Combine(root, "b.asp");
                File.WriteAllText(aPath, "<!--#include file=\"b.asp\"-->");
                File.WriteAllText(bPath, "<!--#include file=\"a.asp\"-->");

                var resolver = new IncludeFileResolver(root);
                var result = resolver.ResolveIncludes(File.ReadAllText(aPath), aPath);

                Assert.Contains(result.Warnings, w => w.Contains("Circular include"));
            }
            finally
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }
}
