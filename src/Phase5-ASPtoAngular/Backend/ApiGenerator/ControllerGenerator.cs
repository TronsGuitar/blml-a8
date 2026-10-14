using System.Text;
using BLML.Phase5ASPtoAngular.Analysis;

namespace BLML.Phase5ASPtoAngular.Backend.ApiGenerator
{
    public class ControllerActionSpec
    {
        public string MethodName { get; set; } = string.Empty; // e.g. "GetProducts" / "CreateProduct"
        public string ServiceMethodName { get; set; } = string.Empty; // matches ServiceMethodSpec.MethodName
        public string HttpVerb { get; set; } = "GET"; // GET, POST, PUT, DELETE
        public bool HasIdRouteParameter { get; set; }
    }

    /// <summary>
    /// Generates a standard ASP.NET Core `[ApiController]` following the REST
    /// conventions ProjectPlan.md's "API Design Patterns" section calls for: plural
    /// resource routes, one verb per CRUD operation, `Ok`/`NotFound`/`NoContent`
    /// mapped to the right status code rather than always returning 200.
    /// </summary>
    public class ControllerGenerator
    {
        public string GenerateController(string resourceName, string controllerClassName, string serviceName, string dtoName,
            IEnumerable<ControllerActionSpec> actions, string @namespace = "BLML.Api.Controllers")
        {
            var sb = new StringBuilder();
            sb.AppendLine("using Microsoft.AspNetCore.Mvc;");
            sb.AppendLine("using System.Threading.Tasks;");
            sb.AppendLine("using BLML.Api.Dtos;");
            sb.AppendLine("using BLML.Api.Services;");
            sb.AppendLine();
            sb.AppendLine($"namespace {@namespace}");
            sb.AppendLine("{");
            sb.AppendLine("    [ApiController]");
            sb.AppendLine($"    [Route(\"api/{resourceName.ToLowerInvariant()}\")]");
            sb.AppendLine($"    public class {controllerClassName} : ControllerBase");
            sb.AppendLine("    {");
            sb.AppendLine($"        private readonly {serviceName} _service;");
            sb.AppendLine();
            sb.AppendLine($"        public {controllerClassName}({serviceName} service)");
            sb.AppendLine("        {");
            sb.AppendLine("            _service = service;");
            sb.AppendLine("        }");

            foreach (var action in actions)
            {
                sb.AppendLine();
                AppendAction(sb, action, dtoName);
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        private void AppendAction(StringBuilder sb, ControllerActionSpec action, string dtoName)
        {
            switch (action.HttpVerb.ToUpperInvariant())
            {
                case "GET":
                    var route = action.HasIdRouteParameter ? "\"{id}\"" : "";
                    var param = action.HasIdRouteParameter ? "int id" : "";
                    var arg = action.HasIdRouteParameter ? "id" : "";
                    sb.AppendLine($"        [HttpGet({route})]");
                    sb.AppendLine($"        public async Task<IActionResult> {action.MethodName}({param})");
                    sb.AppendLine("        {");
                    sb.AppendLine($"            var result = await _service.{action.ServiceMethodName}Async({arg});");
                    sb.AppendLine(action.HasIdRouteParameter
                        ? "            return result.Count > 0 ? Ok(result[0]) : NotFound();"
                        : "            return Ok(result);");
                    sb.AppendLine("        }");
                    break;

                case "POST":
                    sb.AppendLine("        [HttpPost]");
                    sb.AppendLine($"        public async Task<IActionResult> {action.MethodName}([FromBody] {dtoName} body)");
                    sb.AppendLine("        {");
                    sb.AppendLine($"            var affected = await _service.{action.ServiceMethodName}Async(body);");
                    sb.AppendLine("            return affected > 0 ? StatusCode(201) : BadRequest();");
                    sb.AppendLine("        }");
                    break;

                case "PUT":
                    sb.AppendLine("        [HttpPut(\"{id}\")]");
                    sb.AppendLine($"        public async Task<IActionResult> {action.MethodName}(int id, [FromBody] {dtoName} body)");
                    sb.AppendLine("        {");
                    sb.AppendLine($"            var affected = await _service.{action.ServiceMethodName}Async(id, body);");
                    sb.AppendLine("            return affected > 0 ? NoContent() : NotFound();");
                    sb.AppendLine("        }");
                    break;

                case "DELETE":
                    sb.AppendLine("        [HttpDelete(\"{id}\")]");
                    sb.AppendLine($"        public async Task<IActionResult> {action.MethodName}(int id)");
                    sb.AppendLine("        {");
                    sb.AppendLine($"            var affected = await _service.{action.ServiceMethodName}Async(id);");
                    sb.AppendLine("            return affected > 0 ? NoContent() : NotFound();");
                    sb.AppendLine("        }");
                    break;
            }
        }

        /// <summary>Maps a reconstructed SQL statement's leading verb to the REST verb ProjectPlan.md's API conventions call for.</summary>
        public static (string httpVerb, bool isMutation) DeriveVerbFromSql(string sqlText)
        {
            var trimmed = sqlText.TrimStart();
            var firstWord = trimmed.Split(' ', '\t', '\n').FirstOrDefault() ?? "";
            return firstWord.ToUpperInvariant() switch
            {
                "SELECT" => ("GET", false),
                "INSERT" => ("POST", true),
                "UPDATE" => ("PUT", true),
                "DELETE" => ("DELETE", true),
                _ => ("GET", false)
            };
        }
    }
}
