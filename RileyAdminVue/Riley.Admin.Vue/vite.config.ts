import { fileURLToPath, URL } from 'node:url'

import { defineConfig,ConfigEnv } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueJsx from '@vitejs/plugin-vue-jsx'
import VueDevTools from 'vite-plugin-vue-devtools'
import Icons from 'unplugin-icons/vite'
import IconsResolver from 'unplugin-icons/resolver'
import AutoImport from 'unplugin-auto-import/vite'
import Components from 'unplugin-vue-components/vite'
import { ElementPlusResolver } from 'unplugin-vue-components/resolvers'

// https://vitejs.dev/config/
const viteConfig = defineConfig(({mode,command}:ConfigEnv)=>{
  console.log(mode,'mode')
 return {
  plugins: [
    vue(),
    vueJsx(),
    VueDevTools(),
    AutoImport({
      imports: ['vue', 'vue-router','pinia'],
      resolvers: [ElementPlusResolver(), IconsResolver({
        prefix: 'Icon',
      }),],
    }),
    Components({
      resolvers: [  IconsResolver({
        enabledCollections: ['ep'],
      }),
      ElementPlusResolver()],     
    }),
    Icons({
      autoInstall: true,
    }),
  ],
  server: {
    host: '0.0.0.0',
    port: 8080,
    open: true
  },
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    }
  },
  css:{
    preprocessorOptions:{
      scss:{
        // additionalData:`@import "@/styles/index.scss";`
      }
    }
  },
}
})

export default viteConfig
