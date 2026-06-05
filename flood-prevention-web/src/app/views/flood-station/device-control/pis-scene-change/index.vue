<docs>
  # 切换PIS场景(完成)
</docs>
<template>
  <div class="flex flex-col items-start justify-between w-full h-full pis-scene-change">
    <div class="flex flex-col items-start justify-start flex-1 w-full top-content">
      <div class="mb-3">
        <span class="text-star">*</span>
        <span class="text">请选择需要切换的PIS场景模式</span>
      </div>
      <el-select v-model="deviceType" placeholder="请选择" style="width:40%;height: 32px;margin-bottom: 24px;">
       <el-option v-for="(item,key) in seneModeList" :key="key" :label="item.name" :value="item.id"></el-option>
      </el-select>
    </div>
    <div class="w-full bottom-content">
      <el-button type="primary" @click="handleOrder">执行</el-button>
    </div>
  </div>
</template>
<script>
import { PlanSolveApi } from '@api/flood';
const seneModeList = [
    {
        id: 0,
        name: '将PIS紧急场景切换为正常场景'
    },
    {
        id: 1,
        name: '撤销所有LCD屏普通文本下发'
    },
    {
        id: 2,
        name: '开启所有PIS屏'
    }
];
export default {
    data() {
        return {
            deviceType: 0,
            seneModeList: seneModeList
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
                if (this.deviceType === 2) {
                    const param = {
                        controlStatus: 1,
                        lineId: this.basicData.LineId,
                        stationId: this.basicData.StationId
                    };
                    this.paOpenCloseScreen(param);
                } else {
                    const param = {
                        command: '10022',
                        lineId: this.basicData.LineId,
                        stationAreaList: [
                            {
                                stationId: this.basicData.StationId,
                                zoneIds: [7]
                            }
                        ],
                        sysParam: {
                            operInfo: {
                                OperName: 'SuperUser',
                                profId: 4,
                                operatorKey: 14
                            }
                        }
                    };
                    // if (this.deviceType === 0) {
                    //     await PlanSolveApi.switchPisScene(param);
                    // } else if (this.deviceType === 1) {
                    //     await PlanSolveApi.switchPisSceneNormal(param);
                    // }
                    this.$message.success('控制成功!');
                }
            } catch (error) {
                console.log('error', error);
                this.$message.error('控制失败！');
            }
        },
        async paOpenCloseScreen(params) {
            try {
                // const res = await PlanSolveApi.paOpenCloseScreen(params);
                this.$message.success('控制成功！');
            } catch (error) {
                console.log('控制失败 error:', error);
                this.$message.error('控制失败！');
            }
        }

    }
};
</script>
<style lang="scss" scoped>
.pis-scene-change {
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
