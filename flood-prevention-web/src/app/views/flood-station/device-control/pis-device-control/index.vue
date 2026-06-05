<docs>
    # PIS设备控制
</docs>
<template>
    <div class="flex flex-col items-center justify-start w-full h-full pis-device-control">
        <div class="top-button">
            <el-radio-group v-model="type" fill="#0196A3">
                <el-radio-button :label=1>LCD</el-radio-button>
                <el-radio-button :label=2>LED</el-radio-button>
            </el-radio-group>
        </div>
        <div class="flex flex-col items-start justify-start flex-1 w-full main-content">
            <MyTable  ref="multipleTable" class="dialog-table" :data="tableData" height="60%" style="width: 100%;" @selection-change="handleSelectionChange">
                <el-table-column type="selection" width="55"></el-table-column>
                <el-table-column type="index" width="80" label="序号"></el-table-column>
                <el-table-column prop="deviceFullCode" label="设备编号"></el-table-column>
                <el-table-column prop="stationName" label="位置"></el-table-column>
                <el-table-column prop="tiiPISDSSname" label="设备名称"></el-table-column>
                <el-table-column prop="tiiPISDSSscreentype" label="设备类型">
                    <template slot-scope="scope">
                        {{ scope.row.tiiPISDSSscreentype===1 ?'LCD':'LED'}}
                    </template>
                </el-table-column>
                <el-table-column prop="tiiPISDSSnetworkStatus" label="设备状态">
                    <template slot-scope="scope">
                        {{ scope.row.status ? '在线' : '离线' }}
                    </template>
                </el-table-column>
                <el-table-column prop="tiiPISDSSscreenStatus" label="屏幕状态">
                    <template slot-scope="scope">
                        {{ scope.row.screenStatus ? '开启' : '关闭' }}
                    </template>
                </el-table-column>
                <el-table-column v-if="type===1" prop="aiiPISDSSvolume" label="音量大小">
                </el-table-column>
            </MyTable>
            <div class="flex items-center justify-between w-full bottom-content">
                <div class="flex items-center bottom-left justoify-start">
                    <div>已选 <span class="text-warning">{{ selectedGoodsData.length}}</span> 项</div>
                    <el-button type="text" @click="selectedCancel">取消选择</el-button>
                </div>
                <div class="bottom-right">
                    <el-button type="primary" @click="handleOrder">开启播控器</el-button>
                    <el-button type="primary" @click="handleOrder">关闭播控器</el-button>
                    <el-button type="primary" @click="handleOrder">重启播控器</el-button>
                    <el-button type="primary" @click="handleOrder">开启屏幕</el-button>
                    <el-button type="primary" @click="handleOrder">关闭屏幕</el-button>
                    <el-button v-if="type==='LCD'" type="primary" @click="handleOrder">开启静音</el-button>
                    <el-button v-if="type==='LCD'" type="primary" @click="handleOrder">关闭静音</el-button>

                </div>
            </div>
        </div>

    </div>
</template>
<script>
import { PlanSolveApi } from '@api/flood';
import { Table as MyTable, TableColumn } from 'element-ui';
const tableDataMock = [
    {
        code: 'CS6_BGZ_LCDO_001',
        place: '朝阳村',
        name: '上行站台LCD播放控制器',
        type: 'LCD',
        status: 1,
        screenStatus: 1,
        volumn: 100
    },
    {
        code: 'CS6_BGZ_LCD2_001',
        place: '朝阳村',
        name: '站厅LCD播放控制器1',
        type: 'LCD',
        status: 0,
        screenStatus: 1,
        volumn: 65
    }

];
const tableDataMock1 = [
    {
        code: 'CS6_BGZ_LCDO_001',
        place: '朝阳村',
        name: '上行站台LCD播放控制器',
        type: 'LCD',
        status: 1,
        screenStatus: 1,
        volumn: 100
    },
    {
        code: 'CS6_BGZ_LCD2_001',
        place: '朝阳村',
        name: '站厅LCD播放控制器1',
        type: 'LCD',
        status: 0,
        screenStatus: 1,
        volumn: 65
    }

];
export default {
    components: {
        MyTable,
        TableColumn
    },
    data() {
        return {
            type: 1,
            order: 1,
            // tableData: tableDataMock,
            selectedGoodsData: [],
            pisDeviceInfo: []
        };
    },
    computed: {
        tableData() {
            return this.pisDeviceInfo.filter(item => item.tiiPISDSSscreentype === this.type);
        },
        basicData() {
            return JSON.parse(window.basicData);
        }
    },
    methods: {
        handleOrder() {
            // 获取当前时间
            // let now = new Date();
            if (!this.selectedGoodsData.length) {
                this.$message.warning('请选择设备');
                return
            }
            this.$message.success('操作成功');
        },
        handleSelectionChange(val) {
            console.log(val);
            this.selectedGoodsData = val;
        },
        selectedCancel() {
            this.$refs.multipleTable.clearSelection();
        },
        async getPisDevice() {
            try {
                console.log('getPisDevice', this.basicData);
                const params = {
                    lineId: this.basicData.LineId,
                    stationId: this.basicData.StationId
                };
                const res = await PlanSolveApi.getPisDevice(params);
                this.pisDeviceInfo = res;
            } catch (error) {
                console.error('getPisDevice error:', error);
            }
        }
    },
    mounted() {
        this.getPisDevice();
    }
};
</script>
<style lang="scss" scoped>
.pis-device-control {
    opacity: 0.85;
    background: #061F35;
    padding: 16px 24px;

    .top-button {
        margin-bottom: 16px;
    }

    .bottom-left {
        opacity: 0.85;
        font-family: PingFangSC-Regular;
        font-size: 14px;
        color: #FFFFFF;
        font-weight: 400;

        >div:nth-child(1) {
            margin-right: 24px;
        }
    }
}
</style>
