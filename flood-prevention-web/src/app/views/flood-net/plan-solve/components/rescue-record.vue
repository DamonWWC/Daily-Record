<docs>
    # 外部救援联络
</docs>
<template>
    <div>
        <div class="flex flex-col items-start justify-start w-full h-full rescue-record">
            <div class="title">外部救援联络</div>
            <div class="flex-1 w-full content">
                <div v-for="item in rescueList" :key="item.rescueType"
                    class="flex items-center justify-between record-item">
                    <div class="record-item-text">{{ item.rescueType }}</div>
                    <div class="record-item-text">{{ item.contactStatus === 0 ? '未联络' : '已邮箱联系' }}</div>
                    <div class="record-item-text" v-html="item.contactTime || '--'"></div>
                    <div class="flex items-center justify-end">
                        <div class="record-item-btn">
                            <el-button v-if="!item.contactTime" type="primary" size="mini" @click="handleView(item)"
                                :disabled="!isProcessStart">联络</el-button>
                            <el-button v-else type="primary" size="mini" @click="handleView(item)"
                                :disabled="!isProcessStart">查看</el-button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <el-dialog title="联络救援组织" :visible.sync="dialogVisible" :append-to-body="true" center>
            <el-form :model="ruleForm" ref="ruleForm" label-width="120px" class="demo-ruleForm" label-position="top">
                <el-form-item label="联络内容" prop="content">
                    <el-input type="textarea" :row="4" v-model="dialogNotifyContent">
                    </el-input>
                </el-form-item>
                <el-form-item label="联络方式" prop="person">
                    <div class="notify-time flex justify-start items-center">
                        <div class="mr-8"><i class="iconfont icon-wanchengxiaoxiang mr-2 text-primary" />邮箱发送</div>
                        <div>邮箱号：zhangsan@ccsc.cn.com</div>
                    </div>
                </el-form-item>
            </el-form>
            <div slot="footer" class="dialog-footer" v-if="!isView">
                <el-button type="primary" @click="handleContact()">发送</el-button>
            </div>
        </el-dialog>
    </div>
</template>

<script>
import dayjs from 'dayjs';
import { PlanSolveApi } from '@api/flood';
const rescueListMock = [{ "recordId": null, "eventId": 6718402114486784, "rescueConfigId": 20231018101, "rescueType": "110", "contactStatus": 0, "contactTime": null, "arriveStatus": 0, "arriveTime": null }, 
{ "recordId": null, "eventId": 6718402114486784, "rescueConfigId": 20231018102, "rescueType": "120", "contactStatus": 0, "contactTime": null, "arriveStatus": 0, "arriveTime": null }, 
{ "recordId": null, "eventId": 6718402114486784, "rescueConfigId": 20231018103, "rescueType": "119", "contactStatus": 0, "contactTime": null, "arriveStatus": 0, "arriveTime": null }, 
{ "recordId": null, "eventId": 6718402114486784, "rescueConfigId": 20231018104, "rescueType": "应急管理局", "contactStatus": 0, "contactTime": null, "arriveStatus": 0, "arriveTime": null }, 
]

export default {
    data() {
        return {
            rescueList: rescueListMock, //[]
            dialogVisible: false,
            ruleForm: {},
            dialogNotifyContent: 'dsahkjdhjkassaeuoiwqeio                   ---长沙地铁线网指挥中心',
            isView: false,
            dialogData: {}
        };
    },
    computed: {
        isProcessStart() {
            return this.$store.state.planSolve.isProcessStart;
        },
        planSolveParam() {
            return this.$store.state.planSolve.planSolveParam;
        }
    },
    methods: {
        async handleArrival(item) {
            try {
                // var res = await PlanSolveApi.setRescueArrive({
                //     rescueConfigId: item.rescueConfigId,
                //     lineCode: this.$route.query.lineCode,
                //     stationCode: this.$route.query.stationCode,
                //     eventId: this.planSolveParam.eventId
                // });
                item.arrivalTime = dayjs().format('YYYY-MM-DD<br>HH:mm:ss');
                this.rescueList = _.clone(this.rescueList);
            } catch (err) {
                console.log('arriva:', err);
            }
        },
        async handleContact() {
            try {
                // var res = await PlanSolveApi.setRescueContact({
                //     rescueConfigId: item.rescueConfigId,
                //     lineCode: this.$route.query.lineCode,
                //     stationCode: this.$route.query.stationCode,
                //     eventId: this.planSolveParam.eventId
                // });
                const index = this.rescueList.findIndex(item => item.rescueConfigId === this.dialogData.rescueConfigId);
                this.dialogData.contactTime = dayjs().format('YYYY-MM-DD<br>HH:mm:ss');
                this.dialogData.contactStatus = 1;
                this.$set(this.rescueList, index, _.clone(this.dialogData));
                this.dialogVisible = false;
            } catch (err) {
                console.log('arriva:', err);
            }
        },
        handleView(item) {
            this.dialogData = item
            this.isView = !!item.contactStatus 
            this.dialogVisible = true
        },
        async getRescueList() {
            try {
                var res = await PlanSolveApi.getRescueList({
                    lineCode: this.$route.query.lineCode,
                    stationCode: this.$route.query.stationCode,
                    eventId: this.planSolveParam.eventId
                });
                this.rescueList = res;
                console.log('this.rescueList: ', this.rescueList);
            } catch (err) {
                console.log('getRescueList:', err);
            }
        }
    },
    mounted() {
        // this.getRescueList();
    },
    onUnmounted() {
        this.rescueList = []
    }
};
</script>

<style lang="scss" scoped>
.rescue-record {
    // width: 360px;
    // height: 348px;
    background: #061F35;
    border-radius: 2px;
    overflow: hidden;

    .title {
        padding: 10px 12px;
        font-family: PingFangSC-Regular;
        font-size: 14px;
        color: #FFFFFF;
        font-weight: 400;
    }

    .content {
        padding: 0 12px;
        overflow: auto;

        .record-item {
            border-bottom: 1px solid #0C3E5F;
            padding: 8px;

            >div:nth-child(1) {
                width: 60px;
            }

            &-text {
                font-family: PingFangSC-Regular;
                font-size: 12px;
                color: #FFFFFF;
                letter-spacing: 0;
                font-weight: 400;
            }

            &-btn {
                margin-right: 12px;
                width: 40px;
                height: 24px;
                text-align: center;

                div {
                    width: 100%;
                    height: 100%;
                    // margin: 2px 0;
                    font-size: 10px;
                    color: #FFFFFF;
                    letter-spacing: 0;
                    text-align: center;
                    line-height: 14px;
                    font-weight: 400;
                }
            }
        }
    }
}
</style>
