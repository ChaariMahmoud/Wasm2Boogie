using System;
using System.IO;
using Xunit;
using WasmToBoogie.Core.WasmParser;
using WasmToBoogie.Core.WasmAST;

namespace WasmToBoogie.Tests
{
    public class AdvancedParserTests
    {
        private readonly string _testWasmPath;

        public AdvancedParserTests()
        {
            // Convert WAT to WASM
            string sourceWatPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "advanced_test.wat");
            _testWasmPath = Path.Combine(AppContext.BaseDirectory, "advanced_test.wasm");
            
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "wat2wasm",
                    Arguments = $"{sourceWatPath} -o {_testWasmPath}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new Exception($"Failed to convert WAT to WASM: {process.StandardError.ReadToEnd()}");
            }
        }

        [Fact]
        public void TestAdvancedFeatures()
        {
            var parser = new BinaryenWasmParser();
            var module = parser.ParseWasmFile(_testWasmPath);

            Assert.NotNull(module);
            Assert.Equal(7, module.Functions.Count);
            Assert.Equal(3, module.Globals.Count);
            Assert.Single(module.Memories);
            Assert.Single(module.Tables);

            // Test globals
            var counterGlobal = module.Globals[0];
            Assert.Equal("counter", counterGlobal.Name);
            Assert.True(counterGlobal.IsMutable);
            Assert.Equal(WasmValueType.I32, counterGlobal.Type);

            var piGlobal = module.Globals[1];
            Assert.Equal("pi", piGlobal.Name);
            Assert.False(piGlobal.IsMutable);
            Assert.Equal(WasmValueType.F32, piGlobal.Type);

            var maxValueGlobal = module.Globals[2];
            Assert.Equal("max_value", maxValueGlobal.Name);
            Assert.True(maxValueGlobal.IsMutable);
            Assert.Equal(WasmValueType.I64, maxValueGlobal.Type);

            // Test table
            var table = module.Tables[0];
            Assert.Equal(2u, table.InitialSize);
            Assert.Equal(2, table.Elements.Count);

            // Test memory
            var memory = module.Memories[0];
            Assert.Equal(1u, memory.InitialSize);
            Assert.Equal(2u, memory.MaximumSize);

            // Test functions
            var addFunction = module.Functions[0];
            Assert.Equal("add", addFunction.Name);
            Assert.Equal(2, addFunction.Parameters.Count);
            Assert.Equal(WasmValueType.I32, addFunction.Parameters[0]);
            Assert.Equal(WasmValueType.I32, addFunction.Parameters[1]);
            Assert.Single(addFunction.Results);
            Assert.Equal(WasmValueType.I32, addFunction.Results[0]);

            var factorialFunction = module.Functions[3];
            Assert.Equal("factorial", factorialFunction.Name);
            Assert.Single(factorialFunction.Parameters);
            Assert.Equal(WasmValueType.I32, factorialFunction.Parameters[0]);
            Assert.Single(factorialFunction.Results);
            Assert.Equal(WasmValueType.I32, factorialFunction.Results[0]);
            Assert.True(factorialFunction.Body.Count > 0);

            var floatOpsFunction = module.Functions[6];
            Assert.Equal("float_ops", floatOpsFunction.Name);
            Assert.Empty(floatOpsFunction.Parameters);
            Assert.Empty(floatOpsFunction.Results);
            Assert.True(floatOpsFunction.Body.Count > 0);

            // Test exports
            Assert.Equal(11, module.Exports.Count);
            Assert.Contains(module.Exports, e => e.Name == "add" && e.Kind == WasmExportKind.Function);
            Assert.Contains(module.Exports, e => e.Name == "counter" && e.Kind == WasmExportKind.Global);
            Assert.Contains(module.Exports, e => e.Name == "mem" && e.Kind == WasmExportKind.Memory);
        }
    }
} 