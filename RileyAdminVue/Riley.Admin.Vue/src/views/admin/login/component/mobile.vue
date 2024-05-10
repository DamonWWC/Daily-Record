<template>
  <div>
    <el-form ref="formRef" :model="state.ruleForm" size="large" class="login-content-form">
      <el-form-item class="login-animation1" prop="mobile" :rule="[
        { required: true, message: '请输入手机号', trigger: ['blur', 'change'] },
        { validator: textMobile, trigger: ['blur', 'change'] }
      ]">
        <el-input ref="phoneRef" text placeholder="请输入手机号码" maxLength="11" v-model="state.ruleForm.mobile" clearable
          autocomplete="off" @keyup.enter="onSignIn">
          <template #prefix>
            <el-icon class="el-input__icon"><ele-Iphone/></el-icon>
          </template>
        </el-input>
      </el-form-item>
      <el-form-item class="login-animation2" prop="code" :rule="[{required:true,message:'请输入验证码',trigger:['blur', 'change']}]">
        
      </el-form-item>
    </el-form>
  </div>
</template>

<script lang="ts" setup name="loginMobile">

</script>

<style></style>
