
const CodecTypes = {
  UNKNOWN: 0,
  H264: 1,
  H265: 2,
};

const MAX_KEY_FRAME_INTERVAL = 4;

class MsePlayer extends Player {

  constructor(video, callback) {
    super(callback);
    this.ms = null;
    this.sourceBuffer = null;
    this.bufferQueue = [];
    this.historyBuffers = []; //保存若干个历史buffer用于播放器出错时恢复，降低用户感知
    this.recoverBuffers = []; //等待恢复的历史buffer
    this.video = video;
    this.codecStr = "";
    this.videoCodecType = CodecTypes.UNKNOWN;
    this.moov = null;
    this.reloadState = 0; //0: 未重载，1: 尚未append buffer，2: 已append moov
    this.playbackRate = 1.;
    this.clearBufferOnNextKeyFrame = false;
  }

  getWorkerMode() {
    return WorkerMode.REMUXER;
  }

  open(objData) {
    this.codecStr = objData.c;
    if (!MediaSource.isTypeSupported('video/mp4; codecs="' + this.codecStr + '"')) {
      this.reportError(PlayerError.CODEC_UNSUPPORTED, "浏览器不支持此视频编码！")
    }
    if (this.codecStr.indexOf("avc") !== -1) {
      this.videoCodecType = CodecTypes.H264;
    } else if (this.codecStr.indexOf("hvc") !== -1 || this.codecStr.indexOf("hev") !== -1) {
      this.videoCodecType = CodecTypes.H265;
    }
    if (!this.ms) {
      this.openVideo();
    }
  }

  close() {
    this.closeVideo();
    this.codecStr = "";
    this.videoCodecType = CodecTypes.UNKNOWN;
    this.moov = null;
  }

  onMp4Fragment(objData) {
    //this.logger.logInfo("got mp4 data");
    var view = new DataView(objData.d.buffer);
    var offset = 0;
    while (offset <= view.byteLength - 8) {
      var boxLen = view.getUint32(offset, false);
      var boxType = view.getUint32(offset + 4, false);
      if (boxType == 0x6d6f6f76) {  //moov
        this.logger.logInfo("found moov");
        this.moov = objData.d;
        this.reloadState = 0;
      } else if (boxType == 0x6d646174) { //mdat
        var pos = offset + 8;
        while (pos < offset + boxLen - 4) {
          var naluLen = view.getUint32(pos, false);
          if (this.videoCodecType == CodecTypes.H264) {
            var naluType = view.getUint8(pos + 4) & 0x1f;
            if (naluType == 5) {
              objData.d.containKeyFrame = true;
              break;
            }
          } else if (this.videoCodecType == CodecTypes.H265) {
            var naluType = (view.getUint8(pos + 4) & 0x7e) >> 1;
            if (naluType == 19 || naluType == 20) {
              objData.d.containKeyFrame = true;
              break;
            }
          }
          pos += 4 + naluLen;
        }
      }
      offset += boxLen;
    }
    if (objData.d.containKeyFrame && this.clearBufferOnNextKeyFrame) {
      this.logger.logInfo("found key frame after seek");
      objData.d.needClearBuffer = true;
      this.clearBufferOnNextKeyFrame = false;
    }
    
    this.bufferQueue.push(objData.d);
    if (this.bufferQueue.length == 1) {
      this.nextSegment();
    } else if (this.bufferQueue.length > 100) {
      for (var i = 1; i < this.bufferQueue.length; i++) {
        if (this.bufferQueue[i].containKeyFrame) {
          this.logger.logInfo("discard " + i + " buffer");
          this.bufferQueue.splice(0, i);
          break;
        }
      }
      if (this.bufferQueue.length > 100) {
        this.bufferQueue.splice(0, 50);
      }
    }
  }

