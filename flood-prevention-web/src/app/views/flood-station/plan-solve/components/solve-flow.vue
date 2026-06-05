<template>
  <div class="flex flex-col" style="padding:0 12px">
    <header class="flex flex-row items-center justify-between">
      <div>处置流程</div>
      <el-button class="iconfont icon-liandongchuzhi" type="primary" :disabled="!isReady" size="mini"
        @click="handleOtherProcess">&nbsp;联动其他处置</el-button>
    </header>
    <div class="flex flex-1 content">
      <div class="flex items-center flow-step-begin" v-show="!isStart">
        <el-button class="iconfont icon-bofang" type="primary" size="mini" :disabled="!isReady"
          @click="handleProcessStart">&nbsp;启动处置</el-button>
        <div class="flow-step__line" v-show="isStart"></div>
      </div>
      <div class="flex flow-step-middle" v-show="isStart">
        <div :class="['flow-step', item.isActive ? 'flow-step--active' : '']" v-for="(item, index) in stepList"
          :key="index">
          <div class="flow-step__point">
            <h3>{{ item.stepName }}</h3>
            <div v-html="dayjs(item.stepTime).isValid() ? dayjs(item.stepTime).format('YYYY-MM-DD<br>HH:mm:ss') : ''">
            </div>
          </div>
          <div class="flow-step__line"></div>
        </div>
      </div>
      <div class="flex flex-col justify-center flow-step-end" v-show="isStart">
        <el-button v-for="(item, index) in stageButtonList" :key="index" type="primary" class="iconfont icon-bofang"
          size="mini" @click="handleNestStage(item.id)">{{ item.name }}</el-button>
        <el-button type="danger" class="iconfont icon-jieshubofang1" size="mini"
          @click="handleProcessEnd">&nbsp;结束处置恢复正常</el-button>
      </div>
    </div>
    <el-dialog :visible.sync="notifyDialogVisible" :append-to-body="true" width="50%">
      <template slot="title">
        <div class="flex items-center justify-start">
            <div>{{ `查看信息通报内容` }}</div>
            <div class="text-xs text-warning">{{ `(5秒后关闭弹窗)` }}</div>
        </div>
      </template>
      <el-form :model="ruleForm" ref="ruleForm" label-width="120px" class="demo-ruleForm" label-position="top">
        <el-form-item label="发布内容" prop="content">
          <el-descriptions title="" :column="1" border>
            <el-descriptions-item label="模板名称" label-class-name="notify-description-label">{{ `车站水淹信息首报`
            }}</el-descriptions-item>
            <el-descriptions-item label="发布内容" label-class-name="notify-description-label">{{ `12月26日人民东路站报。7:51
              发现出入口路面出现积水，已安排专人在对应出入口处进行值守，并加强现场巡视，车控室进行实时监控。【智慧地铁管控平台】` }}</el-descriptions-item>
          </el-descriptions>
        </el-form-item>
        <el-form-item label="通报对象" prop="person">
          <el-table class="" :data="dialogTableData" height="100%" style="width: 100%;">
            <el-table-column prop="name" label="姓名" width="150"></el-table-column>
            <el-table-column prop="phone" label="电话"></el-table-column>
          </el-table>
          <div class="notify-time">{{ `通报时间：${'2023-08-10 16:09:50'}` }}</div>
        </el-form-item>
      </el-form>
    </el-dialog>
    <el-dialog title="结束提示" :visible.sync="dialogVisible" :append-to-body="true">
      <div style="text-align: center; font-size: .875rem;">
        <span>请确认车站站厅、站台地面积水已排除、轨行区积水不影响列车正常运</span><br />
        <span>行、淹水水位逐渐降低，以上条件是否具备。</span>
        <div style="font-size: .875rem;color: #EFA80D; margin-top: 1rem;"><i class="el-icon-warning"
            style="font-size: 1rem; margin-right: 0.5rem;"></i>结束处置后自动生成处置报告</div>
      </div>
      <div slot="footer" class="dialog-footer">
        <el-button @click="dialogVisible = false">取 消</el-button>
        <el-button type="primary" @click="handleEndAndReport">结束处置并生成报告</el-button>
      </div>
    </el-dialog>
  </div>
</template>

