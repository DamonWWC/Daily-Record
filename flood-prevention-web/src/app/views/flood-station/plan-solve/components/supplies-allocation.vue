<docs>
    # 物资智能调配
</docs>
<template>
    <div>
        <div class="supplies-allocation w-full h-full flex flex-col justify-start items-start">
            <div class="title w-full flex justify-between items-center">
                <div class="title-left">物资智能调配</div>
                <div class="title-right">{{ `已申请：${1}` }}</div>
            </div>
            <div class="content flex-1 w-full flex justify-center items-center">
                <el-button type="primary" size="mini" @click="handleDialog({ isView: false, isDetail: false })"
                    :disabled="!isProcessStart">{{ btnText }}</el-button>
                <el-button size="mini" @click="handleDialog({ isView: true, isDetail: false })"
                    :disabled="!isProcessStart">记录</el-button>
            </div>
        </div>
        <el-dialog :title="dialogTitle" :visible.sync="dialogVisible" :append-to-body="true" width="72%"
            class="supplies-dialog">
            <div class="supplies-dialog-body w-full flex" v-if="!isView">
                <div class="supplies-dialog-body-left w-1/2 border border-solid border-primary">
                    <div class="search flex" style="height: 4rem;">
                        <el-input v-model="searchForm.goodsName" placeholder="请输入名称搜索" clearable
                            suffix-icon="el-icon-search" class="search-input" />
                        <el-select v-model="searchForm.type" placeholder="请选择物资类型" clearable>
                            <el-option v-for="item in goodsTypeOps" :key="item.id" :label="item.name" :value="item.id" />
                        </el-select>
                    </div>
                    <div class="list flex" style="height: calc(100% - 4rem);">
                        <my-table ref="allGoodsDataRef" @selection-change="handleGoodsSelectionChange" :data="allGoodsData"
                            row-key="id" height="100%" style="width: 100%;">
                            <el-table-column type="selection" width="50" reserve-selection></el-table-column>
                            <el-table-column prop="typeName" label="物资类型" width="120"></el-table-column>
                            <el-table-column prop="specifications" label="物资规格及型号"></el-table-column>
                            <el-table-column prop="code" label="物资编码" width="120"></el-table-column>
                            <el-table-column prop="unit" label="单位" width="80"></el-table-column>
                        </my-table>
                    </div>
                </div>
                <div class="supplies-dialog-body-right w-1/2 border border-solid border-primary">
                    <div class="search flex justify-between items-center" style="height: 4rem;;">
                        <div>已选 <span class="text-warning">{{ selectedGoodsData.length || 0 }}</span> 条物资</div>
                    </div>
                    <div class="list flex" style="height: calc(100% - 4rem);">
                        <el-table :data="selectedGoodsData" height="100%" style="width: 100%;">
                            <el-table-column type="index" width="80" label="序号"></el-table-column>
                            <el-table-column prop="specifications" label="物资规格及型号"></el-table-column>
                            <el-table-column prop="code" label="物资编码" width="100"></el-table-column>
                            <el-table-column prop="unit" label="单位" width="60"></el-table-column>
                            <el-table-column prop="number" label="申请数量" width="100">
                                <template slot-scope="scope">
                                    <el-input v-model="scope.row.number" controls-position="right" :min="1" :max="100000"
                                        class="supplies-input-number"></el-input>
                                </template>
                            </el-table-column>
                            <el-table-column prop="action" label="操作" width="80">
                                <template slot-scope="scope">
                                    <el-button @click="handleDelete(scope.row)" type="text" size="small">删除</el-button>
                                </template>
                            </el-table-column>
                        </el-table>
                    </div>
                </div>
            </div>
            <div class="supplies-dialog-body w-full h-full flex flex-col" v-else-if="!isDetail">
                <div class="supplies-dialog-body-head "><span>{{ `我申请的` }}</span></div>
                <el-table class="" :data="dialogTableData" height="100%" style="width: 100%;margin-bottom:1.5rem">
                    <el-table-column type="index" width="50"></el-table-column>
                    <el-table-column prop="numbers" label="单号"></el-table-column>
                    <el-table-column prop="goods" label="申请物资"></el-table-column>
                    <el-table-column prop="urgent" label="是否加急">
                        <template slot-scope="scope">
                            {{ scope.row.urgent ? '是' : '否' }}
                        </template>
                    </el-table-column>
                    <el-table-column prop="applyUserName" label="申请人"></el-table-column>
                    <el-table-column prop="createTime" label="创建时间"></el-table-column>
                    <el-table-column prop="status" label="状态">
                        <template slot-scope="scope">
                            {{ scope.row.status === 0 ? '待调配' : scope.row.status === 1 ? '调配中' : '已结束' }}
                        </template>
                    </el-table-column>
                    <el-table-column prop="handler" label="当前处理人"></el-table-column>
                    <el-table-column prop="action" label="操作" fixed="right" width="100">
                        <template slot-scope="scope">
                            <el-button @click="handleView(scope.row)" type="text" size="small">查看</el-button>
                        </template>
                    </el-table-column>
                </el-table>
            </div>
            <div class="supplies-dialog-body w-full h-full flex flex-col" v-else>
                <div class="supplies-dialog-body-head">
                    <el-button @click="handleReturn()" size="small">
                        <i class="iconfont icon-fanye-zuo search-icon"></i>
                        <span>返回</span>
                    </el-button>
                </div>
                <el-table class="" :data="goodInfoTableData" height="100%" style="width: 100%;">
                    <el-table-column type="index" width="50"></el-table-column>
                    <el-table-column prop="specifications" label="物资规格及型号"></el-table-column>
                    <el-table-column prop="code" label="物资编码"></el-table-column>
                    <el-table-column prop="unit" label="单位"></el-table-column>
                    <el-table-column prop="applyNum" label="申请数量"></el-table-column>
                    <el-table-column prop="planListString" label="调配方案"></el-table-column>
                </el-table>
            </div>
            <div slot="footer" style="height:4rem" v-if="!isView">
                <el-button type="primary" @click="handleApplyGoods">确 定</el-button>
                <el-button @click="dialogVisible = false">取 消</el-button>
            </div>
        </el-dialog>
    </div>
