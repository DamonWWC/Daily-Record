<docs>
  # PIS文本下发
</docs>
<template>
  <div class="flex flex-col items-center justify-start w-full h-full pis-text-delivery">
    <div class="top-button">
      <el-radio-group v-model="type" fill="#0196A3" @change="handleTabChange">
        <el-radio-button label="LCD">LCD</el-radio-button>
        <!-- <el-radio-button label="LED">LED</el-radio-button> -->
        <el-radio-button label="紧急文本下发">紧急文本下发</el-radio-button>
      </el-radio-group>
    </div>
    <div class="flex flex-col items-start justify-between flex-1 w-full main-content">
      <el-form :model="form" :rules="rules" ref="form" label-width="7.5rem" class="w-1/3 demo-form" label-position="top">
        <el-row>
          <el-col :span="12">
            <el-form-item v-if="type === 'LCD'" label="文本类型" prop="textType" class="mr-4">
              <el-select v-model="form.textType" placeholder="请选择" style="width:100%" @change="handlechange">
                <el-option label="文本插播" :value="1001"></el-option>
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item v-if="['LCD', 'LED'].includes(type)" label="显示方式" prop="display" class="mr-4">
              <el-input v-model="form.display"></el-input>
            </el-form-item>
          </el-col>
        </el-row>
        <el-form-item label="文本内容" prop="textContent" class="mr-4">
          <el-input type="textarea" :autosize="{ minRows: 4, maxRows: 6 }" v-model="form.textContent"
            placeholder="最多124字符" maxlength="124" show-word-limit></el-input>
        </el-form-item>
        <el-form-item v-if="['LCD', 'LED'].includes(type)" label="车站设备区域" prop="deviceArea" class="mr-4">
          <div class="form-item-deviceArea">
            <el-checkbox :indeterminate="isIndeterminate" v-model="checkAll"
              @change="handleCheckAllChange">全选</el-checkbox>
            <div style="margin: 15px 0;"></div>
            <el-checkbox-group v-model="checkedAreas" @change="handleCheckedCitiesChange">
              <el-checkbox v-for="area in areas" :label="area" :key="area.zoneId">{{ area.label }}</el-checkbox>
            </el-checkbox-group>
          </div>
        </el-form-item>
        <el-form-item v-else label="播放区域" prop="deviceArea" class="mr-4">
          <div class="form-item-deviceArea">
            <el-checkbox :indeterminate="isIndeterminate1" v-model="checkAll1"
              @change="handleCheckAllChange1">全选</el-checkbox>
            <div style="margin: 15px 0;"></div>
            <el-checkbox-group v-model="checkedAreas1" @change="handleCheckedCitiesChange1">
              <el-checkbox v-for="(area,key) in areas1" :label="area" :key="key">{{ area }}</el-checkbox>
            </el-checkbox-group>
          </div>
        </el-form-item>
        <el-form-item v-if="type === 'LCD'" label="显示时长" prop="duration" class="mr-4">
          <div class="flex items-center justify-start w-1/3 form-item-duration">
            <el-input v-model="form.duration" placeholder="请输入"></el-input>
            <span>分钟</span>
          </div>
        </el-form-item>
        <div class="flex items-center justify-start text-xs form-tips">
          <i class="el-icon-warning text-warning" />
          <span>如需撤销紧急文本，请在“切换PIS显示模式”中操作。</span>
        </div>
      </el-form>
      <div class="flex items-center justify-between w-full bottom-content">
        <el-button type="primary" @click="handleOrder">发布</el-button>
      </div>
    </div>

  </div>
