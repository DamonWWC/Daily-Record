<template>
  <AssessmentPanel title="车站爆管渗水风险评估">
    <template #content>
      <el-button @click="enterdisposal" style="position:absolute; right: 0px; top: -35px;background-color: rgba(1,150,163,1);">进入预案处置</el-button>
      <div class="h-full" style="padding:1rem 0 .75rem 0;">
        <div class="grid h-full grid-cols-2 gap-x-6">
          <div v-for="(item, index) in riskList" :key="index">
            <div class="index-box">
              <div style="text-align: center;">
                <i :class="['iconfont', item.icon]" :style="{ fontSize: '32px', color: item.color }"></i>
                <div style="margin-top: .75rem;">{{ item.desc }}</div>
              </div>
            </div>
            <LikeSelector v-if="item.code === 'pipeBurst'" :businessId="businessId" :businessCode="businessCode">
            </LikeSelector>
          </div>
        </div>
      </div>
    </template>
  </AssessmentPanel>
</template>

<script>
import AssessmentPanel from './assessment-panel.vue';
import LikeSelector from './like-selector.vue';
import MqttMixin from '@/app/mixins/MqttMixin';
import wpf from '@/common/utils/wpf';

export default {
    mixins: [MqttMixin],
    components: {
        AssessmentPanel,
        LikeSelector
    },
    methods: {
        onConnected () {
            this.subscribe();
        },
        onResponse (res) {
            if (res?.topic === this.topic) {
                let msg = JSON.parse(`${res?.message}`);
                this.businessCode = msg?.businessCode;
                this.businessId = msg?.businessId;
                let riskData = msg?.properties;
                this.riskList = [];
                ['pipeBurst', 'waterSeepage'].forEach(x => {
                    let data = riskData.find(y => y.code === x);

                    let riskValue = parseInt(data?.value);

                    data.color = riskValue === 1 ? '#f22f45' : '#2fa83f';

                    switch (x) {
                    case 'pipeBurst':
                        data.desc = riskValue === 1 ? '爆管预警' : '无爆管风险';
                        data.icon = 'icon-youbaoguanfengxian';
                        break;
                    case 'waterSeepage':
                        data.desc = riskValue === 1 ? '渗水预警' : '无渗水风险';
                        data.icon = 'icon-wushenshuifengxian';
                        break;
                    default:
                        break;
                    }

                    this.riskList.push(data);
                });
            }
        },
        subscribe () {
            this.$store.dispatch('mqtt/subscribe',
                {
                    topic: this.topic,
                    qos: 0
                }
            );
        },
        unsubscribe (topic) {
            this.businessId = '';
            this.businessCode = '';
            this.riskList = [{ code: 'pipeBurst', icon: 'icon-youbaoguanfengxian' }, { icon: 'icon-wushenshuifengxian' }];
            this.$store.dispatch('mqtt/unsubscribe', { topic });
        },
        enterdisposal () {
            wpf.emit({
                command: 'enterdisposal',
                param: {
                    type: 'enterdisposal',
                    data: {
                        eventCode: 'sgbg01',
                        isDrill: false,
                        entrance: 1
                    }
                }
            });
        }
    },
    data () {
        return {
            riskList: [{ code: 'pipeBurst', icon: 'icon-youbaoguanfengxian' }, { icon: 'icon-wushenshuifengxian' }],
            businessId: '',
            businessCode: ''
        };
    },
    computed: {
        topic () {
            return JSON.stringify(this.$route.query) === '{}' ? null : `/biz/wia/${this.$route.query.lineId}/${this.$route.query.stationId}/water-meter`;
        }
    }
};
</script>

<style lang="scss" scoped>
.index-box {
  background-image: linear-gradient(179deg, rgba(0, 65, 110, 0.80) 0%, rgba(0, 65, 110, 0.00) 100%);
  display: flex;
  align-items: center;
  justify-content: center;
  height: calc(100% - 32px);
}
</style>
