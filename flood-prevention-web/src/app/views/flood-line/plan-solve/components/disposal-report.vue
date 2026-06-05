  <template>
    <div style="position: relative; height: 100%;">
        <div class="w-full h-full">
            <header class="flex items-center justify-between">
                <div class="ml-3">{{ reportData.topic }}</div>
                <el-button class="self-center mr-3 iconfont icon-guanbi" style="background-color: transparent; border:0"
                    @click="close"></el-button>
            </header>
            <section class="section-wrapper" style="overflow-y: auto; height: calc(100% - 55px);">
                <div>
                    <div class="flex items-center justify-between mb-5">
                        <div>一、事件经过</div>
                        <div class="flex gap-2">
                            <el-button type="primary" @click="downloadreport">
                                <i class="mr-4 iconfont icon-xiazai" />
                                <span>下载报告</span>
                            </el-button>
                            <el-popover placement="bottom-end" width="200" trigger="hover" v-model="visible">
                                <div class="flex flex-col">
                                    <div class="flex items-center justify-between">
                                        <div>邮箱号</div>
                                        <el-button class=" iconfont icon-guanbi"
                                            style="background-color: transparent; border:0"
                                            @click="visible = false"></el-button>
                                    </div>
                                    <el-input placeholder="请输入邮箱号" v-model="email" type="email"
                                        pattern="[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,}$" required></el-input>
                                    <el-button class="self-end mt-5" type="primary" @click="confirmEmail">确定</el-button>
                                </div>
                                <el-button slot="reference" type="primary" @click="sendEmail">
                                    <i class="mr-4 iconfont icon-youjian" />
                                    <span>发送邮箱</span>
                                </el-button>
                            </el-popover>
                        </div>
                    </div>
                    <div class="event-process-item">
                        <div v-for="( item, key ) in  reportData.eventProcessRecords " :key="key">{{ item.createTime }}
                            &nbsp;&nbsp;&nbsp;&nbsp; {{ item.stageName
                            }}</div>
                    </div>
                </div>
                <div>
                    <div>二、应急预案处置过程分析</div>
                    <div class="process-items">
                        <div>1.应急程序执行情况</div>
                        <el-descriptions :column="2" border class-name="section-descriptions">
                            <el-descriptions-item label="处置开始时间">
                                <span>{{ reportData.disposalAnalysis.disposalProcessInfo.startTime }}</span>
                            </el-descriptions-item>
                            <el-descriptions-item label="处置结束时间">
                                <span>{{ reportData.disposalAnalysis.disposalProcessInfo.endTime }}</span>
                            </el-descriptions-item>
                            <el-descriptions-item label="累计处置时长">
                                <span> {{ reportData.disposalAnalysis.disposalProcessInfo.duration }}</span>
                            </el-descriptions-item>
                            <el-descriptions-item label="处置是否超时">
                                <span>{{ reportData.disposalAnalysis.disposalProcessInfo.timeout }}</span>
                            </el-descriptions-item>
                        </el-descriptions>
                    </div>
                    <div class=" process-items">
                        <div>2.相关应急信息</div>
                        <div>
                            <table style="border:1px solid rgba(23, 85, 127, 0.65); width:100%">
                                <tbody>
                                    <tr style="height:55px ; "
                                        v-for="(item, key) in reportData.disposalAnalysis.disposalRelated" :key="key">
                                        <td style=" line-height: 55px;  padding-left: 20px;">{{ key + 1 }}、{{ item.name }}
                                        </td>
                                        <td v-for="(  i, k  ) in   item.list" :key="k">{{ i.topic }}</td>
                                    </tr>
                                </tbody>
                            </table>
                        </div>
                    </div>
                    <div class="process-items">
                        <div>3.预案执行情况</div>
                        <el-table border :data="reportData.disposalAnalysis.planProcessList"
                            style="width: 100%;min-height: 500px;">
                            <el-table-column prop="stageName" label="处置阶段"></el-table-column>
                            <el-table-column prop="name" label="岗位"></el-table-column>
                            <el-table-column prop="action" width="400" label="操作事项"></el-table-column>
                            <el-table-column prop="status" label="执行情况">
                                <template slot-scope="scope">
                                    {{ scope.row.status === 0 ? '未完成' : '已完成' }}
                                </template>
                            </el-table-column>
                            <el-table-column prop="completTime" label="完成时间">
                                <template slot-scope="scope">
                                    {{ scope.row.completTime === '' ? '-' : scope.row.completTime }}
                                </template>
                            </el-table-column>
                        </el-table>
                    </div>
                    <div class="process-items">
                        <div>4.处置要点完成分析</div>
                        <div class="flex items-center pl-5"
                            style="border: 1px solid rgba(23, 85, 127, 0.65);min-height: 55px;">
                            {{ reportData.disposalAnalysis.processResult }}</div>
                    </div>
                    <div class="process-items">
                        <div>5.人员到岗记录</div>
                        <div class="event-process-item">
                            <div class="flex items-center justify-between pl-5 pr-5"
                                v-for="(  item, key  ) in   reportData.disposalAnalysis.personArriveList  " :key="key">
                                <div>{{ item.jobName }}</div>
                                <div>{{ item.personDatas.map(item => item.personName).join('、') }}</div>
                                <div>{{ item.arriveTime === undefined ? '未到岗' : `${item.arriveTime} 到岗` }}</div>
                            </div>
                        </div>
                    </div>
                    <div class="process-items">
                        <div>6.工作指令处置记录</div>
                        <div class="event-process-item">
                            <div class="flex items-center pl-5 pr-5"
                                v-for="(  item, key  ) in   reportData.disposalAnalysis.instructionRecordList  " :key="key">
                                <div>{{ key + 1 }}、{{ item.instructionTitle }}：</div>
                                <div>{{ `${item.createTime}，${item.instructionContent} ` }}</div>
                            </div>
                        </div>
                    </div>
                    <div class="process-items">
                        <div>7.现场救援记录</div>
                        <div class="event-process-item">
                            <div class="flex items-center pl-5 pr-5"
                                v-for="(  item, key  ) in   reportData.disposalAnalysis.rescueRecordList  " :key="key">
                                <div style="width:200px">{{ item.rescueType }}</div>
                                <div class="flex-1">{{
                                    `${item.contactStatus === 1 ? `${item.contactTime}联络` : '未联络'}、${item.arriveStatus === 1
                                        ?
                                        `${item.arriveTime}到达` : '未到达'
                                        } `
                                }}</div>
                            </div>
                        </div>
                    </div>
                    <div class="process-items">
                        <div>8.设备联动控制记录</div>
                        <el-table border :data="reportData.disposalAnalysis.exeResultList"
                            style="width: 100%;min-height: 200px;">
                            <el-table-column prop="exeTime" label="操作时间"></el-table-column>
                            <el-table-column prop="cpName" label="操作任务"></el-table-column>
                            <el-table-column prop="exeStatus" label="执行状态">
                                <template slot-scope="scope">
                                    {{ scope.row.exeStatus === 0 ? '失败' : '成功' }}
                                </template>
                            </el-table-column>
                        </el-table>
                    </div>
                </div>
                <div>
                    <div>三、相关图片</div>
                    <div class="grid grid-cols-3 gap-4">
                        <el-image v-for="(  item, key  ) in   reportData.urls  " :key="key" :src="item"
                            :preview-src-list="[item]"></el-image>
                    </div>
                </div>
            </section>
        </div>
        <section id="pdfpage" class="section-wrapper" style="position:absolute; left: -10000px;top:0; color:black">
            <div>
                <div>一、事件经过</div>
                <div class="event-process-item">
                    <div style="border-color: black;" v-for="( item, key ) in  reportData.eventProcessRecords " :key="key">
                        {{ item.createTime }}
                        &nbsp;&nbsp;&nbsp;&nbsp; {{ item.stageName
                        }}</div>
                </div>
            </div>
            <div>
                <div>二、应急预案处置过程分析</div>
                <div class="process-items-pdf">
                    <div>1.应急程序执行情况</div>
                    <el-descriptions :column="2" border style="border-color: black;" class-name="section-descriptions">
                        <el-descriptions-item label="处置开始时间">
                            <span>{{ reportData.disposalAnalysis.disposalProcessInfo.startTime }}</span>
                        </el-descriptions-item>
                        <el-descriptions-item label="处置结束时间">
                            <span>{{ reportData.disposalAnalysis.disposalProcessInfo.endTime }}</span>
                        </el-descriptions-item>
                        <el-descriptions-item label="累计处置时长">
                            <span> {{ reportData.disposalAnalysis.disposalProcessInfo.duration }}</span>
                        </el-descriptions-item>
                        <el-descriptions-item label="处置是否超时">
                            <span>{{ reportData.disposalAnalysis.disposalProcessInfo.timeout }}</span>
                        </el-descriptions-item>
                    </el-descriptions>
                </div>
                <div class="process-items-pdf">
                    <div>2.相关应急信息</div>
                    <div>
                        <table style="border:1px solid; width:100%">
                            <tbody>
                                <tr style="height:55px ; "
                                    v-for="(item, key) in reportData.disposalAnalysis.disposalRelated" :key="key">
                                    <td style=" line-height: 55px;  padding-left: 20px;">{{ key + 1 }}、{{ item.name }}
                                    </td>
                                    <td v-for="(  i, k  ) in   item.list" :key="k">{{ i.topic }}</td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </div>
                <div class="process-items-pdf">
                    <div>3.预案执行情况</div>
                    <table class="table-style">
                        <thead>
                            <tr style="color:black">
                                <td>处置阶段</td>
                                <td>岗位</td>
                                <td>操作事项</td>
                                <td>执行情况</td>
                                <td>完成时间</td>
                            </tr>
                        </thead>
                        <tbody>
                            <tr v-for="(item, key) in reportData.disposalAnalysis.planProcessList" :key="key">
                                <td>{{ item.stageName }}</td>
                                <td>{{ item.name }}</td>
                                <td width="40%">{{ item.action }}</td>
                                <td>{{ item.status === 0 ? '未完成' : '已完成' }}</td>
                                <td>{{ item.completTime === '' ? '-' : item.completTime }}</td>
                            </tr>
                        </tbody>
                    </table>
                </div>
                <div class="process-items-pdf">
                    <div>4.处置要点完成分析</div>
                    <div class="flex items-center pl-5" style="border: 1px solid black;min-height: 55px;">
                        {{ reportData.disposalAnalysis.processResult }}</div>
                </div>
                <div class="process-items-pdf">
                    <div>5.人员到岗记录</div>
                    <div class="event-process-item-pdf">
                        <div class="flex items-center justify-between pl-5 pr-5"
                            v-for="(  item, key  ) in   reportData.disposalAnalysis.personArriveList  " :key="key">
                            <div>{{ item.jobName }}</div>
                            <div>{{ item.personDatas.map(item => item.personName).join('、') }}</div>
                            <div>{{ item.arriveTime === undefined ? '未到岗' : `${item.arriveTime} 到岗` }}</div>
                        </div>
                    </div>
                </div>
                <div class="process-items-pdf">
                    <div>6.工作指令处置记录</div>
                    <div class="event-process-item-pdf">
                        <div class="flex items-center pl-5 pr-5"
                            v-for="(  item, key  ) in   reportData.disposalAnalysis.instructionRecordList  " :key="key">
                            <div>{{ key + 1 }}、{{ item.instructionTitle }}：</div>
                            <div>{{ `${item.createTime}，${item.instructionContent} ` }}</div>
                        </div>
                    </div>
                </div>
                <div class="process-items-pdf">
                    <div>7.现场救援记录</div>
                    <div class="event-process-item-pdf">
                        <div class="flex items-center pl-5 pr-5"
                            v-for="(  item, key  ) in   reportData.disposalAnalysis.rescueRecordList  " :key="key">
                            <div style="width:200px">{{ item.rescueType }}</div>
                            <div class="flex-1">{{
                                `${item.contactStatus === 1 ? `${item.contactTime}联络` : '未联络'}、${item.arriveStatus === 1
                                    ?
                                    `${item.arriveTime}到达` : '未到达'
                                    } `
                            }}</div>
                        </div>
                    </div>
                </div>
                <div class="process-items-pdf">
                    <div>8.设备联动控制记录</div>
                    <table class="table-style">
                        <thead>
                            <tr style="color:black">
                                <td>操作时间</td>
                                <td>操作任务</td>
                                <td>执行状态</td>
                            </tr>
                        </thead>
                        <tbody>
                            <tr v-for="(item, key) in reportData.disposalAnalysis.exeResultList" :key="key">
                                <td width="33%">{{ item.exeTime }}</td>
                                <td>{{ item.cpName }}</td>
                                <td>{{ item.exeStatus === 0 ? '失败' : '成功' }}</td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </div>
            <div>
                <div>三、相关图片</div>
                <div class="grid grid-cols-3 gap-4">
                    <el-image v-for="(  item, key  ) in   reportData.urls  " :key="key" :src="item"
                        :preview-src-list="[item]"></el-image>
                </div>
            </div>
        </section>
    </div>
