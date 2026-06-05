function PCMPlayer(option) {
    this.init(option);
}

PCMPlayer.prototype.init = function(option) {
    var defaults = {
        encoding: '16bitInt',
        channels: 1,
        sampleRate: 8000,
    };
    this.option = Object.assign({}, defaults, option);
    this.maxValue = this.getMaxValue();
    this.typedArray = this.getTypedArray();
    this.createContext();
};

PCMPlayer.prototype.getMaxValue = function () {
    var encodings = {
        '8bitInt': 128,
        '16bitInt': 32768,
        '32bitInt': 2147483648,
        '32bitFloat': 1
    }

    return encodings[this.option.encoding] ? encodings[this.option.encoding] : encodings['16bitInt'];
};

PCMPlayer.prototype.getTypedArray = function () {
    var typedArrays = {
        '8bitInt': Int8Array,
        '16bitInt': Int16Array,
        '32bitInt': Int32Array,
        '32bitFloat': Float32Array
    }

    return typedArrays[this.option.encoding] ? typedArrays[this.option.encoding] : typedArrays['16bitInt'];
};

PCMPlayer.prototype.createContext = function() {
    this.audioCtx = new (window.AudioContext || window.webkitAudioContext)();
    this.gainNode = this.audioCtx.createGain();
    this.gainNode.gain.value = 1;
    this.gainNode.connect(this.audioCtx.destination);
    this.startTime = this.audioCtx.currentTime;
};

PCMPlayer.prototype.isTypedArray = function(data) {
    return (data.byteLength && data.buffer && data.buffer.constructor == ArrayBuffer);
};

PCMPlayer.prototype.setVolume = function(volume) {
    this.gainNode.gain.value = volume;
};

PCMPlayer.prototype.destroy = function() {
    this.audioCtx.close();
    this.audioCtx = null;
};

PCMPlayer.prototype.getTimestamp = function () {
    if (this.audioCtx) {
        return this.audioCtx.currentTime;
    } else {
        return 0;
    }
};

PCMPlayer.prototype.play = function (data) {
    if (data.length != this.option.channels) {
        console.log("channel count not match");
        return;
    }
    var audioBuffer;
    for (var ch = 0; ch < data.length; ch++) {
        if (!this.isTypedArray(data[ch])) {
            return;
        }
        var channelData = new this.typedArray(data[ch].buffer);
        if (channelData.length === 0) {
            return;
        }
        if (!audioBuffer) {
            audioBuffer = this.audioCtx.createBuffer(this.option.channels, channelData.length, this.option.sampleRate);
        }
        var channelBuffer = audioBuffer.getChannelData(ch);
        for (var i = 0; i < channelData.length; i++) {
            channelBuffer[i] = channelData[i] / this.maxValue;
            if (i < 50) {
                channelBuffer[i] *= i / 50;
            } else if (i >= channelData.length - 50) {
                channelBuffer[i] *= (channelData.length - 1 - i) / 50;
            }
        }
    }

    if (this.startTime < this.audioCtx.currentTime) {
        this.startTime = this.audioCtx.currentTime;
    }
    //console.log('start vs current '+this.startTime+' vs '+this.audioCtx.currentTime+' duration: '+audioBuffer.duration);
    var bufferSource = this.audioCtx.createBufferSource();
    bufferSource.buffer = audioBuffer;
    bufferSource.connect(this.gainNode);
    bufferSource.start(this.startTime);
    this.startTime += audioBuffer.duration;
};

PCMPlayer.prototype.pause = function () {
    if (this.audioCtx.state === 'running') {
        this.audioCtx.suspend()
    }
}

PCMPlayer.prototype.resume = function () {
    if (this.audioCtx.state === 'suspended') {
        this.audioCtx.resume()
    }
}

PCMPlayer.prototype.getBufferedTimeLength = function () {
    return Math.max(this.startTime - this.audioCtx.currentTime, 0);
}
