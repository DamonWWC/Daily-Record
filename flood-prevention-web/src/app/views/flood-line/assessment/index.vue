<template>
    <div class="wrapper">
        <div class="h-full" id="main">
            <div class="flex flex-col gap-1">
                <div class="flex striped-header">
                    <div class="self-center flex-auto pl-3 title">
                        {{ this.$route.query.lineName ? this.$route.query.lineName + '-' : '' }}{{ this.$route.query.stationName }}(水淹评估)
                    </div>
                    <el-button class="self-center mr-3 iconfont icon-guanbi" style="background-color: transparent; border:0"
                        @click="close"></el-button>
                </div>

                <div class="grid grid-cols-3 gap-x-1">
                    <el-button @click="handleWaterPumpMonitor">水泵监视</el-button>
                    <el-button @click="handleVideoLinkage">视频联动</el-button>
                    <el-button @click="handleAssessment">一键评估</el-button>
                </div>
                <div style="font-size: 0.75rem;color: rgba(255, 255, 255, 0.85);text-align: center;">
                    近一次评估时间:{{ assessmentTime }}
                </div>
            </div>
            <WeatherAssessment></WeatherAssessment>
            <station-gate-flood-estimate></station-gate-flood-estimate>
            <RiskAssessment></RiskAssessment>
            <water-pump-fault-detection></water-pump-fault-detection>
        </div>
    </div>
</template>

<script>
import WeatherAssessment from './components/weather-assessment.vue';
import { LineFloodAssessmentApi as FloodAssessmentApi } from '@api/flood';
import { Message } from '@ecp/ecp-ui';
import RiskAssessment from './components/risk-assessment.vue';
import StationGateFloodEstimate from './components/StationGateFloodEstimate.vue';
import WaterPumpFaultDetection from './components/WaterPumpFaultDetection.vue';
import VideoDialog from './components/video-dialog.vue';
import MqttMixin from '@/app/mixins/MqttMixin';
import dayjs from 'dayjs';
import wpf from '@/common/utils/wpf';
export default {
    beforeCreate () {
        window.$viewWidth = 400;
        window.setRemUnit();
    },
    mixins: [MqttMixin],
    components: {
        WeatherAssessment,
        RiskAssessment,
        StationGateFloodEstimate,
        WaterPumpFaultDetection,
        VideoDialog
    },
    data () {
        return {
            loading: false,
            url: '',
            stationName: this.$route.query.stationName,
            assessmentTime: ''
        };
    },
    methods: {
        onResponse (res) {
            let msg = JSON.parse(`${res?.message}`);
            this.assessmentTime = dayjs(msg?.businessTime).format(
                'YYYY/MM/DD HH:mm:ss'
            );
        },
        handleVideoLinkage () {
            wpf.emit({
                command: 'videoLinkage',
                param: {
                    type: 'videoLinkage',
                    data: {
                        lineId: this.$route.query.lineId,
                        stationId: this.$route.query.stationId
                    }
                }
            });
        },
        handleWaterPumpMonitor () {
            wpf.emit({
                command: 'waterPumpMonitor',
                param: {
                    type: 'waterPumpMonitor',
                    data: {
                        lineId: this.$route.query.lineId,
                        stationId: this.$route.query.stationId
                    }
                }
            });
        },
        close () {
            wpf.emit({
                command: 'close'
            });
        },
        async handleAssessment () {
            try {
                await FloodAssessmentApi.setAssessment(this.$route.query.lineId, this.$route.query.stationId);
                Message.success({
                    message: '评估成功',
                    showClose: true
                });
            } catch (error) {
                console.log('handleAssessment Error:', error);
            }
        }
    }
};
</script>

<style lang="scss" scoped>
.wrapper {
    background-color: transparent;
    height: 100%;
    padding: 0px;

    #main {
        width: 100%;
        min-width: 400px;
    }

    #main {
        overflow: hidden;

        > :not(:last-child) {
            margin-bottom: 12px;
        }

        > :nth-child(1) {
            height: calc(100% * 100 / 1032);
            margin-bottom: 0px;
        }

        > :nth-child(2) {
            height: calc(100% * 228 / 1032 - 12px);
        }

        > :nth-child(3) {
            height: calc(100% * 212 / 1032 - 12px);
        }

        > :nth-child(4) {
            height: calc(100% * 228 / 1032 - 12px);
        }

        > :nth-child(5) {
            height: calc(100% * 252 / 1032);
        }
    }
}

.striped-header {
    // &::before {
    //     content: '';
    //     right: 0;
    //     top: 50%;
    //     transform: translateY(50%);
    //     width: 2px;
    //     height: 16px;

    // }
    .title {
        font-size: 14px;
        border-left: 2px solid rgba(0, 123, 182, 1);
        padding-left: 0px;
    }
}
</style>
