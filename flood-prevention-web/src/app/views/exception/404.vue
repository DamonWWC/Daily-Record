<template>
    <div class="exception-404">
        <ecp-empty class="exception-404-status" empty-text="暂未开放，敬请期待~" />
    </div>
</template>

<script>
export default {
    name: 'exception-404',
    components: {},
    data () {
        return {};
    },
    computed: {
        globalConfigs () {
            return this.$store.state.globalConfigs;
        },
        title () {
            return this.globalConfigs?.IMPORT_CONFIGS?.title;
        }
    },
    methods: {}
};
</script>

<style scoped lang="scss">
.exception-404 {
    display: flex;
    height: calc(100% - 48px);
    flex-direction: column;
    justify-content: center;
    margin: 24px;
    background: #ffffff;
    border-radius: 4px;
}
</style>