</template>

<script>

import wpf from '@/common/utils/wpf';
import { getAddress } from '@/common/utils/tools';
import { PlanSolveApi } from '@api/flood';
import topdf from '@/common/utils/tools/topdf';

const eventprocessmock = [
    {
        time: '06:08:08',
        des: '口部路面出现积水'

    },
    {
        time: '06:08:08',
        des: '口部路面出现积水'
    }
];

const emergencyprocessmock = {
    starttime: '2023–07-29 14:09:02',
    endtime: '2023–07-29 14:09:02',
    duration: '00时01分30秒',
    isTimeout: 0
};

const emergencyinfomock = [
    {
        title: '车站水淹预警信息',
        des: '2023-07-29 14:09:02 长沙地铁6号线人民东路站水淹车站 车站水淹预警信息'
    },
    {
        title: '车站水淹预警信息',
        des: '2023-07-29 14:09:02 长沙地铁6号线人民东路站水淹车站 2023-07-29 14:09:02 长沙地铁6号线人民东路站水淹车站 车站水淹预警信息 车站水淹预警信息 2023-07-29 14:09:02 长沙地铁6号线人民东路站水淹车站 车站水淹预警信息 2023-07-29 14:09:02 长沙地铁6号线人民东路站水淹车站 车站水淹预警信息'
    }
];

