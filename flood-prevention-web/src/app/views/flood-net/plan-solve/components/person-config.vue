<docs>
    # 处置人员到场情况
</docs>
<template>
    <div class="flex flex-col items-start justify-start w-full h-full person-config">
        <div class="title w-full flex justify-between items-center">
            <div class="title-left">处置人员到场情况</div>
            <el-select v-model="personType" placeholder="请选择" size="mini">
                <el-option v-for="item in personTypeOptions" :key="item.value" :label="item.name" :value="item.value" />
            </el-select>
        </div>
        <div class="flex-1 w-full content">
            <div v-for="item in person" :key="item.jobName" class="flex items-center justify-between person-item">
                <div class="person-item-text">{{ isProcessStart ? item.jobName : '--' }}</div>
                <div class="person-item-btn">
                    <div class="person-item-btn-item" v-for="ele in item.personDatas"
                        :key="`${ele.personName}${ele.hover}`">
                        <div :class="!isProcessStart ? '' : ele.arriveStatus ? 'arrival' : 'noArrival'">
                            {{ !isProcessStart ? '--' : ele.arriveStatus ? `${ele.personName}(已到岗)` : `${ele.personName}(未到岗)` }}
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script>
import draggable from 'vuedraggable';
import { PlanSolveApi } from '@api/flood';

const personMock1 = [{ "recordId": 6718447624864256, "jobName": "车控室", "personDatas": [{ "personName": "行值", "arriveStatus": 1, "arriveTime": "2023-11-02 14:25:54", "btnText": "行值" }] }, { "recordId": 6718447665709568, "jobName": "1号口值守人员", "personDatas": [{ "personName": "安保1", "arriveStatus": 1, "arriveTime": "2023-11-02 11:41:18", "btnText": "安保1" }] }, { "recordId": 6718447700869632, "jobName": "2号口值守人员", "personDatas": [{ "personName": "安保2", "arriveStatus": 0, "arriveTime": "2023-11-02 14:27:07", "btnText": "安保2" }] }]
const personMock2 = [{ "recordId": 6718447624864265, "jobName": "BAS设备抢险队伍", "personDatas": [{ "personName": "BAS值守人员", "arriveStatus": 1, "arriveTime": "2023-11-02 14:25:54", "btnText": "BAS值守人员" }] }, { "recordId": 6718447665709865, "jobName": "AFC设备抢险队伍", "personDatas": [{ "personName": "AFC值守人员", "arriveStatus": 1, "arriveTime": "2023-11-02 11:41:18", "btnText": "AFC值守人员" }] }, { "recordId": 6718447700869632, "jobName": "供电设备抢险队伍", "personDatas": [{ "personName": "供电值守人员", "arriveStatus": 0, "arriveTime": "2023-11-02 14:27:07", "btnText": "供电值守人员" }] }]
const personTypeOptionsMock = [
    {
        name: '车站人员',
        value: 1,
    },
    {
        name: '维修调度人员',
        value: 2
    }
]
export default {
    components: {
        draggable
    },
    mounted() {
        // this.$utils.eventBus.on('solveFlow/processRecord', msg => {
        //     this.fetchPersonArrive();
        // });
    },
    data() {
        return {
            newBtn: '<i class="el-icon-rank" />',
            newText: '',
            // person: personMock,//[],
            changingItem: '',
            personType: 1,
            personTypeOptions: personTypeOptionsMock
        };
    },
    computed: {
        isProcessStart() {
            return this.$store.state.planSolve.isProcessStart;
        },
        planSolveParam() {
            return this.$store.state.planSolve.planSolveParam;
        },
        person() {
            return this.personType === 1 ? personMock1 : personMock2;
        }
    },
    methods: {
        async handleClick(ele, item) {
            var res = await PlanSolveApi.setPersonArrive({ eventId: this.planSolveParam.eventId, jobName: item.jobName, personName: ele.personName });
            if (ele.arriveStatus === 0) {
                ele.arriveStatus = 1;
            }
        },
        handleHover(event, item) {
            if (event.relatedTarget.className !== event.target.className) { // 避免mouseenter事件重复触发
                console.log('handleHover ===>', event, item);
                item.btnText = item.arriveStatus === 0 ? '到岗' : this.newBtn;
            }
        },
        handleLeave(event, item) {
            item.btnText = item.personName;
        },
        itemChanged(item) {
            this.changingItem['targetJobName'] = item.jobName;
            this.changePersonArrive();
        },
        itemStartChanged(e, item) {
            this.changingItem = '';
            this.changingItem = { eventId: this.planSolveParam.eventId, sourceJobName: item.jobName, personName: item.personDatas[e.oldIndex].personName };
        },
        // 获取人员签到数据
        async fetchPersonArrive() {
            try {
                let res = await PlanSolveApi.getPersonArrive({ eventId: this.planSolveParam.eventId, planId: this.planSolveParam.planId });
                res = res?.map((x) => {
                    x.personDatas = x.personDatas.map(y => ({ ...y, btnText: y.personName }));
                    return x;
                });
                this.person = res;
                // console.log('this.person: ', this.person);
            } catch (error) {
                console.log('fetchPersonArrive Error:', error);
            }
        },
        // 人员签到岗位调整
        async changePersonArrive() {
            try {
                // const res = await PlanSolveApi.changePersonArrive(this.changingItem);
            } catch (err) {
                console.log('changePersonArrive Error:', err);
            }
        }

    },
    destroyed() {
        this.$utils.eventBus.remove('solveFlow/processRecord');
    }
};
</script>

<style lang="scss" scoped>
.person-config {
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

        .person-item {
            border-bottom: 1px solid #0C3E5F;
            padding: 8px;

            &-text {
                font-family: PingFangSC-Regular;
                font-size: 12px;
                color: #FFFFFF;
                letter-spacing: 0;
                font-weight: 400;
            }

            &-btn {
                width: 128px;

                &-item {
                    margin-right: 5px;
                    margin-top: 5px;
                    margin-bottom: 5px;
                    height: 24px;
                    width: 128px;
                    // color: #FFFFFF;
                    // background-color: #06324F;
                    display: inline-block;

                    >div {
                        font-size: 12px;
                        // color: #FFFFFF;
                        text-align: center;
                        line-height: 22px;
                        font-weight: 400;
                    }
                    
                    .arrival {
                        color: #34E74B;
                    }

                    .noArrival {
                        color: #A2C4E8;
                    }

                }
            }
        }
    }
}
</style>
