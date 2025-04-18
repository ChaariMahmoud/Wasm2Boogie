using System.Text;
using WasmToBoogie.Core.WasmAST;

namespace WasmToBoogie.Core.Translation
{
    public class BasicWasmToBoogieTranslator : IWasmToBoogieTranslator
    {
        public string TranslateToBoogie(WasmModule module)
        {
            var sb = new StringBuilder();
            sb.AppendLine("// Boogie program generated from WASM module");
            sb.AppendLine();

            // Add type declarations
            sb.AppendLine("type i32 = int;");
            sb.AppendLine();

            // Translate each function
            foreach (var function in module.Functions)
            {
                sb.AppendLine(TranslateFunction(function));
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public string TranslateFunction(WasmFunction function)
        {
            var sb = new StringBuilder();
            
            // Function signature
            sb.Append($"procedure {function.Name}(");
            
            // Parameters
            for (int i = 0; i < function.Parameters.Count; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append($"param{i}: i32");
            }
            
            sb.Append(") returns (");
            
            // Return values
            for (int i = 0; i < function.Results.Count; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append($"result{i}: i32");
            }
            
            sb.AppendLine(")");
            sb.AppendLine("{");
            
            // Function body
            foreach (var instruction in function.Body)
            {
                switch (instruction.OpCode)
                {
                    case "local.get":
                        int localIndex = (int)instruction.Operands[0];
                        sb.AppendLine($"    // Load local {localIndex}");
                        break;
                    case "i32.add":
                        sb.AppendLine("    // Add operation");
                        break;
                    default:
                        sb.AppendLine($"    // {instruction.OpCode}");
                        break;
                }
            }
            
            sb.AppendLine("}");
            
            return sb.ToString();
        }
    }
} 