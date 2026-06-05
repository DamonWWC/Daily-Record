<template>
    <div class="wrapper">
        <div class="flex flex-row h-full">
            <div id="left">
                <event-oveview />
                <gateway-usage />
                <!-- <person-config /> -->
                <rescue-record />
                <information-notify />
                <device-alarm />
            </div>
            <main class="flex-1">
                <solve-process></solve-process>
                <solve-flow></solve-flow>
                <solve-point></solve-point>
            </main>
            <div id="right">
                <video-view></video-view>
                <linkage-system />
                <supplies-allocation />
                <business-coprocessing></business-coprocessing>
            </div>
        </div>
    </div>
</template>

<script>
import BusinessCoprocessing from './components/business-coprocessing.vue';
import PersonConfig from './components/person-config.vue';
import RescueRecord from './components/rescue-record.vue';
import InformationNotify from './components/information-notify.vue';
import LinkageSystem from './components/linkage-system.vue';
import SuppliesAllocation from './components/supplies-allocation.vue';
import SolvePoint from './components/solve-point.vue';
import SolveProcess from './components/solve-process.vue';
import SolveFlow from './components/solve-flow.vue';
import GatewayUsage from './components/gateway-usage.vue';
import EventOveview from './components/event-overview.vue';
import VideoView from './components/video-view.vue';
import deviceAlarm from './components/device-alarm.vue';
export default {
    components: {
        BusinessCoprocessing,
        PersonConfig,
        RescueRecord,
        InformationNotify,
        LinkageSystem,
        SuppliesAllocation,
        SolvePoint,
        SolveProcess,
        SolveFlow,
        GatewayUsage,
        EventOveview,
        VideoView,
        deviceAlarm
    }
};
</script>

<style lang="scss" scoped>
.wrapper {
    background-color: #06121f;
    padding: 24px;
    height: 100%;

    #left,
    #right {
        width: 19.23%;
        min-width: 360px;
    }

    #left,
    #right,
    main {
        overflow: hidden;

        >div {
            opacity: 0.85;
            background: #061F35;
        }
    }

    main {
        margin: 0 12px;

        > :nth-child(1) {
            height: 72px;
        }

        > :nth-child(2) {
            height: 160px;
        }

        > :nth-child(3) {
            height: calc(100% - 160px - 72px);
        }
    }

    #left {
        > :not(:last-child) {
            margin-bottom: 12px;
        }

        > :nth-child(1) {
            height: calc(100% * 196 / 984 - 12px);
        }

        > :nth-child(2) {
            height: calc(100% * 164 / 984 - 12px);
        }

        > :nth-child(3) {
            height: calc(100% * 264 / 984 - 12px);
        }

        > :nth-child(4) {
            height: calc(100% * 192 / 984 - 12px);
        }

        > :nth-child(5) {
            height: calc(100% * 168 / 984);
        }
    }

    #right {
        > :not(:last-child) {
            margin-bottom: 12px;
        }

        > :nth-child(1) {
            height: calc(100% * 256 / 984 - 12px);
        }

        > :nth-child(2) {
            height: calc(100% * 268 / 984 - 12px);
        }

        > :nth-child(3) {
            height: calc(100% * 132 / 984 - 12px);
        }

        > :nth-child(4) {
            height: calc(100% * 328 / 984);
        }
    }
}
</style>
