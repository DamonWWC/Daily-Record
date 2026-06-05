<template>
  <AssessmentPanel title="站外天气评估">
    <template #content>
      <div class="h-full" style="padding:1rem 0 .75rem 0; ">
        <div class="grid grid-cols-2 gap-x-6" style="height:calc(100% - 1.75rem)">
          <div class="index-box">
            <h3>是否下雨评估</h3>
            <div class="rain-status" :style="rainStatusStyle">
              {{ rainStatus }}
            </div>
          </div>
          <div class="index-box">
            <div class="flex justify-center items-center">
              <h3>近期降雨量</h3>
              <el-tooltip placement="bottom">
                <div slot="content">
                  小雨: 0.1 ≤ 降雨量 ≤ 9.9<br />中雨: 10.0 ≤ 降雨量 ≤ 24.9<br />大雨: 25.0 ≤ 降雨量 ≤ 49.9<br />暴雨: 50.0 ≤ 降雨量
                </div>
                <div class="help-icon"></div>
              </el-tooltip>
            </div>
            <div v-if="rainGrade" class="rain-grade flex items-center justify-center">
              <i :class="['iconfont', rainGrade.icon]"></i>
              <div>{{ rainGrade.name }}</div>
            </div>
            <div v-else class="unrain-tip">
              未下雨，无法触发评估</div>
            <div class="rain-num">
              <span>{{ rainfall }}</span>
              <span>&nbsp;mm/h</span>
            </div>
          </div>
        </div>
        <LikeSelector :businessId="businessId" :businessCode="businessCode"></LikeSelector>
      </div>
    </template>
  </AssessmentPanel>
</template>

<script>
import AssessmentPanel from './assessment-panel.vue';
import LikeSelector from './like-selector.vue';
import _ from 'lodash';
import MqttMixin from '@/app/mixins/MqttMixin';
import { getValueByCode } from '../helper';

export default {
    mixins: [MqttMixin],
    components: {
        AssessmentPanel,
        LikeSelector
    },
    watch: {
        weatherData: {
            immediate: false,
            handler (val) {
                let rainStatusValue = getValueByCode(val, 'rainStatus');
                this.rainStatus = _.get(this.rainStatusOptions, rainStatusValue, '-');
                this.rainStatusStyle.color = rainStatusValue === '-' ? '#fff' : parseInt(rainStatusValue) === 1 ? '#f22f45' : '#2fa83f';

                let rainGradeValue = getValueByCode(val, 'rainGrade');
                this.rainGrade = this.rainGradeOptions.find(x => x.code === parseInt(rainGradeValue));

                this.rainfall = getValueByCode(val, 'rainfall') || '-';
            }
        }
    },
    methods: {
        onConnected () {
            this.subscribe();
        },
        onResponse (res) {
            if (res?.topic === this.topic) {
                let msg = JSON.parse(`${res?.message}`);
                this.weatherData = msg?.properties;
                this.businessCode = msg?.businessCode;
                this.businessId = msg?.businessId;
            }
        },
        subscribe () {
            this.$store.dispatch('mqtt/subscribe',
                {
                    topic: this.topic,
                    qos: 0
                }
            );
        }
    },
    data () {
        return {
            topic: `/biz/wia/${this.$route.query.lineId}/${this.$route.query.stationId}/weather`,
            rainStatusOptions: { 0: '未下雨', 1: '下雨', 2: '停雨' },
            rainGradeOptions: [
                { code: 1, name: '小雨', icon: 'icon-xiaoyu' },
                { code: 2, name: '中雨', icon: 'icon-zhongyu' },
                { code: 3, name: '大雨', icon: 'icon-dayu' },
                { code: 4, name: '暴雨', icon: 'icon-baoyu' }
            ],
            rainStatusStyle: {
                color: '#fff'
            },
            weatherData: [],
            rainStatus: '-',
            rainGrade: '-',
            rinfall: '-',
            businessId: '',
            businessCode: '',
            lineId: this.$route.query.lineId,
            stationId: this.$route.query.stationId
        };
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

  .help-icon {
    margin-left: 4px;
    width: 14px;
    height: 14px;
    background: url('~@assets/images/help.svg') no-repeat;
  }
}

.rain-status {
  padding: 12px;
  font-size: 18px;
  text-align: center;
  font-weight: 500;
}

.rain-grade {
  padding-top: 12px;

  >i {
    color: #0d90e7;
    font-size: 36px;
  }

  >div {
    padding-left: 8px;
    text-align: center;
    color: #FFF;
    font-weight: 500;
    font-size: 16px;
  }
}

.unrain-tip {
  padding-top: 8px;
  text-align: center;
  color: #2FA83F;
  font-weight: 500;
  font-size: 14px;
}

.rain-num {
  padding-top: 8px;
  text-align: center;

  span {
    &:nth-child(1) {
      font-size: 14px;
      color: #FFF;
      font-weight: 500;
    }

    &:nth-child(2) {
      font-size: 12px;
      color: rgba(255, 255, 255, 0.65);
    }
  }
}
</style>
