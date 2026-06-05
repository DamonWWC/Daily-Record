
class WasmLoader {
  constructor() {
    this.loadPromise = null;
    this.logger = new Logger("WasmLoader");
  }

  async load() {
    if (!this.loadPromise) {
      this.loadPromise = this.detectAndLoad();
    }
    return this.loadPromise;
  }

  async detectAndLoad() {
    this.logger.logInfo("load wasm.");
    const threadsSupported = await wasmFeatureDetect.threads();
    const simdSupported = await wasmFeatureDetect.simd();
    this.logger.logInfo("wasm features, threads: " + threadsSupported + ", simd: " + simdSupported);
    if (!threadsSupported) {
      return await this.doLoad("libffmpeg");
    } else if (!simdSupported) {
      return await this.doLoad("libffmpeg_with_threads");
    } else {
      return await this.doLoad("libffmpeg_with_threads_simd");
    }
  }

  async doLoad(moduleName) {
    const wasmUrl = (window._mseVenderPrefixPath || "") + moduleName + ".wasm?version=8.5.0.078b924.20220608173932.master";
    const wasmModule = await WebAssembly.compileStreaming(fetch(wasmUrl));
    this.logger.logInfo("wasm compiled.");
    return {moduleName, wasmModule};
  }
}

const wasmLoader = new WasmLoader();
wasmLoader.load();
