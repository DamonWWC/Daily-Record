<template>
    <div class="flex flex-col" style="padding:0 .75rem">
        <header class="flex">
            <div style="padding-right: .75rem;">处置要点</div>
            <el-select v-model="currentStage" placeholder="请选择" size="mini">
                <el-option v-for="(item, index) in stageList" :key="index" :label="item.stageName"
                    :value="item.stageName" />
            </el-select>
        </header>
        <el-table class="flex-1" :data="tableList" height="100%" style="width: 100%;"
            :span-method="params => objectSpanMethod(params, mergeArr, mergeObj)">
            <el-table-column prop="name" label="岗位" width="180"></el-table-column>
            <el-table-column prop="action" label="处置流程"></el-table-column>
            <el-table-column prop="status" label="是否完成" width="180"
                :filters="[{ text: '是', value: 1 }, { text: '否', value: 0 }]" :filter-method="handleFilterDone"
                filter-placement="right">
                <template slot-scope="scope">
                    <el-switch :disabled="!isProcessStart" v-model="scope.row.status"
                        :active-value="1" :inactive-value="0"
                        @change="handleSolvePointDone($event, scope.row)"></el-switch>
                </template>
            </el-table-column>
            <el-table-column prop="completTime" label="完成时间" width="240">
              <template slot-scope="scope">
                {{ scope.row.completTime ? dayjs(scope.row.completTime).format('YYYY-MM-DD HH:mm:ss') : '' }}
              </template>
            </el-table-column>
        </el-table>
    </div>
</template>

<script>
import { PlanSolveApi } from '@api/flood';
import { getSpanArr, objectSpanMethod } from '@/common/utils/tools';
import dayjs from 'dayjs';

export default {
    data () {
        return {
            currentStage: '',
            stageList: [],
            tableList: [],
            mergeObj: {},
            mergeArr: ['name']
        };
    },
    computed: {
        planSolveParam () {
            return this.$store.state.planSolve.planSolveParam;
        },
        isProcessStart () {
            return this.$store.state.planSolve.isProcessStart;
        },
        isReady () {
            return this.$store.state.planSolve.isReady;
        }
    },
    watch: {
        isReady: {
            handler (newV) {
                if (newV) {
                    this.getStageList();
                }
            },
            immediate: true
        },
        currentStage (newV) {
            this.isProcessStart && this.getTableList(newV, this.isProcessStart);
        },
        isProcessStart (newV) {
            this.getTableList(this.currentStage, newV);
        }
    },
    mounted () {
        this.$utils.eventBus.on('solveFlow/processRecord', msg => {
            this.stageList = msg;
            if (this.stageList?.length > 0) {
                this.currentStage = msg[msg.length - 1].stageName;
            }
        });
    },
    methods: {
        dayjs,
        objectSpanMethod,
        // 修改处置要点状态
        async handleSolvePointDone (value, row) {
            const params = {
                action: row.action,
                eventId: this.planSolveParam.eventId,
                name: this.currentStage,
                planId: this.planSolveParam.planId,
                status: 1
            };
            const res = await PlanSolveApi.setPointExeStatus(params);
            this.getTableList(this.currentStage, true);
        },
        handleFilterDone (value, row, column) {
            return row.status === value;
        },
        // 获取处置阶段下拉列表
        async getStageList () {
            const res = await PlanSolveApi.getProcessRecord({ planId: this.planSolveParam.planId });
            this.stageList = res;
            this.currentStage = res[0].stageName;
        },
        // 获取处置要点表格tableList
        async getTableList (filterName, isExecution) {
            const params = {
                eventId: this.planSolveParam.eventId,
                filter: true,
                filterName: filterName,
                isExecution: isExecution,
                planId: this.planSolveParam.planId
            };
            const res = await PlanSolveApi.getProcess(params);
            let data = res.pointList[0]?.opsItemList;
            if (data?.length > 0) {
                this.mergeObj = getSpanArr(data, this.mergeArr);
            }
            this.tableList = data;
        }
    },
    destroyed () {
        this.$utils.eventBus.remove('solveFlow/processRecord');
    }
};
</script>

<style lang="scss" scoped>
header {
    height: 40px;
    line-height: 40px;
    font-size: 14px;
    color: #fff;
}

::v-deep .el-table {

    th,
    td {
        &.el-table__cell {
            height: 40px;
            font-size: 12px;
            padding: 0;
        }
    }

    .el-table__column-filter-trigger {
        margin-left: 10px;

        i {
            color: #fff;
            font-family: iconfont !important;
            /* iconfont图标*/
            font-size: 14px;
            transform: usnet;

            &::before {
                content: '\e8ad';
                /* iconfont图标*/
            }
        }
    }
}
</style>
