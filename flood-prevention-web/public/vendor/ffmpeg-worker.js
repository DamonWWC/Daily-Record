self.importScripts("common.js?version=8.5.0.078b924.20220608173932.master")

const WasmState = {
  NONE: 0,
  COMPILED: 1,
  INSTANTIATED: 2,
}

self.wasmMod = null;
self.wasmState = WasmState.NONE;
self.coreLogLevel = 'info';

self.Module = {
  instantiateWasm: function(info, receiveInstance) {
    WebAssembly.instantiate(self.wasmMod, info).then(function(instance) {
      receiveInstance(instance, self.wasmMod)
    }, function(reason) {
      err("failed to asynchronously prepare wasm: " + reason);
      abort(reason)
    })
    return {}
  },
  onRuntimeInitialized: function () {
    self.wasmState = WasmState.INSTANTIATED;
    self.worker.onWasmLoaded();
  },
  // print: console.log.bind(console),
  // printErr: console.log.bind(console),
};

const DECODE_THREAD_COUNT = 4;

const WorkerState = {
  DEFAULT: 0,
  CREATED: 1,
  STREAM_OPENED: 2,
  STREAM_OPEN_FAILED: 3,
  DESTROYED: 4,
};

class FFmpegWorker {
  constructor() {
    this.logger             = new Logger("Worker");
    this.mode               = WorkerMode.REMUXER;
    this.workerHandle       = 0;
    this.tmpReqQue          = [];
    this.bufQue             = [];
    this.readCallback       = null;
    this.writeCallback      = null;
    this.videoCallback      = null;
    this.audioCallback      = null;
    this.statisticsCallback = null;
    this.state              = WorkerState.DEFAULT;
    this.foundPsm           = false;
    this.foundVideo         = false;
    this.foundAudio         = false;
    this.cachedPacketCount  = 0;
  }

  createWorker(id, mode) {
    this.logger.setModule("Worker" + id);
    this.mode = mode;
    this.workerHandle = Module._createWorker(this.readCallback, this.writeCallback, this.videoCallback, this.audioCallback, this.statisticsCallback);
    var objData = {
      t: WorkerResTypes.CREATE_WORKER,
      e: this.workerHandle ? 0 : 5,
    };
    self.postMessage(objData);
    this.state = WorkerState.CREATED;
    this.logger.logInfo("worker created.");
  }

  destroyWorker() {
    Module._destroyWorker(this.workerHandle);
    this.logger.logInfo("worker destroyed.");
    this.state = WorkerState.DESTROYED;
    this.workerHandle = 0;
    this.tmpReqQue = [];
    this.bufQue = [];
    this.foundPsm = false;
    this.foundVideo = false;
    this.foundAudio = false;
    this.cachedPacketCount = 0;
  }

  probePackets(arr) {
    for (var i = 3; i < arr.length; i++) {
      if (arr[i-3]===0 && arr[i-2]===0 && arr[i-1]===1) {
        if (arr[i]===0xbc) {
          this.logger.logInfo("psm found");
          if (!this.foundPsm)
            this.bufQue.splice(0, this.bufQue.length - 1);
          this.foundPsm = true;
        } else if (this.foundPsm && !this.foundAudio && (arr[i]>=0xc0 && arr[i]<0xe0)) {
          this.logger.logInfo("audio found");
          this.foundAudio = true;
        } else if (this.foundPsm && !this.foundVideo && arr[i]>=0xe0) {
          this.logger.logInfo("video found");
          this.foundVideo = true;
        }
      }
    }
  }

  cstr(ptr) {
    var arr = Module.HEAPU8.subarray(ptr, ptr + Module._strlen(ptr));
    var result = "";
    for (var i = 0; i < arr.length; i++) {
      result += String.fromCharCode(arr[i]);
    }
    return result;
  }

