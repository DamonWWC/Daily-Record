<template>
    <el-dialog title="水泵故障监测结果" :close-on-click-modal="false" append-to-body @close="$emit('close', false);"
        destroy-on-close :visible.sync="showDialog">
        <div class="flex flex-col content-start h-full">
            <div class="mb-3 place-self-start">监测时间：{{ this.detectTime }}</div>
            <div class="grid grid-cols-2 mb-2 justify-items-center">
                <div class="self-center place-self-start">监测异常项</div>
                <div class="flex flex-row self-center place-self-end flex-inital" style="height:2rem;">
                    <!-- <el-button class="px-3 py-1 mx-1" type="green">一键生成工单</el-button> -->
                    <el-button class="px-3 py-1 mx-1" @click="handleExport">导出检测结果表格</el-button>
                </div>
            </div>
            <el-table max-height="500" :height="'100%'" :data="faultResult" style="width: 100%;margin-top: 1rem;">
                <el-table-column prop="systemName" label="系统所属" width="180px">
                </el-table-column>
                <el-table-column prop="deviceCode" label="设备开关编号" width="150px">
                </el-table-column>
                <el-table-column prop="deviceName" label="设备描述">
                </el-table-column>
                <el-table-column prop="stationName" label="所属车站" width="120px">
                </el-table-column>
                <el-table-column prop="deviceArea" label="设备地点">
                </el-table-column>
                <el-table-column prop="faultDescription" label="告警描述">
                </el-table-column>
            </el-table>
        </div>
    </el-dialog>
</template>
<script>
import { NetFloodAssessmentApi as FloodAssessmentApi } from '@api/flood';
import dayjs from 'dayjs';
import { handleDownload } from '@utils/tools';
export default {
    props: {
        show: {
            type: Boolean,
            required: true,
            default: false
        },
        businessId: {
            type: String,
            default: ''
        }
    },
    methods: {
        async fetchFailureList () {
            try {
                const res = await FloodAssessmentApi.queryFailureList(this.lineId, this.stationId, this.businessId);
                console.log('FailureList:', res.result);
                this.faultResult = res?.result;
                this.detectTime = dayjs(res?.time).format('YYYY年MM月DD日');
            } catch (error) {
                console.log('fetchFailureList Error:', error);
            }
        },
        async handleExport () {
            try {
                const res = await FloodAssessmentApi.downloadFailureListExcel(this.lineId, this.stationId, this.businessId);
                handleDownload(res, `水泵故障检测结果-${dayjs().format('YYYYMMDDHHmmss')}`);
            } catch (error) {
                console.log('WaterPump handleExport Error:', error);
            }
        }
    },
    data () {
        return {
            detectTime: '-',
            showDialog: this.show,
            faultResult: [],
            lineId: this.$route.query.lineId,
            stationId: this.$route.query.stationId

        };
    },
    mounted () {
        this.fetchFailureList();
    }
};
</script>
<style scoped>
::v-deep .el-dialog {
    width: 70vw;
}

::v-deep .el-dialog__body {
    height: 60vh;
}
</style>
