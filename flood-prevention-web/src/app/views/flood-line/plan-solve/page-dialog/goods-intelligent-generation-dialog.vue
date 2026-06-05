<template>
    <el-dialog
      title="应急物资申请"
      class="my-outer-person-assign-apply-box"
      :visible.sync="dialogVisible"
      :append-to-body="true"
    >
      <div class="top-tools">
        <el-button @click="handleReturn()" size="small">
          <i class="iconfont icon-fanye-zuo search-icon"></i>
          <span>返回</span>
        </el-button>
        <div class="right-btns">
          <el-button @click="deleteForm()" type="primary">撤销</el-button>
          <el-button @click="submitForm()" type="primary">提交</el-button>
        </div>
      </div>
      <div class="top-middle-box">
        <el-descriptions :column="2" border class-name="section-descriptions">
          <el-descriptions-item label="单号">
            <span>{{ formData.number }}</span>
          </el-descriptions-item>
          <el-descriptions-item label="申请车站">
            <span>{{ formData.assignClass }}</span>
          </el-descriptions-item>
          <el-descriptions-item label="状态">{{ formData.status }}</el-descriptions-item>
          <el-descriptions-item label="申请原因">{{ '物资紧缺' }}</el-descriptions-item>
          <el-descriptions-item label="是否加急">
            {{ formData.emergency.split('，')[0] }}
          </el-descriptions-item>
          <el-descriptions-item label="要求到库时间">
            {{ formData.emergency.split('，')[1] }}
          </el-descriptions-item>
        </el-descriptions>
      </div>
      <div class="bottom-middle-box">
        <div class="title">物资信息</div>
        <div class="table-wrapper">
          <el-table :key="key" :data="tableData" height="100%" style="width: 100%;">
            <el-table-column type="index" label="序号" width="80" />
            <el-table-column prop="type" label="物资名称规格及型号" min-width="15%"></el-table-column>
            <el-table-column prop="code" label="物资编码" min-width="15%"></el-table-column>
            <el-table-column prop="unit" label="物资单位" min-width="10%"></el-table-column>
            <el-table-column label="申请数量" min-width="10%">
              <template slot-scope="scope">
                <!-- @click="handleNewApplyClick(scope.row)" -->
                <span v-if="!scope.row.isInput">
                  {{ scope.row.number }}
                </span>
                <el-input
                  @blur="onBlur(scope.row)"
                  @keyup.enter.native="onBlur(scope.row)"
                  v-if="scope.row.isInput"
                  v-model.number="scope.row.number"
                  clearable
                />
              </template>
            </el-table-column>
            <el-table-column label="调配方案" min-width="50%">
              <template slot-scope="scope">
                <!-- @click="handleNewApplyClick(scope.row)" -->
                <div class="plan">
                  <!-- <div class="plan-item" v-for="(item, index) in scope.row.plan" :key="index">
                    <div class="plan-label">{{ item.label + '：' }}</div>
                    <div class="plan-data">{{ item.personArrStr }}</div>
                  </div> -->
                  <span v-if="!scope.row.isInput">
                    {{ scope.row.userInput }}
                  </span>
                  <el-input
                    v-if="scope.row.isInput"
                    v-model="scope.row.userInput"
                    @blur="onBlur(scope.row)"
                    @keyup.enter.native="onBlur(scope.row)"
                    clearable
                  />
                  <div v-if="!scope.row.isInput" class="plus-btn" @click="handlePlus(scope.row)">
                    +
                  </div>
                </div>
              </template>
            </el-table-column>
          </el-table>
        </div>
      </div>
      <div class="bottom-box">
        <div class="title">审批流程记录</div>
        <div class="workflow-wrapper">
          <div class="workflow-row" v-for="(item, index) in workflowTable" :key="index">
            <div class="item-box">
              <div class="label">序号:</div>
              <div class="data">{{ item.number }}</div>
            </div>
            <div class="item-box">
              <div class="label">时间：</div>
              <div class="data">{{ item.time }}</div>
            </div>
            <div class="item-box">
              <div class="label">节点名称：</div>
              <div class="data">{{ item.nodeName }}</div>
            </div>
            <div class="item-box">
              <div class="label">操作人：</div>
              <div class="data">{{ item.person }}</div>
            </div>
            <div class="item-box">
              <div class="label">操作：</div>
              <div class="data">{{ item.control }}</div>
            </div>
            <div class="item-box">
              <div class="label">处理意见：</div>
              <div class="data">{{ item.opinion }}</div>
            </div>
          </div>
        </div>
      </div>
    </el-dialog>
  </template>
