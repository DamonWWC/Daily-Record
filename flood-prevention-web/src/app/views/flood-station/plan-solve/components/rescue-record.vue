<docs>
    # 救援记录
</docs>
<template>
    <div class="flex flex-col items-start justify-start w-full h-full rescue-record">
        <div class="title">救援记录</div>
        <div class="flex-1 w-full content">
            <div v-for="item in rescueList" :key="item.rescueType" class="flex items-center justify-between record-item">
                <div class="record-item-text">{{ item.rescueType }}</div>
                <div class="flex items-center justify-end">
                    <div class="record-item-btn">
                        <el-button v-if="!item.contactTime" type="primary" size="mini" @click="handleContact(item)"
                            :disabled="!isProcessStart">已联络</el-button>
                        <div v-else v-html="item.contactTime"></div>
                    </div>
                    <div class="record-item-btn">
                        <el-button v-if="!item.arrivalTime" type="primary" size="mini" @click="handleArrival(item)"
                            :disabled="!isProcessStart">已到达</el-button>
                        <div v-else v-html="item.arrivalTime"></div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script>
import dayjs from 'dayjs';
import { PlanSolveApi } from '@api/flood';

export default {
    data () {
        return {
            rescueList: []
        };
    },
    computed: {
        isProcessStart () {
            return this.$store.state.planSolve.isProcessStart;
        },
        planSolveParam () {
            return this.$store.state.planSolve.planSolveParam;
        }
    },
    methods: {
        async handleArrival (item) {
            try {
                var res = await PlanSolveApi.setRescueArrive({
                    rescueConfigId: item.rescueConfigId,
                    lineCode: this.$route.query.lineCode,
                    stationCode: this.$route.query.stationCode,
                    eventId: this.planSolveParam.eventId
                });
                item.arrivalTime = dayjs().format('YYYY-MM-DD<br>HH:mm:ss');
                this.rescueList = _.clone(this.rescueList);
            } catch (err) {
                console.log('arriva:', err);
            }
        },
        async handleContact (item) {
            try {
                var res = await PlanSolveApi.setRescueContact({
                    rescueConfigId: item.rescueConfigId,
                    lineCode: this.$route.query.lineCode,
                    stationCode: this.$route.query.stationCode,
                    eventId: this.planSolveParam.eventId
                });
                item.contactTime = dayjs().format('YYYY-MM-DD<br>HH:mm:ss');
            } catch (err) {
                console.log('arriva:', err);
            }
        },
        async getRescueList () {
            try {
                var res = await PlanSolveApi.getRescueList({
                    lineCode: this.$route.query.lineCode,
                    stationCode: this.$route.query.stationCode,
                    eventId: this.planSolveParam.eventId
                });
                this.rescueList = res;
                console.log('this.rescueList: ', this.rescueList);
            } catch (err) {
                console.log('getRescueList:', err);
            }
        }
    },
    mounted () {
        this.getRescueList();
    },
    onUnmounted () {
        this.rescueList = [];
    }
};
</script>

<style lang="scss" scoped>
.rescue-record {
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

        .record-item {
            border-bottom: 1px solid #0C3E5F;
            padding: 8px;

            &-text {
                font-family: PingFangSC-Regular;
                font-size: 12px;
                color: #FFFFFF;
                letter-spacing: 0;
                font-weight: 400;
            }

            &-btn {
                margin-right: 12px;
                width: 80px;
                height: 24px;
                text-align: center;

                div {
                    width: 100%;
                    height: 100%;
                    font-size: 10px;
                    color: #FFFFFF;
                    letter-spacing: 0;
                    text-align: center;
                    line-height: 14px;
                    font-weight: 400;
                }
            }
        }
    }
}
</style>
