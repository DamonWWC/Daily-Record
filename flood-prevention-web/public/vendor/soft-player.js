
const PlayerState = {
  IDLE: 0,
  PLAYING: 1,
  BUFFERING: 2,
  PAUSED: 3,
};

const RenderMode = {
  BY_PTS: 1,        //根据pts决定视频帧和音频帧的渲染时间
  NO_BUFFER: 2,     //收到音视频帧立即渲染
  SYNC_AUDIO: 3,    //收到音频帧立即追加到音频播放器，然后根据视频帧pts和当前音频时间决定视频帧渲染时间
};

const MIN_BUFFER_TIME  = 0.3;

const ENABLE_FFMPEG_FILTER = false;
const LATENCY_CONTROL_PREFER_SKIP_OVER_MUTE = true;

class SoftPlayer extends Player {

  constructor(canvas, callback) {
    super(callback);
    this.state              = PlayerState.IDLE;
    this.canvas             = canvas;
    this.pcmPlayer          = null;
    this.webglPlayer        = null;
    this.audioFrameBuffer   = [];
    this.videoFrameBuffer   = [];
    this.pixFmt             = 0;
    this.videoWidth         = 0;
    this.videoHeight        = 0;
    this.videoFps           = 0;
    this.realFps            = 0;
    this.playbackRate       = 1.;
    this.beginTime          = 0;    //BY_PTS模式
    this.beginPts           = 0;    //BY_PTS模式
    this.lastRenderPts      = 0;
    this.audioEncoding      = "";
    this.audioChannels      = 0;
    this.audioSampleRate    = 0;
    this.renderMode         = RenderMode.BY_PTS;
    this.frameCounter       = 0;
    this.counterBeginTime   = 0;
    this.counterBeginPts    = 0;
    this.beginTimeOffset    = 0;    //SYNC_AUDIO模式
    this.firstAudio         = true; //SYNC_AUDIO模式
    this.muted              = false;
    this.volume             = 1.;
  }

  getWorkerMode() {
    return WorkerMode.DECODER;
  }

  setRenderMode(mode) {
    if (mode == RenderMode.SYNC_AUDIO && !this.pcmPlayer) {
      this.logger.logError("request sync audio mode, but no audio track");
      return;
    }
    this.logger.logInfo("set render mode to " + mode);
    this.renderMode = mode;
    this.playbackRate = 1.;
    this.clearBuffer();
  }

  open(objData) {
    this.onVideoParam(objData.i.video);
    if (objData.i.audio && objData.i.audio.channels > 0 && objData.i.audio.sampleRate > 0) {
      this.onAudioParam(objData.i.audio);
    }
    if (this.renderMode == RenderMode.NO_BUFFER) {
      this.startPlaying();
    } else if (this.renderMode == RenderMode.BY_PTS || this.renderMode == RenderMode.SYNC_AUDIO) {
      this.startBuffering();
    }
    this.renderLoop();
    this.logger.logInfo("player ready.");
  }

  close() {
    this.state = PlayerState.IDLE;
    if (this.pcmPlayer) {
      this.pcmPlayer.destroy();
      this.pcmPlayer = null;
    }
    this.audioFrameBuffer   = [];
    this.videoFrameBuffer   = [];
    this.pixFmt             = 0;
    this.videoWidth         = 0;
    this.videoHeight        = 0;
    this.videoFps           = 0;
    this.realFps            = 0;
    this.playbackRate       = 1.;
    this.beginTime          = 0;
    this.beginPts           = 0;
    this.lastRenderPts      = 0;
    this.audioEncoding      = "";
    this.audioChannels      = 0;
    this.audioSampleRate    = 0;
    this.renderMode         = RenderMode.BY_PTS;
    this.frameCounter       = 0;
    this.counterBeginTime   = 0;
    this.counterBeginPts    = 0;
    this.beginTimeOffset    = 0;
    this.firstAudio         = true;
    this.muted              = false;
    this.volume             = 1.;
  }

  onVideoParam(v) {
    this.logger.logInfo("video param, pixFmt:" + v.pixFmt + " width:" + v.width + " height:" + v.height + " fps:" + v.fps + ".");

    this.pixFmt = v.pixFmt;
    this.canvas.width = this.videoWidth = v.width;
    this.canvas.height = this.videoHeight = v.height;
    this.videoFps = v.fps; //ffmpeg推断的fps不可靠

    if (!this.webglPlayer) {
      this.webglPlayer = new WebGLPlayer(this.canvas, {
        preserveDrawingBuffer: false
      });
    }
  }