  openStream() {
    var mediaInfo = {};
    var codec = "";
    var probeSize = 0;
    for (var i = 0; i < this.bufQue.length; i++) {
      probeSize += this.bufQue[i].length;
    }
    var ret = Module._openStream(this.workerHandle, probeSize, 200);
    if (ret == 0) {
      this.state = WorkerState.STREAM_OPENED;
      this.logger.logInfo("stream opened");

      var buf = Module._malloc(512);
      if (this.mode == WorkerMode.REMUXER) {
        this.logger.logInfo("work as remuxer");
        ret = Module._prepareRemuxer(this.workerHandle);
        if (ret == 0) {
          Module._getCodecString(this.workerHandle, buf);
          codec = this.cstr(buf);
          this.logger.logInfo("codec: " + codec);
        } else {
          this.state = WorkerState.STREAM_OPEN_FAILED;
          this.logger.logInfo("prepare remuxer failed. ret: " + ret);
        }
      } else if (this.mode == WorkerMode.DECODER) {
        this.logger.logInfo("work as decoder");
        ret = Module._prepareDecoder(this.workerHandle, DECODE_THREAD_COUNT);
        if (ret == 0) {
          Module._getMediaInfo(this.workerHandle, buf);
          var json = this.cstr(buf);
          //this.logger.logInfo("media info: " + json);
          mediaInfo = JSON.parse(json);
        } else {
          this.state = WorkerState.STREAM_OPEN_FAILED;
          this.logger.logInfo("prepare decoder failed. ret: " + ret);
        }
      }
      Module._free(buf);
    } else {
      this.state = WorkerState.STREAM_OPEN_FAILED;
      this.logger.logInfo("stream failed to open. ret: " + ret);
    }
    
    var objData = {
      t: WorkerResTypes.STREAM_OPENED,
      e: ret,
      c: codec,
      i: mediaInfo,
    };
    self.postMessage(objData);
    return ret;
  }

  workOnce() {
    if (this.mode == WorkerMode.REMUXER) {
      let frameCount = 0;
      for (let i = 0; i < this.bufQue.length; i++) {
        if (this.bufQue[i].buffer.byteLength > 1024) {
          frameCount++;
        }
      }
      if (frameCount < 2) {
        return;
      }
      Module._remuxOnce(this.workerHandle);
      if (this.bufQue.length >= 2) {
        Promise.resolve().then(this.workOnce.bind(this));
      }
    } else {
      Module._decodeOnce(this.workerHandle);
    }
  }

  onData(data, seq) {
    //this.logger.logInfo("on feed data " + data.byteLength + ", worker: " + self.worker.workerHandle);
    var arr = new Uint8Array(data);
    this.bufQue.push(arr);

    if (this.state == WorkerState.STREAM_OPENED) {
      this.workOnce();
      if (this.mode == WorkerMode.DECODER) {
        self.postMessage({
          t: WorkerResTypes.FEED_DATA,
          s: seq,
        });
      }
    } else if (this.state == WorkerState.CREATED) {
      this.probePackets(arr);
      if (this.foundPsm && (++this.cachedPacketCount >= 5 || (this.foundVideo && this.foundAudio))) {
        if (this.openStream() == 0) {
          this.workOnce();
        }
      }
    }
  }

  onSetPlaybackRate(rate) {
    var ret = Module._setPlaybackRate(this.workerHandle, rate);
    if (ret != 0) {
      this.logger.logError("set playback rate failed");
    }
    var objData = {
      t: WorkerResTypes.SET_PLAYBACK_RATE,
      e: ret,
      r: rate,
    };
    self.postMessage(objData);
  }

  onResetStatistics(enableDynamicRate) {
    Module._resetStatistics(this.workerHandle);
    var objData = {
      t: WorkerResTypes.RESET_STATISTICS,
      e: enableDynamicRate,
    };
    self.postMessage(objData);
  }

  processReq(req) {
    //this.logger.logInfo("process req, type: " + req.t + ".");
    try {
      switch (req.t) {
      case WorkerReqTypes.CREATE_WORKER:
        this.createWorker(req.i, req.m);
        break;
      case WorkerReqTypes.DESTROY_WORKER:
        this.destroyWorker();
        self.close();
        break;
      case WorkerReqTypes.FEED_DATA:
        this.onData(req.d, req.s);
        break;
      case WorkerReqTypes.SET_PLAYBACK_RATE:
        this.onSetPlaybackRate(req.r);
        break;
      case WorkerReqTypes.RESET_STATISTICS:
        this.onResetStatistics(req.e);
        break;
      default:
        this.logger.logError("Unsupport messsage " + req.t);
      }
    } catch (e) {
      this.logger.logError("got exception when process req: " + e);
      var objData = {
        t: WorkerResTypes.EXCEPTION,
        e: e,
      };
      self.postMessage(objData);
    }
  }

  cacheReq(req) {
    this.logger.logInfo("cache req, type: " + req.t + ".");
    this.tmpReqQue.push(req);
  }

