
class PlayerScriptsLoader {
  constructor() {
    this.loadPromise = null;
  }
  
  async load() {
    if (!this.loadPromise) {
      this.loadPromise = this.doLoad();
    }
    return this.loadPromise;
  }

  async doLoad() {
    let scripts = ['common.js', 'wasm-feature-detect.js', 'wasm-loader.js', 'pcm-player.js', 'webgl.js', 'player.js', 'soft-player.js', 'mse-player.js', 'controller.js'];
    let loadedScriptsArr = scripts.map(scriptName => {
      return new Promise((resolve, reject) => {
        const script = document.createElement('script');
        script.type = 'text/javascript';
        script.async = false; // 保证脚本的执行顺序
        script.src = (window._mseVenderPrefixPath || "") + scriptName + "?version=8.5.0.078b924.20220608173932.master";
        script.onerror = reject;
        document.body.appendChild(script);
        script.onload = resolve;
      });
    });
    console.log('%c [ loadedScriptsArr ]-17', 'font-size:13px; background:pink; color:#bf2c9f;', loadedScriptsArr)
    return Promise.all(loadedScriptsArr);
  }
}

if (!window._msePlayerScriptsLoader) {
  window._msePlayerScriptsLoader = new PlayerScriptsLoader();
  window._msePlayerScriptsLoader.load();
}
