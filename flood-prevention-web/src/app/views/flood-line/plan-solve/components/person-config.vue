<docs>
    # 人员配置
</docs>
<template>
    <div class="flex flex-col items-start justify-start w-full h-full person-config">
        <div class="title">人员配置</div>
        <div class="flex-1 w-full content">
            <div v-for="item in person" :key="item.jobName" class="flex items-center justify-between person-item">
                <div class="person-item-text">{{ item.jobName }}</div>
                <div class="person-item-btn">
                    <draggable v-model="item.personDatas" @start="(e) => itemStartChanged(e, item)" @add="itemChanged(item)"
                        group="btnGroup" handle=".new-btn" :forceFallback="true">
                        <div class="person-item-btn-item" v-for="ele in item.personDatas"
                            :key="`${ele.personName}${ele.hover}`">
                            <div :class="{ 'origin-btn': ele.arriveStatus === 0, 'new-btn': ele.arriveStatus === 1, 'isNoStart': !isProcessStart }"
                                @click="isProcessStart && handleClick(ele, item)"
                                @mouseenter="isProcessStart && handleHover($event, ele)"
                                @mouseleave="isProcessStart && handleLeave($event, ele)">
                                <!--  @mousedown="handleDown($event, ele)"
                                @mouseup="handleUp($event, ele)" -->
                                <span v-html="ele.btnText"></span>
                            </div>
                        </div>
                    </draggable>
                </div>
            </div>
        </div>
    </div>
</template>

<script>
import draggable from 'vuedraggable';
import { PlanSolveApi } from '@api/flood';

const personMock = [{"recordId":6718447624864256,"jobName":"车控室","personDatas":[{"personName":"行值","arriveStatus":0,"arriveTime":"2023-11-02 14:25:54","btnText":"行值"}]},{"recordId":6718447665709568,"jobName":"1号口值守人员","personDatas":[{"personName":"安保1","arriveStatus":0,"arriveTime":"2023-11-02 11:41:18","btnText":"安保1"}]},{"recordId":6718447700869632,"jobName":"2号口值守人员","personDatas":[{"personName":"安保2","arriveStatus":0,"arriveTime":"2023-11-02 14:27:07","btnText":"安保2"}]}]

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
            person: personMock,//[],
            changingItem: ''
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
                width: 212px;

                &-item {
                    margin-right: 5px;
                    margin-top: 5px;
                    margin-bottom: 5px;
                    height: 24px;
                    width: 64px;
                    color: #FFFFFF;
                    background-color: #06324F;
                    display: inline-block;

                    >div {
                        font-size: 12px;
                        color: #FFFFFF;
                        text-align: center;
                        line-height: 22px;
                        font-weight: 400;
                    }

                    .origin-btn {
                        position: relative;
                        opacity: 0.25;

                        &:hover {
                            background-color: var(--color-primary);
                            opacity: 1;
                            cursor: pointer;
                        }
                    }

                    .new-btn {
                        position: relative;
                        opacity: 1;
                        width: 100%;

                        &:hover {
                            width: 100%;
                            opacity: 0.9;
                            background: #1476B7;
                            cursor: pointer;
                        }
                    }

                    .origin-btn.isNoStart:hover,
                    .new-btn.isNoStart:hover {
                        cursor: not-allowed;
                        background-color: transparent;
                        opacity: 0.25;
                    }
                }
            }
        }
    }
}
</style>
