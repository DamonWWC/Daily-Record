
class Player {

  constructor(callback) {
    this.id = Player.idGen++;
    this.logger = new Logger("Player" + this.id);
    this.callback = callback;
    this.url = "";
    this.avgLatency = 0;
    this.isRealtimeStream = false;
    this.paused = false;
    this.latencyControlTimer = null;
    this.dataSeq = 0;
    this.processedSeq = 0;
    this.worker = null;
    this.workerBusy = false;
    this.dynamicRate = false; //根据接收数据的速率动态调速
    this.baseRate = 1.0;
    this.rateMultiplier = 1.0;

    this.sessionId = 0;
    this.mediaDuration = 0; //录像时长

    this.detectStreamRetryTimes = 0;
    this.workerErrorRetryTimes = 0;
    this.openStreamRetryTimes = 0;
    this.noStreamDataRetryTimes = 0;

    //websocket流
    this.ws = null;
    this.firstChunk = true;
    this.reconnectInterval = 0;
    this.reconnectTimer = null;
    this.reconnectTimes = 0;
    this.lastRecvTime = 0;
    this.checkAliveTimer = null;

    //显示网速等信息
    this.statsLabel = null;
    this.recvBytes = 0;
    this.totalRecvBytes = 0;
    this.updateStatsTimer = null;

    //调试
    this.psChunks = [];
    this.mp4Chunks = [];
  }

  reportError(code, message) {
    if (this.callback) {
      this.callback({event: "error", target: this, code: code || 0, message: message});
    }
  }

  emit(event) {
    if (this.callback) {
      this.callback({event: event, target: this});
    }
  }

  async createWorker() {
    this.logger.logInfo("create worker");
    this.worker = new Worker((self._mseVenderPrefixPath || "") + "ffmpeg-worker.js?version=8.5.0.078b924.20220608173932.master");
    this.worker.onmessage = this.onWorkerMessage.bind(this);
    const {moduleName, wasmModule} = await wasmLoader.load();
    this.worker.postMessage({
      t: WorkerReqTypes.INIT_WASM,
      n: moduleName,
      w: wasmModule,
      l: Player.logLevel,
    });
    this.worker.postMessage({
      t: WorkerReqTypes.CREATE_WORKER,
      i: this.id,
      m: this.getWorkerMode(),
    });
  }

  onWorkerMessage(evt) {
    var objData = evt.data;
    switch (objData.t) {
    case WorkerResTypes.CREATE_WORKER:
      break;
    case WorkerResTypes.DESTROY_WORKER:
      break;
    case WorkerResTypes.STREAM_OPENED:
      if (objData.e) {
        this.logger.logError("open stream failed, recreate remuxer");
        if (++this.detectStreamRetryTimes <= Player.maxDetectStreamRetryTimes) {
          this.destroyWorker();
          this.createWorker();
        } else {
          this.reportError(PlayerError.DETECT_STREAM_FAILED, "检测流失败！");
        }
        break;
      }
      this.detectStreamRetryTimes = 0;
      this.openPlayer(objData);
      break;
    case WorkerResTypes.MP4_FRAGMENT:
      this.workerErrorRetryTimes = 0;
      if (this.onMp4Fragment) {
        this.onMp4Fragment(objData);
      }
      if (Player.enableDump) {
        this.mp4Chunks.push(objData.d.buffer);
      }
      break;
    case WorkerResTypes.VIDEO_FRAME:
      this.workerErrorRetryTimes = 0;
      if (this.onVideoFrame) {
        this.onVideoFrame(objData);
      }
      break;
    case WorkerResTypes.AUDIO_FRAME:
      this.workerErrorRetryTimes = 0;
      if (this.onAudioFrame) {
        this.onAudioFrame(objData);
      }
      break;
    case WorkerResTypes.SET_PLAYBACK_RATE:
      if (this.onSetPlaybackRateRsp) {
        this.onSetPlaybackRateRsp(objData);
      }
      break;
    case WorkerResTypes.FEED_DATA:
      this.processedSeq = objData.s;
      break;
    case WorkerResTypes.STATISTICS:
      this.onStatistics(objData);
      break;
    case WorkerResTypes.EXCEPTION:
      this.logger.logError("got remuxer error, recreate remuxer");
      if (++this.workerErrorRetryTimes <= Player.maxWorkerErrorRetryTimes) {
        this.destroyWorker();
        this.createWorker();
      } else {
        this.reportError(PlayerError.WORKER_EXCEPTION, "格式转换或解码失败！");
      }
      break;
    }
  }

  openPlayer(objData) {
    this.logger.logInfo("open player");
    this.open(objData);
    var that = this;
    this.latencyControlTimer = setTimeout(function fn() {
      that.latencyControl();
      that.latencyControlTimer = setTimeout(fn, 200);
    }, 200);
  }

