const WorkerReqTypes = {
  CREATE_WORKER: 0,
  DESTROY_WORKER: 1,
  FEED_DATA: 2,
  SET_PLAYBACK_RATE: 3,
  RESET_STATISTICS: 4,
  INIT_WASM: 5,
};

const WorkerResTypes = {
  CREATE_WORKER: 0,
  DESTROY_WORKER: 1,
  MP4_FRAGMENT: 2,
  VIDEO_FRAME: 3,
  AUDIO_FRAME: 4,
  STREAM_OPENED: 5,
  EXCEPTION: 6,
  SET_PLAYBACK_RATE: 7,
  FEED_DATA: 8,
  STATISTICS: 9,
  RESET_STATISTICS: 10,
}

const WorkerMode = {
  REMUXER: 1,
  DECODER: 2,
}

const PlayerError = {
  WEBSOCKET_CONNECT_FAILED: 1000,
  OPEN_STREAM_FAILED: 1001,
  NO_STREAM_DATA: 1002,
  DETECT_STREAM_FAILED: 1003,
  WORKER_EXCEPTION: 1004,
  CODEC_UNSUPPORTED: 1005,
}

class Utils {
  static padZero(i) {
    return (i < 10 ? "0" : "") + i;
  }

  static padZero3(i) {
    if (i < 10) return "00" + i;
    if (i < 100) return "0" + i;
    return "" + i;
  }

  static format02f(n) {
    return Math.round(n * 100) / 100;
  }
}

class Logger {
  //日志累计行数达到这个值将触发console.clear()，防止日志太多影响ui响应速度
  static maxLines = 10000;
  static lineCount = 0;

  constructor(module) {
    this.module = module;
  }

  log(line) {
    console.log(this.currentTimeStr() + " [" + this.module + "] " + line);
    Logger.countLines();
  }

  logError(line) {
    console.error(this.currentTimeStr() + " [" + this.module + "] [ERROR] " + line);
    Logger.countLines();
  }

  logWarn(line) {
    console.warn(this.currentTimeStr() + " [" + this.module + "] [WARN] " + line);
    Logger.countLines();
  }

  logInfo(line) {
    console.log(this.currentTimeStr() + " [" + this.module + "] [INFO] " + line);
    Logger.countLines();
  }

  logDebug(line) {
    console.log(this.currentTimeStr() + " [" + this.module + "] [DEBUG] " + line);
    Logger.countLines();
  }

  currentTimeStr() {
    var t = new Date();
    return Utils.padZero(t.getMonth() + 1) + '-' + Utils.padZero(t.getDate()) + ' ' + Utils.padZero(t.getHours()) + ':' +
      Utils.padZero(t.getMinutes()) + ':' + Utils.padZero(t.getSeconds()) + '.' + Utils.padZero3(t.getMilliseconds());
  }

  setModule(module) {
    this.module = module;
  }

  static countLines() {
    if (++Logger.lineCount >= Logger.maxLines) {
      Logger.lineCount = 0;
      console.clear();
      console.warn("previous logs has been cleared.");
    }
  }
}
