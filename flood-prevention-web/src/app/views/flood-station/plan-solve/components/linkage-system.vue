<docs>
    # 联动系统
</docs>
<template>
    <div class="linkage-system w-full h-full flex flex-col justify-start items-start">
        <div class="title">联动系统</div>
        <div class="content flex-1 w-full">
            <div v-for="(item, index) in informationList" :key="index"
                class="linkage-item flex flex-col justify-start items-start">
                <div class="linkage-item-msg">{{ `${item.name} ${isProcessStart ? item.status : '待执行'}` }}</div>
                <div class="flex justify-start items-center">
                    <div class="linkage-item-time mr-3">{{ isProcessStart ?  item.time : '--' }}</div>
                </div>
            </div>
        </div>
    </div>
</template>

<script>
import { PlanSolveApi } from '@api/flood';
import dayjs from 'dayjs';

export default {
    computed: {
        planSolveParam () {
            return this.$store.state.planSolve.planSolveParam;
        },
        isProcessStart() {
            return this.$store.state.planSolve.isProcessStart;
        }
    },
    watch: {
        planSolveParam: {
            handler (newV) {
                if (newV?.planId || newV?.eventId) {
                    this.fetchExecutionLog();
                }
            },
            immediate: true
        }
    },
    data () {
        return {
            statusOptions: { 0: '待执行', 1: '成功', 2: '失败', 3: '未执行' },
            informationList: []
        };
    },
    methods: {
        async fetchExecutionLog () {
            try {
                let params = {};
                if (this.planSolveParam.planId) {
                    params.planId = this.planSolveParam.planId;
                }
                if (this.planSolveParam.eventId) {
                    params.eventId = this.planSolveParam.eventId;
                }

                const res = await PlanSolveApi.getExecutionLog(params);
                this.informationList = res?.map(x => {
                    return {
                        name: x.cpName,
                        time: dayjs(x.exeTime).isValid() ? dayjs(x.exeTime).format('YYYY-MM-DD HH:mmss') : '-',
                        status: _.get(this.statusOptions, x.exeStatus, '-')
                    };
                });
            } catch (error) {
                console.log('Error:', error);
            }
        }
    }
};
</script>

<style lang="scss" scoped>
.linkage-system {
    background: #061F35;
    border-radius: 2px;
    overflow: hidden;

    .title {
        padding: 10px 12px;
        font-family: PingFangSC-Regular;
        font-size: 14px;
        color: #FFFFFF;
        font-weight: 400;
    }

    .content {
        padding: 0 12px;
        overflow: auto;

        .linkage-item {
            border-bottom: 1px solid #0C3E5F;
            padding: 12px 0;

            &-msg {
                font-family: PingFangSC-Regular;
                font-size: 12px;
                color: #FFFFFF;
                font-weight: 400;
            }

            &-time {
                opacity: 0.85;
                font-family: PingFangSC-Regular;
                font-size: 12px;
                color: #FFFFFF;
                font-weight: 400;
            }
        }
    }
}
</style>
