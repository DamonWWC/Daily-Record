<template>
  <div>
    <el-form ref="formRef" :model="state.ruleForm" size="large" class="login-content-form">
      <el-form-item class="login-animation1" prop="mobile" :rule="[
        { required: true, message: '请输入手机号', trigger: ['blur', 'change'] },
        { validator: testMobile, trigger: ['blur', 'change'] }
      ]">
        <el-input ref="phoneRef" text placeholder="请输入手机号码" maxLength="11" v-model="state.ruleForm.mobile" clearable
          autocomplete="off" @keyup.enter="onSignIn">
          <template #prefix>
            <el-icon class="el-input__icon"><i-ep-Iphone /></el-icon>
          </template>
        </el-input>
      </el-form-item>
      <el-form-item class="login-animation2" prop="code"
        :rule="[{ required: true, message: '请输入短信验证码', trigger: ['blur', 'change'] }]">
        <MyInputCode v-model="state.ruleForm.code" @keyup.enter="onSignIn" :mobile="state.ruleForm.mobile"
          :validate="validate" @send="onSend" />
      </el-form-item>
      <el-form-item class="login-animation3">
        <el-button round type="primary" v-waves class="login-content-submit" @click="onSignIn"
          :loading="state.loading.signIn">
          <span>登录</span>
        </el-button>
      </el-form-item>
    </el-form>
  </div>
</template>

<script lang="ts" setup name="loginMobile">
import { computed, defineAsyncComponent, reactive, ref } from "vue";
import type { AuthMobileLoginInput } from '@/api/admin/data-contracts'
import { testMobile } from '@/utils/test'
import { formatAxis } from '@/utils/formatTime'
import { AuthApi } from '@/api/admin/Auth'
import { useUserInfo } from "@/stores/userInfo";

const MyInputCode = defineAsyncComponent(() => import('@/components/my-input-code/index.vue'))


const formRef = ref()
const phoneRef = ref()

const state = reactive({
  ruleForm: {
    mobile: '',
    code: '',
    codeId: ''
  } as AuthMobileLoginInput,
  loading: {
    signIn: false
  }
})

const validate = (callback: Function) => {
  formRef.value.validate('mobile', (isVaild: boolean) => {
    if (!isVaild) {
      phoneRef.value?.focus()
      return
    }
    callback?.()
  })
}

const currentTime = computed(() => {
  return formatAxis(new Date())
})

const onSend = (codeId: string) => {
  state.ruleForm.codeId = codeId
}

const onSignIn = () => {
  formRef.value.validate(async (valid: boolean) => {
    if (!valid) return

    state.loading.signIn = true
    const res = await new AuthApi().mobileLogin(state.ruleForm).catch(() => {
      state.loading.signIn = false
    })
    if (!res?.success) {
      state.loading.signIn = false
      return
    }

    const token = res.data?.token
    useUserInfo().setToken(token)

    state.loading.signIn = false

  })
}


</script>

<style lang="scss" scoped>
.login-content-form {
  margin-top: 20px;

  @for $i from 1 through 3 {
    .login-animation#{$i} {
      opacity: 0;
      animation-name: error-num;
      animation-duration: 0.5s;
      animation-fill-mode: forwards;
      animation-delay: calc($i/10) + s;
    }
  }

  .login-content-submit {
    width: 100%;
    letter-spacing: 2px;
    font-weight: 300;
    margin-top: 15px;
  }
}
</style>
