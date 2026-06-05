<template>
  <el-dialog title="事故处理主任信息" :visible.sync="dialogVisible" :append-to-body="true">
    <div class="director">事故处理主任：<span>车站值班站长</span></div>
    <div class="subtitle">变更记录</div>
    <div class="record" style="overflow: scroll;">
      <div class="record-item" v-for="(item, index) in recordList" :key="index">
        <div class="record-content">{{ item.content }}</div>
        <div class="record-props">{{ item.time }}</div>
      </div>
    </div>
  </el-dialog>
</template>

<script>
export default {
    data () {
        return {
            dialogVisible: false,
            recordList: [
                { content: '1、OCC 设置事故处理主任为“车务部经理” ；', time: '2023-12-21 10:37:28' },
                { content: '2、OCC 设置事故处理主任为“中心站正站长” ；', time: '2023-12-21 10:37:28' },
                { content: '3、智慧地铁平台 自动设置事故处理主任为“车站值班站长” ；', time: '2023-12-21 10:37:28' },
                { content: '3、智慧地铁平台 自动设置事故处理主任为“车站值班站长” ；', time: '2023-12-21 10:37:28' },
                { content: '3、智慧地铁平台 自动设置事故处理主任为“车站值班站长” ；', time: '2023-12-21 10:37:28' },
                { content: '3、智慧地铁平台 自动设置事故处理主任为“车站值班站长” ；', time: '2023-12-21 10:37:28' }
            ]
        };
    },
    methods: {
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
.director {
  height: 32px;
  margin-bottom: 24px;
  font-size: 14px;
  color: #FFFFFF;
  line-height: 32px;

  span {
    color: #13FFF5;
  }
}

.subtitle {
  height: 40px;
  line-height: 40px;
  font-size: 16px;
  color: #FFFFFF;
}

::v-deep .el-dialog {
  width: 800px;

  .el-dialog__body {
    height: 512px;
    padding: 24px;
    overflow-y: hidden;
  }
}

.record {
  overflow: auto;
  height: calc(100% - 96px);

  .record-content {
    height: 48px;
    line-height: 48px;
    font-size: 14px;
    color: #FFFFFF;
  }

  .record-props {
    height: 32px;
    line-height: 32px;
    opacity: 0.65;
    font-size: 12px;
    color: #FFFFFF;
    border-bottom: 1px solid rgba(#17557F, 0.65);
  }
}
</style>
