using System.Text;
using BLML.Phase5ASPtoAngular.Analysis;

namespace BLML.Phase5ASPtoAngular.Backend.Infrastructure
{
    /// <summary>
    /// Classic ASP session-based auth has no single equivalent in a stateless Web API -
    /// this converts it to JWT bearer auth, and specifically normalizes the three
    /// interchangeable "is this session value unset" idioms SessionVariableTracker
    /// found (`= ""`, `IsEmpty(...)`, `Is Nothing`) into one consistent claims check,
    /// since a straight per-idiom translation would leave the generated code just as
    /// inconsistent as the ASP it came from.
    /// </summary>
    public class AuthConverter
    {
        public string GenerateAuthService(IEnumerable<string> identityClaimKeys, string @namespace = "BLML.Api.Services")
        {
            var sb = new StringBuilder();
            sb.AppendLine("using System.IdentityModel.Tokens.Jwt;");
            sb.AppendLine("using System.Security.Claims;");
            sb.AppendLine("using System.Text;");
            sb.AppendLine("using Microsoft.IdentityModel.Tokens;");
            sb.AppendLine();
            sb.AppendLine($"namespace {@namespace}");
            sb.AppendLine("{");
            sb.AppendLine("    public class AuthService");
            sb.AppendLine("    {");
            sb.AppendLine("        private readonly string _signingKey;");
            sb.AppendLine();
            sb.AppendLine("        public AuthService(string signingKey)");
            sb.AppendLine("        {");
            sb.AppendLine("            _signingKey = signingKey;");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        // Session(\"key\") values that identified the user in the original ASP pages become");
            sb.AppendLine("        // JWT claims here, issued once at login instead of being re-read from server-side session state on every request.");
            sb.AppendLine("        public string IssueToken(Dictionary<string, string> identityValues)");
            sb.AppendLine("        {");
            sb.AppendLine("            var claims = new List<Claim>();");
            foreach (var key in identityClaimKeys)
            {
                sb.AppendLine($"            if (identityValues.TryGetValue(\"{key}\", out var {ToCamelCase(key)}Value)) claims.Add(new Claim(\"{key}\", {ToCamelCase(key)}Value));");
            }
            sb.AppendLine("            var keyBytes = Encoding.UTF8.GetBytes(_signingKey);");
            sb.AppendLine("            var credentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256);");
            sb.AppendLine("            var token = new JwtSecurityToken(claims: claims, expires: DateTime.UtcNow.AddHours(8), signingCredentials: credentials);");
            sb.AppendLine("            return new JwtSecurityTokenHandler().WriteToken(token);");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        /// <summary>
        /// Emits one normalized claims-null-check regardless of which idiom(s) the
        /// original ASP page mixed for this session key - `User.HasClaim` is the single
        /// C# equivalent of "is this identity value set" that all three VBScript forms
        /// collapse to.
        /// </summary>
        public string GenerateNormalizedNullCheck(SessionVariableInfo info)
        {
            if (info.NullCheckIdiomsObserved.Count > 1)
            {
                return $"// NOTE: original ASP pages checked \"{info.Name}\" for emptiness using {info.NullCheckIdiomsObserved.Count} different idioms ({string.Join(", ", info.NullCheckIdiomsObserved)}); normalized to one check below.\n"
                     + $"!User.HasClaim(c => c.Type == \"{info.Name}\")";
            }
            return $"!User.HasClaim(c => c.Type == \"{info.Name}\")";
        }

        private static string ToCamelCase(string name) =>
            string.IsNullOrEmpty(name) ? name : char.ToLowerInvariant(name[0]) + name[1..];
    }
}
