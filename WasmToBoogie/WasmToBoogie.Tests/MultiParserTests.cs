using System;
using System.IO;
using Xunit;
using WasmToBoogie.Core.WasmParser;
using WasmToBoogie.Core.WasmAST;

namespace WasmToBoogie.Tests
{
    public class MultiParserTests
    {
        private readonly string _testWasmPath;

        public MultiParserTests()
        {
            // Convert WAT to WASM
            string sourceWatPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "multi_test.wat");
            _testWasmPath = Path.Combine(AppContext.BaseDirectory, "multi_test.wasm");
            
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
        public void TestMultipleFunctions()
        {
            var parser = new BinaryenWasmParser();
            var module = parser.ParseWasmFile(_testWasmPath);

            Assert.NotNull(module);
            Assert.Equal(4, module.Functions.Count);
            Assert.Single(module.Memories);

            // Test add function
            var addFunction = module.Functions[0];
            Assert.Equal("add", addFunction.Name);
            Assert.Equal(2, addFunction.Parameters.Count);
            Assert.Equal(WasmValueType.I32, addFunction.Parameters[0]);
            Assert.Equal(WasmValueType.I32, addFunction.Parameters[1]);
            Assert.Single(addFunction.Results);
            Assert.Equal(WasmValueType.I32, addFunction.Results[0]);
            Assert.Equal(3, addFunction.Body.Count);

            // Test sum function
            var sumFunction = module.Functions[1];
            Assert.Equal("sum", sumFunction.Name);
            Assert.Single(sumFunction.Parameters);
            Assert.Equal(WasmValueType.I32, sumFunction.Parameters[0]);
            Assert.Single(sumFunction.Results);
            Assert.Equal(WasmValueType.I32, sumFunction.Results[0]);
            Assert.True(sumFunction.Body.Count > 0);

            // Test store_and_load function
            var storeLoadFunction = module.Functions[2];
            Assert.Equal("store_and_load", storeLoadFunction.Name);
            Assert.Equal(2, storeLoadFunction.Parameters.Count);
            Assert.Equal(WasmValueType.I32, storeLoadFunction.Parameters[0]);
            Assert.Equal(WasmValueType.I32, storeLoadFunction.Parameters[1]);
            Assert.Single(storeLoadFunction.Results);
            Assert.Equal(WasmValueType.I32, storeLoadFunction.Results[0]);
            Assert.True(storeLoadFunction.Body.Count > 0);

            // Test divmod function
            var divmodFunction = module.Functions[3];
            Assert.Equal("divmod", divmodFunction.Name);
            Assert.Equal(2, divmodFunction.Parameters.Count);
            Assert.Equal(WasmValueType.I32, divmodFunction.Parameters[0]);
            Assert.Equal(WasmValueType.I32, divmodFunction.Parameters[1]);
            Assert.Equal(2, divmodFunction.Results.Count);
            Assert.Equal(WasmValueType.I32, divmodFunction.Results[0]);
            Assert.Equal(WasmValueType.I32, divmodFunction.Results[1]);
            Assert.True(divmodFunction.Body.Count > 0);

            // Test memory
            var memory = module.Memories[0];
            Assert.Equal(1u, memory.InitialSize);
        }
    }
} 