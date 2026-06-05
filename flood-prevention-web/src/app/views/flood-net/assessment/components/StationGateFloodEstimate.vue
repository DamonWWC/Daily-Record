<template>
    <AssessmentPanel title="车站出入口水淹风险评估">
        <template #content>
            <div class="grid items-center grid-cols-3 entrance-exit">
                <div v-for="(item, key) in eaeList" :key="key">
                    {{ item.eaeName }}：<span :style="colorChange(item.value)">{{ valueTodes(item.value) }}</span>
                </div>
            </div>
            <LikeSelector :businessId="businessId" :businessCode="businessCode"></LikeSelector>
        </template>
    </AssessmentPanel>
</template>

<script>
import AssessmentPanel from './assessment-panel.vue';
import LikeSelector from './like-selector.vue';
import { NetFloodAssessmentApi as FloodAssessmentApi } from '@api/flood';
import MqttMixin from '@/app/mixins/MqttMixin';
export default {
    mixins: [MqttMixin],
    components: { AssessmentPanel, LikeSelector },
    data () {
        return {
            topic: `/biz/wia/${this.$route.query.lineId}/${this.$route.query.stationId}/eae`,
            eaeList: [], // 出入口列表
            eaeData: [], // 订阅的出入口数据
            businessId: '',
            businessCode: '',
            lineId: this.$route.query.lineId,
            stationId: this.$route.query.stationId
        };
    },
    methods: {
        onConnected () {
            this.subscribe();
        },
        async onResponse (res) {
            if (res?.topic === this.topic) {
                let msg = JSON.parse(`${res?.message}`);
                this.eaeData = msg?.properties;
                this.businessCode = msg?.businessCode;
                this.businessId = msg?.businessId;
                this.eaeList.forEach(item => {
                    var re = this.eaeData.find(p => p.code === item.eaeCode);
                    if (re) {
                        item['value'] = re.value;
                    }
                });
            }
        },
        async fetchEntranceExitList () {
            try {
                const res = await FloodAssessmentApi.queryEntranceExitList(this.$route.query.lineId, this.$route.query.stationId);
                this.eaeList = res;
                console.log('eaeList:', this.eaeList);
            } catch (error) {
                console.log('fetchEaeList Error:', error);
            }
        },
        async subscribe () {
            this.eaeList = [];
            await this.fetchEntranceExitList();
            this.$store.dispatch('mqtt/subscribe',
                {
                    topic: this.topic
                }
            );
        },
        unsubscribe () {
            this.$store.dispatch('mqtt/unsubscribe',
                {
                    topic: this.topic
                });
        },
        colorChange (value) {
            value = parseInt(value);
            let color = { 'color': '#2FA83F' };
            switch (value) {
            case 1: color = { 'color': '#2FA83F' };
                break;
            case 2: color = { 'color': '#EFA80D' };
                break;
            case 3: color = { 'color': '#F22F45' };
                break;
            default:
                break;
            }
            return color;
        },
        valueTodes (value) {
            value = parseInt(value);
            let des = '无风险';
            switch (value) {
            case 1: des = '低风险';
                break;
            case 2: des = '中风险';
                break;
            case 3: des = '高风险';
                break;
            default:
                break;
            }
            return des;
        }
    },
    unmounted () {
        this.unsubscribe();
    }
};
</script>

<style lang="scss" scoped>
.entrance-exit {
    height: calc(100% - 64px);
    margin: 12px 0 8px 0;
    overflow-y: auto;
    font-size: 12px;
    color: #fff;
}

.like-selector {
    margin-bottom: 12px;
}
</style>