  onAudioParam(a) {
    this.logger.logInfo("audio param, sampleFmt:" + a.sampleFmt + " channels:" + a.channels + " sampleRate:" + a.sampleRate + ".");

    var encoding = "16bitInt";
    switch (a.sampleFmt) {
      case 0: encoding = "8bitInt"; break;
      case 1: encoding = "16bitInt"; break;
      case 2: encoding = "32bitInt"; break;
      case 3: encoding = "32bitFloat"; break;
      default: this.logger.logError("unsupported audio sampleFmt " + a.sampleFmt + "!");
    }
    this.logger.logInfo("audio encoding " + encoding + ".");

    this.audioEncoding      = encoding;
    this.audioChannels      = a.channels;
    this.audioSampleRate    = a.sampleRate;

    this.pcmPlayer = new PCMPlayer({
      encoding: this.audioEncoding ,
      channels: this.audioChannels,
      sampleRate: this.audioSampleRate,
    });
  }

  restartAudio() {
    if (this.pcmPlayer) {
      this.pcmPlayer.destroy();
      this.pcmPlayer = null;
    }

    this.pcmPlayer = new PCMPlayer({
      encoding: this.audioEncoding,
      channels: this.audioChannels,
      sampleRate: this.audioSampleRate,
    });
    
    this.firstAudio = true;
    this.beginTimeOffset = 0;
  }

  onVideoFrame(frame) {
    this.bufferFrame(frame);
    //this.logger.logDebug("on video frame, remain: " + this.videoFrameBuffer.length + ", time: " + this.getBufferedTimeLength());
    if ((!this.videoWidth || !this.videoHeight) && frame.w && frame.h) {
      this.logger.logDebug("set video width: " + frame.w + ", height: " + frame.h);
      this.canvas.width = this.videoWidth = frame.w;
      this.canvas.height = this.videoHeight = frame.h;
    }
  }

  onAudioFrame(frame) {
    this.bufferFrame(frame);
    //this.logger.logDebug("on audio frame, remain: " + this.audioFrameBuffer.length);
  }

  bufferFrame(frame) {
    if (this.state == PlayerState.IDLE) {
      return;
    }
    if (frame.t == WorkerResTypes.AUDIO_FRAME) {
      this.audioFrameBuffer.push(frame);
    } else if (frame.t == WorkerResTypes.VIDEO_FRAME) {
      this.videoFrameBuffer.push(frame);
    }
    if (this.state == PlayerState.BUFFERING && this.getBufferedTimeLength() >= MIN_BUFFER_TIME) {
      this.startPlaying();
    }
    //this.calcRealFps(frame);
  }

  calcRealFps(frame) {
    var now = Date.now();
    if (this.counterBeginTime == 0) {
      this.counterBeginTime = now;
      this.counterBeginPts = frame.s;
      return;
    }
    this.frameCounter++;
    if (now >= this.counterBeginTime + 5) {
      this.videoFps = this.frameCounter / (frame.s - this.counterBeginPts);
      this.realFps = this.frameCounter * 1000 / (now - this.counterBeginTime);
      if (this.frameCounter % 25 == 0) {
        this.logger.logDebug("fps: " + this.videoFps + ", real fps: " + this.realFps);
      }
    }
  }

  startBuffering() {
    this.state = PlayerState.BUFFERING;
    if (this.pcmPlayer) {
      this.pcmPlayer.pause();
    }
    this.logger.logInfo("buffering");
    this.emit("buffering");
  }

  startPlaying() {
    this.state = PlayerState.PLAYING;
    if (this.pcmPlayer) {
      this.pcmPlayer.resume();
    }
    this.beginTime = 0;
    this.beginPts = 0;
    this.logger.logInfo("playing");
    this.emit("playing");
  }

  doPause() {
    if (this.state == PlayerState.IDLE) {
      return;
    }
    this.state = PlayerState.PAUSED;
    if (this.pcmPlayer) {
      this.pcmPlayer.pause();
    }
  }

  doResume() {
    if (this.state != PlayerState.PAUSED) {
      return;
    }
    if (this.getBufferedTimeLength() >= MIN_BUFFER_TIME) {
      this.startPlaying();
    } else {
      this.startBuffering();
    }
  }

  isMuted() {
    return this.muted;
  }

  setMuted(muted) {
    if (this.pcmPlayer) {
      this.muted = muted;
      this.pcmPlayer.setVolume(muted ? 0 : this.volume);
    }
  }

  getVolume() {
    return this.volume;
  }

  setVolume(volume) {
    if (this.pcmPlayer) {
      this.volume = volume;
      this.muted = !volume;
      this.pcmPlayer.setVolume(volume);
    }
  }

