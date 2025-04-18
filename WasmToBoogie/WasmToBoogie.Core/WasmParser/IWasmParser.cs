using System;
using System.IO;
using WasmToBoogie.Core.WasmAST;

namespace WasmToBoogie.Core.WasmParser
{
    public interface IWasmParser
    {
        /// <summary>
        /// Parses a WASM file and returns its AST representation
        /// </summary>
        /// <param name="wasmFilePath">Path to the WASM file</param>
        /// <returns>WASM AST representation</returns>
        WasmModule ParseWasmFile(string wasmFilePath);

        /// <summary>
        /// Parses WASM binary data and returns its AST representation
        /// </summary>
        /// <param name="wasmData">WASM binary data</param>
        /// <returns>WASM AST representation</returns>
        WasmModule ParseWasmData(byte[] wasmData);
    }
} 