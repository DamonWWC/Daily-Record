<template>
  <div class="flex h-full">
    <aside>
      <div v-for="(item, index) in elevatorList" :key="index"
        :class="['elevator-item', 'flex', 'items-center', 'justify-between', 'cursor-pointer', item.name === currElevatorName ? 'active' : '']"
        @click="handleSelectElevator(item)">
        <div>{{ item.name }}</div>
        <!-- <div :style="{ color: item.status === '关' ? '#FF5555' : '#72FF48' }">{{ item.queryValue }}</div> -->
      </div>
    </aside>
    <main class="flex-1">
      <el-carousel trigger="click" style="height:calc(100% - 64px);padding: 16px;">
        <el-carousel-item v-for="item in 3" :key="item">
          <div class="grid h-full grid-cols-2 grid-rows-2 gap-px">
            <div style="background-color: black;"></div>
            <div style="background-color: black;"></div>
            <div style="background-color: black;"></div>
            <div style="background-color: black;"></div>
          </div>
        </el-carousel-item>
      </el-carousel>
      <footer style="height:4rem;position: relative;">
        <div class="flex items-center tip">
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
import { PlanSolveApi } from '@api/flood';
import { v4 as uuidv4 } from 'uuid';
export default {
    data () {
        return {
            currElevatorName: '',
            elevatorList: [],
            socket: '',
            heartbeatInterval: ''
        };
    },
    methods: {
        handleSelectElevator (elevator) {
            this.currElevatorName = elevator?.name;
        },
        async getRollingShutterDoorList() {
            try {
                const res = await PlanSolveApi.getRollingShutterDoorList({ lineId: 6, stationId: 79 });
                if (res[0].children) {
                    this.elevatorList = res[0].children.map(item => {
                        let obj = { name: item.contentName, queryValue: item.expand.queryValue, ctrlValue: item.expand.ctrlValue };
                        if (item.deviceList && item.deviceList.length > 0) {
                            const device = item.deviceList[0];
                            obj = {
                                ...obj,
                                deviceId: device.deviceId,
                                ctrlName: device.expand?.ctrlName,
                                queryName: device.expand?.queryName
                            };
                        }
                        return obj;
                    });
                    this.init();
                    console.log('elevatorList:', this.elevatorList);
                }
            } catch (error) {
                console.error('getRollingShutterDoor error:', error);
            }
        },
        init() {
            try {
                const host = process.env.NODE_ENV === 'development' ? '10.51.9.133' : window.location.hostname;
                this.socket = new WebSocket(`ws://${host}:30769/webimserver/websocket/UUID:${uuidv4()}`);

                this.socket.onopen = () => {
                    this.socket.send({ 'command': 'SB1000', 'param': { 'pointList': this.elevatorList?.map(item => item.queryName) } });
                };
                this.socket.onmessage = (msg) => {
                    try {

                    } catch (error) {
                        console.error('websocket error:', error);
                    }
                };
                this.socket.onclose = () => {
                    clearInterval(this.heartbeatInterval);
                };
                this.heartbeatInterval = setInterval(() => {
                    this.socket.send({ 'command': '00001' });
                }, 295000);
            } catch (error) {
                console.error('websocket error:', error);
            }
        }
    },
    mounted () {
        this.getRollingShutterDoorList();
    },
    beforeDestroy() {
        if (this.socket) {
            this.socket.close();
        }
        clearInterval(this.heartbeatInterval);
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
