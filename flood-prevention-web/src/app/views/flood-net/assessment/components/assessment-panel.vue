<template>
  <div class="flex flex-col corner-box overflow-hidden">
    <header class="striped-header"> {{ title }} </header>
    <div class="flex-1" style="height:calc(100% - 2.5rem)">
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