</template>

<script>
import { Table as MyTable, TableColumn } from 'element-ui';
import { PlanSolveApi } from '@api/flood';
export default {
    name: 'SuppliesAllocation',
    components: {
        MyTable, // 单独引入element-ui的table组件; 默认使用的<el-table>是经过ecp-UI封装的，其中的toggleRowSelection()存在勾选项checkbox不显示的问题。
        TableColumn
    },
    data() {
        return {
            isView: false,
            dialogVisible: false,
            allGoodsData: [],
            selectedGoodsData: [],
            dialogTableData: [],
            goodInfoTableData: [],
            searchForm: {
                goodsName: '',
                type: ''
            },
            currSelection: [],
            goodsTypeOps: [],
            isDetail: false,
            btnText: '物资申请'
        };
    },
    watch: {
        searchForm: {
            async handler() {
                await this.getMaterial();

                if (!this.$refs.allGoodsDataRef) return;

                let selectedRows = this.allGoodsData.filter(x => this.selectedGoodsData.some(y => y.id === x.id));
                selectedRows.forEach(row => {
                    this.$refs.allGoodsDataRef.toggleRowSelection(row, true);
                });
            },
            deep: true
        }
    },
    computed: {
        dialogTitle() {
            return this.isView ? '物资调配记录信息' : '物资申请';
        },
        planSolveParam() {
            return this.$store.state.planSolve.planSolveParam;
        },
        isProcessStart() {
            return this.$store.state.planSolve.isProcessStart;
        }
    },
    mounted() {
        this.getGoodsTypeList();
    },
    methods: {
        async getApplyMaterial() {
            try {
                const res = await PlanSolveApi.getApplyMaterial({ page: 1, pageSize: 1000, planId: this.planSolveParam.planId });
                this.dialogTableData = res.records;
            } catch (error) {
                console.log('error：', error);
            }
        },
        handleGoodsSelectionChange(val) {
            this.selectedGoodsData = val;
        },
        async handleApplyGoods() {
            try {
                const res = await PlanSolveApi.applyAllocate({
                    urgent: 1,
                    stationCode: this.$route.query.stationCode,
                    planId: this.planSolveParam.planId,
                    reason: '',
                    stationName: this.$route.query.stationName || '',
                    goodsList: this.selectedGoodsData.map(x => ({ goodsId: x.id, num: parseInt(x.number) }))
                });

                this.dialogVisible = false;
                this.$message.success({
                    message: '申请提交成功',
                    showClose: true
                });
            } catch (error) {
                console.log('error:', error);
            }
        },
        async getMaterial() {
            try {
                const res = await PlanSolveApi.getoverviewPage({ specifications: this.searchForm.goodsName.trim(), type: this.searchForm.type });
                this.allGoodsData = res?.records;
            } catch (error) {
                console.log('error:', error);
            }
        },
        async getGoodsTypeList() {
            try {
                const res = await PlanSolveApi.getGoodsTypeList();
                if (res?.length > 0) {
                    this.searchForm.type = res[0].id;
                    this.goodsTypeOps = res;
                }
            } catch (error) {
                console.log('error:', error);
            }
        },
        async getGoodsInfo(id) {
            try {
                const res = await PlanSolveApi.getGoodsInfo(id);

                const goodsList = res?.goodsDistributeList;
                goodsList.map(x => {
                    x.planListString = x.planList.map(y => { return `${y.stationName}: ${y.num}`; }).join(' ');
                    return x;
                });

                this.goodInfoTableData = res?.goodsDistributeList;
            } catch (error) {
                console.log('error:', error);
            }
        },
        handleDialog({ isView, isDetail }) {
            this.isView = isView;
            this.isDetail = isDetail;

            if (!this.isView) {
                this.getMaterial();
            } else if (!this.isDetail) {
                this.getApplyMaterial();
            }

            this.dialogVisible = true;
        },
        handleView(row) {
            this.goodInfoTableData = [];
            this.isDetail = true;
            this.getGoodsInfo(row.id);
        },
        handleDelete(row) {
            const selectrow = this.allGoodsData.findIndex(item => item.id === row.id);
            this.$nextTick(() => {
                this.$refs.allGoodsDataRef.toggleRowSelection(this.allGoodsData[selectrow]);
            });
        },
        handleReturn() {
            this.isDetail = false;
        }
    }
};
</script>

