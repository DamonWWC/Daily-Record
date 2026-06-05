<docs>
    # 应急物资申请
</docs>
<template>
    <div class="w-full h-full">
        <div class="flex flex-col items-start justify-start w-full h-full emergency-supplies-config">
            <div class="flex items-center justify-between w-full header-button">
                <el-button type="primary" @click="handleApplyDialog">
                    <i class="mr-4 iconfont icon-zhinengyingyong-tianjia" />
                    <span>物资申请</span>
                </el-button>
                <div class="w-1/5">
                    <el-input v-model="searchForm.name" placeholder="请输入名称搜索" clearable
                        suffix-icon="el-icon-search"></el-input>
                </div>
            </div>
            <el-table class="flex-1" :data="suppliesList" height="100%" style="width: 100%;">
                <el-table-column type="index" width="80" label="序号"></el-table-column>
                <el-table-column prop="numbers" label="单号"></el-table-column>
                <el-table-column prop="goods" label="申请物资"></el-table-column>
                <el-table-column prop="urgent" label="是否加急">
                    <template slot-scope="scope">
                        {{ scope.row.urgent ? `是, ${scope.row.arrivalTime}` : '否' }}
                    </template>
                </el-table-column>
                <el-table-column prop="applyUserName" label="申请人"></el-table-column>
                <el-table-column prop="createTime" label="创建时间"></el-table-column>
                <el-table-column prop="updateTime" label="更新时间"></el-table-column>
                <el-table-column prop="status" label="状态">
                    <template slot-scope="scope">
                        {{ scope.row.status === 0 ? '待调配' : scope.row.status === 1 ? '调配中' : scope.row.status
                            === 2 ? '已撤销' : '已结束' }}
                    </template>
                </el-table-column>
                <el-table-column prop="handler" label="当前处理人"></el-table-column>
                <!-- <el-table-column prop="action" label="操作" width="80">
                    <template slot-scope="scope">
                        <el-button @click="handleView(scope.row)" type="text" size="small">查看</el-button>
                    </template>
                </el-table-column> -->
            </el-table>
            <el-pagination class="self-end" :page-size="size" @current-change="handleCurrentChange" :current-page="current"  layout="prev, pager, next" :total="total">
            </el-pagination>
        </div>
        <el-dialog title="物资申请" :visible.sync="dialogVisible" :append-to-body="true" width="67%" :before-close="beforeClose"
            class="emergency-supplies-config-dialog">
            <div class="dialog-content-title">基本信息</div>
            <el-form :model="dialogForm" :rules="rules" ref="dialogForm" label-width="7.5rem" class="demo-dialogForm"
                label-position="top">
                <el-row>
                    <el-col :span="8">
                        <el-form-item label="单号" prop="numbers" class="mr-4">
                            <el-input v-model="dialogForm.numbers" placeholder="提交时自动生成" readonly></el-input>
                        </el-form-item>
                    </el-col>
                    <el-col :span="8">
                        <el-form-item label="操作人" prop="handler" class="mr-4">
                            <el-input v-model="dialogForm.handler" placeholder="请输入"></el-input>
                        </el-form-item>
                    </el-col>
                    <el-col :span="8">
                        <el-form-item label="日期" prop="time" class="mr-4">
                            <el-date-picker type="datetime" v-model="dialogForm.time" placeholder="选择日期时间"
                                style="width: 100%;"></el-date-picker>
                        </el-form-item>
                    </el-col>
                </el-row>
                <el-row>
                    <el-col :span="8">
                        <el-form-item label="所属车站" prop="station" class="mr-4">
                            <el-input v-model="dialogForm.station" placeholder="请输入"></el-input>
                        </el-form-item>
                    </el-col>
                    <el-col :span="8">
                        <el-form-item label="申请原因" prop="reason" class="mr-4">
                            <el-input v-model="dialogForm.reason" placeholder="请输入"></el-input>
                        </el-form-item>
                    </el-col>
                    <el-col :span="8">
                        <el-form-item label="是否加急" prop="urgent" class="mr-4">
                            <el-select v-model="dialogForm.urgent" placeholder="请选择" style="width:100%">
                                <el-option label="是" :value="1"></el-option>
                                <el-option label="否" :value="0"></el-option>
                            </el-select>
                        </el-form-item>
                    </el-col>
                </el-row>
                <el-form-item label="相关证明">
                    <upload-tool :fileList.sync="fileList" :upload-type="'.pdf'" :max-limit="4" :max-memory="10"
                        :upload-request="uploadRequest" :show-download="false" :show-delete="false" show-place="bottom">
                    </upload-tool>
                </el-form-item>
            </el-form>
            <div class="flex items-center justify-between dialog-content-title">
                <div>物资信息</div>
                <el-button type="primary" @click="handleApplyDialog" class="header-button">
                    <i class="mr-4 iconfont icon-zhinengyingyong-tianjia" />
                    <span>添加</span>
                </el-button>
            </div>
            <el-table class="dialog-table" :data="dialogTableData" height="100%" style="width: 100%;">
                <el-table-column type="selection" width="50"></el-table-column>
                <el-table-column type="index" width="80" label="序号"></el-table-column>
                <el-table-column prop="goods" label="物资规格及型号"></el-table-column>
                <el-table-column prop="code" label="物资编码"></el-table-column>
                <el-table-column prop="unit" label="物资单位"></el-table-column>
                <el-table-column prop="number" label="数量">
                    <template slot-scope="scope">
                        <el-input v-model="scope.row.number" class="mr-4 supplies-input-number"></el-input>
                    </template>
                </el-table-column>
                <el-table-column prop="action" label="操作" width="80">
                    <template slot-scope="scope">
                        <el-button @click="handleDelete(scope.row)" type="text" size="small">移除</el-button>
                    </template>
                </el-table-column>
            </el-table>
            <div slot="footer" class="dialog-footer">
                <el-button @click="handleClose">取 消</el-button>
                <el-button type="primary" @click="handleSubmit">提交</el-button>
            </div>
        </el-dialog>
    </div>