const implementationListmock = [
    {
        stage: '做好一级客控准备',
        job: '行值',
        operation: '向临站请求支援并通知2号线及时做好联动，合理安排各岗位。',
        implementation: '已完成',
        endTime: '2022/12/30 11:32:32'
    },
    {
        stage: '做好一级客控准备',
        job: '行值',
        operation: '向临站请求支援并通知2号线及时做好联动，合理安排各岗位。',
        implementation: '已完成',
        endTime: '2022/12/30 11:32:32'
    },
    {
        stage: '做好一级客控准备',
        job: '行值',
        operation: '向临站请求支援并通知2号线及时做好联动，合理安排各岗位。',
        implementation: '已完成',
        endTime: '2022/12/30 11:32:32'
    }, {
        stage: '做好一级客控准备',
        job: '行值',
        operation: '向临站请求支援并通知2号线及时做好联动，合理安排各岗位。',
        implementation: '已完成',
        endTime: '2022/12/30 11:32:32'
    },
    {
        stage: '做好一级客控准备',
        job: '行值',
        operation: '向临站请求支援并通知2号线及时做好联动，合理安排各岗位。',
        implementation: '已完成',
        endTime: '2022/12/30 11:32:32'
    },
    {
        stage: '做好一级客控准备',
        job: '行值',
        operation: '向临站请求支援并通知2号线及时做好联动，合理安排各岗位。',
        implementation: '已完成',
        endTime: '2022/12/30 11:32:32'
    },
    {
        stage: '做好一级客控准备',
        job: '行值',
        operation: '向临站请求支援并通知2号线及时做好联动，合理安排各岗位。',
        implementation: '已完成',
        endTime: '2022/12/30 11:32:32'
    },
    {
        stage: '做好一级客控准备',
        job: '行值',
        operation: '向临站请求支援并通知2号线及时做好联动，合理安排各岗位。',
        implementation: '已完成',
        endTime: '2022/12/30 11:32:32'
    }
];

