<template>
  <div class="flex flex-col" style="padding:0 .75rem">
    <header class="flex flex-row justify-between items-center">
      <div>处置过程</div>
      <div class="message" @click="handleDirectorInfo">{{ `事故处理主任信息：${isProcessStart ? '车站值班班长' : '-'}`}}</div>
    </header>
    <div class="content flex-1 grid grid-cols-3 content-center justify-items-center">
      <div>处置状态：{{ _.get(statusOptions, status, '-') }}</div>
      <div>处置开始时间：{{ startTime }}</div>
      <div>累计处置时长：{{ duration }}</div>
    </div>
    <director-info-dialog ref="directorDialogRef" />
  </div>
</template>
<script>
import DirectorInfoDialog from './director-info-dialog.vue';
import dayjs from 'dayjs';
import duration from 'dayjs/plugin/duration';
dayjs.extend(duration);
export default {
    mounted () {
        this.$utils.eventBus.on('solveFlow/processInfo', msg => {
            this.startTime = dayjs(msg?.startTime).format('YYYY-MM-DD HH:mm:ss');
            this.status = msg?.status;
            this.updateDuration();
            setInterval(this.updateDuration, 1000);
        });
    },
    components: {
        DirectorInfoDialog
    },
    computed: {
        planSolveParam () {
            return this.$store.state.planSolve.planSolveParam;
        },
        isProcessStart() {
            return this.$store.state.planSolve.isProcessStart;
        },
    },
    data () {
        return {
            status: 0,
            startTime: '--',
            duration: '--',
            statusOptions: { 0: '未开始', 1: '处置中', 2: '结整' }
        };
    },
    methods: {
        handleDirectorInfo () {
            this.$refs.directorDialogRef.openDialog();
        },
        // 计算累计时长
        updateDuration () {
            this.duration = dayjs(this.startTime).isValid() ? dayjs.duration(dayjs().diff(dayjs(this.startTime))).format('HH时mm分ss秒') : '--';
        }
    },
    destroyed () {
        this.$utils.eventBus.remove('solveFlow/processInfo');
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

.message {
  opacity: 0.5;
  font-size: 12px;
  color: #13FFF5;
  text-align: center;
  cursor: pointer;
}

.content {
  >div {
    font-size: 12px;
    color: #FFFFFF;
  }
}
</style>
