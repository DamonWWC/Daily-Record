<template>
  <div class="flex h-full">
    <aside>
      <div v-for="(item, index) in elevatorList" :key="index"
        :class="['elevator-item', 'flex', 'items-center', 'justify-between', 'cursor-pointer', item.name === currElevatorName ? 'active' : '']"
        @click="handleSelectElevator(item)">
        <div>{{ item.name }}</div>
        <div :style="{ color: item.status === '停' ? '#FF5555' : '#72FF48' }">{{ item.status }}</div>
      </div>
    </aside>
    <main class="flex-1">
      <el-carousel trigger="click" style="height:calc(100% - 64px);padding: 16px;">
        <el-carousel-item v-for="item in 3" :key="item">
          <div class="h-full grid grid-cols-2 grid-rows-2 gap-px">
            <div style="background-color: black;"></div>
            <div style="background-color: black;"></div>
            <div style="background-color: black;"></div>
            <div style="background-color: black;"></div>
          </div>
        </el-carousel-item>
      </el-carousel>
      <footer style="height:4rem;position: relative;">
        <div class="tip flex items-center">
          <i class="iconfont icon-jinggao"></i>操作前请视频确认
        </div>
        <div style="text-align: center;line-height: 4rem;">
          <el-button type="primary" class="iconfont icon-shanghangqidong" style="width:8rem;"> 上升</el-button>
          <el-button type="primary" class="iconfont icon-kongzhi_zanting" style="width:8rem;background: #73899A;border:0">
            停止</el-button>
          <el-button type="primary" class="iconfont icon-xiahangqidong" style="width:8rem;"> 下降</el-button>
        </div>
      </footer>
    </main>
  </div>
</template>

<script>
export default {
    data () {
        return {
            currElevatorName: '',
            elevatorList: [
                { name: '1号扶梯', status: '停' },
                { name: '2号扶梯', status: '上' },
                { name: '3号扶梯', status: '下' },
                { name: '4号扶梯', status: '上' },
                { name: '5号扶梯', status: '下' },
                { name: '6号扶梯', status: '上' },
                { name: '7号扶梯', status: '下' }
            ]
        };
    },
    methods: {
        handleSelectElevator (elevator) {
            this.currElevatorName = elevator?.name;
        }
    }

};
</script>

<style lang="scss" scoped>
aside {
  width: 256px;
  border-right: 1px solid #0C3E5F;
  padding: 4px 0;

  .elevator-item {
    margin: 8px;
    padding: 0 8px;
    height: 32px;
    line-height: 32px;
    font-size: 14px;
    color: #FFFFFF;
  }

  .active {
    background: #0f3958;
  }
}

footer {
  height: 64px;
  position: relative;

  .tip {
    font-size: 12px;
    color: #fff;
    position: absolute;
    top: 50%;
    left: 16px;
    transform: translateY(-50%);

    i {
      font-size: 16px;
      color: #EFA80D;
      margin-right: 8px;
    }
  }
}
</style>
