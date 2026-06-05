<template>
  <div class="flex h-full">
    <div class="flex flex-col flex-1">
      <main class="flex-1">
        <el-form  :label-position="labelPosition" :rules="rules"  size="small"
          style="width:32rem;">
          <el-form-item label="播放区域" prop="region">
            <div class="form-item-deviceArea">
              <el-checkbox :indeterminate="isIndeterminate" v-model="checkAll"
                @change="handleCheckAllChange">全选</el-checkbox>
              <div style="margin: 15px 0;"></div>
              <el-checkbox-group v-model="checkedAreas" @change="handleCheckChange1" style="overflow-y: auto; height:300px">
                <el-checkbox v-for="area in areas" :label="area" :key="area.id">{{ area.label }}</el-checkbox>
              </el-checkbox-group>
            </div>
          </el-form-item>
        </el-form>
      </main>
      <footer >
          <el-button type="primary" class="iconfont icon-kaishibofang1" @click="play" style="width:8rem;"> 播放</el-button>
          <el-button type="primary" class="iconfont icon-kongzhi_zanting" @click="stop" style="width:8rem;background: #73899A;border:0">
            停止</el-button>
      </footer>
    </div>
  </div>
</template>

<script>
import { PlanSolveApi } from '@api/flood';

export default {
    data () {
        return {
            labelPosition: 'top',
            isIndeterminate: true,
            checkAll: false,
            checkedAreas: [],
            areas: [],
            rules: {
                setting: [
                ]
            }
        };
    },
    computed: {
        basicData() {
            return JSON.parse(window.basicData);
        }
    },
    methods: {
        handleCheckAllChange (val) {
            this.checkedAreas = val ? this.areas : [];
            this.isIndeterminate = false;
        },
        handleCheckChange1(val) {
            let checkedCount = val.length;
            this.checkAll = checkedCount === this.areas.length;
            this.isIndeterminate = checkedCount > 0 && checkedCount < this.areas.length;
        },
        async getPaAreas() {
            try {
                const res = await PlanSolveApi.getPaAreas(this.basicData.lineNo, this.basicData.stationCode);
                this.areas = res;
                console.log('getPaAreas res:', res);
            } catch (error) {
                console.log('getPaAreas error:', error);
            }
        },
        async operate(command) {
            if (!this.checkedAreas.length) {
              this.$message.warning('请选择播放区域');
              return;
            }
            try {
              
                const areaParams = [
                    {
                        lineId: this.basicData.LineId,
                        stationId: this.basicData.StationId,
                        pazonIds: this.checkedAreas.map(item => {
                            return item.pazoneId;
                        })
                    }
                ];
                const sysParam = {
                    loopCount: 1,
                    operInfo: {
                        operName: 'SuperUser',
                        profId: 4,
                        operatorKey: 14
                    },
                    priority: 15,
                    bySidOrProfid: 0
                };
                const params = new Map([['command', command], ['lineStationAreaList', areaParams], ['param', sysParam], ['version', 'v1.0']]);
                // const res = await PlanSolveApi.PlayBroadcast(params);
                this.$message.success('操作成功');
            } catch (error) {
                console.error('error:', error);
            }
        },
        async play() {
            this.operate('10009');
        },
        async stop() {
            this.operate('10010');
        }
    },
    mounted() {
        this.getPaAreas();
    }

};
</script>

<style lang="scss" scoped>
main {
  margin: 24px 24px 16px 24px;
}

footer{
margin: 0 24px;
height: 56px;
}

.form-item-deviceArea {
  border: 1px solid rgba(255, 255, 255, 0.2);
  border-radius: 4px;
  padding: 12px;

  &:hover {
    border-color: #0196A3;
  }
}

::v-deep .el-checkbox {
  display: block;
  margin-bottom: 18px;
}
</style>
