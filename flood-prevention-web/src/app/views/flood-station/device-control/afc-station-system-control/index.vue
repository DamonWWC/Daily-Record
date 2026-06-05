<docs>
    # AFC车站系统控制(接口完成)
</docs>
<template>
    <div class="flex flex-col items-start justify-between w-full h-full afc-station-system-control">
        <div class="flex flex-col items-start justify-start flex-1 w-full top-content">
            <div class="mb-3">
                <span class="text-star">*</span>
                <span class="text">请选择需要执行的AFC车站系统控制命令</span>
            </div>
            <el-radio-group v-model="order" fill="#0196A3">
                <el-radio-button v-for="(item,key) in controlInfos" :key="key" :label="item.name">{{item.name}}</el-radio-button>
            </el-radio-group>
        </div>
        <div class="w-full bottom-content">
            <el-button type="primary" @click="handleOrder">指令下发</el-button>
        </div>
    </div>
</template>
<script>
import { PlanSolveApi } from '@api/flood';
const controlInfos = [
    {
        name: '正常服务控制',
        pointCode: 'CYC.AFC.SC.SC-20.dioAFC-StaSerCmd'
    },
    {
        name: '停止服务控制',
        pointCode: '"CYC.AFC.SCSC-20.dioAFC-StoSerCmd'
    },
    {
        name: '紧急放行模式',
        pointCode: 'CYCAFC.SCSC-20.doAFC-URESerCmd'
    }
];
export default {
    data() {
        return {
            order: '正常服务控制',
            controlInfos: controlInfos
        };
    },
    computed: {
        basicData() {
            return JSON.parse(window.basicData);
        }
    },
    methods: {
        async handleOrder() {
            try {
                const params = new Map(
                    [['lineId', this.basicData.LineId], ['mode', 1], ['instruction', new Map([[this.controlInfos.find(item => item.name === this.order).pointCode, 1]])], ['stationId', this.basicData.StationId]]
                );
                console.log('AFC车站系统控制 params:', params);
                // const res = await PlanSolveApi.setDeviceControlCommand(params);
                this.$message.success('控制成功！');
            } catch (error) {
                console.log('车站门禁控制 error:', error);
                this.$message.error('控制失败！');
            }
        }
    }
};
</script>
<style lang="scss" scoped>
.afc-station-system-control {
    // opacity: 0.85;
    // background: #061F35;
    padding: 16px 24px;

    .top-content {
        font-family: PingFangSC-Regular;
        font-size: 14px;
        color: #FFFFFF;
        line-height: 22px;
        font-weight: 400;

        .text-star {
            color: red;
        }
    }

    ::v-deep .el-radio-button__inner {
        background: #1A4868;
    }
}
</style>
