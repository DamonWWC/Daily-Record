<template>
  <div class="flex like-selector">
    <div class="label">是否准确？</div>
    <el-button :class="['iconfont', 'icon-dianzan', this.status === 1 ? 'active' : '']" @click="setOpinion(1)"></el-button>
    <el-button :class="['iconfont', 'icon-diancai', this.status === 2 ? 'active' : '']" @click="setOpinion(2)"></el-button>
  </div>
</template>

<script>
import { NetFloodAssessmentApi as FloodAssessmentApi } from '@api/flood';
export default {
    props: {
        businessId: {
            type: String,
            default: ''
        },
        businessCode: {
            type: String,
            default: ''
        }
    },
    data () {
        return {
            status: 0
        };
    },
    methods: {
        async setOpinion (status) {
            this.status = status;
            try {
                await FloodAssessmentApi.setOpinion(this.businessId, this.businessCode, status);
            } catch (error) {
                console.log('setOpinion Error:', error);
            }
        }
    }
};
</script>

<style lang="scss" scoped>
@import "@/styles/flood.scss";

.like-selector {
  .label {
    max-width: 60px;
    height: 32px;
    line-height: 32px;
    font-size: 12px;
    color: rgba(255, 255, 255, 0.65);
  }

  .el-button {
    max-width: 45px;
    height: 32px;
    margin: 0px;

    &.active {
      color: #EFA80D;
    }
  }

}
</style>
