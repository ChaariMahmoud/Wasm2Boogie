using System;
using Xunit;
using WasmToBoogie.Core.WasmParser;
using WasmToBoogie.Core.Translation;
using WasmToBoogie.Core.WasmAST;
using System.Collections.Generic;

namespace WasmToBoogie.Tests
{
    public class BasicTests
    {
        [Fact]
        public void TestBasicWasmParser()
        {
            var parser = new BasicWasmParser();
            var module = parser.ParseWasmData(new byte[0]); // We don't actually need the data for our test implementation

            Assert.NotNull(module);
            Assert.Single(module.Functions);
            
            var function = module.Functions[0];
            Assert.Equal("add", function.Name);
            Assert.Equal(2, function.Parameters.Count);
            Assert.Equal(WasmValueType.I32, function.Parameters[0]);
            Assert.Equal(WasmValueType.I32, function.Parameters[1]);
            Assert.Single(function.Results);
            Assert.Equal(WasmValueType.I32, function.Results[0]);
            Assert.Equal(3, function.Body.Count);
        }

        [Fact]
        public void TestBasicTranslator()
        {
            var parser = new BasicWasmParser();
            var translator = new BasicWasmToBoogieTranslator();
            
            var module = parser.ParseWasmData(new byte[0]);
            var boogieCode = translator.TranslateToBoogie(module);

            Assert.NotNull(boogieCode);
            Assert.Contains("type i32 = int;", boogieCode);
            Assert.Contains("procedure add(", boogieCode);
            Assert.Contains("param0: i32, param1: i32", boogieCode);
            Assert.Contains("returns (result0: i32)", boogieCode);
        }
    }
} 