<style lang="scss" scoped>
.supplies-allocation {
    background: #061F35;
    border-radius: 2px;

    .title {
        padding: 10px 12px;
        font-family: PingFangSC-Regular;
        color: #FFFFFF;
        font-weight: 400;

        &-left {
            font-size: 14px;
        }

        &-right {
            font-size: 12px;
        }
    }

    .content {
        padding: 0 12px;

        ::v-deep .el-button {
            width: 128px;
            height: 24px;
            margin: 0 20px;
        }

    }
}
</style>

<style lang="scss">
.supplies-dialog {

    .el-dialog__body {
        padding: 0 24px;
    }

    .el-dialog__footer {
        margin: 0;
        padding: 0 24px 0 0 !important;
        line-height: 64px;
    }

    &-body {
        height: 600px;
        overflow-y: hidden;

        &-left,
        &-right {
            padding-right: 8px;

            .search {
                padding: 16px 24px;

                &-input,
                &-icon {
                    margin-right: 8px;
                }
            }

            .list {
                padding: 0px 24px;

                .supplies-input-number {
                    width: 50px;
                }
            }
        }

        &-head {
            font-size: 16px;
            color: #FFFFFF;
            line-height: 22px;
            font-weight: 400;
            margin-bottom: 16px;
            display: flex;
            justify-content: space-between;
            align-items: center;

            span {
                padding-bottom: 4px;
            }

            .search-icon {
                margin-right: 8px;
            }
        }
    }
}
</style>