  openVideo() {
    this.ms = new MediaSource();
    this.ms.addEventListener('sourceopen', this.onMediaSourceOpen.bind(this));
    var that = this;
    this.video.src = window.URL.createObjectURL(this.ms);
    this.video.onwaiting = this.onVideoWaiting.bind(this);
    this.video.onplaying = this.onVideoPlaying.bind(this);
    this.video.onerror = function() {
      that.logger.logError("video error, url: " + that.url + ", error: " + that.video.error.message);
      that.reloadVideo();
    };
  }

  onVideoWaiting() {
    var latency = this.getBufferedTimeLength();
    this.logger.logInfo("video waiting, buffered " + Utils.format02f(latency));
    this.emit("buffering");
    if (!this.isRealtimeStream || latency > 0.7) {
      return;
    }
    if (this.video.playbackRate > 1) {
      this.video.playbackRate = 1;
    }
    latency = Math.min(Math.max(latency, 0), 0.75);
    this.minBufTime = this.minBufTime ? this.minBufTime * 0.6 + latency * 0.4 : latency;
    this.logger.logInfo("set minBufTime to " + Utils.format02f(this.minBufTime));
    if (this.minBufTime > 0.4) {
      if (this.bufTimeDecayTimer) {
        clearTimeout(this.bufTimeDecayTimer);
      }
      var that = this;
      this.bufTimeDecayTimer = setTimeout(function fn() {
        if (that.minBufTime > 0.4) {
          that.minBufTime *= 0.9;
          that.logger.logInfo("no waiting in 60 seconds, minBufTime decay to " + Utils.format02f(that.minBufTime));
          that.bufTimeDecayTimer = setTimeout(fn, 60 * 1000);
        }
      }, 60 * 1000);
    }
  }

  onVideoPlaying() {
    this.emit("playing");
  }

  onMediaSourceOpen() {
    //this.logger.logInfo("on media source open");
    this.sourceBuffer = this.ms.addSourceBuffer('video/mp4; codecs="' + this.codecStr + '"');
    this.sourceBuffer.mode = 'sequence';
    this.sourceBuffer.addEventListener('updateend', this.nextSegment.bind(this));
    this.nextSegment();
    this.video.play();
  }

  closeVideo() {
    if (this.ms) {
      if (this.sourceBuffer) {
        this.sourceBuffer.removeEventListener('updateend', this.nextSegment.bind(this));
        this.ms.removeSourceBuffer(this.sourceBuffer);
        this.sourceBuffer = null;
      }
      this.ms.endOfStream();
      this.ms = null;
    }
    if (this.video) {
      this.video.onwaiting = null;
      this.video.onplaying = null;
      this.video.onerror = null;
      this.video.playbackRate = 1.;
      this.video.pause();
      this.video.removeAttribute('src');
      this.video.load();
    }
    this.reloadState = 0;
    this.minBufTime = 0;
    if (this.bufTimeDecayTimer) {
      clearTimeout(this.bufTimeDecayTimer);
      this.bufTimeDecayTimer = null;
    }
    this.bufferQueue = [];
    this.historyBuffers = [];
    this.recoverBuffers = [];
  }

  reloadVideo() {
    this.avgLatency = 0;
    this.minBufTime = 0;
    if (this.bufTimeDecayTimer) {
      clearTimeout(this.bufTimeDecayTimer);
      this.bufTimeDecayTimer = null;
    }
    this.reloadState = 1;
    if (this.recoverBuffers.length == 0) {
      for (var i = 0; i < this.historyBuffers.length; i++) {
        if (this.historyBuffers[i].containKeyFrame) {
          this.logger.logInfo("reload video, start from earliest key frame");
          this.historyBuffers.splice(0, i);
          break;
        }
      }
      this.recoverBuffers = this.historyBuffers;
      this.historyBuffers = [];
      this.logger.logInfo("recover " + this.recoverBuffers.length + " buffers");
    }
    this.video.src = window.URL.createObjectURL(this.ms);
  }