<script>
import dayjs from 'dayjs';
import wpf from '@/common/utils/wpf';
import { PlanSolveApi } from '@api/flood';
import store from '@/app/store';
import { getAddress } from '@/common/utils/tools';
const dialogTableDataMock = [
    {
        name: '朱世康',
        phone: '13918323940'
    },
    {
        name: '孙常梁',
        phone: '13918323940'
    },
    {
        name: '何荣',
        phone: '13918323940'
    },
    {
        name: '邓怡',
        phone: '13918323940'
    },
    {
        name: '彭力豪',
        phone: '13918323940'
    }
];
export default {
    computed: {
        planSolveParam () {
            return this.$store.state.planSolve.planSolveParam;
        },
        isReady () {
            return this.$store.state.planSolve.isReady;
        }
    },
    data () {
        return {
            dialogVisible: false,
            stepList: [],
            stageButtonList: [], // 当前阶段操作按钮列表
            currStageId: 0, // 当前步骤ID
            isStart: false,
            isEnd: false,
            ruleForm: {},
            dialogTableData: dialogTableDataMock,
            notifyDialogVisible: false // 信息通报内容弹窗
        };
    },
    async created () {
        // 进入页面先判断当前流程是否已经开始，开始则渲染已有流程(存在eventId以及stepList数据则认为是流程已经开始)
        this.isReady && this.planSolveParam.eventId && await this.fetchProcessRecord(false);
        if (this.stepList.length > 0) {
            this.isStart = true;
            // 更新store信息，处置要点监听信息并更新
            store.commit('planSolve/SET_PROCESSSTART', this.isStart);
            // 更新处置流程节点信息
            this.currStageId = this.stepList[this.stepList.length - 1].stepNum;
            this.fetchNextProcessStageInfo(this.currStageId);
            // 设置处置过程组件信息
            this.fetchProcessInfo(this.planSolveParam.processId || 6716962461352448);
        }
    },
    methods: {
        dayjs,
        handleProcessEnd () {
            this.dialogVisible = true;
        },
        async handleProcessStart () {
            this.notifyDialogVisible = true;
            setInterval(() => {
                this.notifyDialogVisible = false;
            }, 5 * 1000);
            try {
                let params = {
                    eventCode: this.planSolveParam.eventCode,
                    rehearsal: this.planSolveParam.rehearsal,
                    planId: this.planSolveParam.planId
                };
                if (this.planSolveParam.eventId) {
                    params.eventId = this.planSolveParam.eventId;
                }

                // 启动处置流程
                let res = await PlanSolveApi.setProcessStart(params);
                this.currStageId = res?.stageId;
                this.$utils.eventBus.emit('solveFlow/disposalProcessId', res?.disposalProcessId);
                this.isStart = true;
                store.commit('planSolve/SET_PROCESSSTART', this.isStart);
                let planSolveParam = this.planSolveParam || {};
                planSolveParam.eventId = res?.eventId;
                planSolveParam.processId = res?.disposalProcessId;
                store.commit('planSolve/SET_PLANSOLVEPARAM', planSolveParam);
                this.fetchProcessInfo(res?.disposalProcessId);
                // this.fetchCurrentProcessStageInfo(this.currStageId);
                this.fetchNextProcessStageInfo(this.currStageId);
                this.fetchProcessRecord(true);
            } catch (error) {
                console.log('Error:', error);
            }
        },
        async handleNestStage (id) {
            await this.triggerProcessStage(id);
            this.fetchProcessRecord(false);
        },
        handleOtherProcess () {
            wpf.emit({
                command: 'other_process',
                param: { data: { ...this.planSolveParam } }
            });
        },
        async fetchCurrentProcessStageInfo (id) {
            try {
                const res = await PlanSolveApi.getCurrentProcessStageInfo({ id });
                this.stageButtonList = [];
                this.stageButtonList.push(res);
            } catch (error) {
                console.log('Error:', error);
            }
        },
        async fetchNextProcessStageInfo (id) {
            try {
                const res = await PlanSolveApi.getNextProcessStageInfo({ id });
                this.stageButtonList = [];
                this.stageButtonList = res;
                // console.log('res:', res);
            } catch (error) {
                console.log('Error:', error);
            }
        },
        async fetchProcessInfo (id) {
            try {
                const res = await PlanSolveApi.getProcessInfo({ disposalProcessId: id });
                this.$utils.eventBus.emit('solveFlow/processInfo', res);
            } catch (error) {
                console.log('Error:', error);
            }
        },
        async triggerProcessStage (id) {
            this.currStageId = id;
            try {
                const res = await PlanSolveApi.triggerProcessStage({
                    eventId: this.planSolveParam.eventId,
                    stageId: this.currStageId,
                    disposalProcessId: this.planSolveParam.processId
                });
                this.stageButtonList = res || [];
            } catch (error) {
                console.log('Error:', error);
            }
        },
        async fetchProcessRecord (isFirst) {
            try {
                let params = {
                    planId: this.planSolveParam.planId
                };

                // 启动处置不用传eventId
                if (this.planSolveParam.eventId && !isFirst) {
                    params.eventId = this.planSolveParam.eventId;
                }

                const res = await PlanSolveApi.getProcessRecord(params);
                console.log('fetchProcessRecord: ', res);
                this.$utils.eventBus.emit('solveFlow/processRecord', res);
                let stepArray = res.map(x => ({ stepId: x.id, stepName: x.stageName, isActive: false, stepTime: x.createTime, stepNum: x.stageId }));
                this.stepList = stepArray;
                if (this.stepList?.length > 0) {
                    this.stepList[this.stepList.length - 1].isActive = true;
                }
            } catch (error) {
                console.log('Error:', error);
            }
        },
        async handleEndAndReport () {
            try {
                // this.$wpf.emit({ command: 'close_dispose' });
                this.isEnd = true;
                const res = await PlanSolveApi.setProcessEnd({ id: this.planSolveParam.processId });
                store.commit('planSolve/SET_PROCESSSTART', !this.isEnd);
                this.dialogVisible = false;
                this.showDisposalReport();
            } catch (error) {
                console.log('Error:', error);
            }
        },
        showDisposalReport () {
            const address = getAddress(this.$router.options.routes, 'disposalReport', this.$route.path);
            wpf.emit({
                command: 'showDisposalReport',
                param: {
                    type: 'showDisposalReport',
                    data: {
                        address: `${address}&disposalProcessId=${this.planSolveParam.processId}`
                    }
                    // 6724005408080384
                }
            });
        }
    }
};
</script>

