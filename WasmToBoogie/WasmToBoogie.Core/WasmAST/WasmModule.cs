using System.Collections.Generic;

namespace WasmToBoogie.Core.WasmAST
{
    public class WasmModule
    {
        public required string Name { get; set; }
        public List<WasmFunction> Functions { get; set; } = new();
        public List<WasmMemory> Memories { get; set; } = new();
        public List<WasmTable> Tables { get; set; } = new();
        public List<WasmGlobal> Globals { get; set; } = new();
        public List<WasmExport> Exports { get; set; } = new();
    }

    public class WasmFunction
    {
        public required string Name { get; set; }
        public List<WasmValueType> Parameters { get; set; } = new();
        public List<WasmValueType> Results { get; set; } = new();
        public List<WasmInstruction> Body { get; set; } = new();
    }

    public class WasmMemory
    {
        public required string Name { get; set; }
        public uint InitialSize { get; set; }
        public uint? MaximumSize { get; set; }
    }

    public class WasmTable
    {
        public required string Name { get; set; }
        public uint InitialSize { get; set; }
        public uint? MaximumSize { get; set; }
        public List<string> Elements { get; set; } = new();
    }

    public class WasmGlobal
    {
        public required string Name { get; set; }
        public WasmValueType Type { get; set; }
        public bool IsMutable { get; set; }
    }

    public class WasmExport
    {
        public required string Name { get; set; }
        public WasmExportKind Kind { get; set; }
        public required string Target { get; set; }
    }

    public enum WasmExportKind
    {
        Function,
        Global,
        Memory,
        Table
    }

    public class WasmInstruction
    {
        public required string OpCode { get; set; }
        public List<object> Operands { get; set; } = new();
    }

    public enum WasmValueType
    {
        I32,
        I64,
        F32,
        F64
    }

    public enum WasmOpCode
    {
        LocalGet,
        LocalSet,
        I32Add,
        I32Sub,
        I32Mul,
        I32DivS,
        I32RemS,
        I32LtS,
        I32Const,
        I32Store,
        I32Load,
        I32Store8,
        I32Load8U,
        I32Store16,
        I32Load16U,
        F32Add,
        F32Const,
        F64Add,
        F64Const,
        GlobalGet,
        GlobalSet,
        CallIndirect,
        Block,
        Loop,
        If,
        Br,
        BrIf,
        Drop
    }
} 