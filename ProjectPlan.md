# VB6 to C# Converter - Prioritized Task List

---

## **PHASE 1: Foundation (Critical - Must Complete First)**

### Core Infrastructure
1. **VB6 Lexical Analyzer** - Complete tokenization of all VB6 syntax
2. **VB6 Parser** - Build complete grammar parser for VB6 statements
3. **Symbol Table** - Track variables, functions, classes across project
4. **AST Builder** - Create abstract syntax tree from parsed code
5. **Type Inference Engine** - Determine types for Variant and implicit declarations
6. **Project File Parser** (.vbp) - Extract project structure and dependencies
7. **Dependency Graph** - Map inter-project and module dependencies

### Basic Code Generation
8. **CodeDom/Roslyn Generator** - Core C# code emission engine
9. **Namespace Generator** - Map VB6 modules to C# namespaces
10. **Using Statement Generator** - Auto-generate required imports
11. **.csproj Generator** - Create valid C# project files
12. **Solution Generator** - Handle multi-project solutions

**Deliverable:** Can parse simple VB6 code files and generate compilable C# skeleton

---

## **PHASE 2: Core Language Features (High Priority)**

### Essential Conversions
13. **Variable Declarations** - Dim, Public, Private, Static → C# equivalents
14. **Basic Data Types** - Integer, Long, String, Boolean, Date conversions
15. **Operators** - Arithmetic, logical, comparison, concatenation (&, +)
16. **Control Flow** - If/Then/Else, For/Next, Do/Loop, While/Wend
17. **Functions & Subs** - Convert to C# methods with proper signatures
18. **Arrays** - Fixed and dynamic arrays, ReDim, Preserve
19. **String Operations** - Mid, Left, Right, Len, InStr → C# equivalents
20. **Math Functions** - Abs, Round, Int, Sqr → System.Math
21. **Comments** - Preserve ' and Rem as // or ///

### Error Handling
22. **On Error GoTo** → try-catch blocks (basic pattern)
23. **On Error Resume Next** → try-catch with continue
24. **Error Object** → Exception properties
25. **Err.Raise** → throw new Exception

**Deliverable:** Can convert 70% of typical business logic code

---

## **PHASE 3: Forms & UI (High Priority - User Visible)**

### Form Conversion
26. **.frm Parser** - Parse VB6 form files completely
27. **Form to WinForms** - Convert forms to C# Windows Forms
28. **Control Mapping** - Map VB6 controls to .NET equivalents (use property table)
29. **Event Handlers** - Convert VB6 events to C# event handlers
30. **Control Arrays** - Convert to List<Control> or dynamic creation
31. **Form Layout** - Preserve positioning and sizing
32. **.frx Resources** - Extract and convert binary resources

### Common Controls
33. **TextBox, Label, CommandButton** - Basic controls
34. **ComboBox, ListBox** - List controls with DataSource
35. **CheckBox, OptionButton** - Selection controls
36. **PictureBox, Image** - Image controls
37. **Frame** - GroupBox conversion
38. **Timer** - System.Windows.Forms.Timer

**Deliverable:** Can convert simple forms with basic controls

---

## **PHASE 4: Data Access (High Priority)**

### Database Conversion
39. **ADO Connection** → ADO.NET or Entity Framework
40. **Recordset** → DataTable or LINQ queries
41. **SQL Command Execution** - Convert Execute, ExecuteScalar
42. **Connection String** - Update to modern format
43. **Data Controls** - Convert Data1, Adodc to BindingSource
44. **Data Binding** - Convert field binding to modern patterns

### Access to SQL Server
45. **Schema Extraction** - Parse Access .mdb structure
46. **Table Migration** - Create SQL Server tables
47. **Query Conversion** - Access SQL → T-SQL
48. **Relationship Mapping** - FK constraints
49. **Index Migration** - Preserve indexes

**Deliverable:** Can convert database-driven applications

---

## **PHASE 5: Classic ASP to Angular + .NET Core + SQL Server**

### ASP Analysis & Parsing
50. **ASP/VBScript Lexer** - Tokenize ASP code (<% %>, <%= %>)
51. **ASP Parser** - Parse VBScript, HTML, and inline code
52. **Page Flow Analysis** - Map navigation and form submissions
53. **Include File Resolver** - Handle <!-- #include --> directives
54. **Session Variable Tracker** - Catalog Session() usage
55. **Database Call Analyzer** - Find all ADO/OLEDB operations
56. **Global.asa Parser** - Extract application events and settings
57. **Business Logic Extractor** - Separate logic from presentation