  getBufferedTimeLength() {
    let oldest = 0, newest = 0;
    if (this.videoFrameBuffer && this.videoFrameBuffer.length > 0) {
      oldest = this.videoFrameBuffer[0].s;
      newest = this.videoFrameBuffer[this.videoFrameBuffer.length - 1].s;
    }
    if (newest == 0) {
      return 0;
    } else if (this.lastRenderPts) {
      return newest - this.lastRenderPts;
    } else {
      return newest - oldest;
    }
  }

  getMinBufferTimeLength() {
    return MIN_BUFFER_TIME;
  }

  getPlaybackRate() {
    return this.playbackRate;
  }

  setPlaybackRate(rate) {
    if (rate == this.playbackRate) {
      return;
    }
    if (!ENABLE_FFMPEG_FILTER && this.renderMode == RenderMode.SYNC_AUDIO) {
      this.logger.logError("setPlaybackRate not supported in sync-audio mode without ffmpeg filter");
      return;
    }
    if (ENABLE_FFMPEG_FILTER && this.pcmPlayer) {
      this.worker.postMessage({
        t: WorkerReqTypes.SET_PLAYBACK_RATE,
        r: this.playbackRate,
      });
    } else {
      this.playbackRate = rate;
      this.beginTime = 0;
      this.beginPts = 0;
    }
    this.counterBeginTime = 0;
    this.counterBeginPts = 0;
    this.videoFps = 0;
    this.realFps = 0;
  }

  onSetPlaybackRateRsp(objData) {
    if (objData.e != 0) {
      return;
    }
    this.playbackRate = objData.r;
    this.clearBuffer();
  }

  latencyControlSkipOnly() {
    var latency = this.getBufferedTimeLength();
    this.avgLatency = this.avgLatency * 0.8 + latency * 0.2;
    var minBufTime = this.getMinBufferTimeLength();
    if (this.avgLatency > minBufTime * 2.5) {
      this.logger.logInfo("skip " + (latency - minBufTime));
      this.skip(latency - minBufTime);
    }
  }

  latencyControl() {
    if (this.renderMode == RenderMode.BY_PTS) {
      if (this.pcmPlayer && !ENABLE_FFMPEG_FILTER && LATENCY_CONTROL_PREFER_SKIP_OVER_MUTE) {
        this.latencyControlSkipOnly();
      } else {
        super.latencyControl();
      }
    } else if (this.renderMode == RenderMode.SYNC_AUDIO) {
      if (!ENABLE_FFMPEG_FILTER) {
        this.latencyControlSkipOnly();
      } else {
        super.latencyControl();
      }
    }
  }

  isPtsReached(frame, now) {
    if (this.beginTime == 0 || this.beginPts == 0) {
      this.beginTime = now;
      this.beginPts = frame.s;
      return true;
    }
    var presentTime = (frame.s - this.beginPts) / this.playbackRate + this.beginTime;
    if (Math.abs(presentTime - now) > 0.3) {
      console.log("ts jump detected");
      this.beginTime = now;
      this.beginPts = frame.s;
      return true;
    }
    return presentTime <= now;
  }

  isPtsReached2(frame) {
    if (this.firstAudio) {
      return true;
    }
    var now = this.pcmPlayer.getTimestamp();
    var presentTime = frame.s / this.playbackRate - this.beginTimeOffset;
    if (Math.abs(presentTime - now) > 0.5) {
      console.log("ts jump detected");
      this.clearBuffer();
      return true;
    }
    return presentTime <= now;
  }

  renderAudioFrame(frame) {
    if (this.state != PlayerState.PLAYING) {
      return false;
    }
    //变速时，音频需经过ffmpeg atempo变速，否则会导致音画不同步
    if (this.playbackRate == 1. || ENABLE_FFMPEG_FILTER) {
      this.pcmPlayer.play(frame.d);
    }
    return true;
  }

  renderVideoFrame(frame) {
    if (this.state != PlayerState.PLAYING) {
      return false;
    }
    this.webglPlayer.renderFrame(frame.d, frame.p, frame.w, frame.h);
    return true;
  }

  renderNoBuffer() {
    if (this.audioFrameBuffer.length > 0) {
      var frame = this.audioFrameBuffer[0];
      if (this.renderAudioFrame(frame)) {
        this.audioFrameBuffer.shift();
      }
    }
    if (this.pcmPlayer.getBufferedTimeLength() > 0.5) {
      this.restartAudio();
    }
    if (this.videoFrameBuffer.length > 0) {
      var frame = this.videoFrameBuffer[0];
      if (this.renderVideoFrame(frame)) {
        this.lastRenderPts = frame.s;
        this.videoFrameBuffer.shift();
      }
    }
  }

