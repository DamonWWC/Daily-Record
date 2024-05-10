<template>
    <div>
        <el-form ref="formRef" :model="state.ruleForm" size="large" class="login-content-form">
            <el-form-item class="login-animation1" prop="userName"
                :rules="[{ required: true, message: '请输入用户名', trigger: ['blur', 'change'] }]">
                <el-input text placeholder="请输入用户名" v-model="state.ruleForm.userName" clearable autocomplete="off"
                    @keyup.enter="onSignIn">
                    <template #prefix>
                        <el-icon> <i-ep-User /></el-icon>
                    </template>
                </el-input>
            </el-form-item>
            <el-form-item class="login-animation2" prop="password"
                :rules="[{ required: true, message: '请输入密码', trigger: ['blur', 'change'] }]">
                <el-input :type="state.isShowPassword ? 'text' : 'password'" placeholder="请输入密码"
                    v-model="state.ruleForm.password" autocomplete="off" @keyup-enter="onSignIn">
                    <template #prefix>
                        <el-icon> <i-ep-Unlock /></el-icon>
                    </template>
                    <template #suffix>
                        <i class="iconfont login-content-password"
                            :class="state.isShowPassword ? 'icon-yincangmima' : 'icon-xianshimima'"
                            @click="state.isShowPassword = !state.isShowPassword"></i>
                    </template>
                </el-input>
            </el-form-item>
            <el-form-item class="login-animation3">
                <el-button type="primary" class="login-content-submit" round v-waves @click="onSignIn"
                    :disabled="state.disabled.signIn" :loading="state.loading.signIn">
                    <span>登录</span>
                </el-button>
            </el-form-item>
        </el-form>
    </div>
</template>

<script setup lang="ts" name="loginAccount">
import { reactive, ref } from 'vue'
import { type AuthLoginInput } from '@/api/admin/data-contracts'
import { useUserInfo } from '@/stores/userInfo';
import { AuthApi } from '@/api/admin/Auth';


const formRef = ref()
const state = reactive({
    showDialog: false,
    isShowPassword: false,
    ruleForm: {
        userName: 'admin',
        password: '123asd',
        captchaId: '',
        captchaData: ''
    } as AuthLoginInput,
    loading: {
        signIn: false
    },
    disabled: {
        signIn: false
    }
})

const onSignIn = () => {
    formRef.value.validate(async (valid: boolean) => {
        if (!valid) return

        //state.disabled.signIn=true
        login()
    })

}

const login = async () => {
    state.loading.signIn = true
    const res = await new AuthApi().login(state.ruleForm).catch(() => {
        state.loading.signIn = false
    })
    if(!res?.success){
        state.loading.signIn=false
        return 
    }
    state.loading.signIn=false
    const token = res.data?.token
    useUserInfo().setToken(token)
}

</script>

<style scoped lang="scss">
.login-content-form {
    margin-top: 20px;

    @for $i from 1 through 3 {
        .login-animation#{$i} {
            opacity: 0;
            animation-name: error-num;
            animation-duration: 0.5s;
            animation-fill-mode: forwards;
            animation-delay: calc($i/10) + 5;
        }
    }

    .login-content-password {
        display: inline-block;
        width: 20px;
        cursor: pointer;

        &:hover {
            color: #909399;
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
