<template>
  <el-dialog title="应急物资申请" class="my-outer-person-assign-apply-box" :visible.sync="dialogVisible" :append-to-body="true">
    <div class="table-wrapper">
      <el-table :data="tableListTodo" height="100%" style="width: 100%;">
        <el-table-column type="index" label="序号" width="80" />
        <el-table-column prop="numbers" label="单号" width="200"></el-table-column>
        <el-table-column prop="goods" label="申请物资"></el-table-column>
        <el-table-column prop="urgent" label="是否加急" width="100" :formatter="x => (x == 1 ? '是' : '否')"></el-table-column>
        <el-table-column prop="applyUserName" label="申请人" width="160"></el-table-column>
        <el-table-column prop="createTime" label="创建时间" width="160"></el-table-column>
        <el-table-column prop="status" label="状态" :formatter="(row) => formatStatus(row.status)"
          width="120"></el-table-column>
        <!-- <el-table-column label="操作" min-width="12%">
          <template slot-scope="scope">
            <div class="table-btn" @click="handleNewApplyClick(scope.row)">
              智能生成调度方案
            </div>
          </template>
        </el-table-column> -->
      </el-table>
    </div>
    <!-- <IntelligentGenerationDialog ref="intelligentGenerationDialogRef" /> -->
  </el-dialog>
</template>
<script>
// import IntelligentGenerationDialog from './goods-intelligent-generation-dialog.vue';
import { PlanSolveApi } from '@api/flood';
export default {
    // components: {
    //     IntelligentGenerationDialog
    // },
    computed: {
        planSolveParam() {
            return this.$store.state.largePassengerFlow.planSolveParam;
        }
    },
    data() {
        return {
            dialogVisible: false,
            tableListTodo: [],
            allocateStatus: [
                {
                    value: 0,
                    label: '待调配'
                },
                {
                    value: 1,
                    label: '已撤销'
                },
                {
                    value: 2,
                    label: '已驳回'
                },
                {
                    value: 3,
                    label: '调配中'
                },
                {
                    value: 4,
                    label: '全部出库'
                },
                {
                    value: 5,
                    label: '已结束'
                }
            ]
        };
    },
    methods: {
        formatStatus(status) {
            return this.allocateStatus?.find(x => x.value === parseInt(status))?.label;
        },
        async waitHandlePage() {
            try {
                const res = await PlanSolveApi.waitHandlePage({ page: 1, pageSize: 1000, planId: this.planSolveParam.planId });
                this.tableListTodo = res.records;
            } catch (error) {
                console.log('error：', error);
            }
        },
        handleNewApplyClick(obj) {
            this.$refs.intelligentGenerationDialogRef.openDialog(obj);
        },
        handleLookClick(obj) {
            this.$refs.persolAssignDetailDialogRef.openDialog(obj);
        },
        handleExportClick(obj) {
            this.$message.success('导出成功');
        },
        openDialog() {
            this.dialogVisible = true;
            this.waitHandlePage();
        },
        closeDialog() {
            this.dialogVisible = false;
        }
    }
};
</script>

<style lang="scss" scoped>
.my-outer-person-assign-apply-box {
  // width: 1280px;
  // height: 800px;
  display: flex;
  flex-direction: column;
  align-items: center;

  .switch-btn-group {
    width: 100%;
    display: flex;
    justify-content: center;

    .btn-item {
      flex-shrink: 0;
      width: 120px;
      height: 32px;

      display: flex;
      justify-content: center;
      align-items: center;

      background-color: #1a4868;
      cursor: pointer;

      font-size: 14px;
      font-family: PingFangSC-Semibold, PingFang SC;
      color: #ffffff;

      &.curr {
        background-color: #4d85ac;
      }
    }
  }

  .table-wrapper {
    // border: 1px solid red;
    width: 100%;
    height: 520px;
    margin-top: 16px;

    .table-btn {
      font-size: 14px;
      font-family: PingFangSC-Regular, PingFang SC;
      font-weight: 400;
      color: #13fff5;
      cursor: pointer;
    }

    .mine-btns {
      display: flex;
    }

    .table-vertical-line {
      font-size: 14px;
      font-family: PingFangSC-Regular, PingFang SC;
      font-weight: 400;
      color: #13fff5;
      margin-left: 3px;
      margin-right: 3px;
      cursor: default;
    }
  }
}

::v-deep .el-dialog {
  width: 1280px;
  height: 800px;

  .el-dialog__body {
    height: 700px;
  }
}
</style>
