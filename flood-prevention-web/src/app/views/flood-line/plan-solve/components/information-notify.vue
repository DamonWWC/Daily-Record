<docs>
    # 信息通报
</docs>
<template>
    <div>
        <div class="information-notify w-full h-full flex flex-col justify-start items-start">
            <div class="title">信息通报</div>
            <div class="content flex-1 w-full">
                <div v-for="(item, index) in  informationList " :key="index"
                    class="information-item flex flex-col justify-start items-start">
                    <div class="information-item-msg hover:cursor-pointer" @click="handleView(item)">
                        <span>{{ item.title }}</span>&nbsp;
                        <span>{{ item.notificationType === 'SMS' ? '短信发送' : item.notificationType === 'Email' ? '邮件发送' :
                            '-' }}</span>&nbsp;
                        {{ statusOptions.find(x => x.code === item.executeStatus) || '-' }}
                    </div>
                    <div class="flex justify-start items-center">
                        <div class="information-item-status mr-3"
                            :class="{ 'isNoStart': !isProcessStart, 'isAuto': isProcessStart && item.isAuto }">
                            {{ item.mode === 'Manual' ? '手动' : item.mode === 'Automatic' ? '自动' : '-' }}
                        </div>
                        <div class="information-item-time">{{ item.notificationTime }}</div>
                    </div>
                </div>
            </div>
        </div>
        <el-dialog title="查看信息通报内容" :visible.sync="dialogVisible" :append-to-body="true">
            <el-form :model="ruleForm" ref="ruleForm" label-width="120px" class="demo-ruleForm" label-position="top">
                <el-form-item label="发布内容" prop="content">
                    <el-descriptions title="" :column="1" border>
                        <el-descriptions-item label="模板名称" label-class-name="notify-description-label">{{
                            dialogNotifyContent.title }}</el-descriptions-item>
                        <el-descriptions-item label="发布内容" label-class-name="notify-description-label">{{
                            dialogNotifyContent.text }}</el-descriptions-item>
                    </el-descriptions>
                </el-form-item>
                <el-form-item label="通报对象" prop="person">
                    <el-table class="" :data="dialogTableData" height="100%" style="width: 100%;">
                        <el-table-column prop="name" label="姓名" width="150"></el-table-column>
                        <el-table-column prop="phone" label="电话"></el-table-column>
                    </el-table>
                    <div class="notify-time">{{ `通报时间：${dialogNotifyTime} ` }}</div>
                </el-form-item>
            </el-form>
        </el-dialog>
    </div>
</template>

<script>
import { PlanSolveApi } from '@api/flood';
export default {
    data() {
        return {
            dialogVisible: false,
            informationList: [],
            ruleForm: {
                content: '',
                person: ''
            },
            dialogTableData: [],
            dialogNotifyContent: {
                title: '',
                text: ''
            },
            dialogNotifyTime: '-',
            statusOptions: [
                { code: 'Success', name: '成功' },
                { code: 'Fail', name: '失败' },
                { code: 'Wait', name: '待执行' }
            ]
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
    mounted() {
        this.$utils.eventBus.on('solveFlow/processRecord', msg => {
            this.fetchInfoNotification();
        });
    },
    methods: {
        handleView(item) {
            this.dialogNotifyContent.title = item.title;
            this.dialogNotifyContent.text = item.content;
            this.dialogNotifyTime = item.notificationTime;
            this.dialogTableData = JSON.parse(item.config);
            this.dialogVisible = true;
        },
        async fetchInfoNotification() {
            try {
                let res = await PlanSolveApi.getInfoNotificationRecord({
                    tag: this.planSolveParam.planId
                });

                this.informationList = res.records;
            } catch (error) {
                console.log('Error:', error);
            }
        }
    }
};
</script>

<style lang="scss" scoped>
.information-notify {
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

        .information-item {
            border-bottom: 1px solid #0C3E5F;
            padding: 12px 0;

            &-msg {
                font-family: PingFangSC-Regular;
                font-size: 12px;
                color: #FFFFFF;
                font-weight: 400;

                &:hover {
                    cursor: pointer;
                }
            }

            &-status {
                // width: 28px;
                // height: 16px;
                background: #FF8E3B;
                border-radius: 1px;
                padding: 2px 4px;
                margin-right: 8px;

                font-family: PingFangSC-Regular;
                font-size: 10px;
                color: #FFFFFF;
                text-align: center;
                line-height: 12px;
                font-weight: 400;
                white-space: nowrap;

            }

            &-status.isAuto {
                background: #297AFF;
            }

            &-status.isNoStart {
                background: #6D7B8A;
            }

            &-time {
                opacity: 0.85;
                font-family: PingFangSC-Regular;
                font-size: 12px;
                color: #FFFFFF;
                font-weight: 400;
                margin-right: 8px;
            }
        }
    }
}
</style>
<style lang="scss">
.notify-description-label {
    width: 150px;
    text-align: center !important;
    color: rgba(255, 255, 255, 0.85) !important;
    font-size: 14px;
}

.notify-time {
    // text-white text-sm font-normal my-6
    color: #fff;
    font-size: 14px;
    font-weight: 400;
    margin: 18px;
}
</style>
