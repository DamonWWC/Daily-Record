<docs>
    # AFC设备控制(接口给的数据不完整)
</docs>
<template>
    <div class="flex flex-col items-start justify-between w-full h-full afc-device-control">
        <div class="flex flex-col items-start justify-start flex-1 w-full top-content">
            <div class="mb-3">
                <span class="text-star">*</span>
                <span class="text"> 请选择需要执行AFC设备类型的控制命令</span>
            </div>
            <el-select v-model="deviceType" placeholder="请选择" @change="handleChange" style="width:20%;height: 32px;margin-bottom: 24px;">
                <el-option v-for="(item,key) in afcCtlPanels" :key="key" :label="item.contentName" :value="item.deviceList">
                </el-option>
                <!-- <el-option label="AGM通道闸机"
                    :value="1">
                </el-option> -->
            </el-select>
            <el-radio-group v-model="order" fill="#0196A3">
                <!-- <el-radio-button v-for="(item,key) in controlInfo" :key="key" :label="item.ctrlText">{{ item.ctrlName }}</el-radio-button> -->
                <el-radio-button label="正常服务控制">正常服务控制</el-radio-button>
                <el-radio-button label="停止服务控制">停止服务控制</el-radio-button>
                <el-radio-button label="设置单向进模式">设置单向进模式</el-radio-button>
                <el-radio-button label="设置单向出模式">设置单向出模式</el-radio-button>
                <el-radio-button label="设置双向进出模式">设置双向进出模式</el-radio-button>
            </el-radio-group>
        </div>
        <div class="w-full bottom-content">
            <el-button type="primary" @click="handleOrder">指令下发</el-button>
        </div>
    </div>
</template>
<script>
import { PlanSolveApi } from '@api/flood';
export default {
    data() {
        return {
            afcCtlPanels: [],
            deviceType: '',
            controlInfo: [],
            order: '正常服务控制'
        };
    },
    computed: {
        basicData() {
            return JSON.parse(window.basicData);
        }
    },
    methods: {
        async getAFCControlPanel() {
            try {
                const res = await PlanSolveApi.getAFCControlPanel(this.basicData.LineId, this.basicData.StationId);
                this.afcCtlPanels = res.at(0).children;
                this.deviceType = this.afcCtlPanels.at(0).contentName;
                console.log('获取AFC控制面板成功', res);
            } catch (error) {
                console.log('获取AFC控制面板失败', error);
            }
        },
        handleChange(val) {
            console.log(val);
            // this.controlInfo = val.at(0).expand;
            // this.order = this.controlInfo.at(0).ctrlText;
        },
        async handleOrder() {
            try {
                let instructionSet = new Map();
                this.deviceType.forEach(item => {
                    var exp = item.expand.find(item => item.ctrlText === this.order.ctrlText);
                    instructionSet.set(exp.ctrlName, exp.ctrValue);
                });
                const params = new Map(
                    [['lineId', this.basicData.LineId], ['mode', 1], ['instruction', instructionSet], ['stationId', this.basicData.StationId]]
                );
                console.log('AFC车站系统控制 params:', params);
                // const res = await PlanSolveApi.setDeviceControlCommand(params);
                this.$message.success('控制成功！');
            } catch (error) {
                console.log('车站门禁控制 error:', error);
                this.$message.error('控制失败！');
            }
        }
    },
    mounted() {
        this.getAFCControlPanel();
    }
};
</script>
<style lang="scss" scoped>
.afc-device-control {
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
