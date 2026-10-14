using System.Text;

namespace BLML.Phase5ASPtoAngular.Backend.Infrastructure
{
    /// <summary>
    /// Generates the top-level-statement `Program.cs` for the migrated Web API:
    /// DI registration for every generated service, JWT bearer auth (matching
    /// AuthConverter's issued tokens), CORS scoped to the Angular dev/prod origin
    /// (ProjectPlan.md item 63), global exception handling instead of letting an
    /// unhandled ADO exception leak a stack trace to the client, and Swagger/OpenAPI
    /// (item 121) wired up from the start rather than bolted on later.
    /// </summary>
    public class MiddlewareGenerator
    {
        public string GenerateProgramCs(IEnumerable<string> serviceClassNames, string angularOrigin, string connectionStringConfigKey = "DefaultConnection")
        {
            var sb = new StringBuilder();
            sb.AppendLine("using Microsoft.AspNetCore.Authentication.JwtBearer;");
            sb.AppendLine("using Microsoft.IdentityModel.Tokens;");
            sb.AppendLine("using System.Text;");
            sb.AppendLine("using BLML.Api.Services;");
            sb.AppendLine();
            sb.AppendLine("var builder = WebApplication.CreateBuilder(args);");
            sb.AppendLine();
            sb.AppendLine("builder.Services.AddControllers();");
            sb.AppendLine("builder.Services.AddEndpointsApiExplorer();");
            sb.AppendLine("builder.Services.AddSwaggerGen();");
            sb.AppendLine();
            sb.AppendLine($"var connectionString = builder.Configuration.GetConnectionString(\"{connectionStringConfigKey}\") ?? string.Empty;");
            foreach (var service in serviceClassNames)
            {
                sb.AppendLine($"builder.Services.AddScoped<{service}>(_ => new {service}(connectionString));");
            }
            sb.AppendLine("builder.Services.AddScoped<AuthService>(_ => new AuthService(builder.Configuration[\"Jwt:SigningKey\"] ?? string.Empty));");
            sb.AppendLine();
            sb.AppendLine("builder.Services.AddCors(options =>");
            sb.AppendLine("{");
            sb.AppendLine("    options.AddDefaultPolicy(policy =>");
            sb.AppendLine($"        policy.WithOrigins(\"{angularOrigin}\").AllowAnyHeader().AllowAnyMethod());");
            sb.AppendLine("});");
            sb.AppendLine();
            sb.AppendLine("builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)");
            sb.AppendLine("    .AddJwtBearer(options =>");
            sb.AppendLine("    {");
            sb.AppendLine("        var signingKey = builder.Configuration[\"Jwt:SigningKey\"] ?? string.Empty;");
            sb.AppendLine("        options.TokenValidationParameters = new TokenValidationParameters");
            sb.AppendLine("        {");
            sb.AppendLine("            ValidateIssuer = false,");
            sb.AppendLine("            ValidateAudience = false,");
            sb.AppendLine("            ValidateLifetime = true,");
            sb.AppendLine("            ValidateIssuerSigningKey = true,");
            sb.AppendLine("            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey))");
            sb.AppendLine("        };");
            sb.AppendLine("    });");
            sb.AppendLine("builder.Services.AddAuthorization();");
            sb.AppendLine();
            sb.AppendLine("var app = builder.Build();");
            sb.AppendLine();
            sb.AppendLine("app.UseExceptionHandler(errorApp =>");
            sb.AppendLine("{");
            sb.AppendLine("    errorApp.Run(async context =>");
            sb.AppendLine("    {");
            sb.AppendLine("        context.Response.StatusCode = 500;");
            sb.AppendLine("        context.Response.ContentType = \"application/json\";");
            sb.AppendLine("        await context.Response.WriteAsJsonAsync(new { error = \"An unexpected error occurred.\" });");
            sb.AppendLine("    });");
            sb.AppendLine("});");
            sb.AppendLine();
            sb.AppendLine("if (app.Environment.IsDevelopment())");
            sb.AppendLine("{");
            sb.AppendLine("    app.UseSwagger();");
            sb.AppendLine("    app.UseSwaggerUI();");
            sb.AppendLine("}");
            sb.AppendLine();
            sb.AppendLine("app.UseHttpsRedirection();");
            sb.AppendLine("app.UseCors();");
            sb.AppendLine("app.UseAuthentication();");
            sb.AppendLine("app.UseAuthorization();");
            sb.AppendLine("app.MapControllers();");
            sb.AppendLine("app.Run();");
            return sb.ToString();
        }
    }
}