<script>
export default {
    data () {
        return {
            key: Math.random(),
            dialogVisible: false,
            formData: {
                number: 'yrsq143533',
                assignClass: '6号线朝阳村',
                emergency: '是，2023-11-25',
                person: '李小鑫',
                newTime: '2023-11-25 16:38:25',
                status: '待调派',
                nowPerson: '袁子云'
            }, // 上方表单信息
            tableData: [
                {
                    type: '防洪沙袋', // 35560
                    code: 35560,
                    unit: '袋',
                    number: 30,
                    plan: [
                        {
                            label: '长庆',
                            personArrStr: '10'
                        },
                        {
                            label: '迎宾路口：',
                            personArrStr: '10'
                        },
                        {
                            label: '向NOCC申请：',
                            personArrStr: '10'
                        }
                    ],
                    userInput: '长庆：10；迎宾路口：10；向NOCC申请：10',
                    isInput: false
                },
                {
                    type: '防洪沙袋',
                    code: 35561,
                    unit: '袋',
                    number: 16,
                    plan: [
                        {
                            label: '麓谷',
                            personArrStr: '10'
                        }
                    ],
                    userInput: '麓谷：10；向NOCC申请：6',
                    isInput: false
                }
            ], // 调派人员表格
            workflowTable: [
                {
                    number: 1,
                    time: '2022-03-12 10:00:28',
                    nodeName: '起草节点',
                    person: '范世平',
                    control: '提交申请',
                    opinion: '提交方案'
                }
            ] // 审批流程记录表格
        };
    },
    methods: {
        openDialog (obj) {
            // obj 为传来的父级表格当前row对象
            // console.log(obj);
            this.formData.number = obj.number;
            this.formData.assignClass = obj.assignClass;
            this.formData.emergency = obj.emergency;
            this.formData.person = obj.person;
            this.formData.newTime = obj.newTime;
            this.formData.status = obj.status;
            this.formData.nowPerson = obj.nowPerson;

            // 将下方工作流的申请人 改为当前处理人 时间改为当前时间
            this.workflowTable[0].person = obj.nowPerson;
            this.workflowTable[0].time = obj.newTime;

            this.dialogVisible = true;
        },
        closeDialog () {
            this.dialogVisible = false;
        },
        deleteForm () {
            // 撤销
            this.dialogVisible = false;
        },
        submitForm () {
            // 提交
            this.$message.success('提交申请成功');
            this.$utils.eventBus.emit('supplies/apply', '');
            this.dialogVisible = false;
        },
        handlePlus (row) {
            // 加号按钮
            row.isInput = true;
        },
        onBlur (row) {
            // 输入框失去焦点
            row.isInput = false;
            this.updateTable();
        },
        // 更新表格
        updateTable () {
            this.key = Math.random();
        },
        handleReturn () {
            // 返回按钮
            this.dialogVisible = false;
        }
    },
    mounted () {}
};
</script>
  <style lang="scss" scoped>
    .my-outer-person-assign-apply-box {
      // width: 1280px;
      // height: 800px;
      display: flex;
      flex-direction: column;
      align-items: center;
      .top-tools {
        display: flex;
        justify-content: space-between;
        margin-bottom: 16px;
      }
      .top-middle-box {
      }
      .bottom-middle-box {
        //   border: 1px solid red;
        .table-wrapper {
          width: 100%;
          height: 180px;
        }
      }
      .bottom-box {
        //   border: 1px solid red;
        .workflow-wrapper {
          height: 150px;
          overflow-y: auto;
          .workflow-row {
            display: flex;
            align-items: center;
            width: 100%;
            height: 56px;
            background: #0b3c5d;
            padding-left: 24px;
            margin-bottom: 24px;
            .item-box {
              display: flex;
              align-items: center;
              margin-right: 45px;

              font-size: 14px;
              font-family: PingFangSC-Regular, PingFang SC;
              font-weight: 400;
              color: #ffffff;
              .label {
              }
              .data {
              }
            }
          }
        }
      }
    }
    .search-icon {
      margin-right: 8px;
    }
    ::v-deep .el-dialog {
      width: 1280px;
      height: 800px;

      .el-dialog__body {
        height: 700px;
      }
      .el-descriptions-item__label.is-bordered-label {
        background-color: #0b3c5d;
        font-size: 14px;
        font-family: PingFangSC-Medium, PingFang SC;
        font-weight: 500;
        color: rgba(255, 255, 255, 0.85);
      }
      .plan {
        display: flex;
        align-items: center;
        .plan-item {
          display: flex;
          align-items: center;
          margin-right: 12px;
          .plan-label {
            font-size: 14px;
            font-family: PingFangSC-Regular, PingFang SC;
            font-weight: 400;
            color: #ffffff;
            opacity: 0.65;
          }
          .plan-data {
            font-size: 14px;
            font-family: PingFangSC-Regular, PingFang SC;
            font-weight: 400;
            color: #ffffff;
          }
        }
        .plus-btn {
          margin-left: 4px;
          font-size: 22px;
          cursor: pointer;
        }
      }
    }
    .title {
      font-size: 16px;
      font-family: PingFangSC-Regular, PingFang SC;
      font-weight: 400;
      color: #ffffff;
      margin-bottom: 18px;
      margin-top: 18px;
    }
  </style>
