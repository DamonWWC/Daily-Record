<template>
    <div class="h-full" style="background-color:rgb(5,15,25);padding:1.5rem">
        <div class="flex flex-row h-full">
            <main class="wrapper corner-box flex flex-col flex-1" style="margin-right: 1.5rem;">
                <header>
                    <div class="title">水淹评估 - {{ stationName }}</div>
                    <img src="@assets/images/main-header-underline.png" alt="" />
                    <nav class="nav-bar flex flex-col items-center justify-center">
                        <div class="flex flex-wrap">
                            <div v-for="(value, key) in tabs" :key="key"
                                :class="['active-tab', currentTab == key ? 'active' : '']" @click="currentTab = key">
                                {{ value }}
                            </div>
                        </div>
                    </nav>
                </header>
                <section v-show="currentTab == 1">
                    <iframe :src="url" width="100%" height="100%" allowtransparency="true" frameBorder="0"></iframe>
                </section>
                <section v-show="currentTab == 2" class="flex flex-col">
                    <div class="flex items-center justify-between" style="height: 4rem;">
                        <div style="font-size: 1rem;">水泵运行异常项</div>
                        <div>
                            <!-- <el-button class="iconfont icon-shengchenggongdan" type="primary">一键生成工单</el-button> -->
                            <!-- <el-button class="iconfont icon-zhinengyingyong-daochu"
                                @click="handleDownload">导出异常项表格</el-button> -->
                        </div>
                    </div>
                    <el-table class="flex-1" :data="tableList" height="100%" style="width: 100%;">
                        <el-table-column type="index" label="序号" width="100" />
                        <el-table-column prop="deviceCode" label="设备开关编号" width="180"></el-table-column>
                        <el-table-column prop="deviceName" label="设备描述"></el-table-column>
                        <el-table-column prop="stationName" label="所属车站" width="180"></el-table-column>
                        <el-table-column prop="deviceArea" label="设备地点"></el-table-column>
                        <el-table-column prop="abnormalDesc" label="告警描述"></el-table-column>
                    </el-table>
                </section>
            </main>
            <aside class="flex flex-col gap-y-3" style="width:19.23%;min-width: 22.5rem;">
                <div class="grid grid-cols-2 gap-x-1" style="height: 4rem;">
                    <el-button class="blue-img-button" @click="handleVideoLinkage">视频联动</el-button>
                    <el-button class="blue-img-button" @click="handleAssessment">一键评估</el-button>
                </div>
                <div style="font-size: .75rem;color: rgba(255,255,255,0.85);text-align: center;">
                    近一次评估时间:{{ assessmentTime }}
                </div>
                <div class="widgets flex-1">
                    <WeatherAssessment></WeatherAssessment>
                    <station-gate-flood-estimate></station-gate-flood-estimate>
                    <RiskAssessment></RiskAssessment>
                    <water-pump-fault-detection></water-pump-fault-detection>
                </div>
            </aside>
        </div>

        <VideoDialog ref="videoDialogRef" />
    </div>
</template>

<script>
import WeatherAssessment from './components/weather-assessment.vue';
import { NetFloodAssessmentApi as FloodAssessmentApi } from '@api/flood';
import { Message } from '@ecp/ecp-ui';
import RiskAssessment from './components/risk-assessment.vue';
import StationGateFloodEstimate from './components/StationGateFloodEstimate.vue';
import WaterPumpFaultDetection from './components/WaterPumpFaultDetection.vue';
import VideoDialog from './components/video-dialog.vue';
import MqttMixin from '@/app/mixins/MqttMixin';
import dayjs from 'dayjs';
export default {
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
            currentTab: 1,
            tabs: { 1: '水泵监视', 2: '水泵运行异常项' },
            url: '',
            stationName: this.$route.query.stationName,
            assessmentTime: '',
            tableList: [],
            lineId: this.$route.query.lineId,
            stationId: this.$route.query.stationId
        };
    },
    watch: {
        async currentTab (newValue) {
            if (parseInt(newValue) === 2) {
                await this.fetchStationStatus();
            }
        }
    },
    mounted () {
        this.fetchConfiguration();
    },
    methods: {
        onResponse (res) {
            let msg = JSON.parse(`${res?.message}`);
            this.assessmentTime = dayjs(msg?.businessTime).format('YYYY/MM/DD HH:mm:ss');
        },
        handleVideoLinkage () {
            this.$refs.videoDialogRef.openDialog();
        },
        async handleAssessment () {
            try {
                await FloodAssessmentApi.setAssessment(this.lineId, this.stationId);
                Message.success({
                    message: '评估成功',
                    showClose: true
                });
            } catch (error) {
                console.log('handleAssessment Error:', error);
            }
        },
        async fetchStationStatus () {
            try {
                let result = await FloodAssessmentApi.queryStationStatus(this.lineId, this.stationId);
                this.tableList = result;
            } catch (error) {
                console.log('fetchStationStatus Error:', error);
            }
        },
        async fetchConfiguration () {
            try {
                let result = await FloodAssessmentApi.queryConfiguration(this.$route.query.lineId, this.$route.query.stationId);
                this.url = '/hjmos-wia' + result?.cftPath;
            } catch (error) {
                console.log('fetchConfiguration Error:', error);
            }
        }
    }
};
</script>

<style lang="scss" scoped>
@import "@/styles/flood.scss";

.wrapper {
    header {
        min-height: 64px;
        position: relative;

        .title {
            height: 55px;
            line-height: 55px;
            font-size: 18px;
            text-align: center;
        }

        img {
            position: absolute;
            bottom: 14px;
            left: 50%;
            transform: translateX(-50%);
        }
    }

    section {
        margin: 16px;
        height: 100%;
    }
}

.nav-bar {
    height: 64px;
    position: absolute;
    left: 12px;
    top: 0;

    .active-tab {
        width: 146px;
        height: 32px;
        font-weight: 400;
        line-height: 32px;
        display: inline-block;
        padding: 0 8px;
        cursor: pointer;
        opacity: 0.85;
        font-size: 14px;
        color: #FFFFFF;
        text-align: center;
        background-color: #1A4868;

        &.active {
            background-color: #4D85AC;
            color: #fff;
        }
    }
}

iframe {
    border-radius: 0px;
}

.widgets {
    overflow: hidden;

    > :not(:last-child) {
        margin-bottom: 12px;
    }

    > :nth-child(1) {
        height: calc(100% * 216 / 848 - 12px);
    }

    > :nth-child(2) {
        height: calc(100% * 176 / 848 - 12px);
    }

    > :nth-child(3) {
        height: calc(100% * 216 / 848 - 12px);
    }

    > :nth-child(4) {
        height: calc(100% * 240 / 848);
    }
}
</style>
