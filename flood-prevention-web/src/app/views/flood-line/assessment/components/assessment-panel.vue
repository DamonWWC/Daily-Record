<template>
  <div class="flex flex-col overflow-hidden corner-box">
    <header class="striped-header"> {{ title }} </header>
    <div class="flex-1" style="height:calc(100% - 2.5rem);position: relative;">
      <slot name="content" />
    </div>
  </div>
</template>

<script>
export default {
    props: {
        title: {
            type: String
        }
    }
};
</script>

<style lang="scss" scoped>
@import "@/styles/flood.scss";
</style>
