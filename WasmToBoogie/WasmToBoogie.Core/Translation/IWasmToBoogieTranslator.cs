using WasmToBoogie.Core.WasmAST;

namespace WasmToBoogie.Core.Translation
{
    public interface IWasmToBoogieTranslator
    {
        /// <summary>
        /// Translates a WASM module to Boogie code
        /// </summary>
        /// <param name="module">The WASM module to translate</param>
        /// <returns>Boogie code as a string</returns>
        string TranslateToBoogie(WasmModule module);

        /// <summary>
        /// Translates a specific WASM function to Boogie
        /// </summary>
        /// <param name="function">The WASM function to translate</param>
        /// <returns>Boogie code for the function</returns>
        string TranslateFunction(WasmFunction function);
    }
} 