<template>
  <el-scrollbar>
    <div class="flex login-container">
      <div class="login-left">
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
          <div class="flex flex-col login-right-warp-main">
            <div class="login-right-warp-main-title">Riley中台欢迎您！</div>
            <div class="flex-1 login-right-warp-main-form">
              <div v-if="!state.isScan">
                <el-tabs v-model="state.tabsActiveName">
                  <el-tab-pane label="账号密码" name="account">
                    <Account />
                  </el-tab-pane>
                  <el-tab-pane label="手机号" name="phone">
                    <Mobile />
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
import { defineAsyncComponent, reactive } from 'vue'
import logoMini from '@/assets/logo-mini.svg'
import loginMain from '@/assets/login-main.svg'
import loginBg from '@/assets/login-bg.svg'

const Account = defineAsyncComponent(() => import('./component/account.vue'))
const Mobile = defineAsyncComponent(() => import('./component/mobile.vue'))
const Scan = defineAsyncComponent(() => import('./component/scan.vue'))

const state = reactive({
  tabsActiveName: 'account',
  isScan: false
})
</script>

<style scoped lang="scss">
:deep() {
  .el-scrollbar__view {
    height: 100%;
  }
}

.login-container {
  height: 100%;
  background: var(--el-color-white);
  min-height: 500px;
  .login-left {
    flex: 1;
    height: 100%;
    position: relative;
    background-color: rgba(211, 239, 255, 1);
    margin-right: 100px;
    .login-left-logo {
      align-items: center;
      position: absolute;
      top: 50px;
      left: 80px;
      z-index: 1;
      animation: logoAnimation 0.3s ease;
      img {
        width: 52px;
        height: 52px;
      }
      .login-left-logo-text {
        span {
          margin-left: 10px;
          font-size: 20px;
          color: var(--el-color-primary);
        }
        .login-left-logo-text-msg {
          font-size: 12px;
          color: var(--el-color-primary);
        }
      }
    }
    .login-left-img {
      position: absolute;
      top: 50%;
      left: 50%;
      transform: translate(-50%, -50%);
      width: 100%;
      height: 52%;
      img {
        width: 100%;
        height: 100%;
        animation: error-num 0.6s ease;
      }
    }
    .login-left-waves {
      position: absolute;
      top: 0;
      left: 100%;
      height: 100%;
    }
  }
  .login-right {
    width: 700px;
    .login-right-warp {
      margin: auto;
      border: 1px solid var(--el-color-primary-light-3);
      border-radius: 3px;
      width: 500px;
      height: 500px;
      position: relative;
      overflow: hidden;
      background-color: var(--el-color-white);
      .login-right-warp-one,
      .login-right-warp-two {
        position: absolute;
        display: block;
        width: inherit;
        height: inherit;
        &::before,
        &::after {
          content: '';
          position: absolute;
          z-index: 1;
        }
      }
      .login-right-warp-one {
        &::before {
          filter: hue-rotate(0deg);
          top: 0px;
          left: 0;
          width: 100%;
          height: 3px;
          background: linear-gradient(90deg, transparent, var(--el-color-primary));
          animation: loginLeft 3s linear infinite;
        }
        &::after {
          filter: hue-rotate(60deg);
          top: -100%;
          right: 2px;
          width: 3px;
          height: 100%;
          background: linear-gradient(180deg, transparent, var(--el-color-primary));
          animation: loginTop 3s linear infinite;
          animation-delay: 0.7s;
        }
      }
      .login-right-warp-two {
        &::before {
          filter: hue-rotate(120deg);
          bottom: 2px;
          right: -100%;
          width: 100%;
          height: 3px;
          background: linear-gradient(270deg, transparent, var(--el-color-primary));
          animation: loginRight 3s linear infinite;
          animation-delay: 1.4s;
        }
        &::after {
          filter: hue-rotate(300deg);
          bottom: -100%;
          left: 0px;
          width: 3px;
          height: 100%;
          background: linear-gradient(360deg, transparent, var(--el-color-primary));
          animation: loginBottom 3s linear infinite;
          animation-delay: 2.1s;
        }
      }

      .login-right-warp-main {
        height: 100%;
        .login-right-warp-main-title {
          height: 130px;
          line-height: 130px;
          font-size: 27px;
          text-align: center;
          letter-spacing: 3px;
          animation: logoAnimation 0.3s ease;
          animation-delay: 0.3s;
          color: var(--el-text-color-primary);
        }
        .login-right-warp-main-form {
          padding: 0 50px 50px;
          .login-content-main-scan {
            position: absolute;
            top: 0;
            right: 0;
            width: 50px;
            height: 50px;
            overflow: hidden;
            cursor: pointer;
            transition: all ease 0.3s;
            color: var(--el-color-primary);
            &-delta {
              position: absolute;
              width: 35px;
              height: 70px;
              z-index: 2;
              top: 2px;
              right: 21px;
              background: var(--el-color-white);
              transform: rotate(-45deg);
            }
            &:hover {
              opacity: 1;
              transition: all ease 0.3s;
              color: var(--el-color-primary) !important;
            }
            i {
              width: 47px;
              height: 50px;
              display: inline-block;
              font-size: 48px;
              position: absolute;
              right: 1px;
              top: 0px;
            }
          }
        }
      }
    }
  }
}
</style>
