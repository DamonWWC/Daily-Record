<docs>
    # 车站门禁控制（完成接口）
</docs>
<template>
    <div class="flex flex-col items-start justify-between w-full h-full station-access-control">
        <div class="flex flex-col items-start justify-start flex-1 w-full top-content">
            <div class="mb-3">
                <span class="text-star">*</span>
                <span class="text"> 请选择需要执行的车站所有门控制命令</span>
            </div>
            <el-radio-group v-model="order" @input="handleChange" fill="#0196A3">
                <el-radio-button label="0" >开门</el-radio-button>
                <el-radio-button label="1" >关门</el-radio-button>
                <el-radio-button label="2" >常开</el-radio-button>
                <el-radio-button label="3" >常闭</el-radio-button>
                <el-radio-button label="4" >正常</el-radio-button>
                <el-radio-button label="5">取消报警</el-radio-button>
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
            order: '0',
            pointCode: 'CYC.ACS.WSD.WSD-1.eioACS-AIDoCoCom'
        };
    },
    computed: {
        basicData() {
            return JSON.parse(window.basicData);
        }
    },
    methods: {
        handleChange(val) {
            console.log(this.order);
        },
        async handleOrder() {
            try {
                const params = new Map(
                    [['lineId', this.basicData.LineId], ['mode', 1], ['instruction', new Map([[this.pointCode, this.order]])], ['stationId', this.basicData.StationId]]
                );
                console.log('车站门禁控制 params:', params);
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
.station-access-control {
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