</template>

<script>
import { HjmosDfsApi } from '@api/common';
import { PlanSolveApi } from '@api/flood';
import UploadTool from '@/app/components/common/upload-tool.vue';
const suppliesListMock = [
    {
        numbers: 'WFE546457',
        goods: '防护服XL',
        urgent: true,
        applyUserName: '郑钰',
        createTime: '2023-10-13 10:23:23',
        updateTime: '2023-10-14 14:12:46',
        status: 1,
        handler: '何春梅',
        code: '434234234',
        unit: '件',
        number: 5
    },
    {
        goods: '手持台800M',
        numbers: 'WFE546457',
        urgent: false,
        applyUserName: '郑钰',
        createTime: '2023-10-13 10:23:23',
        updateTime: '2023-10-14 14:12:46',
        status: 1,
        handler: '何春梅',
        code: '434234234',
        unit: '件',
        number: 5
    },
    {
        goods: '固定电话',
        numbers: 'WFE546457',
        urgent: true,
        applyUserName: '吕洪燕',
        createTime: '2023-10-13 10:23:23',
        updateTime: '2023-10-14 14:12:46',
        status: 0,
        handler: '何春梅',
        code: '434234234',
        unit: '件',
        number: 5
    },
    {
        goods: '调度电话',
        numbers: 'WFE546457',
        urgent: false,
        applyUserName: '吕洪燕',
        createTime: '2023-10-13 10:23:23',
        updateTime: '2023-10-14 14:12:46',
        status: 2,
        handler: '何春梅',
        code: '434234234',
        unit: '件',
        number: 5
    },
    {
        goods: '腰挂式扩音器',
        numbers: 'WFE546457',
        urgent: false,
        applyUserName: '李明平',
        createTime: '2023-10-13 10:23:23',
        updateTime: '2023-10-14 14:12:46',
        status: 2,
        handler: '李欣铭',
        code: '434234234',
        unit: '件',
        number: 5
    },
    {
        goods: '执法记录仪',
        numbers: 'WFE546457',
        urgent: false,
        applyUserName: '李明平',
        createTime: '2023-10-13 10:23:23',
        updateTime: '2023-10-14 14:12:46',
        status: 0,
        handler: '李欣铭',
        code: '434234234',
        unit: '件',
        number: 5
    },
    {
        goods: '折叠担架',
        numbers: 'WFE546457',
        urgent: false,
        applyUserName: '李万海',
        createTime: '2023-10-13 10:23:23',
        updateTime: '2023-10-14 14:12:46',
        status: 0,
        handler: '李欣铭',
        code: '434234234',
        unit: '件',
        number: 5
    },
    {
        goods: '医疗箱',
        numbers: 'WFE546457',
        urgent: true,
        applyUserName: '李万海',
        createTime: '2023-10-13 10:23:23',
        updateTime: '2023-10-14 14:12:46',
        status: 3,
        handler: '陈涵',
        code: '434234234',
        unit: '件',
        number: 5
    },
    {
        goods: '铁马',
        numbers: 'WFE546457',
        urgent: true,
        applyUserName: '郑钰',
        createTime: '2023-10-13 10:23:23',
        updateTime: '2023-10-14 14:12:46',
        status: 3,
        handler: '陈涵',
        code: '434234234',
        unit: '件',
        number: 5
    }
];
const dialogTableDataMock = [
    {
        numbers: 'WFE546457',
        goods: '防护服XL',
        urgent: false,
        applyUserName: '张三',
        createTime: '2023-10-13 10:23:23',
        updateTime: '2023-10-14 14:12:46',
        status: 1,
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
];

export default {
    components: {
        UploadTool
    },
    data() {
        return {
            suppliesList: [], // [],
            dialogVisible: false,
            dialogForm: {},
            rules: {},
            dialogTableData: dialogTableDataMock,
            fileList: [], // 图片列表
            searchForm: {
                name: ''
            },
            total: 0,
            size: 10,
            current: 1
        };
    },
    methods: {
        async getApplyMaterial(num) {
            try {
                const res = await PlanSolveApi.getApplyMaterial({ page: num, pageSize: 10 });
                this.suppliesList = res.records;
                this.total = res.total;
                this.current = res.current;
                console.log('getApplyMaterial res：', res);
            } catch (error) {
                console.log('getApplyMaterial error：', error);
            }
        },
        handleCurrentChange(val) {
            this.getApplyMaterial(val);
        },
        handleApplyDialog() {
            this.dialogVisible = true;
        },
        beforeClose() {
            this.dialogVisible = false;
        },
        async uploadRequest(params) {
            const res = await HjmosDfsApi.fileUploaderByStream(params);
            const fileItem = this.fileList.find(item => item.uid === params.get('file').uid);
            let host = process.env.NODE_ENV === 'development' ? '10.51.9.130' : window.location.hostname;
            fileItem.shortUrl = res;
            fileItem.url = `http://${host}:30768/dfs${res}`;
        },
        handleDelete(row) {
            this.dialogTableData = this.dialogTableData.filter(item => item.numbers !== row.numbers);
        },
        handleSubmit() {
            this.$message.success('提交申请成功');
            this.handleClose();
        },
        handleClose() {
            this.dialogForm = {};
            this.dialogVisible = false;
        },
        handleView() {
            // this.$router.push('apply-detail');
        }
    },
    mounted() {
        this.getApplyMaterial(1);
    }
};
</script>

<style lang="scss" scoped>
.emergency-supplies-config {
    // opacity: 0.85;
    // background: #061F35;
    padding: 16px 24px;

    .header-button {
        margin-bottom: 16px;
    }
}
</style>
<style lang="scss">
.emergency-supplies-config-dialog {
    .dialog-content-title {
        font-family: PingFangSC-Regular;
        font-size: 16px;
        color: #FFFFFF;
        font-weight: 400;
        margin-bottom: 20px;
    }

    .dialog-table {
        .supplies-input-number {
            width: 100px;
        }
    }
}
</style>
