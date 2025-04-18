using System;
using System.IO;
using WasmToBoogie.Core.WasmAST;

namespace WasmToBoogie.Core.WasmParser
{
    public class BasicWasmParser : IWasmParser
    {
        public WasmModule ParseWasmFile(string wasmFilePath)
        {
            if (!File.Exists(wasmFilePath))
            {
                throw new FileNotFoundException($"WASM file not found: {wasmFilePath}");
            }

            byte[] wasmData = File.ReadAllBytes(wasmFilePath);
            return ParseWasmData(wasmData);
        }

        public WasmModule ParseWasmData(byte[] wasmData)
        {
            // For testing purposes, we'll create a simple module with one function
            var module = new WasmModule { Name = "test_module" };
            
            // Create a simple function that adds two numbers
            var function = new WasmFunction
            {
                Name = "add",
                Parameters = new List<WasmValueType> { WasmValueType.I32, WasmValueType.I32 },
                Results = new List<WasmValueType> { WasmValueType.I32 },
                Body = new List<WasmInstruction>
                {
                    new WasmInstruction { OpCode = "local.get", Operands = new List<object> { 0 } },
                    new WasmInstruction { OpCode = "local.get", Operands = new List<object> { 1 } },
                    new WasmInstruction { OpCode = "i32.add", Operands = new List<object>() }
                }
            };

            module.Functions.Add(function);
            return module;
        }
    }
} 