const arrivalRecordMock = [
    {
        name: '张三',
        job: '行政员',
        time: '2022/12/30 11:32:32'
    },
    {
        name: '张三',
        job: '行政员',
        time: '2022/12/30 11:32:32'
    }
];

const wordOrderRecordMock = [
    {
        name: '发送加开列车工作指令',
        content: '2022-12-30 11:32:32 长沙地铁6号线人民东路站水淹车站 车站水淹预警信息'
    },
    {
        name: '发送加开列车工作指令',
        content: '2022-12-30 11:32:32 长沙地铁6号线人民东路站水淹车站 车站水淹预警信息'
    },
    {
        name: '发送加开列车工作指令',
        content: '2022-12-30 11:32:32 长沙地铁6号线人民东路站水淹车站 车站水淹预警信息'
    }
];
const rescueRecordMock = [
    {
        name: '110',
        content: '2023-07-29 14:09:02 长沙地铁6号线人民东路站水淹车站 车站水淹预警信息'
    },
    {
        name: '120',
        content: '2023-07-29 14:09:02 长沙地铁6号线人民东路站水淹车站 车站水淹预警信息'
    },
    {
        name: '专业救援人员',
        content: '2023-07-29 14:09:02 长沙地铁6号线人民东路站水淹车站 车站水淹预警信息'
    }
];
const devicCtrRecordMock = [
    {
        time: '2023-07-29 14:09:02',
        operation: '车站水淹预警信息',
        status: '成功'
    },
    {
        time: '2023-07-29 14:09:02',
        operation: '车站水淹预警信息',
        status: '失败'
    },
    {
        time: '2023-07-29 14:09:02',
        operation: '车站水淹预警信息',
        status: '成功'
    }

];
const picListMock = [
    'https://fuss10.elemecdn.com/e/5d/4a731a90594a4af544c0c25941171jpeg.jpeg',
    'https://fuss10.elemecdn.com/e/5d/4a731a90594a4af544c0c25941171jpeg.jpeg',
    'https://fuss10.elemecdn.com/e/5d/4a731a90594a4af544c0c25941171jpeg.jpeg',
    'https://cube.elemecdn.com/6/94/4d3ea53c084bad6931a56d5158a48jpeg.jpeg'
];
export default {
    beforeCreate () {
        window.$viewWidth = 960;
        window.setRemUnit();
    },
    data () {
        return {
            eventprocess: eventprocessmock,
            emergencyprocess: emergencyprocessmock,
            emergencyinfo: emergencyinfomock,
            implementationList: implementationListmock,
            arrivalRecord: arrivalRecordMock,
            wordOrderRecord: wordOrderRecordMock,
            rescueRecord: rescueRecordMock,
            deviceCtrRecord: devicCtrRecordMock,
            picList: picListMock,
            reportData: {},
            email: '',
            visible: false
        };
    },
    methods: {
        confirmEmail () {
            var re = /^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
            const isEmail = re.test(this.email);
            if (!isEmail) {
                this.$message({
                    message: '邮箱格式错误',
                    type: 'error'
                });
            }
        },
        close () {
            const address = getAddress(this.$router.options.routes, 'disposalReport', this.$route.path);
            console.log('close', address);
            wpf.emit({
                command: 'close'
            });
        },
        downloadreport () {
            topdf.downloadPDF('pdfpage', '处置报告.pdf');
        },
        sendEmail () {

        },
        async getDisposalReport () {
            try {
                const data = await PlanSolveApi.getDisposalReport({ disposalProcessId: this.$route.query.disposalProcessId });
                console.log('reportData:', data);
                this.reportData = data;
            } catch (error) {
                console.log('Error:', error);
            }
        },
        async saveDisposalReport () {
            try {
                await PlanSolveApi.saveDisposalReport({ disposalProcessId: 111, topic: this.reportData.topic, config: JSON.stringify(this.reportData) });
            } catch (error) {
                console.log('Error:', error);
            }
        }
    },
    mounted () {
        this.getDisposalReport();
    }
};
</script>