  closePlayer() {
    this.logger.logInfo("close player");
    this.baseRate = 1.0;
    this.rateMultiplier = 1.0;
    this.close();
    if (this.latencyControlTimer) {
      clearTimeout(this.latencyControlTimer);
      this.latencyControlTimer = null;
    }
  }

  destroyWorker() {
    if (this.worker) {
      this.worker.postMessage({
        t: WorkerReqTypes.DESTROY_WORKER,
      })
      this.worker.onmessage = null;
      this.worker = null;
    }
    this.dataSeq = 0;
    this.processedSeq = 0;
    this.workerBusy = false;
    this.dynamicRate = false;
  }

  //录像缓冲区超过一定大小直接跳到最新位置不一定合适，其它备选方案：
  //1. 录像进入后台播放自动暂停？
  //2. 使用Chromium专有api WebSocketStream来限流
  //   https://stackoverflow.com/questions/19414277/can-i-have-flow-control-on-my-websockets
  //   https://web.dev/i18n/en/websocketstream/
  //3. 增加websocket限流协议，比如客户端给服务器发送令牌
  
  //调整播放速度，保持较低延时
  latencyControl() {
    var latency = this.getBufferedTimeLength();
    this.avgLatency = this.avgLatency * 0.8 + latency * 0.2;
    if (this.isRealtimeStream) {
      var minBufTime = Math.min(Math.max(this.getMinBufferTimeLength(), 0.3), 0.65);
      if (this.avgLatency > 3) {
        this.logger.logInfo("skip " + Utils.format02f(latency - minBufTime * 1.25));
        this.skip(latency - minBufTime * 1.25);
        this.rateMultiplier = 1;
      } else if (this.avgLatency > minBufTime * 2.5) {
        this.rateMultiplier = 1.1;
      } else if (this.avgLatency > minBufTime * 1.6 && this.avgLatency < minBufTime * 2.25) {
        this.rateMultiplier = 1.05;
      } else if (latency < minBufTime * 1.25) {
        this.rateMultiplier = 1;
      }
    } else {
      if (this.avgLatency > 10) {
        this.logger.logInfo("skip " + Utils.format02f(latency - 1));
        this.skip(latency - 1);
        this.rateMultiplier = 1;
      } else if (this.avgLatency > 3) {
        this.rateMultiplier = 1.1;
      } else if (this.avgLatency > 2) {
        this.rateMultiplier = 1.05;
      } else if (this.avgLatency < 1) {
        this.rateMultiplier = 0.95;
      } else {
        this.rateMultiplier = 1;
      }
    }
    //this.logger.logInfo("latency " + latency + ", avg latency " + this.avgLatency);
    this.updatePlaybackRate();
  }

  updatePlaybackRate() {
    var playbackRate = this.baseRate * this.rateMultiplier;
    if (playbackRate != this.getPlaybackRate()) {
      this.logger.logInfo("buffered " + Utils.format02f(this.avgLatency) + ", set playback rate to " + Utils.format02f(playbackRate));
      this.setPlaybackRate(playbackRate);
    }
  }

  enableDynamicRate(enable) {
    if (!this.isRealtimeStream) {
      this.dynamicRate = enable;
      this.worker.postMessage({
        t: WorkerReqTypes.RESET_STATISTICS,
        e: this.dynamicRate,
      });
    }
  }

  resetStatistics() {
    if (!this.isRealtimeStream && this.dynamicRate) {
      this.worker.postMessage({
        t: WorkerReqTypes.RESET_STATISTICS,
        e: this.dynamicRate,
      });
    }
  }

  onStatistics(objData) {
    var frameRate = objData.f, recvFrameRate = objData.r;
    if (!this.dynamicRate || !frameRate|| !recvFrameRate) {
      return;
    }
    this.logger.logInfo("frameRate: " + Utils.format02f(frameRate) + ", recvFrameRate: " + Utils.format02f(recvFrameRate));
    this.baseRate = recvFrameRate / frameRate;
    this.updatePlaybackRate();
  }

  //FIXME: 浏览器页面进入后台时，会停止播放，此时应该调pausePlayback，不然一直接收数据会出现QuotaExceededError
  onStreamData(chunk) {
    //this.logger.logInfo("on data");
    if (this.firstChunk) {
      this.logger.logInfo("got first packet");
      this.firstChunk = false;
      this.openStreamRetryTimes = 0;
      if (this.callback) {
        this.callback({event: this.reconnectTimer ? "reconnected" : "connected"});
      }
    }
    this.recvBytes += chunk.byteLength;
    this.totalRecvBytes += chunk.byteLength;

    if (this.workerBusy) {
      var arr = new Uint8Array(chunk);
      for (var i = 3; i < arr.length; i++) {
        if (arr[i-3]===0 && arr[i-2]===0 && arr[i-1]===1 && arr[i]===0xbc) {
          this.workerBusy = false;
          break;
        }
      }
    } else if (this.processedSeq && this.processedSeq + 100 <= this.dataSeq) {
      this.logger.logDebug("worker busy, lag: " + (this.dataSeq - this.processedSeq))
      this.workerBusy = true;
    }
    if (this.worker && !this.workerBusy) {
      this.worker.postMessage({
        t: WorkerReqTypes.FEED_DATA,
        s: ++this.dataSeq,
        d: chunk,
      }, Player.enableDump ? [] : [chunk]);
    }

    if (Player.enableDump) {
      this.psChunks.push(chunk);
    }
  }

