using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BLML.Phase4DataAccess.ADO
{
    public class AdoConverter
    {
        // Converts ADODB.Connection, Recordset to System.Data.SqlClient equivalents

        public StatementSyntax ConvertConnectionOpen(string connectionString)
        {
            // ADODB: cn.Open "..."
            // C#: using (var cn = new SqlConnection("...")) { cn.Open(); ... }
            
            return SyntaxFactory.ParseStatement($"var connection = new System.Data.SqlClient.SqlConnection({connectionString}); connection.Open();");
        }

        public StatementSyntax ConvertRecordsetOpen(string sql, string connectionVar)
        {
            // ADODB: rs.Open sql, cn
            // C#: var command = new SqlCommand(sql, connection); var reader = command.ExecuteReader();

            var block = SyntaxFactory.Block(
                SyntaxFactory.ParseStatement($"var command = new System.Data.SqlClient.SqlCommand({sql}, {connectionVar});"),
                SyntaxFactory.ParseStatement($"var reader = command.ExecuteReader();")
            );
            return block;
        }

        public StatementSyntax ConvertRecordsetLoop()
        {
            // ADODB: Do Until rs.EOF ... rs.MoveNext Loop
            // C#: while (reader.Read()) { ... }
            return SyntaxFactory.ParseStatement("while (reader.Read()) { /* body */ }");
        }

        public ExpressionSyntax ConvertFieldValue(string rsVar, string fieldName)
        {
            // ADODB: rs.Fields("Name").Value
            // C#: reader["Name"]
            return SyntaxFactory.ParseExpression($"{rsVar}[\"{fieldName}\"]");
        }

        public StatementSyntax ConvertMoveNext(string rsVar)
        {
            // ADODB: rs.MoveNext
            // C#: // Handled by while(reader.Read()) loop
            return SyntaxFactory.ParseStatement("// rs.MoveNext handled by Read() loop");
        }

        public StatementSyntax ConvertClose(string connectionVar)
        {
            // ADODB: cn.Close
            // C#: connection.Close();
            return SyntaxFactory.ParseStatement($"{connectionVar}.Close();");
        }
    }
}
