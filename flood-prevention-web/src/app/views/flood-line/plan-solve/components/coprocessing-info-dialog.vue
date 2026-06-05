<template>
  <el-dialog title="业务协同处置" :visible.sync="dialogVisible" :append-to-body="true">
    <table class="basic">
      <tr>
        <td class="title">处置措施名称</td>
        <td>{{ instructionDetail.instructionName }}</td>
        <td class="title">发送时间</td>
        <td>{{ dayjs(instructionDetail.createTime).format('YYYY-MM-DD HH:mm') }}</td>
      </tr>
      <tr>
        <td class="title">发送单位</td>
        <td>长沙地铁6号线朝阳村</td>
        <td class="title">接收单位</td>
        <td>{{ receiverName }}</td>
      </tr>
      <tr style="height: 7rem;">
        <td class="title" style="vertical-align: top;">处置措施内容</td>
        <td colspan="3" style="vertical-align: top;">{{ instructionDetail.instructionContent }}</td>
      </tr>
    </table>
    <table class="extras">
      <tr v-if="hasAttachment">
        <td class="title">附件：</td>
        <td>
          <upload-tool :fileList.sync="fileList" :upload-type="'picture-card'" :max-limit="4" :max-memory="10"
            :show-download="false" :show-delete="false" show-place="bottom" :is-edit="false">
          </upload-tool>
        </td>
      </tr>
      <tr>
        <td class="title">处理状态：</td>
        <td>{{ instructionDetail.statusName }}</td>
      </tr>
    </table>
    <div slot="footer" class="dialog-footer" v-if="isReceiver && instructionDetail.status === 0">
      <el-button type="primary" @click="handleReject" style="width: 128px;">驳回</el-button>
      <el-button type="primary" @click="handleProcess" style="width: 128px;">已处理</el-button>
    </div>
  </el-dialog>
</template>

<script>
import { PlanSolveApi } from '@api/flood';
import UploadTool from '@/app/components/common/upload-tool.vue';
import dayjs from 'dayjs';
export default {
    components: {
        UploadTool
    },
    data () {
        return {
            dialogVisible: false,
            fileList: [], // 图片列表
            instructionDetail: {},
            currId: '',
            clientCode: this.$route.query.stationCode
        };
    },
    computed: {
        receiverName () {
            let receivers = this.instructionDetail?.receivers || [];
            return receivers?.length < 1 ? '' : receivers[0].receiverName;
        },
        isReceiver () {
            let receivers = this.instructionDetail?.receivers || [];

            return receivers?.length < 1 ? false : receivers.recenverCode === this.clientCode;
        },
        hasAttachment () {
            return this.instructionDetail?.attachments?.length > 0;
        }
    },
    methods: {
        dayjs,
        openDialog (id) {
            this.currId = id;
            this.fetchInstructionDetail();
            this.dialogVisible = true;
        },
        closeDialog () {
            this.dialogVisible = false;
        },
        async fetchInstructionDetail () {
            try {
                const res = await PlanSolveApi.publishedInstructionsDetail({ id: this.currId });
                this.instructionDetail = res;

                let host = process.env.NODE_ENV === 'development' ? '10.51.9.130:30768' : window.location.host;
                this.fileList = this.instructionDetail?.attachments?.map(x => {
                    return {
                        shortUrl: x.url,
                        url: 'http://' + host + '/dfs' + x.url,
                        name: x.name
                    };
                });

                console.log('fileList:', this.fileList);
            } catch (error) {
                console.log('Error:', error);
            }
        },
        handleDownload (name, url) {
            this.$message.confirm('确定下载此文件吗?', '提示', {
                confirmButtonText: '确定',
                cancelButtonText: '取消',
                type: 'warning'
            })
                .then(() => {
                    this.downloadFile(`/dfs/${url}`, name);
                })
                .catch();
        },
        downloadFile (url, name) {
            var x = new XMLHttpRequest();
            x.open('GET', url, true);
            x.responseType = 'blob';
            x.onload = function () {
                var url = window.URL.createObjectURL(x.response);
                var a = document.createElement('a');
                a.href = url;
                a.download = name;
                a.click();
            };
            x.send();
        },
        async handleProcess () {
            try {
                const res = await PlanSolveApi.auditInstruction({ id: this.currId, status: 'PROCESSED', clientCode: this.clientCode });
                this.$message.success({
                    message: '处理成功',
                    showClose: true
                });
                this.dialogVisible = false;
            } catch (error) {
                console.log('Error:', error);
            }
        },
        async handleReject () {
            try {
                const res = await PlanSolveApi.auditInstruction({ id: this.currId, status: 'REJECT', clientCode: this.clientCode });
                this.$message.success({
                    message: '驳回成功',
                    showClose: true
                });
                this.dialogVisible = false;
            } catch (error) {
                console.log('Error:', error);
            }
        }
    }
};
</script>

<style lang="scss" scoped>
::v-deep .el-dialog {
  width: 960px;

  .el-dialog__body {
    height: 600px;
    padding: 0 24px 24px 24px;
    overflow-y: hidden;
  }

  .el-dialog__footer {
    text-align: center;
    padding: 16px 24px !important;
    border-top: 1px solid rgba(#17557F, 0.65);
  }
}

::v-deep {
  .el-upload__tip {
    opacity: 0.6;
    font-size: 14px;
    color: #FFFFFF;
  }
}

table.basic {
  border: 1px solid rgba(#17557F, 0.65);
  border-collapse: collapse;
  background: transparent;
  width: 100%;

  tr {
    height: 55px;

    td {
      padding: 16px 24px;
      vertical-align: middle;
      font-size: 14px;
      color: rgba(#fff, 0.85);
      border: 1px solid rgba(#17557F, 0.65);
    }

    .title {
      width: 145px;
      font-size: 14px;
      color: rgba(#fff, 0.85);
      background: #0B3C5D;
      font-weight: 500;
    }
  }
}

table.extras {
  border: 0;
  background: transparent;
  border-collapse: separate;
  border-spacing: 0px 24px;

  tr {

    td {
      opacity: 0.85;
      font-size: 14px;
      color: rgba(#fff, 0.85);
    }

    .title {
      font-size: 14px;
      color: #FFFFFF;
    }
  }
}

// .attchment-list {
//   display: flex;
//   flex-direction: column;
//   justify-content: flex-start;
//   align-items: flex-start;
// }

// .attchment {
//   display: flex;
//   justify-content: space-between;
//   align-items: center;
//   min-width: 176px;

//   &:not(:last-child) {
//     margin-bottom: 16px;
//   }

//   &-text {
//     opacity: 0.85;
//     font-family: SourceHanSansSC-Regular;
//     font-size: 14px;
//     color: #FFFFFF;
//     line-height: 22px;
//     font-weight: 400;
//     align-self: flex-start;
//   }

//   &-icon {
//     margin-right: 8px;
//   }

//   &-icon-active {
//     color: #13FFF5;
//   }
// }
</style>
