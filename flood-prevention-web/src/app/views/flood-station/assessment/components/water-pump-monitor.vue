<template>
    <div class="h-full">
        <div class="flex flex-col flex-1 h-full wrapper corner-box">
            <nav class="flex flex-col items-center justify-center nav-bar">
                <div class="flex flex-wrap">
                    <div v-for="(value, key) in tabs" :key="key" :class="['active-tab', currentTab == key ? 'active' : '']"
                        @click="currentTab = key">
                        {{ value }}
                    </div>
                </div>
            </nav>
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
        </div>
    </div>
</template>

<script>
import { NetFloodAssessmentApi as FloodAssessmentApi } from '@api/flood';
export default {
    beforeCreate () {
        window.$viewWidth = 1488;
        window.setRemUnit();
    },
    data () {
        return {
            currentTab: 1,
            tabs: { 1: '水泵监视', 2: '水泵运行异常项' },
            url: '',
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
        async fetchStationStatus () {
            try {
                console.log('this.lineId:', this.lineId);
                console.log('this.stationId:', this.stationId);
                let result = await FloodAssessmentApi.queryStationStatus(this.lineId, this.stationId);
                console.log('result:', result);
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
.wrapper {
    section {
        margin: 0;
        height: 100%;
    }
}

.nav-bar {
    height: 64px;
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
</style>