  //https://developer.mozilla.org/en-US/docs/Web/Guide/Audio_and_video_delivery/buffering_seeking_time_ranges
  //https://stackoverflow.com/questions/62817465/can-javascript-mse-play-segmented-mp4-from-the-middle
  nextSegment() {
    if (!this.sourceBuffer || this.sourceBuffer.updating) {
      return;
    }
    //this.logger.logInfo("append buffer");
    if (this.getPlayedTimeLength() >= 5) {
      this.sourceBuffer.remove(0, this.video.currentTime - MAX_KEY_FRAME_INTERVAL); //留的时间余量应大于关键帧间隔
      return;
    }
    //this.logBufferedTimeRanges();
    try {
      if (this.reloadState == 1 && this.moov) {
        this.sourceBuffer.appendBuffer(this.moov);
        this.reloadState = 2;
      } else if (this.recoverBuffers.length > 0) {
        this.sourceBuffer.appendBuffer(this.recoverBuffers[0]);
        this.recoverBuffers.shift();
      } else if (this.bufferQueue.length > 0) {
        this.sourceBuffer.appendBuffer(this.bufferQueue[0]);
        if (this.bufferQueue[0].needClearBuffer) {
          this.clearBuffer();
        }
        this.historyBuffers.push(this.bufferQueue.shift());
        if (this.historyBuffers.length > 15) {
          this.historyBuffers.shift();
        }
      }
      if (this.reloadState == 2 && this.video.buffered.length > 0) {
        this.video.currentTime = this.video.buffered.start(0);
        this.video.playbackRate = this.playbackRate;
        this.reloadState = 0;
      }
    } catch (error) {
      this.logger.logError("appendBuffer failed, url: " + this.url + ", error: " + error);
      if (error.code == 22) { //QuotaExceededError
        this.sourceBuffer.remove(0, this.video.currentTime - MAX_KEY_FRAME_INTERVAL);
      }
    }
  }

  doPause() {
    if (this.video) {
      this.video.pause();
    }
  }

  doResume() {
    if (this.video) {
      this.video.play();
    }
  }

  isMuted() {
    return this.video.muted;
  }

  setMuted(muted) {
    if (this.video) {
      this.video.muted = muted;
    }
  }

  getVolume() {
    if (this.video) {
      return this.video.volume;
    }
    return 0;
  }

  setVolume(volume) {
    if (this.video) {
      this.video.muted = !volume;
      this.video.volume = volume;
    }
  }

  getBufferedTimeLength() {
    if (this.video.buffered.length > 0) {
      return this.video.buffered.end(this.video.buffered.length-1) - this.video.currentTime;
    }
    return 0;
  }

  getMinBufferTimeLength() {
    return this.minBufTime;
  }

  getPlayedTimeLength() {
    if (this.video.buffered.length > 0) {
      return this.video.currentTime - this.video.buffered.start(0);
    }
    return 0;
  }

  getPlaybackRate() {
    return this.video.playbackRate;
  }

  setPlaybackRate(rate) {
    this.video.playbackRate = this.playbackRate = rate;
  }

  skip(interval) {
    this.video.currentTime += interval;
  }

  clearBuffer() {
    if (this.video.buffered.length > 0) {
      this.video.currentTime = this.video.buffered.end(this.video.buffered.length-1);
    }
  }

  onSeekDone(pos) {
    this.clearBufferOnNextKeyFrame = true;
  }

  logBufferedTimeRanges() {
    var s = "buffered ranges: ";
    for (var i = 0; i < this.video.buffered.length; i++) {
      s += "[" + this.video.buffered.start(i) + "-" + this.video.buffered.end(i) + "] ";
    }
    this.logger.logInfo(s);
  }

  fullscreen() {
    var video = this.video;
    var fullscreen = video.requestFullscreen || video.webkitRequestFullscreen || video.mozRequestFullScreen || video.msRequestFullscreen;
    if (fullscreen) {
      fullscreen.call(video);
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
              "编码: " + this.codecStr + "<br />" + 
              "缓冲: " + Utils.format02f(this.getBufferedTimeLength()) + "s" + " " +
              "最低: " + Utils.format02f(this.getMinBufferTimeLength()) + "s" + "<br />" +
              "速率: " + this.getPlaybackRate() + "x";
  }
}
