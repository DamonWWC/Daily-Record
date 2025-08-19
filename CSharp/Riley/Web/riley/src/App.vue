<script setup>
import { RouterLink, RouterView } from 'vue-router'
</script>

<template>
  <div id="app" class="app">
    <header class="app__header">
      <img alt="Vue logo" class="app__logo" src="@/assets/logo.svg" width="125" height="125" />

      <div class="app__wrapper">
        <nav class="app__nav">
          <RouterLink to="/" class="app__nav-link">首页</RouterLink>
          <RouterLink to="/about" class="app__nav-link">关于</RouterLink>
          <RouterLink to="/example" class="app__nav-link">示例</RouterLink>
        </nav>
      </div>
    </header>

    <main class="app__main">
      <RouterView />
    </main>
  </div>
</template>

<style scoped lang="scss">
.app {
  min-height: 100vh;
  display: flex;
  flex-direction: column;

  &__header {
    line-height: 1.5;
    max-height: 100vh;
    padding: 1rem;
    background-color: var(--el-bg-color);
    border-bottom: 1px solid var(--el-border-color-light);
  }

  &__logo {
    display: block;
    margin: 0 auto 2rem;
  }

  &__wrapper {
    display: flex;
    align-items: center;
    justify-content: center;
  }

  &__nav {
    width: 100%;
    font-size: 14px;
    text-align: center;
    margin-top: 1rem;

    &-link {
      display: inline-block;
      padding: 0.5rem 1rem;
      margin: 0 0.5rem;
      color: var(--el-text-color-regular);
      text-decoration: none;
      border-radius: var(--el-border-radius-base);
      transition: all 0.3s ease;

      &:hover {
        color: var(--el-color-primary);
        background-color: var(--el-color-primary-light-9);
      }

      &.router-link-active {
        color: var(--el-color-primary);
        background-color: var(--el-color-primary-light-8);
      }
    }
  }

  &__main {
    flex: 1;
    padding: 2rem;
  }
}

@media (min-width: 1024px) {
  .app {
    &__header {
      display: flex;
      align-items: center;
      padding: 1rem 2rem;
    }

    &__logo {
      margin: 0 2rem 0 0;
    }

    &__wrapper {
      display: flex;
      align-items: center;
      flex: 1;
    }

    &__nav {
      text-align: left;
      margin-top: 0;
      margin-left: auto;

      &-link {
        margin: 0 0.5rem;
      }
    }
  }
}
</style>
