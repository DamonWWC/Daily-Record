<docs>
    # 物资智能调配
</docs>
<template>
    <div>
        <div class="supplies-allocation w-full h-full flex flex-col justify-start items-start">
            <div class="title w-full flex justify-between items-center">
                <div class="title-left">物资智能调配</div>
                <div class="title-right">{{ `待调配：${1}` }}</div>
            </div>
            <div class="content flex-1 w-full flex justify-center items-center">
                <el-button type="primary" size="mini" @click="handleDialog({ isView: false, isDetail: false })"
                    :disabled="!isProcessStart">{{ btnText }}</el-button>
                <el-button size="mini" @click="handleDialog({ isView: true, isDetail: false })"
                    :disabled="!isProcessStart">记录</el-button>
            </div>
        </div>
        <el-dialog :title="dialogTitle" :visible.sync="dialogVisible" :append-to-body="true" width="55%"
            :before-close="beforeClose" class="supplies-dialog">
            <div class="supplies-dialog-body w-full h-full flex" v-if="!isView && !isAllocation">
                <div class="supplies-dialog-body-left w-full flex flex-col">
                    <div class="list flex-1 flex">
                        <el-table ref="allGoodsDataRef" class="" :data="allGoodsData" height="100%" style="width: 100%;">
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
                                    {{ scope.row.status === 0 ? '待调配' : scope.row.status === 1 ? '调配中' : scope.row.status
                                        === 2 ? '已撤销' : '已结束' }}
                                </template>
                            </el-table-column>
                            <el-table-column prop="action" label="操作">
                                <template slot-scope="scope">
                                    <el-button @click="handleAllocate(scope.row)" type="text"
                                        size="small">智能生成调度方案</el-button>
                                </template>
                            </el-table-column>
                        </el-table>
                    </div>
                </div>
            </div>
            <div class="supplies-dialog-body w-full h-full flex flex-col" v-else-if="isAllocation">
                <div class="supplies-dialog-body-head">
                    <el-button @click="handleAllocationReturn()" size="small">
                        <i class="iconfont icon-fanye-zuo search-icon"></i>
                        <span>返回</span></el-button>
                    <el-button @click="handleAllocationConfirm()" type="primary" size="small">
                        <i class="iconfont icon-tuichuxitong search-icon"></i>
                        <span>确认调度方案</span>
                    </el-button>
                </div>
                <el-table class="" :data="dialogTableData" height="100%" style="width: 100%;">
                    <el-table-column type="index" width="50"></el-table-column>
                    <el-table-column prop="goods" label="物资规格及型号"></el-table-column>
                    <el-table-column prop="code" label="物资编码"></el-table-column>
                    <el-table-column prop="unit" label="单位"></el-table-column>
                    <el-table-column prop="number" label="申请数量"></el-table-column>
                    <el-table-column prop="planList" label="调配方案" width="300">
                        <template slot-scope="scope">
                            <div class="flex justify-start items-center">
                                <div class="flex justify-start items-center" v-for="item in scope.row.planList"
                                    :key="item.name">
                                    <div style="white-space: nowrap;">{{ `${item.name}：` }}</div>
                                    <el-input v-model="item.number" controls-position="right" @change="handleApplyNumber"
                                        class="supplies-input-number mr-4"></el-input>
                                </div>
                                <i class="iconfont icon-zhinengyingyong-tianjia" />
                            </div>
                        </template>
                    </el-table-column>
                </el-table>
            </div>
            <div class="supplies-dialog-body w-full h-full flex flex-col" v-else-if="!isDetail">
                <div class="supplies-dialog-body-head "><span>{{ `我申请的` }}</span></div>
                <el-table class="" :data="dialogTableData" height="100%" style="width: 100%;">
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
                            {{ scope.row.status === 0 ? '待调配' : scope.row.status === 1 ? '调配中' : scope.row.status === 2 ?
                                '已撤销' : '已结束' }}
                        </template>
                    </el-table-column>
                    <el-table-column prop="handler" label="当前处理人"></el-table-column>
                    <el-table-column prop="action" label="操作" fixed="right" width="100">
                        <template slot-scope="scope">
                            <el-button @click="handleView(scope.row)" type="text" size="small">查看</el-button>
                            <el-button @click="handleExport(scope.row)" type="text" size="small">导出</el-button>
                        </template>
                    </el-table-column>
                </el-table>
            </div>
            <div class="supplies-dialog-body w-full h-full flex flex-col" v-else>
                <div class="supplies-dialog-body-head">
                    <el-button @click="handleReturn()" size="small">
                        <i class="iconfont icon-fanye-zuo search-icon"></i>
                        <span>返回</span></el-button>
                    <el-button @click="handleExport(dialogTableData)" type="primary" size="small">
                        <i class="iconfont icon-tuichuxitong search-icon"></i>
                        <span>导出</span>
                    </el-button>
                </div>
                <el-table class="" :data="dialogTableData" height="100%" style="width: 100%;">
                    <el-table-column type="index" width="50"></el-table-column>
                    <el-table-column prop="goods" label="物资规格及型号"></el-table-column>
                    <el-table-column prop="code" label="物资编码"></el-table-column>
                    <el-table-column prop="unit" label="单位"></el-table-column>
                    <el-table-column prop="number" label="申请数量"></el-table-column>
                    <el-table-column prop="" label="调配方案">{{ '--' }}
                    </el-table-column>
                </el-table>
            </div>
        </el-dialog>
    </div>