  onWasmLoaded() {
    this.logger.logInfo("wasm instantiated.");
    switch (self.coreLogLevel) {
      case 'error': Module._setLogLevel(1); break;
      case 'warn': Module._setLogLevel(2); break;
      case 'info': Module._setLogLevel(3); break;
      case 'debug': Module._setLogLevel(4); break;
    }
    var that = this;

    this.readCallback  = Module.addFunction(function (buff, size) {
      var need = size;
      while (that.bufQue.length > 0 && size > 0) {
        if (that.bufQue[0].length <= size) {
          var tmpBuf = that.bufQue.shift();
          Module.HEAPU8.set(tmpBuf, buff);
          buff += tmpBuf.length;
          size -= tmpBuf.length;
        } else {
          var tmpBuf = new Uint8Array(that.bufQue[0].buffer, that.bufQue[0].byteOffset, size);
          Module.HEAPU8.set(tmpBuf, buff);
          that.bufQue[0] = new Uint8Array(that.bufQue[0].buffer, that.bufQue[0].byteOffset + size);
          buff += size;
          size = 0;
        }
      }
      //that.logger.logInfo("read " + (need - size) + " bytes");
      return need == size ? -11 : need - size;    //E_AGAIN
    }, 'iii');

    this.writeCallback = Module.addFunction(function (buff, size) {
      var outArray = Module.HEAPU8.subarray(buff, buff + size);
      var data = new Uint8Array(outArray);
      var objData = {
        t: WorkerResTypes.MP4_FRAGMENT,
        d: data
      };
      self.postMessage(objData, [objData.d.buffer]);
      //that.logger.logInfo("write " + size + " bytes");
      return size;
    }, 'iii');

    this.videoCallback = Module.addFunction(function (data, linesize, w, h, timestamp) {
      var ptrs = Module.HEAPU32.subarray(data/4, data/4 + 3);
      var pitches = Module.HEAPU32.subarray(linesize/4, linesize/4 + 3);
      var planes = [];
      var transferables = [];
      for (var i = 0; i < 3; i++) {
        var arr = Module.HEAPU8.subarray(ptrs[i], ptrs[i] + pitches[i]*(i ? h/2 : h));
        var data = arr.slice(0);
        planes.push(data);
        transferables.push(data.buffer);
      }
      var objData = {
        t: WorkerResTypes.VIDEO_FRAME,
        s: timestamp,
        d: planes,
        p: pitches.slice(0),
        w: w,
        h: h,
      };
      self.postMessage(objData, transferables);
    }, 'viiiid');

    this.audioCallback = Module.addFunction(function (data, size, ch, timestamp) {
      var ptrs = Module.HEAPU32.subarray(data/4, data/4 + ch);
      var channels = [];
      var transferables = [];
      for (var i = 0; i < ch; i++) {
        var arr = Module.HEAPU8.subarray(ptrs[i], ptrs[i] + size);
        var data = arr.slice(0);
        channels.push(data);
        transferables.push(data.buffer);
      }
      var objData = {
        t: WorkerResTypes.AUDIO_FRAME,
        s: timestamp,
        d: channels,
      };
      self.postMessage(objData, transferables);
    }, 'viiid');

    this.statisticsCallback = Module.addFunction(function (frameRate, recvFrameRate) {
      var objData = {
        t: WorkerResTypes.STATISTICS,
        f: frameRate,
        r: recvFrameRate,
      };
      self.postMessage(objData);
    }, 'vdd');

    while (this.tmpReqQue.length > 0) {
      var req = this.tmpReqQue.shift();
      this.processReq(req);
    }
  }
}

self.onmessage = function (evt) {
  var req = evt.data;
  if (self.wasmState == WasmState.NONE) {
    console.assert(req.t == WorkerReqTypes.INIT_WASM);
    self.coreLogLevel = req.l;
    self.wasmMod = req.w;
    self.wasmState = WasmState.COMPILED;
    self.worker = new FFmpegWorker;
    self.worker.logger.logInfo("instantiate wasm.");
    self.importScripts(req.n + ".js?version=8.5.0.078b924.20220608173932.master");
  } else if (self.wasmState == WasmState.COMPILED) {
    self.worker.cacheReq(req);
  } else if (self.wasmState == WasmState.INSTANTIATED) {
    self.worker.processReq(req);
  }
};
