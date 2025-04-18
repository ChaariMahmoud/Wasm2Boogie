using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using WasmToBoogie.Core.WasmAST;
using System.Text.RegularExpressions;
using System.Linq;

namespace WasmToBoogie.Core.WasmParser
{
    public class BinaryenWasmParser : IWasmParser
    {
        private readonly string _binaryenPath;
        private readonly string _tempDir;

        public BinaryenWasmParser(string binaryenPath = "wasm-dis")
        {
            _binaryenPath = binaryenPath;
            _tempDir = Path.Combine(Path.GetTempPath(), "WasmToBoogie");
            Directory.CreateDirectory(_tempDir);
        }

        public WasmModule ParseWasmFile(string wasmFilePath)
        {
            if (!File.Exists(wasmFilePath))
            {
                throw new FileNotFoundException($"WASM file not found: {wasmFilePath}");
            }

            string watContent = ConvertWasmToWat(wasmFilePath);
            return ParseWatContent(watContent);
        }

        public WasmModule ParseWasmData(byte[] wasmData)
        {
            string tempWasmPath = Path.Combine(_tempDir, $"temp_{Guid.NewGuid()}.wasm");
            File.WriteAllBytes(tempWasmPath, wasmData);

            try
            {
                return ParseWasmFile(tempWasmPath);
            }
            finally
            {
                File.Delete(tempWasmPath);
            }
        }

        private string ConvertWasmToWat(string wasmFilePath)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _binaryenPath,
                    Arguments = wasmFilePath,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new Exception($"Failed to convert WASM to WAT: {error}");
            }