</template>

<script>
const allGoodsDataMock = [
    {
        numbers: '202110121678',
        goods: '防护服XL',
        urgent: false,
        applyUserName: '张三',
        createTime: '2023-10-13 10:23:23',
        status: 1,
        handler: '夏春逸',
        code: '434234234',
        unit: '件',
        number: 5,
        planList: '--'
    },
    {
        numbers: '202110121000',
        goods: '口罩成人',
        urgent: true,
        applyUserName: '李小鑫',
        createTime: '2023-10-13 10:23:23',
        status: 0,
        planList: '--'
    },
    {
        numbers: '202110121032',
        goods: '沙袋50斤',
        urgent: true,
        applyUserName: '叶达龙',
        createTime: '2023-10-13 10:23:23',
        status: 3,
        planList: '--'
    },
    {
        numbers: '202110121004',
        goods: '氧气瓶500ml',
        urgent: false,
        applyUserName: '曹伟',
        createTime: '2023-10-13 10:23:23',
        status: 2,
        handler: '夏春逸',
        code: '434234234',
        unit: '件',
        number: 5,
        planList: '--'
    },
];
const selectedGoodsDataMock = [
    {
        type: '应急物资',
        goods: '防护服XL',
        code: '434234234',
        unit: '件',
        number: 5
    },
    {
        type: '应急物资',
        goods: '手持800M',
        code: '434234212',
        unit: '部',
        number: 8
    },
]
const goodsTypeMock = [
    {
        id: 1,
        name: '应急物资'
    },
    {
        id: 2,
        name: '通信照明'
    },
    {
        id: 3,
        name: '伤员急救'
    },
    {
        id: 4,
        name: '防护用品'
    },
]
const dialogTableDataMock = [
    {
        numbers: '63256434234212',
        goods: '防护服XL',
        urgent: false,
        applyUserName: '张三',
        createTime: '2023-10-13 10:23:23',
        status: '1',
        handler: '夏春逸',
        code: '434234234',
        unit: '件',
        number: 5,
        number1: 2,
        number2: 3,
        planList: [
            {
                name: '长庆',
                number: 3
            },
            {
                name: '迎宾路口',
                number: 2
            }
        ]
    }
]
export default {
    name: 'SuppliesAllocation',
    props: {
    },
    data() {
        return {
            isView: false,
            dialogVisible: false,
            allGoodsData: allGoodsDataMock, // [],
            selectedGoodsData: selectedGoodsDataMock, // [],
            dialogTableData: dialogTableDataMock, // [],
            searchForm: {
                goodsName: '',
                type: 1
            },
            goodsTypeOps: goodsTypeMock,
            isDetail: false,
            btnText: '物资调配',
            isAllocation: false
        };
    },
    computed: {
        dialogTitle() {
            return this.isView ? '物资调配记录信息' : '物资申请';
        },
        isProcessStart() {
            return this.$store.state.planSolve.isProcessStart;
        },
        // btnText() {
        //     if (!this.isProcessStart) {
        //         return '待检索短缺物资'
        //     }
        //     let text = '自动检索短缺物资中...'
        //     setInterval(() => {text = '一键申请短缺物资(3)'}, 5 * 1000)
        //     return text;
        // }
    },
    mounted() {
        // setInterval(() => { this.btnText = '一键申请短缺物资(3)' }, 5 * 1000);

    },
    methods: {
        handleDialog({ isView, isDetail }) {
            this.isView = isView;
            this.isDetail = isDetail
            this.dialogVisible = true;
        },
        handleView(row) {
            console.log('row', row);
            this.isDetail = true
        },
        handleExport(row) {
            console.log('row', row);
            this.$confirm('确定导出吗?', '提示', {
                confirmButtonText: '确定',
                cancelButtonText: '取消',
                type: 'warning'
            })
                .then(() => {
                    // this.downloadFile(`/dfs/${url}`, name);
                    this.$message.success('导出成功');
                })
                .catch();
        },
        downloadFile(url, name) {
            var x = new XMLHttpRequest();
            x.open('GET', url, true);
            x.responseType = 'blob';
            x.onload = function () {
                var url = window.URL.createObjectURL(x.response);
                var a = document.createElement('a');
                a.href = url;
                a.download = name;
                a.click();
            };
            x.send();
        },
        handleAllocate(row) {
            this.isAllocation = true
        },
        handleAllocationReturn() {
            this.isAllocation = false
        },
        handleAllocationConfirm() {
            this.isAllocation = false
            this.dialogVisible = false
        },
        handleApplyNumber() {

        },
        handleReturn() {
            this.isDetail = false
        },
        beforeClose() {
            this.isAllocation = false
            this.dialogVisible = false
        }
    }
};
</script>

<style lang="scss" scoped>
.supplies-allocation {
    // width: 360px;
    // height: 348px;
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
    font-family: PingFangSC-Regular;

    &-body {

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
            }
        }

        &-head {
            // text-white text-base font-normal mb-6
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

        ::v-deep .el-input {
            width: 80px !important;

        }
    }
}
</style>