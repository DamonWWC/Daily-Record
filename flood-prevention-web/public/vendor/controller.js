class Controller {
  constructor(httpServerAddr) {
    this.server = httpServerAddr;
    this.logger = new Logger("Controller");
  }

  sendHttpReq(path, req, cb, disableLog) {
    var that = this;
    if (!cb) {
      cb = function(e) {
        if (e.OpCode != 0) {
          that.logger.logError("req " + path + " error, code: " + e.OpCode + ", desc: " + e.OpDesc);
        }
      };
    }
    var params = [];
    for (var p in req) {
      if (req.hasOwnProperty(p)) {
        params.push(encodeURIComponent(p) + "=" + encodeURIComponent(req[p]));
      }
    }
    var url = "http://" + this.server + path + "?" + params.join("&");
    var xhr = new XMLHttpRequest();
    xhr.onload = function() {
      if (xhr.status != 200) {
        cb({OpCode: xhr.status, OpDesc: "http request failed"});
        return;
      }
      if (!disableLog) {
        that.logger.logDebug("got http resp: " + xhr.response);
      }
      var res;
      try {
        res = JSON.parse(xhr.response);
      } catch (e) {
        console.error("json parse exception: " + e);
        cb({OpCode: xhr.status, OpDesc: "parse json failed"});
        return;
      }
      cb(res);
    };
    xhr.open("get", url, true);
    xhr.send();
    if (!disableLog) {
      this.logger.logDebug("send http req: " + url);
    }
  }

  getStreamUrl(url, cb) {
    this.sendHttpReq(url, {}, cb);
  }

  getRecordList(dstId, devId, startTime, endTime, cb) {
    this.sendHttpReq("/video-api/get_record_list", {"dst_id": dstId, "device_id": devId, "starttime": startTime, "endtime": endTime}, cb);
  }

  ptzCtrl(devId, cmd, stop, speed, cb) {
    this.sendHttpReq("/video-api/ptz_ctrl", {"device_id": devId, "command": cmd, "stop": stop, "speed": speed}, cb);
  }

  presetCtrl(devId, presetNum, action, cb) {
    this.sendHttpReq("/video-api/preset_ctrl", {"device_id": devId, "presetnum": presetNum, "action": action}, cb);
  }

  pausePlayback(sessionId, cb) {
    this.sendHttpReq("/video-api/pause_playback", {"session_id": sessionId}, cb);
  }

  resumePlayback(sessionId, cb) {
    this.sendHttpReq("/video-api/resume_playback", {"session_id": sessionId}, cb);
  }

  setPlaybackSpeed(sessionId, speed, cb) {
    this.sendHttpReq("/video-api/set_playback_speed", {"session_id": sessionId, speed: speed}, cb);
  }

  seekPlayback(sessionId, offset, whence, cb) {
    this.sendHttpReq("/video-api/seek_playback", {"session_id": sessionId, offset: offset, whence: whence}, cb);
  }

  getPlaybackProgress(sessionId, cb, disableLog) {
    this.sendHttpReq("/video-api/get_playback_progress", {"session_id": sessionId}, cb, disableLog);
  }

}