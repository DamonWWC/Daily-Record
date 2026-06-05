<docs>
    # 车站设备异常监测
</docs>
<template>
    <div class="device-alarm w-full h-full flex flex-col justify-start items-start">
        <div class="title">车站设备异常监测</div>
        <div class="content flex-1 w-full">
            <div class="linkage-item flex justify-between items-center" v-for="(item, index) in abnormalList" :key="index">
                <div class="linkage-item-msg">({{ item.deviceCode }}){{ item.deviceArea }}</div>
                <div class="flex justify-start items-center">
                    <div class="linkage-item-time mr-3">{{ item.abnormalDesc }}</div>
                </div>
            </div>
        </div>
    </div>
</template>

<script>
import { PlanSolveApi } from '@api/flood';
export default {
    name: 'EventOveview',
    data() {
        return {
            stationCode: null,
            lineCode: null,
            abnormalList: []
        };
    },
    mounted() {
        this.$utils.eventBus.on('solveFlow/currStation', msg => {
            this.stationCode = msg.stationCode || '—';
            this.lineCode = msg.lineNo || '-';
            this.getDeviceAbnormalItem();
        });
    },
    methods: {
        async getDeviceAbnormalItem() {
            try {
                const res = await PlanSolveApi.getDeviceAbnormalItem('SB', { lineCode: this.lineCode, stationCode: this.stationCode });
                this.abnormalList = res;
            } catch (error) {
                console.error(error);
            }
        }
    }
};
</script>

<style lang="scss" scoped>
.device-alarm {
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