</template>
<script>
import { PlanSolveApi } from '@api/flood';
export default {
    data() {
        return {
            type: 'LCD',
            order: 1,

            selectedGoodsData: 0,
            form: {
                textType: 1001,
                display: '字幕滚动显示',
                textContent: '',
                deviceArea: [],
                duration: 0
            },
            rules: {
                textType: [
                    { required: true, message: '请选择文本类型', trigger: 'change' }
                ],
                deviceArea: [
                    { required: true, validator: this.validateDeviceArea, trigger: 'change' }
                ],
                textContent: [
                    { required: true, message: '请填写文本内容', trigger: 'blur' }
                ]
            },
            checkAll: false,
            checkAll1: false,
            checkedAreas: [],
            checkedAreas1: [],
            areas: [],
            areas1: ['车站全部LED设备', '车站全部LCD设备'],
            isIndeterminate: true,
            isIndeterminate1: true
        };
    },
    computed: {
        basicData() {
            return JSON.parse(window.basicData);
        }
    },
    watch: {
    },
    mounted() {
        this.getPidsAreasNew();
    },
    methods: {
        handlechange(val) {
            this.form.display = val === 1001 ? '字幕滚动显示' : '';
        },
        async getPidsAreasNew() {
            try {
                const res = await PlanSolveApi.getPidsAreasNew({ lineNo: this.basicData.lineNo, stationNo: this.basicData.stationCode });
                this.areas = res;
            } catch (error) {
                console.error('getPidsAreasNew error:', error);
            }
        },
        handleTabChange() {
          this.form.textContent = ''
        },

        handleOrder() {
            console.log(this.form);
            let isValid = true
            this.$refs['form'].validate((valid, obj) => {
              isValid = valid
            })
            if (!isValid) {
                this.$message.warning('请填写必填项')
                return
            }
            if (this.type === 'LCD' && this.form.duration === 0) {
              this.$message.warning('请填写显示时长')
              return
            }
            if (this.type === 'LCD') {
                this.playLCD();
            } else if (this.type === '紧急文本下发') {
                this.playEmergencyText();
            }
        },
        async playLCD() {
            try {
                const pidsTextDistribution = {
                    version: 'v1.0',
                    command: '10021',
                    lineId: this.basicData.LineId,
                    textContent: this.form.textContent,
                    displayDuration: this.form.duration,
                    displayLocation: this.form.display === '全屏显示' ? 1 : 2,
                    pidsAreaList: [
                        {
                            stationId: this.basicData.StationId,
                            zoneIds: this.checkedAreas.map(item => item.zoneId)
                        }
                    ],
                    sysParam: {
                        operInfo: {
                            operName: 'SuperUser',
                            profId: 4,
                            operatorKey: 14
                        }
                    }
                };
                console.log('pidsTextDistribution:', pidsTextDistribution);
                // const res = await PlanSolveApi.distributionPidsText(pidsTextDistribution);
                this.$message.success('操作成功');
            } catch (error) {
                console.error('playLCD error:', error);
            }
        },
        async playEmergencyText() {
            try {
                const pidsTextDistribution = {
                    version: 'v1.0',
                    command: '10021',
                    lineId: this.basicData.LineId,
                    textContent: this.form.textContent,
                    emergency: 1,
                    displayLocation: 1,
                    pidsAreaList: [
                        {
                            stationId: this.basicData.StationId,
                            zoneIds: [7]
                        }
                    ],
                    sysParam: {
                        operInfo: {
                            operName: 'SuperUser',
                            profId: 4,
                            operatorKey: 14
                        }
                    }
                };
                console.log('playEmergencyText:', pidsTextDistribution);
                // const res = await PlanSolveApi.distributionPidsText(pidsTextDistribution);
                this.$message.success('操作成功');
            } catch (error) {
                console.error('playEmergencyText error:', error);
            }
        },
        handleCheckAllChange(val) {
            console.log(val);
            this.checkedAreas = val ? this.areas : [];
            this.isIndeterminate = false;
        },
        handleCheckedCitiesChange(value) {
            let checkedCount = value.length;
            this.checkAll = checkedCount === this.areas.length;
            this.isIndeterminate = checkedCount > 0 && checkedCount < this.areas.length;
        },
        handleCheckAllChange1(val) {
            this.checkedAreas1 = val ? this.areas1 : [];
            this.isIndeterminate1 = false;
        },
        handleCheckedCitiesChange1(value) {
            let checkedCount = value.length;
            this.checkAll1 = checkedCount === this.areas1.length;
            this.isIndeterminate1 = checkedCount > 0 && checkedCount < this.areas1.length;
        },
        validateDeviceArea(rule, value, callback) {
            if (this.checkedAreas.length === 0) {
                callback(new Error('请选择设备区域'));
            }
        }
    }
};
</script>
<style lang="scss" scoped>
.pis-text-delivery {
  opacity: 0.85;
  background: #061F35;
  padding: 16px 24px;

  .form-item-deviceArea {
    border: 1px solid rgba(255, 255, 255, 0.2);
    border-radius: 4px;
    padding: 12px;

    &:hover {
      border-color: #0196A3;
    }
  }

  .form-item-duration {
    >span {
      white-space: nowrap;
      margin-left: 8px;
    }
  }

  .form-tips {
    i {
      font-size: 16px;
    }

    >span {
      margin-left: 4px;
    }
  }

  ::v-deep .el-checkbox {
    display: block;
    margin-bottom: 18px;
  }

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
