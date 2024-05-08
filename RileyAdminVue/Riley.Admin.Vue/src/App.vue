<template>
  <el-config-provider>
    <RouterView />
  </el-config-provider>
</template>

<script setup lang="ts">
import { RouterLink, RouterView } from 'vue-router'
import { onBeforeMount } from 'vue'
import setIntroduction from '@/utils/setIconfont'

onBeforeMount(() => {
  setIntroduction.cssCdn()

  setIntroduction.jsCdn()
})
</script>

<style scoped>

</style>
