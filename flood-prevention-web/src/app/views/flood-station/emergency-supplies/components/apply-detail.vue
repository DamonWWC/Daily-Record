<docs>
    # 应急物资申请详情
</docs>
<template>
    <div class="emergency-supplies-apply-detail w-full h-full flex flex-col justify-start items-start">
        <div class="">
            <el-button @click="handleReturn">
                <i class="iconfont icon-fanye-zuo mr-4" /><span>应急物资申请</span>
            </el-button>
        </div>
        <div class="section-wrapper">
            <div class="section-title">基本信息</div>
            <el-descriptions :column="2" border class-name="section-descriptions">
                <el-descriptions-item label="单号" >
                    <span>{{ applyInfo.numbers }}</span>
                </el-descriptions-item>
                <el-descriptions-item label="申请车站">
                    <span>{{ applyInfo.applyStationName }}</span>
                </el-descriptions-item>
                <el-descriptions-item label="状态">
                    <span>{{ applyInfo.status === 0 ? '待调配' : applyInfo.status === 1 ? '调配中' : applyInfo.status
                        === 2 ? '已撤销' : '已结束' }}</span>
                </el-descriptions-item>
                <el-descriptions-item label="申请原因">
                    <span>{{ applyInfo.reson }}</span>
                </el-descriptions-item>
                <el-descriptions-item label="是否加急">
                    <span>{{ applyInfo.urgent }}</span>
                </el-descriptions-item>
                <el-descriptions-item label="要求到库时间">
                    <span>{{ applyInfo.arrivalTime }}</span>
                </el-descriptions-item>
            </el-descriptions>
        </div>
        <div class="section-wrapper">
            <div class="section-title">物资信息</div>
            <el-table class=" w-full h-full flex-1" :data="suppliesList" height="100%" style="width: 100%;">
                <el-table-column type="index" width="80" label="序号"></el-table-column>
                <el-table-column prop="goods" label="物资规格及型号"></el-table-column>
                <el-table-column prop="code" label="物资编码"></el-table-column>
                <el-table-column prop="unit" label="单位"></el-table-column>
                <el-table-column prop="number" label="申请数量"></el-table-column>
                <el-table-column prop="planList" label="调配方案" width="600">
                </el-table-column>
            </el-table>
        </div>
        <div class="section-wrapper">
            <div class="section-title">审批流程信息</div>
            <div class="section-process-item grid gap-2 grid-cols-6 items-center">
                <div>序号：1</div>
                <div>时间：2023-10-13 13:52:01</div>
                <div>节点名称：起草节点</div>
                <div>操作人：范世平</div>
                <div>操作：提交申请</div>
                <div>处理意见：提交方案</div>
            </div>
            <div class="section-process-item grid gap-2 grid-cols-6 items-center">
                <div>序号：2</div>
                <div>时间：2023-10-13 15:25:53</div>
                <div>节点名称：撰写人</div>
                <div>操作人：钱强</div>
                <div>操作：提交方案</div>
                <div>处理意见：同意</div>
            </div>
        </div>
    </div>
</template>

<script>
const suppliesListMock = [
    {
        numbers: '63256434234212',
        goods: '防护服XL',
        urgent: false,
        applyUserName: '张三',
        createTime: '2023-10-13 10:23:23',
        status: '1',
        handler: '夏春逸',
        code: '434234235',
        unit: '件',
        number: 5,
        planList: '长庆：10、迎宾路口：20、和馨园：15、谢家桥：25、龙峰：10、麓谷体育公园：30'
    },
    {
        numbers: '63256434234212',
        goods: '折叠担架',
        urgent: false,
        applyUserName: '张三',
        createTime: '2023-10-13 10:23:23',
        status: '1',
        handler: '夏春逸',
        code: '434234234',
        unit: '件',
        number: 20,
        planList: '麓谷：10、长庆：10、迎宾路口：20、和馨园：15、谢家桥：25'
    },
    {
        numbers: '63256434234212',
        goods: '执法记录仪',
        urgent: false,
        applyUserName: '张三',
        createTime: '2023-10-13 10:23:23',
        status: '1',
        handler: '夏春逸',
        code: '434234236',
        unit: '件',
        number: 15,
        planList: '省图书新馆：30、和馨园：15、谢家桥：25、龙峰：10、麓谷体育公园：30'
    }
]
export default {
    data() {
        return {
            applyInfo: {
                numbers: 'WFE546457',
                goods: '防护服XL',
                urgent: true,
                applyUserName: '郑钰',
                applyStationName: '谢家桥站',
                reson: '物资紧缺',
                arrivalTime: '2023-10-16 18:12:46',
                createTime: '2023-10-13 10:23:23', updateTime: '2023-10-14 14:12:46',
                status: 1,
                handler: '何春梅',
                code: '434234234',
                unit: '件',
                number: 5,
            },
            suppliesList: suppliesListMock
        }
    },
    methods: {
        handleReturn() {
            this.$router.push('apply')
        }
    },
}
</script>

<style lang="scss" scoped>
.emergency-supplies-apply-detail {
    // opacity: 0.85;
    // background: #061F35;
    padding: 16px 24px;

    .header-button {
        margin-bottom: 16px;
    }
}

.section-wrapper {
    width: 100%;
    // height: 100%;
    display: flex;
    flex-direction: column;
    margin-top: 20px;

    ::v-deep .el-descriptions-item__label.is-bordered-label {
        color: rgba(255,255,255,0.85);
        font-weight: 500;
        background: #06324F;
    }
}

.section-title {
    font-family: PingFangSC-Regular;
    font-size: 16px;
    color: #FFFFFF;
    font-weight: 400;
    margin-bottom: 20px;
}

.section-process-item {
    padding: 0 24px;
    height: 56px;
    line-height: 56px;
    margin-bottom: 16px;
    background-color: #06324F;

    >div {
        display: inline-block;
        // width: 110px;
        font-size: 14px;
        font-weight: 400;
    }
}
</style>