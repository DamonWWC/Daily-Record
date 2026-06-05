<template>
  <el-dialog class="video-dialog" title="出入口视频联动" width="1680px" height="960px" :visible.sync="dialogVisible">
    <el-carousel trigger="click" class="h-full">
      <el-carousel-item v-for="item in 4" :key="item">
        <div class="h-full grid grid-cols-2 grid-rows-2 gap-px">
          <ecp-player :isRecordMode="false" :autoplay="false" playerType="mse" :showControls="false"
            url="ws://172.25.23.41:33211/play_real/44012200001321010406/0" :default-volume="0.1"
            :customBtnsStyle="customBtnsStyle" progressColor="blue">
          </ecp-player>
          <ecp-player :isRecordMode="false" :autoplay="false" playerType="mse" :showControls="false"
            url="ws://172.25.23.41:33211/play_real/44012200001321010406/1" :default-volume="0.1"
            :customBtnsStyle="customBtnsStyle" progressColor="blue">
          </ecp-player>
          <ecp-player :isRecordMode="false" :autoplay="false" playerType="mse" :showControls="false"
            url="ws://172.25.23.41:33211/play_real/44012200001321010406/2" :default-volume="0.1"
            :customBtnsStyle="customBtnsStyle" progressColor="blue">
          </ecp-player>
          <ecp-player :isRecordMode="false" :autoplay="false" playerType="mse" :showControls="false"
            url="ws://172.25.23.41:33211/play_real/44012200001321010406/3" :default-volume="0.1"
            :customBtnsStyle="customBtnsStyle" progressColor="blue">
          </ecp-player>
        </div>
      </el-carousel-item>
    </el-carousel>
    <div slot="footer" class="dialog-footer flex justify-end">
      <div class="grid grid-cols-4 items-center">
        <i class="iconfont icon-jiufenping"></i>
        <i class="iconfont icon-sifenping is-active"></i>
        <i class="iconfont icon-yifenping"></i>
        <i class="iconfont icon-jieshubofang1"></i>
      </div>
    </div>
  </el-dialog>
</template>

<script>
export default {
    data () {
        this.customBtnsStyle = {};
        let urls = {
            mse: 'ws://172.25.23.41:33211/play_real/44012200001321010406/0'
        };
        Object.keys(urls).forEach(key => {
            urls[`${key}_`] = urls[key];
        });
        return {
            dialogVisible: false,
            typeValue: 'mse',
            urls: urls
        };
    },
    methods: {
        handleInputFocus (key) {
            this.$refs[`inputRef_${key}`][0].select();
        },
        handleVideoPlay (key) {
            let urlValue = this.urls[`${key}_`];
            if (!urlValue) {
                this.$message.warning('请输入播放地址');
                return;
            }
            this.urls[`${key}`] = urlValue;
            this.$refs[key][0].handleStopVideo(true);
            setTimeout(() => {
                this.$refs[key][0].handlePlayVideo();
            }, 100);
        },
        handleChange (key, newVal) {
            this.urls[`${key}_`] = newVal;
        },
        openDialog () {
            this.dialogVisible = true;
        },
        closeDialog () {
            this.dialogVisible = false;
        }
    }
};
</script>

<style lang="scss" scoped>
@import "@/styles/flood.scss";

.dev-video-wrapper {
  width: 500px;
  display: inline-block;
  margin-right: 15px;
}

.ecp-player {
  position: relative;
  display: flex;
  box-sizing: border-box;
}
</style>
<style lang="scss">
.video-dialog .el-dialog {
  width: 1680px;
  max-width: 92vw;
  height: 960px;
  max-height: 84vh;
  margin-top: 8vh !important;
  margin-bottom: 8vh;

  .el-dialog__body {
    overflow: auto;
    height: calc(100% - 56px - 32px) !important;
    padding: 0 32px;
  }

  .el-dialog__footer {
    margin: 0;
    padding: 0 !important;
  }

  .dialog-footer {
    height: 32px;
    opacity: 0.85;
    background: #18486F;

    i {
      width: 16px;
      margin: 0 8px;

      &.is-active {
        color: #13FFF5;
      }
    }
  }
}
</style>