  saveFile(filename, blob) {
    if (window.navigator.msSaveOrOpenBlob) {
      window.navigator.msSaveOrOpenBlob(blob, filename);
    } else {
      const a = document.createElement('a');
      document.body.appendChild(a);
      const url = window.URL.createObjectURL(blob);
      a.href = url;
      a.download = filename;
      a.click();
      setTimeout(() => {
        window.URL.revokeObjectURL(url);
        document.body.removeChild(a);
      }, 0);
    }
  }

  //此接口为私有接口，暂停录像回放应调用controller.pausePlayback()
  pause() {
    this.doPause();
    this.paused = true;
  }

  resume() {
    if (!this.paused) {
      return;
    }
    this.doResume();
    this.paused = false;
    this.lastRecvTime = Date.now();
  }

  openWebSocket(url) {
    var that = this;
    this.url = url;
    var query = "r=" + this.reconnectTimes;
    this.ws = new WebSocket(url + "?" + query);
    this.ws.binaryType = 'arraybuffer';
    this.ws.onopen = function(evt) {
      that.logger.logInfo("websocket connected. " + query);
    };
    this.ws.onclose = function(evt) {
      that.logger.logInfo("websocket closed. " + query);
      if (that.ws !== evt.target) {
        return;
      }
      that.logger.logInfo("total received bytes: " + that.totalRecvBytes);
      that.firstChunk = true;
      that.totalRecvBytes = 0;
      //打开流失败重试时，不计重连次数
      if (that.openStreamRetryTimes || that.noStreamDataRetryTimes || ++that.reconnectTimes <= Player.maxWebSocketReconnectTimes) {
        that.logger.logInfo("reconnect websocket in " + that.reconnectInterval + "ms");
        that.reconnectTimer = setTimeout(that.openWebSocket.bind(that, url), that.reconnectInterval);
        if (that.reconnectInterval < 3000) {
          that.reconnectInterval += 1000;
        }
      } else {
        that.reportError(PlayerError.WEBSOCKET_CONNECT_FAILED, "打开WebSocket连接失败！");
      }
    };
    this.ws.onerror = function(evt) {
      that.logger.logError("websocket error.");
    };
    this.ws.onmessage = function(evt) {
      if (that.ws !== evt.target) {
        return;
      }
      that.reconnectTimes = 0;
      if (typeof evt.data === "string") {
        that.onWebSocketMessage(evt.data);
      } else if (evt.data instanceof ArrayBuffer) {
        that.reconnectInterval = 0;
        that.lastRecvTime = Date.now();
        that.noStreamDataRetryTimes = 0;
        that.onStreamData(evt.data);
      }
    };
    this.paused = false;
    this.lastRecvTime = Date.now();
    if (this.checkAliveTimer) {
      clearTimeout(this.checkAliveTimer);
    }
    this.checkAliveTimer = setTimeout(function fn() {
      if (!that.paused && Date.now() >= that.lastRecvTime + 20000) {
        if (++that.noStreamDataRetryTimes <= Player.maxNoStreamDataRetryTimes) {
          that.logger.logError("not receiving any stream data, reconnect now");
          that.ws.close();
        } else {
          that.reportError(PlayerError.NO_STREAM_DATA, "没有收到流数据！");
        }
      } else {
        that.checkAliveTimer = setTimeout(fn, 1000);
      }
    }, 1000);
  }

  closeWebSocket() {
    if (this.ws) {
      this.ws.onmessage = null;
      this.ws.close();
      this.ws = null;
    }
    if (this.reconnectTimer) {
      clearTimeout(this.reconnectTimer);
      this.reconnectTimer = null;
    }
    this.reconnectInterval = 0;
    if (this.checkAliveTimer) {
      clearTimeout(this.checkAliveTimer);
      this.checkAliveTimer = null;
    }
    this.paused = false;
    this.lastRecvTime = 0;
    this.totalRecvBytes = 0;
    this.url = "";
    this.firstChunk = true;
  }

  resetRetryTimes() {
    this.detectStreamRetryTimes = 0;
    this.workerErrorRetryTimes = 0;
    this.openStreamRetryTimes = 0;
    this.noStreamDataRetryTimes = 0;
    this.reconnectTimes = 0;
  }

