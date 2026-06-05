<template>
    <div class="flex flex-col w-full h-full">
        <div class="self-start" style="font-size: 1rem;">{{ dutyInfos.name }}</div>
        <div style="overflow-y:auto; height:calc(100% - 2.5rem)">
            <el-descriptions class="mt-3" v-for="(item, key) in dutyInfos.configJson" :key="key" :title="item.name"
                :column="1" border>
                <el-descriptions-item v-for="(item1, key1) in Object.entries(item.content)" :key="key1"
                    :labelStyle="{ width: '9.375rem', 'text-align': 'center', 'color': '#FFFFFF' }" :label='item1[0]'>{{ item1[1]
                    }}</el-descriptions-item>
            </el-descriptions>
        </div>

    </div>
</template>

<script>
import { moreFunctionAPI } from '@api/flood';
export default {
    beforeCreate() {
        window.$viewWidth = 1920;
        window.setRemUnit();
    },
    data() {
        return {
            dutyInfos: {}
        };
    },
    computed: {
    },
    methods: {
        async getData() {
            try {
                const res = await moreFunctionAPI.getDutyInfo(this.$route.query.eventCode);
                this.dutyInfos = res;
                console.log('获取应急机构职责成功', res);
            } catch (error) {
                console.error('获取应急机构职责失败', error);
            }
        }
    },
    mounted() {
        this.getData();
    }
};
</script>

<style lang='scss' scoped>
  *{
    font-size:14px
}
</style>