            return output;
        }

        public WasmModule ParseWatContent(string watContent)
        {
            var module = new WasmModule { Name = "module" };
            var lines = watContent.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            WasmFunction? currentFunction = null;
            WasmTable? currentTable = null;
            WasmGlobal? currentGlobal = null;
            var functionIdToName = new Dictionary<string, string>();
            var nextFunctionId = 0;

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (string.IsNullOrEmpty(trimmedLine)) continue;

                if (trimmedLine.StartsWith("(module"))
                {
                    continue;
                }
                else if (trimmedLine.StartsWith("(type"))
                {
                    // Type definitions are handled implicitly through function signatures
                    continue;
                }
                else if (trimmedLine.StartsWith("(func"))
                {
                    var funcId = ExtractFunctionId(trimmedLine);
                    currentFunction = new WasmFunction { Name = funcId ?? $"func{nextFunctionId++}" };
                    module.Functions.Add(currentFunction);
                    ParseFunctionSignature(trimmedLine, currentFunction);
                }
                else if (trimmedLine.StartsWith("(memory"))
                {
                    var memory = new WasmMemory { Name = "memory" };
                    ParseMemoryDefinition(trimmedLine, memory);
                    module.Memories.Add(memory);
                }
                else if (trimmedLine.StartsWith("(table"))
                {
                    currentTable = new WasmTable { Name = "table" };
                    ParseTableDefinition(trimmedLine, currentTable);
                    module.Tables.Add(currentTable);
                }
                else if (trimmedLine.StartsWith("(global"))
                {
                    currentGlobal = new WasmGlobal { Name = "global" };
                    ParseGlobalDefinition(trimmedLine, currentGlobal);
                    module.Globals.Add(currentGlobal);
                }
                else if (trimmedLine.StartsWith("(export"))
                {
                    var export = ParseExportDefinition(trimmedLine);
                    module.Exports.Add(export);

                    // Update function name if this is a function export
                    if (export.Kind == WasmExportKind.Function)
                    {
                        var function = module.Functions.FirstOrDefault(f => f.Name == export.Target);
                        if (function != null)
                        {
                            function.Name = export.Name;
                        }
                    }
                }
                else if (trimmedLine.StartsWith("(elem"))
                {
                    if (currentTable != null)
                    {
                        ParseTableElement(trimmedLine, currentTable);
                    }
                }
                else if (currentFunction != null)
                {
                    if (trimmedLine.StartsWith(")"))
                    {
                        currentFunction = null;
                    }
                    else
                    {
                        ParseFunctionBody(trimmedLine, currentFunction);
                    }
                }
            }

            return module;
        }

        private string? ExtractFunctionId(string line)
        {
            var match = System.Text.RegularExpressions.Regex.Match(line, @"\(func\s+(\$[^\s\(]+)");
            return match.Success ? match.Groups[1].Value.TrimStart('$') : null;
        }

        private void UpdateFunctionName(WasmFunction function, string line)
        {
            // Extract parameters and results
            var paramMatches = System.Text.RegularExpressions.Regex.Matches(line, @"\(param(?:\s+\$[^\s)]+)?\s+([^)]+)\)");
            foreach (Match paramMatch in paramMatches)
            {
                var paramType = paramMatch.Groups[1].Value.Trim();
                function.Parameters.Add(ParseValueType(paramType));
            }

            var resultMatches = System.Text.RegularExpressions.Regex.Matches(line, @"\(result\s+([^)]+)\)");
            foreach (Match resultMatch in resultMatches)
            {
                var resultType = resultMatch.Groups[1].Value.Trim();
                function.Results.Add(ParseValueType(resultType));
            }
        }

        private void ParseFunctionBody(string line, WasmFunction function)
        {
            var instruction = new WasmInstruction { OpCode = "" };
            var parts = line.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length > 0)
            {
                instruction.OpCode = parts[0];
                for (int i = 1; i < parts.Length; i++)
                {
                    if (int.TryParse(parts[i], out int intValue))
                    {
                        instruction.Operands.Add(intValue);
                    }
                    else if (float.TryParse(parts[i], out float floatValue))
                    {
                        instruction.Operands.Add(floatValue);
                    }
                    else
                    {
                        instruction.Operands.Add(parts[i]);
                    }
                }
                function.Body.Add(instruction);
            }
        }

        private void ParseMemoryDefinition(string line, WasmMemory memory)
        {
            var nameMatch = System.Text.RegularExpressions.Regex.Match(line, @"\(memory\s+(?:(\$[^\s)]+)\s+)?");
            if (nameMatch.Success && nameMatch.Groups[1].Success)
            {
                memory.Name = nameMatch.Groups[1].Value.TrimStart('$');
            }

            var sizeMatch = System.Text.RegularExpressions.Regex.Match(line, @"(\d+)(?:\s+(\d+))?");
            if (sizeMatch.Success)
            {
                memory.InitialSize = uint.Parse(sizeMatch.Groups[1].Value);
                if (sizeMatch.Groups[2].Success)
                {
                    memory.MaximumSize = uint.Parse(sizeMatch.Groups[2].Value);
                }
            }
        }

        private void ParseTableDefinition(string line, WasmTable table)
        {
            var nameMatch = System.Text.RegularExpressions.Regex.Match(line, @"\(table\s+(?:(\$[^\s)]+)\s+)?");
            if (nameMatch.Success && nameMatch.Groups[1].Success)
            {
                table.Name = nameMatch.Groups[1].Value.TrimStart('$');
            }

            var sizeMatch = System.Text.RegularExpressions.Regex.Match(line, @"(\d+)(?:\s+(\d+))?");
            if (sizeMatch.Success)
            {
                table.InitialSize = uint.Parse(sizeMatch.Groups[1].Value);
                if (sizeMatch.Groups[2].Success)
                {
                    table.MaximumSize = uint.Parse(sizeMatch.Groups[2].Value);
                }
            }
        }

        private void ParseGlobalDefinition(string line, WasmGlobal global)
        {
            var nameMatch = System.Text.RegularExpressions.Regex.Match(line, @"\(global\s+(?:(\$[^\s)]+)\s+)?");
            if (nameMatch.Success && nameMatch.Groups[1].Success)
            {
                global.Name = nameMatch.Groups[1].Value.TrimStart('$');
            }

            var mutableMatch = System.Text.RegularExpressions.Regex.Match(line, @"\(mut\s+([^)]+)\)");
            if (mutableMatch.Success)
            {
                global.IsMutable = true;
                global.Type = ParseValueType(mutableMatch.Groups[1].Value.Trim());
            }
            else
            {
                var typeMatch = System.Text.RegularExpressions.Regex.Match(line, @"\(([^)]+)\)");
                if (typeMatch.Success)
                {
                    global.Type = ParseValueType(typeMatch.Groups[1].Value.Trim());
                }
            }
        }

        private WasmExport ParseExportDefinition(string line)
        {
            // Match both formats:
            // (export "name" (func $id))
            // (export "name" func $id)
            var match = System.Text.RegularExpressions.Regex.Match(line, @"\(export\s+""([^""]+)""\s+(?:\(([^\s]+)\s+([^\s\)]+)\)|([^\s]+)\s+([^\s\)]+))");
            
            if (!match.Success)
            {
                throw new FormatException($"Invalid export definition: {line}");
            }

            var name = match.Groups[1].Value;
            
            // Handle both formats
            string kind = match.Groups[2].Success ? match.Groups[2].Value : match.Groups[4].Value;
            string target = match.Groups[3].Success ? match.Groups[3].Value : match.Groups[5].Value;
            
            return new WasmExport
            {
                Name = name,
                Kind = ParseExportKind(kind),
                Target = target.TrimStart('$')
            };
        }

        private WasmExportKind ParseExportKind(string kind)
        {
            return kind.ToLower() switch
            {
                "func" => WasmExportKind.Function,
                "memory" => WasmExportKind.Memory,
                "table" => WasmExportKind.Table,
                "global" => WasmExportKind.Global,
                _ => throw new FormatException($"Unknown export kind: {kind}")
            };
        }

        private void ParseTableElement(string line, WasmTable table)
        {
            var elementMatch = System.Text.RegularExpressions.Regex.Match(line, @"\(elem\s+[^)]+\)\s+([^)]+)\)");
            if (elementMatch.Success)
            {
                var elements = elementMatch.Groups[1].Value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var element in elements)
                {
                    table.Elements.Add(element.TrimStart('$'));
                }
            }
        }

        private WasmValueType ParseValueType(string typeStr)
        {
            return typeStr switch
            {
                "i32" => WasmValueType.I32,
                "i64" => WasmValueType.I64,
                "f32" => WasmValueType.F32,
                "f64" => WasmValueType.F64,
                _ => throw new FormatException($"Unknown value type: {typeStr}")
            };
        }

        private void ParseFunctionSignature(string line, WasmFunction function)
        {
            // Extract parameters and results
            var paramMatches = System.Text.RegularExpressions.Regex.Matches(line, @"\(param(?:\s+\$[^\s)]+)?\s+([^)]+)\)");
            foreach (Match paramMatch in paramMatches)
            {
                var paramTypes = paramMatch.Groups[1].Value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var paramType in paramTypes)
                {
                    function.Parameters.Add(ParseValueType(paramType.Trim()));
                }
            }

            var resultMatches = System.Text.RegularExpressions.Regex.Matches(line, @"\(result\s+([^)]+)\)");
            foreach (Match resultMatch in resultMatches)
            {
                var resultTypes = resultMatch.Groups[1].Value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var resultType in resultTypes)
                {
                    function.Results.Add(ParseValueType(resultType.Trim()));
                }
            }
        }
    }
} 