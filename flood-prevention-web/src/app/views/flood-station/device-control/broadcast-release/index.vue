<docs>
  # 广播发布(完成)
</docs>
<template>
  <div class="flex h-full">
    <aside>
      <el-tree style="overflow-y: auto; height:calc(100% - 10px)" :data="treeData" :props="defaultProps"
        @node-click="handleCheckChange" :default-expanded-keys="[1, 2]" :highlight-current="true" node-key="id">
        <template #default="{ node, data }">
          <span class="custom-tree-node">
            <i :class="['iconfont', data.icon]"></i>
            <span>{{ node.label }}</span>
          </span>
        </template>
      </el-tree>
    </aside>
    <div class="flex flex-col flex-1">
      <main class="flex-1">
        <el-form :model="ruleForm" :label-position="labelPosition" :rules="rules" ref="ruleForm" size="small"
          style="width:32rem;">
          <el-form-item label="广播设置" prop="setting">
            <div style="color:#13FFF5">{{ ruleForm.name }}</div>
          </el-form-item>
          <el-form-item label="广播内容" prop="content">
            <el-input style="font-size: .875rem" type="textarea" :readonly="true" :rows="2" placeholder="根据广播信息的ID匹配所对应广播区播放的内容"
              v-model="ruleForm.content">
            </el-input>
          </el-form-item>
          <el-form-item label="播放区域" prop="region">
            <div class="form-item-deviceArea">
              <el-checkbox :indeterminate="isIndeterminate" ref="checkall" v-model="checkAll"
                @change="handleCheckAllChange">全选</el-checkbox>
              <div style="margin: 15px 0;"></div>
              <el-checkbox-group v-model="checkedAreas" @change="handleCheckChange1" style="overflow-y: auto; height:100px">
                <el-checkbox v-for="area in areas" :label="area" :key="area.id">{{ area.label }}</el-checkbox>
              </el-checkbox-group>
            </div>
          </el-form-item>
          <el-form-item label="播放间隔" prop="interval">
            <el-select v-model="ruleForm.interval" placeholder="请选择播放间隔" style="width:15rem;">
              <el-option label="0秒" :value=0></el-option>
              <el-option label="5秒" :value=5></el-option>
              <el-option label="10秒" :value=10></el-option>
              <el-option label="15秒" :value=15></el-option>
              <el-option label="20秒" :value=20></el-option>
              <el-option label="25秒" :value=25></el-option>
              <el-option label="30秒" :value=30></el-option>
              <el-option label="60秒" :value=60></el-option>
            </el-select>
          </el-form-item>
          <el-form-item label="循环播放次数" prop="times">
            <el-input style="width:15rem;" v-model="ruleForm.times" placeholder="请输入播放次数"></el-input>
          </el-form-item>
        </el-form>
      </main>
      <footer>
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
    data() {
        return {
            labelPosition: 'top',
            isIndeterminate: true,
            checkAll: false,
            checkedAreas: [],
            areas: [],
            defaultProps: {
                label: 'name',
                children: 'children'
            },
            ruleForm: {
                name: '请选择广播信息',
                setting: '',
                content: '',
                region: '',
                interval: 0,
                times: 1
            },
            rules: {
                setting: [
                    // { required: true, message: '请选择广播信息', trigger: 'change' }
                ]
            },
            broadCastSequence: [],
            presetBroadcast: [],
            checkedbroadcast: {}

        };
    },
    computed: {
        treeData() {
            return [
                {
                    id: 1,
                    name: '广播序列',
                    icon: 'icon-mulu',
                    children: this.broadCastSequence.map(item => {
                        return {
                            audioIds: item.audioIds,
                            name: item.seqName,
                            icon: 'icon-guangbofabu',
                            type: 1,
                            broadcastId: item.seqId,
                            isLeaf: true
                        };
                    })
                },
                {
                    id: 2,
                    name: '预置广播',
                    icon: 'icon-mulu',
                    children: this.presetBroadcast.map(item => {
                        return {
                            id: item.id,
                            name: item.label,
                            icon: 'icon-guangbofabu',
                            msgDescription: item.msgDescription,
                            broadcastId: item.padmesId,
                            isLeaf: true,
                            type: 0
                        };
                    })
                }
            ];
        },
        basicData() {
            return JSON.parse(window.basicData);
        }
    },
    methods: {
        handleCheckChange(data, checked, indeterminate) {
            console.log(data, checked, indeterminate);
            if (data.isLeaf) {
                if (data.type === 1) {
                    const ids = data.audioIds.split(',').map(item => Number(item));
                    console.log('ids:', ids);
                    let voicecontent = '';
                    ids.forEach(item => {
                        const ps = this.presetBroadcast.find(p => p.padmesId === item);
                        console.log('ps:', ps, item);
                        if (ps) {
                            voicecontent += ps.msgDescription + '\n';
                        }
                    });
                    console.log('voicecontent:', voicecontent);
                    this.ruleForm.content = voicecontent;
                } else if (data.type === 0) {
                    this.ruleForm.content = data.msgDescription;
                }
                this.checkedbroadcast = data;
                this.ruleForm.name = data.name;
            }
        },
        handleCheckChange1() {
            if (this.checkedAreas.length === this.areas.length) {
                this.checkAll = true;
                this.isIndeterminate = false;
            } else {
                this.checkAll = false;
                this.isIndeterminate = true;
            }
        },
        handleCheckAllChange(val) {
            this.checkedAreas = val ? this.areas : [];
            this.isIndeterminate = false;
        },
        async getBroadcastSequenceList() {
            try {
                const res = await PlanSolveApi.getBroadcastSequenceList(this.basicData.LineId, this.basicData.StationId);
                this.broadCastSequence = res;
                console.log('getBroadcastSequenceList res:', res);
            } catch (error) {
                console.log('getBroadcastSequenceList error:', error);
            }
        },
        async getPresetBroadcast() {
            try {
                const res = await PlanSolveApi.getPresetBroadcast(this.basicData.lineNo, this.basicData.stationCode);
                this.presetBroadcast = res;
                console.log('getPresetBroadcast res:', res);
            } catch (error) {
                console.log('getPresetBroadcast error:', error);
            }
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
        async play() {
            console.log(this.checkedAreas, this.checkedbroadcast);
            if (JSON.stringify(this.checkedbroadcast) === '{}'){
              this.$message.warning('请选择广播信息');
              return
            }
            if (!this.checkedAreas.length) {
              this.$message.warning('请选择播放区域');
              return;
            }
            try {
                // const broadcastParam = {
                //     businessId: this.checkedbroadcast.broadcastId,
                //     lineStationAreaList: [
                //         {
                //             lineId: this.basicData.LineId,
                //             stationId: this.basicData.StationId,
                //             pazonIds: this.checkedAreas.map(item => {
                //                 return item.pazoneId;
                //             })
                //         }
                //     ],
                //     content: this.ruleForm.content,
                //     playInterval: this.ruleForm.interval,
                //     playNum: this.ruleForm.times,
                //     types: this.checkedbroadcast.type,
                //     clientId: this.$store.state.clientId,
                //     sessionId: this.$store.state.sessionId

                // };
                // console.log('broadcastParam:', broadcastParam);
                // const res = await PlanSolveApi.playBroadcast2(broadcastParam);
                this.$message.success('操作成功');
            } catch (error) {
                console.log('play error:', error);
            }
        },
        async stop() {
            try {
                // const boradcastParam = {
                //     lineStationAreaList: [
                //         {
                //             lineId: this.basicData.LineId,
                //             stationId: this.basicData.StationId,
                //             pazonIds: this.checkedAreas.map(item => {
                //                 return item.pazoneId;
                //             })
                //         }
                //     ]
                // };
                // const res = await PlanSolveApi.stopBroadcast(boradcastParam);
                this.$message.success('操作成功');
            } catch (error) {
                console.log('stop error:', error);
            }
        }
    },
    mounted() {
        console.log('session:', window.basicData);
        console.log('clientId: ', this.$store.state.clientId);
        this.getBroadcastSequenceList();
        this.getPresetBroadcast();
        this.getPaAreas();
    }
};
</script>

<style lang="scss" scoped>
aside {
  width: 300px;
  border-right: .0625rem solid #0C3E5F;
  padding: 1rem .5rem;

  .el-tree {
    font-size: .875rem;
    color: rgba(255, 255, 255, 0.85);
    line-height: 1.375rem;
    font-weight: 400;

    .custom-tree-node {
      display: flex;
      align-items: center;

      i {
        font-size: .875rem;
        color: #1BB3E8;
      }

      span {
        margin-right: .75rem;
        margin-left: .375rem;
      }
    }

    ::v-deep .el-tree-node__content {
      height: 2rem;
    }

    ::v-deep .is-current {
      >.el-tree-node__content {
        background: rgba(23, 85, 127, 0.5);
      }
    }
  }
}

main {
  margin: 24px 24px 16px 24px;
  .el-form-item--small.el-form-item {
    margin-bottom: 0;
}
}

::v-deep .el-form--label-top .el-form-item__label{
  padding-bottom:0;
}
footer {
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