  play(url, opts) {
    this.logger.logInfo("open " + url);
    this.stop();
    this.createWorker();
    if (url.startsWith("ws://")) {
      this.openWebSocket(url);
    }
    if (opts && opts.hasOwnProperty('isRealtimeStream')) {
      this.isRealtimeStream = opts.isRealtimeStream;
    } else {
      this.checkPlayType();
    }
    if (Player.printStats) {
      this.showStats(opts && opts.statsLabel);
    }
  }

  stop() {
    this.hideStats();
    this.generateDump();
    this.closeWebSocket();
    this.destroyWorker();
    this.closePlayer();
    this.resetRetryTimes();
  }

  generateDump() {
    if (Player.enableDump) {
      if (this.psChunks.length > 0) {
        var blob = new Blob(this.psChunks, {type: "octet/stream"});
        this.saveFile("player" + this.id + "_input.mpg", blob);
      }
      if (this.mp4Chunks.length > 0) {
        var blob = new Blob(this.mp4Chunks, {type: "octet/stream"});
        this.saveFile("player" + this.id + "_output.mp4", blob);
      }
    }
    this.psChunks = [];
    this.mp4Chunks = [];
  }

  showStats(label) {
    var that = this;
    this.statsLabel = label;
    if (this.updateStatsTimer) {
      clearTimeout(this.updateStatsTimer);
    }
    this.updateStatsTimer = setTimeout(function fn() {
      var html = that.getStatsHtml();
      if (that.statsLabel) {
        that.statsLabel.innerHTML = html;
      } else {
        that.logger.logDebug(html.split("<br />").join("\n"));
      }
      that.recvBytes = 0;
      that.updateStatsTimer = setTimeout(fn, 1000);
    }, 1000);
  }

  hideStats() {
    this.recvBytes = 0;
    if (this.updateStatsTimer) {
      clearTimeout(this.updateStatsTimer);
      this.updateStatsTimer = null;
    }
    this.statsLabel = null;
  }

  //业务代码

  checkPlayType() {
    this.isRealtimeStream = (this.url.indexOf("play_real") !== -1);
    this.logger.logInfo("is realtime stream: " + this.isRealtimeStream);
  }

  setSessionId(sessionId) {
    this.sessionId = sessionId
  }
  getSessionId() {
    return this.sessionId;
  }

  onWebSocketMessage(msg) {
    var res;
    try {
      res = JSON.parse(msg.replace(/\0$/, ""));
    } catch (e) {
      this.logger.logError("json parse exception: " + e);
      return;
    }
    this.logger.logInfo("got websocket msg: " + msg);
    if (res["FC"] == "FC_WS_NOTIFY_STREAM_PAUSE") {
      if (res["OpCode"] === 0) {
        this.pause();
      }
    } else if (res["FC"] == "FC_WS_NOTIFY_STREAM_RESUME") {
      if (res["OpCode"] === 0) {
        this.resetStatistics();
        this.resume();
      }
    } else if (res["FC"] == "FC_WS_NOTIFY_STREAM_SPEED") {
      if (res["OpCode"] === 0 && res["Data"]) {
        this.baseRate = res["Data"]["Speed"];
        this.enableDynamicRate(res["Data"]["Speed"] != 1);
        this.updatePlaybackRate();
      }
    } else if (res["FC"] == "FC_WS_NOTIFY_STREAM_SEEK") {
      if (res["OpCode"] === 0 && res["Data"]) {
        this.resetStatistics();
        this.onSeekDone(res["Data"]["Pos"]);
      }
    } else if (res["FC"] == "FC_WS_NOTIFY_OPEN_RESULT") {
      if (res["OpCode"] === 0) {
        this.openStreamRetryTimes = 0;
        if (this.url.startsWith("ws://") && !this.isRealtimeStream && res["Data"]) {
          this.mediaDuration = Number(res["Data"]["TotalTime"]);
          this.sessionId = Number(res["Data"]["SessionId"]);
          this.logger.logInfo("session id: " + this.sessionId + ", duration: " + this.mediaDuration);
        }
      } else if (++this.openStreamRetryTimes <= Player.maxOpenStreamRetryTimes) {
        this.logger.logError("play stream failed, close websocket");
        this.ws.close();
      } else {
        this.reportError(PlayerError.OPEN_STREAM_FAILED, "打开流失败！");
      }
    }
  }

}

//静态字段

Player.idGen = 1;
Player.enableDump = false;
Player.logLevel = 'info';
Player.printStats = false;

Player.maxWebSocketReconnectTimes = 0;
Player.maxOpenStreamRetryTimes = 0;
Player.maxNoStreamDataRetryTimes = 0;
Player.maxDetectStreamRetryTimes = 1;
Player.maxWorkerErrorRetryTimes = 1;