### Backend: ASP → .NET Core Web API
58. **API Project Generator** - Create .NET Core 8 Web API project
59. **Controller Generation** - Map ASP pages to API endpoints
60. **Service Layer Creation** - Extract business logic to services
61. **DTO Generation** - Create data transfer objects from recordsets
62. **Authentication Conversion** - Session-based → JWT/Cookie auth
63. **CORS Configuration** - Setup for Angular frontend
64. **Dependency Injection** - Register services and repositories
65. **Middleware Pipeline** - Logging, error handling, validation
66. **Connection String Migration** - ASP connection → appsettings.json
67. **ADO/COM Objects** → Entity Framework Core
   - **Connection** → DbContext
   - **Command** → LINQ or FromSqlRaw
   - **Recordset** → IQueryable<T> or List<T>
   - **Parameters** → Parameterized queries

### Database: Access/SQL Server → SQL Server (Modern)
68. **Schema Analysis** - Analyze Access .mdb structure
69. **SQL Server Database Creation** - Modern schema with best practices
70. **Table Normalization** - Fix Access denormalization issues
71. **Stored Procedure Generation** - Convert complex queries
72. **View Creation** - Optimize common queries
73. **Index Optimization** - Add missing indexes
74. **Constraint Migration** - FK, Check, Default constraints
75. **Data Migration Scripts** - ETL from Access to SQL Server
76. **EF Core DbContext** - Generate context and entity classes
77. **Repository Pattern** - Create generic repository

### Frontend: ASP HTML/Forms → Angular
78. **Angular Project Setup** - Create Angular 20 application
79. **Component Generation** - One component per ASP page
80. **Routing Configuration** - Map ASP page flow to Angular routes
81. **Template Conversion** - ASP HTML → Angular templates
82. **Form Migration** - ASP forms → Angular Reactive Forms
83. **Validation Rules** - Server-side → client-side + server API
84. **Data Table Components** - Recordset loops → Angular Material tables
85. **HTTP Service Layer** - API communication with HttpClient
86. **State Management** - Session vars → NgRx or BehaviorSubject
87. **Authentication Guard** - Protect routes based on auth state
88. **Interceptors** - JWT token injection, error handling
89. **Material UI Integration** - Apply Angular Material components
90. **Responsive Layout** - Make mobile-friendly (ASP was desktop-focused)

### ASP-Specific Conversions
91. **Response.Write** → Return JSON from API
92. **Request.Form/QueryString** → API parameters and route values
93. **Server.MapPath** → IWebHostEnvironment.ContentRootPath
94. **Application() variables** → IMemoryCache or Redis
95. **Session() variables** → JWT claims or distributed cache
96. **Request.ServerVariables** → HttpContext properties
97. **Response.Redirect** → Angular router navigation
98. **Server.Execute** → Component composition
99. **Server.Transfer** → Route parameters
100. **Response.Cookies** → Angular cookie service + API

### ASP Authentication/Authorization
101. **Form-based Auth** → JWT or Cookie authentication
102. **Session tracking** → Token-based stateless auth
103. **Role checking** → .NET Core authorization policies
104. **User validation** → Identity Framework or custom auth
105. **Login/Logout** → Angular auth service + API endpoints

### Data Binding Patterns
106. **ASP Recordset Loop** → *ngFor with async pipe
   ```vbscript
   <% While Not rs.EOF %>
     <tr><td><%=rs("Name")%></td></tr>
   <% rs.MoveNext : Wend %>
   ```
   →
   ```html
   <tr *ngFor="let item of items$ | async">
     <td>{{ item.name }}</td>
   </tr>
   ```

107. **Inline VBScript** → TypeScript methods
108. **Form Submission** → HTTP POST with observables
109. **Master Pages** → Angular layout components
110. **User Controls** → Reusable Angular components

### API Design Patterns
111. **RESTful Endpoints** - Design proper REST API
   - GET /api/customers → List customers
   - GET /api/customers/{id} → Get customer
   - POST /api/customers → Create customer
   - PUT /api/customers/{id} → Update customer
   - DELETE /api/customers/{id} → Delete customer

112. **Pagination Support** - Add paging to large datasets
113. **Filtering/Sorting** - Query parameters for data operations
114. **API Versioning** - /api/v1/ structure
115. **Response Standardization** - Consistent JSON format
116. **Error Responses** - Proper HTTP status codes

### Testing Strategy
117. **API Unit Tests** - xUnit tests for controllers/services
118. **Integration Tests** - Test database operations
119. **Angular Unit Tests** - Jasmine/Karma component tests
120. **E2E Tests** - Protractor or Cypress for full workflows
121. **API Documentation** - Swagger/OpenAPI generation

