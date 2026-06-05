<template>
    <div class="wrapper">
        <ul class="flex items-center">
            <router-link v-for="route in routes" :key="route.path" tag="li"
                :to="'/flood-station/emergency-supplies/' + route.path">
                {{ route.label }}
            </router-link>
        </ul>
        <main>
            <router-view></router-view>
        </main>
    </div>
</template>

<script>
import { _emergencySuppliesRoutes } from '@views/flood-station/flood-station.route';
export default {
    data () {
        return {

        };
    },
    computed: {
        routes () {
            return _emergencySuppliesRoutes.filter(item => item.hidden !== true);
        }
    }
};
</script>

<style lang="scss" scoped>
.wrapper {
    background-color: transparent;
    height: 100%;

    ul {
        background-color: transparent;
        height: 48px;
        line-height: 48px;
        margin: 0 8px;

        li {
            height: 48px;
            position: relative;
            padding: 0 16px;
            text-align: center;
            font-size: 14px;
            color: #FFFFFF;
            opacity: 0.65;
            cursor: pointer;

            &:hover {
                opacity: 1;
            }
        }

        .router-link-active {
            color: #13FFF5;
            opacity: 1;

            &::before {
                content: '';
                bottom: 6px;
                left: 50%;
                transform: translateX(-50%);
                position: absolute;
                width: 24px;
                height: 2px;
                background: #13FFF5;
                border-radius: 1px;
            }
        }
    }
}

main {
    margin: 0 24px 24px 24px;
    opacity: 0.85;
    background: #061F35;
    height: calc(100% - 72px);
}
</style>
