<template>
  <div class="wrapper">
    <ul class="bg-red-400">
      <router-link v-for="route in routes" :key="route.path" slots="li" :to="route.path">
        {{ route.meta.title }}
      </router-link>
    </ul>
    <main style="height: 100%;">
      <router-view></router-view>
    </main>
  </div>
</template>
<script setup>
import { RouterLink, RouterView } from 'vue-router'
import HelloWorld from './components/HelloWorld.vue'
import { useRoute, useRouter } from "vue-router";
import { allroute } from './router/index';
import { ref } from 'vue';
const routes = ref(allroute);
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
        // bottom: 6px;
        // left: 50%;
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
  background: #051d32;
  height: calc(100% - 72px);
}


/* header {
  line-height: 1.5;
  max-height: 100vh;
}

.logo {
  display: block;
  margin: 0 auto 2rem;
}

nav {
  width: 100%;
  font-size: 12px;
  text-align: center;
  margin-top: 2rem;
}

nav a.router-link-exact-active {
  color: var(--color-text);
}

nav a.router-link-exact-active:hover {
  background-color: transparent;
}

nav a {
  display: inline-block;
  padding: 0 1rem;
  border-left: 1px solid var(--color-border);
}

nav a:first-of-type {
  border: 0;
}

@media (min-width: 1024px) {
  header {
    display: flex;
    place-items: center;
    padding-right: calc(var(--section-gap) / 2);
  }

  .logo {
    margin: 0 2rem 0 0;
  }

  header .wrapper {
    display: flex;
    place-items: flex-start;
    flex-wrap: wrap;
  }

  nav {
    text-align: left;
    margin-left: -1rem;
    font-size: 1rem;

    padding: 1rem 0;
    margin-top: 1rem;
  }
} */
</style>
