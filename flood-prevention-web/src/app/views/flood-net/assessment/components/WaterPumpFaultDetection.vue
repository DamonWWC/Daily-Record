<template>
  <assessment-panel title="水泵故障监测">
    <template #content>
      <div class="flex flex-col h-full">
        <div class="flex flex-auto " style="font-size: .75rem">
          <div class="self-center">
            本轮总共监测水泵{{ getValueByCode("waterPumpTotal") }}个。检出故障水泵：
          </div>
        </div>
        <div class="grid flex-auto grid-cols-2 gap-x-6">
          <div class="index-box">
            <h3>集水坑沉积物变多</h3>
            <div style="padding:.75rem ;font-size:1.5rem;text-align: center;font-weight: 500;">
              {{ getValueByCode("sediment") || '-' }}<span style="font-size: .75rem;color: rgba(255,255,255,0.65);">个</span>
            </div>
          </div>
          <div class="index-box">
            <h3>水泵或者管网堵塞</h3>
            <div style="padding:.75rem ;font-size:1.5rem;text-align: center;font-weight: 500;">
              {{ getValueByCode("blockage") || '-' }}<span style="font-size: .75rem;color: rgba(255,255,255,0.65);">个</span>
            </div>
          </div>
        </div>
        <div class="flex flex-row self-center flex-auto">
          <el-button class="px-3 py-1 mx-1" @click="dialogVisible = true"
            style="background-color: #0a3755; color: #ffffff">查看检测结果</el-button>
          <el-button class="px-3 py-1 mx-1" @click="showWorkOrder = true"
            style="background-color: #0196a3; color: #ffffff">一键故障工单</el-button>
        </div>
      </div>
      <water-pump-fault-detection-result-vue :show="dialogVisible" :businessId="businessId"
        @close="(value) => (dialogVisible = value)"></water-pump-fault-detection-result-vue>
      <create-work-order-vue :show="showWorkOrder" @close="(value) => (showWorkOrder = value)"></create-work-order-vue>
    </template>
  </assessment-panel>
</template>

<script>
import AssessmentPanel from './assessment-panel.vue';
import CreateWorkOrderVue from './CreateWorkOrder.vue';
import WaterPumpFaultDetectionResultVue from './WaterPumpFaultDetectionResult.vue';
import MqttMixin from '@/app/mixins/MqttMixin';
export default {
    mixins: [MqttMixin],
    components: {
        AssessmentPanel,
        WaterPumpFaultDetectionResultVue,
        CreateWorkOrderVue
    },
    data () {
        return {
            topic: `/biz/wia/${this.$route.query.lineId}/${this.$route.query.stationId}/water-pump`,
            dialogVisible: false,
            showWorkOrder: false,
            businessId: '',
            faultDetectionData: [],
            lineId: this.$route.query.lineId,
            stationId: this.$route.query.stationId
        };
    },
    computed: {
        getValueByCode () {
            return (code) => {
                let result = this.faultDetectionData.find((p) => p.code === code);
                return result?.value;
            };
        }
    },
    methods: {
        onConnected () {
            this.subscribe();
        },
        onResponse (res) {
            if (res?.topic === this.topic) {
                let msg = JSON.parse(`${res?.message}`);
                this.businessId = msg?.businessId;
                this.faultDetectionData = msg?.properties;
            }
        },
        subscribe () {
            this.$store.dispatch('mqtt/subscribe',
                {
                    topic: this.topic
                }
            );
        }
    }
};
</script>

<style lang="scss" scoped>
.index-box {
  height: 100%;
  padding-top: 16px;
  padding-bottom: 4px;
  background-image: linear-gradient(179deg, rgba(0, 65, 110, 0.80) 0%, rgba(0, 65, 110, 0.00) 100%);

  h3 {
    font-size: 14px;
    color: #FFF;
    text-align: center;
  }
}
</style>