<style lang="scss" scoped>
* {
    font-size: 14px;
}

header {
    height: 56px;
}

.section-wrapper {
    width: 100%;
    display: flex;
    flex-direction: column;
    padding: 0px 20px 20px;
    height: calc(100% - 56px);
    overflow-y: auto;

    div {
        margin-top: 5px;
    }

}

.event-process-item {
    >div {
        min-height: 55px;
        border: 0.5px solid rgba(23, 85, 127, 0.65);
        display: flex;
        align-items: center;
        padding-left: 10px;
        margin-top: 0;
    }

    >:first-child {
        border-top: 1px solid rgba(23, 85, 127, 0.65);
    }

    >:last-child {
        border-bottom: 1px solid rgba(23, 85, 127, 0.65);
    }
}

.event-process-item-pdf {
    >div {
        min-height: 55px;
        border: 0.5px solid black;
        display: flex;
        align-items: center;
        padding-left: 10px;
        margin-top: 0;
    }

    >:first-child {
        border-top: 1px solid black;
    }

    >:last-child {
        border-bottom: 1px solid black;
    }
}

.process-items {
    padding: 10px 0;

    ::v-deep .el-descriptions-item__label.is-bordered-label {
        color: rgba(255, 255, 255, 0.85);
        font-weight: 500;
        background: rgba(11, 60, 93, 1);
    }
}