<style lang="scss" scoped>
header {
  height: 40px;
  line-height: 40px;
  font-size: 14px;
  color: #fff;
}

.content {
  overflow: auto;
}

.flow-step-begin {
  height: 100%;

  .flow-step__line {
    background: #17557F;
    width: 88px;
    height: 0.5px;
    margin: 0 8px;
  }
}

.flow-step-middle {
  .flow-step {
    height: 100%;
    width: 132px;
    position: relative;

    .flow-step__point {
      position: absolute;
      background: #17557F;
      top: 50%;
      left: 0;
      width: 12px;
      height: 12px;
      border-radius: 6px;
      transform: translateY(-50%);

      h3 {
        position: absolute;
        font-size: 12px;
        color: #FFFFFF;
        text-align: center;
        min-width: 200px;
        top: calc(50% - 12px);
        left: 50%;
        transform: translate(-50%, -100%);
      }

      div {
        font-size: 10px;
        color: #FFFFFF;
        text-align: center;
        position: absolute;
        width: 100px;
        top: calc(50% + 12px);
        left: 50%;
        transform: translate(-50%, 0%);
        line-height: 14px;
      }
    }

    .flow-step__line {
      position: absolute;
      background: #17557F;
      width: 104px;
      height: 1px;
      top: 50%;
      left: 20px;
      transform: translateY(-50%);
    }
  }

  .flow-step--active {
    .flow-step__point {
      background: #13FFF5;
    }

    .flow-step__line {
      background: transparent;
      background-image: linear-gradient(to right, #13FFF5 60%, transparent 40%);
      background-size: 12px 1px;
      background-repeat: repeat-x;
    }
  }
}

.flow-step-middle .flow-step:last-of-type {
  width: calc(132px - 16px);

  .flow-step__line {
    width: 88px;
  }
}

.flow-step-middle .flow-step:first-of-type {
  margin-left: 66px;
}

.flow-step-end {
  .el-button {
    margin: 8px 0;
  }
}

::v-deep .el-dialog {
  width: 496px;
  margin-top: 0 !important;
  top: 50%;
  transform: translateY(-50%);

  .el-dialog__body {
    // height: 104px;
    padding: 24px;
  }

  .el-dialog__footer {
    text-align: center;
    padding: 16px 24px !important;
  }
}
</style>
