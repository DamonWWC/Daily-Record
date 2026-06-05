<template>
    <div style="padding:0 .75rem .75rem .75rem;overflow: hidden;">
        <header class="flex flex-row justify-between items-center">
            <el-select v-model="videoType" placeholder="请选择" size="mini">
                <el-option v-for="item in videoTypeOptions" :key="item.value" :label="item.name" :value="item.value" />
            </el-select>
            <el-popover placement="bottom" trigger="click" v-model="popoverVisable">
                <div style="width: 19rem;">
                    <div class="flex flex-row justify-between items-center">
                        <div style="font-size: 1rem;color: #FFFFFF;">路线管理</div>
                        <i class="iconfont icon-guanbi" @click="handleClose"></i>
                    </div>
                    <div class="flex flex-col gap-y-1" style="margin-top: 4px;">
                        <el-input type="text" v-model="moduleName" placeholder="请输入模块名称"></el-input>
                        <el-select v-model="currRoutingPath" placeholder="请选择巡检路线" style="width:100%">
                            <el-option v-for="item in routingPathList" :key="item.id" :label="item.name" :value="item.id" />
                        </el-select>
                        <el-button class="self-end" type="primary" size="mini" @click="handleRoutingPathConfirm">确定</el-button>
                    </div>
                </div>
                <div slot="reference" class="line-mng" v-show="parseInt(videoType) === 2">路线管理</div>
            </el-popover>
        </header>
        <div class="video-view">
            <ecp-simple-player :showControls="false" :url="url" ref="player" @onError="onError" @onPlaying="onPlaying">
            </ecp-simple-player>
        </div>
    </div>
</template>

<script>
import { PlanSolveApi } from '@api/flood';
export default {
    mounted () {
        this.fetchCameraByPlanId();
        this.fetchRoutingPath();
    },
    computed: {
        planSolveParam () {
            return this.$store.state.largePassengerFlow.planSolveParam;
        }
    },
    watch: {
        videoType (newV) {
            if (parseInt(newV) === 1) {
                this.cameraList = this.cameraList1;
            } else if (parseInt(newV) === 2) {
                this.cameraList = this.cameraList2;
            }

            this.initCamera();
        }
    },
    data () {
        return {
            routingPathList: [],
            timer: null,
            moduleName: '',
            popoverVisable: false,
            currRoutingPath: '',
            cameraList1: [],
            cameraList2: [],
            cameraIndex: 0,
            cameraList: [],
            urls: [],
            url: '',
            videoType: 1,
            videoTypeOptions: [
                { value: 1, name: '智能联动监控' },
                { value: 2, name: '视频巡检画面' }
            ]
        };
    },
    methods: {
        initCamera () {
            if (!this.cameraList?.length > 0) {
                this.url = '';
                return;
            }

            this.cameraIndex = 0;
            let camera = this.cameraList[0];
            this.url = camera.url;

            this.timer && clearTimeout(this.timer);
            this.setNextCamera();
        },
        setNextCamera () {
            this.cameraIndex = this.cameraIndex + 1 >= this.cameraList?.length ? 0 : (this.cameraIndex + 1);
            let camera = this.cameraList[this.cameraIndex];

            // console.log('Next camera:', camera);

            this.timer = setTimeout(() => {
                this.url = camera.url;
                this.setNextCamera();
            }, camera.pollingTime * 1000);
        },
        async fetchRoutingPath () {
            try {
                const res = await PlanSolveApi.getRoutingPathWidthCodes({
                    type: 3,
                    lineCode: this.$route.query.lineCode,
                    stationCode: this.$route.query.stationCode
                });
                this.routingPathList = res;
            } catch (error) {
                console.log('Error:', error);
            }
        },
        async fetchCameraList (pathId) {
            try {
                const res = await PlanSolveApi.getCameraList({ pathId });

                let routingPath = this.routingPathList.find(o => { return o.id === pathId; });

                this.cameraList2 = res?.map(x => { x.pollingTime = routingPath?.pollingTime; return x; });
                console.log('list2:', this.cameraList2);
                await this.matchCameralUrl(this.cameraList2);
            } catch (error) {
                console.log('Error:', error);
            }
        },
        async matchCameralUrl (cameraList) {
            let wsUrls = await this.fetchCameraWsUrl(cameraList);

            cameraList.forEach(x => {
                let obj = wsUrls.find(o => { return `${o.cameraId}` === `${x.cameraId}`; });
                x.url = obj?.ws;
            });

            this.url = cameraList[0]?.url;

            this.playVideo();
        },
        async fetchCameraByPlanId () {
            try {
                let planId = this.$route.query?.planId;
                if (planId) {
                    let params = {
                        planId,
                        lineCode: this.$route.query.lineCode,
                        stationCode: this.$route.query.stationCode
                    };
                    const res = await PlanSolveApi.getCameraByPlanId(params);
                    this.cameraList1 = res;

                    await this.matchCameralUrl(this.cameraList1);
                }
            } catch (error) {
                console.log('Error:', error);
            }
        },
        async fetchCameraWsUrl (cameraList) {
            try {
                const res = await PlanSolveApi.getCameraWsUrl({
                    clientCode: this.$route.query.stationCode,
                    playList: cameraList.map(x => ({ cameraId: x.cameraId }))
                });

                console.log('fetchCameraWsUrl:', res);
                return res;
            } catch (error) {
                console.log('Error:', error);
            }
        },
        async handleRoutingPathConfirm () {
            if (this.currRoutingPath) {
                await this.fetchCameraList(this.currRoutingPath);

                this.cameraList = this.cameraList2;

                if (parseInt(this.videoType) === 2) {
                    this.initCamera();
                }
            }

            this.popoverVisable = false;
        },
        playVideo () {
            if (!this.url) {
                this.$message({
                    type: 'warning',
                    message: '请输入播放url'
                });
                return;
            };
            this.$refs['player'].playVideo();
        },
        onPlaying () {
            console.log('playing');
        },
        onError () {
            console.log('error', arguments);
        },
        handleClose () {
            this.popoverVisable = false;
        }
    }
};
</script>

<style lang="scss" scoped>
header {
    height: 40px;
    line-height: 40px;
}

.video-view {
    height: calc(100% - 40px);
}

.line-mng {
    font-size: 12px;
    color: #13FFF5;
}
</style>
