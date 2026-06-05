<docs>
    # 车站口部关闭情况
</docs>
<template>
    <div class="flex flex-col items-start justify-start w-full h-full gateway-usage">
        <div class="title w-full flex justify-between items-center">
            <div class="title-left">车站口部关闭情况</div>
            <div class="title-right" :class="!isProcessStart ? 'isNoStart' : ''" @click="handleGatewayCloseAll">{{ `关闭出入口`
            }}</div>
        </div>
        <div class="flex-1 w-full flex items-start justify-start flex-wrap content">
            <div v-for="item in btnList" :key="item.pointCode" class="gateway-item">
                <el-button :disabled="!isProcessStart" type="primary" size="mini"
                    :class="item.status !== 1 ? 'gatewayClose' : ''" @click="handleBtnClick(item)">{{
                        item.name }}</el-button>
            </div>
        </div>
    </div>
</template>

<script>
import { PlanSolveApi } from '@api/flood';
export default {
    name: 'GatewayUsage',
    mounted() {
        this.$utils.eventBus.on('solveFlow/currStation', msg => {
            this.stationCode = msg?.stationCode;
            this.getExitEntranceClosedStatus();
            this.queryOriginalPoint();

            // 每 10 秒刷新
            setInterval(this.queryOriginalPoint, 10 * 1000);
        });
    },
    data() {
        return {
            // status: 0未定义 1开到位 2关到位 3未定义
            btnList: [],
            stationCode: '0606'
        };
    },
    computed: {
        isProcessStart() {
            return this.$store.state.planSolve.isProcessStart;
        },
        planSolveParam() {
            return this.$store.state.planSolve.planSolveParam;
        }
    },
    methods: {
        async handleBtnClick(item) {
            if (item.status !== 1 || item.status !== 2) return;
            let instructionSet = {};
            instructionSet[item.pointCode] = 3 - item.status;
            this.syncControlCommand(instructionSet);
        },
        async syncControlCommand(instructionSet) {
            try {
                const res = await PlanSolveApi.syncControlCommand({
                    lineId: this.$route.query.lineCode,
                    stationId: this.stationCode,
                    instructionSet
                });

                this.$message.success({
                    message: '发送成功',
                    showClose: true
                });
            } catch (error) {
                console.log('Error:', error);
            }
        },
        async queryOriginalPoint() {
            try {
                let array = Array.from(this.btnList, x => x.pointCode);
                const res = await PlanSolveApi.queryOriginalPoint(array);
                Object.keys(res).forEach(e => {
                    let btn = this.btnList.find(x => x.pointCode === e);
                    btn && (btn.status = res[e]);
                    if (res[e] === 2) {
                        btn.name = `${btn.name}(关)`;
                    }
                });
            } catch (error) {
                console.log('Error:', error);
            }
        },
        async getExitEntranceClosedStatus() {
            try {
                const res = await PlanSolveApi.getExitEntranceClosedStatus({
                    lineId: this.$route.query.lineCode,
                    stationId: this.stationCode
                });
                this.btnList = res?.map(x => ({ id: x.id, name: x.expand.name, pointCode: x.pointCode, status: 0 }));
            } catch (error) {
                console.log('Error:', error);
            }
        },
        handleGatewayCloseAll() {
            if (!this.isProcessStart) return;
            this.btnList.forEach(ele => {
                this.handleBtnClick(ele);
            });
        }
    }
};
</script>

<style lang="scss" scoped>
.gateway-usage {
    background: #061F35;
    border-radius: 2px;
    overflow: hidden;

    .title {
        padding: 10px 12px;
        font-family: PingFangSC-Regular;
        font-size: 14px;
        color: #FFFFFF;
        font-weight: 400;

        &-right {
            font-size: 12px;
            color: #13FFF5;
            text-align: right;
            line-height: 20px;
            font-weight: 400;

            &:hover {
                cursor: pointer;
            }
        }

        .isNoStart {
            opacity: 0.5;
            color: #FFFFFF;

            &:hover {
                cursor: not-allowed;
            }
        }
    }

    .content {
        padding: 0 12px;
        overflow: auto;

        .gateway-item {
            padding: 8px;
            width: 84px;

            ::v-deep .el-button {
                width: 66px;
            }

            &-text {
                font-family: PingFangSC-Regular;
                font-size: 12px;
                color: #FFFFFF;
                letter-spacing: 0;
                font-weight: 400;
            }

            &-btn {
                width: 212px;

                &-item {
                    margin-right: 5px;
                    margin-top: 5px;
                    margin-bottom: 5px;
                    height: 24px;
                    width: 64px;
                    color: #FFFFFF;
                    background-color: #06324F;
                    display: inline-block;

                    >div {
                        font-size: 12px;
                        color: #FFFFFF;
                        text-align: center;
                        line-height: 22px;
                        font-weight: 400;
                    }

                }
            }
        }
    }

    .gatewayClose {
        background: #6D7B8A !important;
        border: none !important;
    }
}
</style>
