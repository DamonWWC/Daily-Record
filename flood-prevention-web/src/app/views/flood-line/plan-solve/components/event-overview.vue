<docs>
    # 应急事件概况
</docs>
<template>
    <div class="linkage-system w-full h-full flex flex-col justify-start items-start">
        <div class="title">应急事件概况</div>
        <div class="content flex-1 w-full">
            <div class="linkage-item flex flex-col justify-start items-start">
                <div class="linkage-item-msg">{{ `事发车站` }}</div>
                <div class="flex justify-start items-center">
                    <div class="linkage-item-time mr-3">{{ isProcessStart ? eventStation : '--' }}</div>
                </div>
            </div>
            <div class="linkage-item flex flex-col justify-start items-start">
                <div class="linkage-item-msg">{{ `处置阶段` }}</div>
                <div class="flex justify-start items-center">
                    <div class="linkage-item-time mr-3">{{ isProcessStart ? solveStage : '--' }}</div>
                </div>
            </div>
        </div>
    </div>
</template>

<script>
export default {
    name: 'EventOveview',
    computed: {
        isProcessStart() {
            return this.$store.state.planSolve.isProcessStart;
        }
    },
    mounted() {
        this.$utils.eventBus.on('solveFlow/processRecord', msg => {
            if (msg?.length > 0) {
                this.solveStage = msg[msg.length - 1].stageName;
            }
        });
        this.$utils.eventBus.on('solveFlow/currStation', msg => {
            this.eventStation = msg?.stationName || '—';
        });
    },
    data() {
        return {
            eventStation: '—',
            solveStage: '—'
        };
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