### Deployment & DevOps
122. **Docker Containers** - Containerize API and Angular
123. **CI/CD Pipeline** - GitHub Actions or Azure DevOps
124. **Environment Configuration** - Dev, Staging, Production
125. **Azure App Service** - Deploy API and Angular
126. **SQL Server Setup** - Azure SQL Database
127. **HTTPS/SSL** - Secure communication
128. **CDN Configuration** - Serve Angular assets
129. **Monitoring** - Application Insights integration

**Deliverable:** Complete modern web stack migration

---

## **PHASE 6: Advanced Features (Medium Priority)**

### Complex Language Features
130. **Property Procedures** - Get/Set/Let → C# properties
131. **With Statements** - Expand to explicit references
132. **Late Binding** - Variant → dynamic type
133. **Optional Parameters** - Default values
134. **ParamArray** → params keyword
135. **Named Arguments** - Preserve or convert
136. **Collections** - VB Collection → List<T> or Dictionary<K,V>
137. **Enums** - Convert VB6 Enum to C# enum

### COM & Interop
138. **Type Library Import** - Generate interop assemblies
139. **CreateObject** → COM interop or P/Invoke
140. **API Declares** → DllImport attributes
141. **OCX Controls** → Managed wrappers
142. **ActiveX** → .NET equivalents

### Advanced Controls
143. **SSTab** → TabControl
144. **MSFlexGrid** → DataGridView
145. **TreeView** → TreeView control
146. **ListView** → ListView control
147. **CommonDialog** → OpenFileDialog, SaveFileDialog, etc.
148. **RichTextBox** - Preserve RTF formatting
149. **Third-party Controls** - Manual mapping strategy

**Deliverable:** Handle 90% of VB6 code patterns

---

## **PHASE 7: Optimization & Polish (Medium Priority)**

### Code Quality
150. **Dead Code Removal** - Eliminate unused code
151. **Code Formatting** - Apply C# style guidelines
152. **Naming Conventions** - PascalCase, camelCase
153. **LINQ Optimization** - Efficient query generation
154. **Async/Await** - Modernize I/O operations (optional)
155. **Nullable Reference Types** - Add nullability annotations
156. **Pattern Matching** - Use modern C# patterns where applicable

### Documentation
157. **XML Documentation** - Generate /// comments
158. **Migration Report** - Summary of changes made
159. **Manual Review List** - Items needing human attention
160. **Code Metrics** - Complexity, maintainability scores

**Deliverable:** Production-ready, maintainable code

---

## **PHASE 8: Tooling & UX (Lower Priority)**

### IDE Support
161. **VS Code Extension** - VB6 language support
162. **LSP Server** - IntelliSense and navigation
163. **Syntax Highlighting** - VB6 color coding
164. **Debugger Support** - Step through VB6 code
165. **Converter GUI** - User-friendly interface

### Distribution
166. **NuGet Packages** - VB6 runtime helper library
167. **CLI Tool** - Command-line converter
168. **VS Extension** - Visual Studio integration
169. **Web Interface** - Online converter
170. **Documentation Site** - Complete user guide

**Deliverable:** Professional developer tooling

---

## **PRIORITY MATRIX**

| Phase | Priority | Effort | User Value | Dependencies |
|-------|----------|--------|------------|--------------|
| 1: Foundation | CRITICAL | High | Low | None |
| 2: Core Language | HIGH | High | Medium | Phase 1 |
| 3: Forms & UI | HIGH | Medium | HIGH | Phase 1, 2 |
| 4: Data Access | HIGH | Medium | HIGH | Phase 2 |
| 5: ASP→Angular | HIGH | Very High | HIGH | Phase 1, 2, 4 |
| 6: Advanced | MEDIUM | High | Medium | Phase 2 |
| 7: Polish | MEDIUM | Low | Medium | All previous |
| 8: Tooling | LOW | Medium | Low | Phase 2+ |

---

## **RECOMMENDED EXECUTION ORDER**

1. **Weeks 1-4:** Phase 1 (Foundation)
2. **Weeks 5-8:** Phase 2 (Core Language)
3. **Weeks 9-12:** Phase 3 (Forms) OR Phase 4 (Data Access) - parallel tracks
4. **Weeks 13-20:** Phase 5 (ASP to Angular) - separate team if available
5. **Weeks 21-24:** Phase 6 (Advanced Features)
6. **Weeks 25-26:** Phase 7 (Optimization)
7. **Ongoing:** Phase 8 (Tooling) - as resources allow

**Total Timeline:** 6-7 months for core functionality

---

## **FILE MAPPING TO PHASES**

See [REORGANIZATION.md](REORGANIZATION.md) for detailed file organization according to this plan.
