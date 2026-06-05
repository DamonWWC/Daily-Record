<template>
  <div class="flex flex-col" style="padding:0 12px">
    <header>业务协同处置</header>
    <div class="content flex-1">
      <div :class="['item-row', showCorner ? 'item-after' : '']" v-if="instructionList.length > 0">
        <div v-for="item in instructionList" :key="item.id"
          class="information-item flex flex-col justify-start items-start" @click="handleCoprocessingInfo(item.id)">
          <div class="information-item-msg">{{ `${dayjs(item.createTime).format('HH:mm')} ${item.instructionTitle}` }}
          </div>
          <div class="flex justify-start items-center">
            <div class="information-item-status"
              :class="{ 'isNoStart': !isProcessStart, 'isSend': isProcessStart && parseInt(item.creatorCode) === parseInt(clientCode) }">
              <span>{{
                parseInt(item.creatorCode) === parseInt(clientCode) ?
                '发送' :
                '收到'
              }}</span>
            </div>
            <div class="information-item-time">{{ item.statusName }}
            </div>
          </div>
        </div>
      </div>
    </div>
    <footer class="grid gap-y-2 grid-cols-4 items-center" v-if="disposalProcessId > 0">
      <el-button v-for="(item, index) in quictButtons" :key="index" type="primary" size="mini"
        @click="handleCoprocessingEdit(item)" :disabled="!isProcessStart">{{ item.instructionName }}</el-button>
      <el-button size="mini" @click="handleCoprocessingEdit('')" :disabled="!isProcessStart">更多</el-button>
    </footer>

    <coprocessing-edit-dialog ref="coprocessingEditDialogRef" />
    <coprocessing-info-dialog ref="coprocessingInfoDialogRef" />
  </div>
</template>

<script>
import CoprocessingEditDialog from './coprocessing-edit-dialog.vue';
import CoprocessingInfoDialog from './coprocessing-info-dialog.vue';
import { PlanSolveApi } from '@api/flood';
import dayjs from 'dayjs';

export default {
    components: {
        CoprocessingEditDialog,
        CoprocessingInfoDialog
    },
    data() {
        return {
            instructionList: [], // 业务协同处置列表
            measureList: [], // 处置措施下拉列表
            quictButtons: [], // 处置措施按钮列表
            disposalProcessId: 1, // 处置ID
            statusOptions: [
                { key: 0, name: '待处理', code: 'PENDING' },
                { key: 1, name: '已处理', code: 'PROCESSED' },
                { key: 2, name: '驳回', code: 'REJECT' },
                { key: 3, name: '已通过', code: 'PASS' }
            ],
            clientCode: this.$route.query.stationCode
        };
    },
    computed: {
        isProcessStart() {
            return this.$store.state.planSolve.isProcessStart;
        },
        showCorner() {
            return !this.isProcessStart;
        }
    },
    mounted() {
        this.fetchInstructionSets();

        this.$utils.eventBus.on('solveFlow/disposalProcessId', msg => {
            this.disposalProcessId = msg || 0;

            this.fetchInstructionsInfo();

            // 每 10 秒刷新
            setInterval(this.fetchInstructionsInfo, 10 * 1000);
        });
    },
    destroyed() {
        this.$utils.eventBus.remove('solveFlow/disposalProcessId');
    },
    methods: {
        dayjs,
        handleCoprocessingEdit(instructionItem) {
            this.$refs.coprocessingEditDialogRef.openDialog(instructionItem, this.disposalProcessId);
        },
        handleCoprocessingInfo(id) {
            this.$refs.coprocessingInfoDialogRef.openDialog(id);
        },
        async fetchInstructionsInfo() {
            try {
                const res = await PlanSolveApi.publishedInstructionsInfo({
                    clientCode: this.$route.query.stationCode,
                    instructionTag: 'YACZ',
                    processId: this.disposalProcessId
                });
                this.instructionList = res;
            } catch (error) {
                console.log('Error:', error);
            }
        },
        async fetchInstructionSets() {
            try {
                let typeCode = this.$route.query.stationCode ? 1 : 0;
                const res = await PlanSolveApi.getInstructionSets({
                    typeCode,
                    instructionTag: 'YACZ'
                });
                this.measureList = res;

                this.quictButtons = _.orderBy(
                    _.filter(res, (o) => parseInt(o.display) === 1),
                    x => x.orders,
                    'asc'
                );
            } catch (error) {
                console.log('Error:', error);
            }
        }
    }
};
</script>

<style lang="scss" scoped>
header {
  height: 40px !important;
  line-height: 40px;
  font-size: 14px;
  color: #fff;
}

.content {
  height: calc(100% - 88px);
  overflow: auto;

  .item-row {
    height: 64px;
    position: relative;

    >div {
      cursor: pointer;
    }
  }

  .information-item {
    border-bottom: 1px solid #0C3E5F;
    padding: 12px 0;

    &-msg {
      font-family: PingFangSC-Regular;
      font-size: 12px;
      color: #FFFFFF;
      font-weight: 400;
    }

    &-status {
      // width: 28px;
      // height: 16px;
      background: #FF8E3B;
      border-radius: 1px;
      padding: 2px 4px;
      margin-right: 8px;

      font-family: PingFangSC-Regular;
      font-size: 10px;
      color: #FFFFFF;
      text-align: center;
      line-height: 12px;
      font-weight: 400;
      white-space: nowrap;

    }

    &-status.isSend {
      background: #068F10;
    }

    &-status.isNoStart {
      background: #6D7B8A;
    }

    &-time {
      opacity: 0.85;
      font-family: PingFangSC-Regular;
      font-size: 12px;
      color: #FFFFFF;
      font-weight: 400;
      margin-right: 8px;
    }
  }

  .item-after {

    &::after {
      content: '';
      position: absolute;
      right: 0;
      top: 0;
      width: 38px;
      height: 38px;
      background: url('~@assets/images/demo.png') no-repeat;
    }
  }

}

footer {
  height: 48px;
  line-height: 48px;

  ::v-deep .el-button {
    padding: 0;
    margin: 0 4px !important;

    // span {
    //   font-family: SourceHanSansSC-Regular;
    //   font-size: 12px;
    //   // color: #FFFFFF;
    //   text-align: center;
    //   line-height: 22px;
    //   font-weight: 400;
    // }
  }
}
</style>