  renderByPts() {
    var now = Date.now() / 1000.;
    while (this.audioFrameBuffer.length > 0) {
      var frame = this.audioFrameBuffer[0];
      if (this.renderAudioFrame(frame)) {
        this.audioFrameBuffer.shift();
      } else {
        break;
      }
    }
    while (this.videoFrameBuffer.length > 0) {
      var frame = this.videoFrameBuffer[0];
      if (this.isPtsReached(frame, now) && this.renderVideoFrame(frame)) {
        this.lastRenderPts = frame.s;
        this.videoFrameBuffer.shift();
      } else {
        break;
      }
    }
    if (this.audioFrameBuffer.length == 0 && this.videoFrameBuffer.length == 0) {
      this.startBuffering();
    }
  }

  renderSyncAudio() {
    while (this.audioFrameBuffer.length > 0) {
      var frame = this.audioFrameBuffer[0];
      if (this.renderAudioFrame(frame)) {
        this.audioFrameBuffer.shift();
        if (this.firstAudio) {
          this.firstAudio = false;
          this.beginTimeOffset = frame.s / this.playbackRate - this.pcmPlayer.getTimestamp();
        }
      } else {
        break;
      }
    }
    if (this.pcmPlayer.getBufferedTimeLength() == 0) {
      this.firstAudio = true;
      this.beginTimeOffset = 0;
      this.videoFrameBuffer = [];
      this.startBuffering();
    }
    while (this.videoFrameBuffer.length > 0) {
      var frame = this.videoFrameBuffer[0];
      if (this.isPtsReached2(frame) && this.renderVideoFrame(frame)) {
        this.lastRenderPts = frame.s;
        this.videoFrameBuffer.shift();
      } else {
        break;
      }
    }
  }

  renderLoop() {
    if (this.state !== PlayerState.IDLE) {
      requestAnimationFrame(this.renderLoop.bind(this));
    }
    if (this.state != PlayerState.PLAYING) {
      return;
    }

    if (this.renderMode == RenderMode.NO_BUFFER) {
      this.renderNoBuffer();
    } else if (this.renderMode == RenderMode.BY_PTS) {
      this.renderByPts();
    } else if (this.renderMode == RenderMode.SYNC_AUDIO) {
      this.renderSyncAudio();
    }
  }

  skip(interval) {
    if (this.state == PlayerState.IDLE) {
      return;
    }
    var pts = this.lastRenderPts + interval;
    for (var i = 0; i < this.videoFrameBuffer.length; i++) {
      if (this.videoFrameBuffer[i].s >= pts) {
        this.videoFrameBuffer.splice(0, i);
        break;
      }
    }
    for (var i = 0; i < this.audioFrameBuffer.length; i++) {
      if (this.audioFrameBuffer[i].s >= pts) {
        this.audioFrameBuffer.splice(0, i);
        break;
      }
    }
    if (this.pcmPlayer) {
      this.restartAudio();
    }
  }

  clearBuffer() {
    if (this.state == PlayerState.IDLE) {
      return;
    }
    this.videoFrameBuffer = [];
    this.audioFrameBuffer = [];
    if (this.pcmPlayer) {
      this.restartAudio();
    }
    this.beginPts = 0;
    this.beginTime = 0;
  }

  onSeekDone(pos) {
    this.clearBuffer();
  }

  fullscreen() {
    var canvas = this.canvas;
    var fullscreen = canvas.requestFullscreen || canvas.webkitRequestFullscreen || canvas.mozRequestFullScreen || canvas.msRequestFullscreen;
    if (fullscreen) {
      fullscreen.call(canvas);
    } else {
      alert("This browser doesn't support fullscreen");
    }
  }

  exitFullscreen() {
    var exitFullscreen = document.exitFullscreen || document.webkitExitFullscreen || document.mozCancelFullScreen || document.msExitFullscreen;
    if (exitFullscreen) {
      exitFullscreen.call(document);
    } else {
      alert("This browser doesn't support exit fullscreen");
    }
  }

  getStatsHtml() {
    return "Player" + this.id + "<br />" +
              "下载: " + Math.round(this.recvBytes / 1024) + " kB/s" + "<br />" +
              "模式: " + this.renderMode + "<br />" +
              "视频: " + this.videoWidth + "x" + this.videoHeight + "@" + (Utils.format02f(this.videoFps) || "-") + "<br />" +
              "音频: " + (this.audioSampleRate || "-") + "<br />" +
              "缓冲: " + Utils.format02f(this.getBufferedTimeLength()) + "s" + "<br />" +
              "速率: " + this.getPlaybackRate() + "x";
  }
}
