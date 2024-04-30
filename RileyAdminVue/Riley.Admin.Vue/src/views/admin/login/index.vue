<template>
  <el-scrollbar>
    <div class="flex login-container">
      <div class="flex-1 login-left">
        <div class="flex login-left-logo">
          <img :src="logoMini" />
          <div class="flex flex-col login-left-logo-text">
            <span>Riley中台</span>
            <span class="login-left-logo-text-msg">后端管理框架</span>
          </div>
        </div>
        <div class="login-left-img">
          <img :src="loginMain" />
        </div>
        <img :src="loginBg" class="login-left-waves" />
      </div>
      <div class="flex login-right">
        <div class="login-right-warp">
          <span class="login-right-warp-one"></span>
          <span class="login-right-warp-two"></span>
          <div class="login-right-warp-main">
            <div class="login-right-warp-main-title">Riley中台欢迎您！</div>
            <div class="login-right-warp-main-form">
              <div v-if="!state.isScan">
                <el-tabs v-model="state.tabsActiveName">
                  <el-tab-pane label="账号密码" name="account">
                    <!-- <LoginForm/> -->
                  </el-tab-pane>
                  <el-tab-pane label="手机号" name="phone">
                    <!-- <LoginPhone/> -->
                  </el-tab-pane>
                </el-tabs>
              </div>
              <Scan v-if="state.isScan" />
              <div class="login-content-main-scan" @click="state.isScan = !state.isScan">
                <i class="iconfont" :class="state.isScan ? 'icon-diannao1' : 'icon-barcode-qr'"></i>
                <div class="login-content-main-scan-delta"></div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </el-scrollbar>
</template>

<script setup lang="ts">
import { reactive } from 'vue'
import logoMini from '@/assets/logo-mini.svg'
import loginMain from '@/assets/login-main.svg'
import loginBg from '@/assets/login-bg.svg'

const state = reactive({
  tabsActiveName: 'account',
  isScan: false
})
</script>

<style lang="scss">
:deep() {
  .el-scrollbar__view {
    height: 100%;
  }
}

.login-container {
  height: 100%;
  min-height: 500px;
  background: var(--el-color-white);
  .login-left {
    position: relative;
    background-color: rgba(211, 239, 255, 1);
    margin-right: 100px;
    .login-left-logo{
        align-items: center;
        position: absolute;
        top: 50px;
        left:80px;
        z-index: 1;
        animation:logoAnimation 0.3s ease;
        img {
            width: 52px;
            height: 52px;
        }
        .login-left-logo-text{
            // display: flex;
            // flex-direction: column;
        }
    }
  }
}
</style>
