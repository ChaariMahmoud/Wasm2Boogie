using System;
using System.IO;
using Xunit;
using WasmToBoogie.Core.WasmParser;
using WasmToBoogie.Core.WasmAST;

namespace WasmToBoogie.Tests
{
    public class BinaryenParserTests
    {
        private readonly string _testWasmPath;

        public BinaryenParserTests()
        {
            // Copy test.wasm to the output directory
            string sourceWasmPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "test.wasm");
            _testWasmPath = Path.Combine(AppContext.BaseDirectory, "test.wasm");
            File.Copy(sourceWasmPath, _testWasmPath, true);
        }

        [Fact]
        public void TestParseWasmFile()
        {
            var parser = new BinaryenWasmParser();
            var module = parser.ParseWasmFile(_testWasmPath);

            Assert.NotNull(module);
            Assert.Single(module.Functions);
            Assert.Single(module.Memories);

            var function = module.Functions[0];
            Assert.Equal("add", function.Name);
            Assert.Equal(2, function.Parameters.Count);
            Assert.Equal(WasmValueType.I32, function.Parameters[0]);
            Assert.Equal(WasmValueType.I32, function.Parameters[1]);
            Assert.Single(function.Results);
            Assert.Equal(WasmValueType.I32, function.Results[0]);

            // Check function body
            Assert.Equal(3, function.Body.Count);
            Assert.Equal("local.get", function.Body[0].OpCode);
            Assert.Equal("local.get", function.Body[1].OpCode);
            Assert.Equal("i32.add", function.Body[2].OpCode);

            // Check memory
            var memory = module.Memories[0];
            Assert.Equal(1u, memory.InitialSize);
        }

        [Fact]
        public void TestParseWasmData()
        {
            var parser = new BinaryenWasmParser();
            byte[] wasmData = File.ReadAllBytes(_testWasmPath);
            var module = parser.ParseWasmData(wasmData);

            Assert.NotNull(module);
            Assert.Single(module.Functions);
            Assert.Single(module.Memories);
        }
    }
} 