.process-items-pdf {
    color: black;
    padding: 10px 0;

    ::v-deep .el-descriptions-item__label.is-bordered-label {
        color: black;
        font-weight: 500;
        background: transparent;
    }

    ::v-deep .el-descriptions .is-bordered .el-descriptions-item__cell {
        border-color: black
    }

    ::v-deep .el-descriptions__body {
        color: black
    }
}

.el-table-pdf {
    ::v-deep .cell {
        color: black !important;
        border-color: black
    }

    ::v-deep th.el-table__cell {
        background-color: gray !important;
    }

}

.table-style {
    font-family: Arial, sans-serif;
    font-size: 14px;
    background-color: transparent;
    border-color: black;
    border-width: 0.5px;
    table-layout: auto;
    width: 100%;
    text-align: center;
    border-style: solid;
    margin-top: 10px;

    thead {
        text-align: left;
        line-height: 40px;
        font-weight: bold;
        color: rgba(255, 255, 255, 1);
        background: gray;
    }

    tr {
        border-width: 0.5px;
        border-style: solid;
        border-color: black;
        line-height: 23px;
    }

    td {
        padding: 10px 0px 10px 20px;
        font-size: 14px;
        font-family: Verdana;
        word-break: break-all; // 元素换行
        text-align: left;
        color: black;
        border-width: 0.5px;
        border-style: solid;
        border-color: black;
    }

    // background-image: linear-gradient(180deg, #0B2F4C 1%, rgba(9,57,81,0.90) 98%);

    // // 斑马纹效果stripe
    // tr:nth-child(even) {
    //     background: #F5F7F9;
    // }

    // tr:nth-child(odd) {
    //     background: #FFF;
    // }
}
</style>
