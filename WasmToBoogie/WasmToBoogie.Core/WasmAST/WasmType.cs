using System.Collections.Generic;

namespace WasmToBoogie.Core.WasmAST
{
    public class WasmType
    {
        public List<WasmValueType> Parameters { get; set; } = new List<WasmValueType>();
        public List<WasmValueType> Results { get; set; } = new List<WasmValueType>();
    }
} 