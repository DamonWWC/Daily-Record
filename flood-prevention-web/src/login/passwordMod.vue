<docs>
# 修改密码表单
</docs>

<template>
    <div class="login-password-mod">
        <el-form size="small" class="login-password-mod-form login-form" :model="pwForm" :rules="pwRules" ref="pwForm">
            <el-form-item class="login-password-mod-form-item" label="原密码：" prop="OldPassword">
                <el-input v-model="pwForm.OldPassword" placeholder="请输入原密码" autocomplete="new-password" show-password @blur="$refs.pwForm.validateField('Password')" />
            </el-form-item>
            <el-form-item class="login-password-mod-form-item" label="新密码：" prop="Password">
                <el-input ref="newPassword" v-model="pwForm.Password" placeholder="请输入新密码" autocomplete="new-password" show-password />
            </el-form-item>
            <el-form-item class="login-password-mod-form-item" label="确认密码：" prop="ConfirmPassword">
                <el-input v-model="pwForm.ConfirmPassword" placeholder="请重新输入新密码" autocomplete="new-password" show-password />
            </el-form-item>
        </el-form>
        <div class="login-password-mod-footer">
            <el-button type="primary" @click="passwordMod">确 定</el-button>
        </div>
    </div>
</template>

<script>
import axios from 'axios';
import Api from '@api';
import { LoginUtils } from 'ecp-login-component';

const NumberPattern = /[0-9]/;
const SpecialCharPattern = /[!@#$%^&*]/;
const LetterUpperPattern = /[A-Z]/;
const LetterLowerPattern = /[a-z]/;
const verifyTypeRegMap = {
    1: {
        pattern: NumberPattern,
        desc: '数字'
    },
    2: {
        pattern: SpecialCharPattern,
        desc: '特殊字符("!@#$%^&*")'
    },
    3: {
        pattern: LetterUpperPattern,
        desc: '大写字母'
    },
    4: {
        pattern: LetterLowerPattern,
        desc: '小写字母'
    }
};

export default {
    name: 'login-password-mod',
    props: ['userCodeProp'],
    data () {
        return {
            // 修改密码参数
            pwFormDef: {
                OldPassword: '',
                Password: '',
                ConfirmPassword: ''
            },
            pwForm: {},
            pwRules: {
                OldPassword: [{ required: true, message: '原密码不能为空' }],
                Password: [
                    {
                        required: true,
                        trigger: 'blur',
                        validator: this.passwordValidator
                    }
                ],
                ConfirmPassword: [
                    {
                        required: true,
                        trigger: 'blur',
                        validator: this.confirmPasswordValidator
                    }
                ]
            }
        };
    },
    computed: {
        userCode () {
            return this.getUrlParam('userCode') || this.userCodeProp;
        }
    },
    mounted () {
        this.$nextTick(() => {
            this.init();
        });
    },
    methods: {
        init () {
            this.pwForm = _.cloneDeep(this.pwFormDef);
            this.$refs.pwForm.resetFields();
        },
        // 获取url传参
        getUrlParam (variable) {
            var query = window.location.search.substring(1);
            var vars = query.split('&');
            for (var i = 0; i < vars.length; i++) {
                var pair = vars[i].split('=');
                if (pair[0] === variable) {
                    return pair[1];
                }
            }
            return '';
        },
        // 密码校验规则
        passwordValidator (rule, value, callback) {
            Api.SystemService.getGlobalSetting().then(({ Data }) => {
                const pwdMinLength = Data.PasswordLength;
                const verifyType = Data.PasswordVerify;
                const verifyRegList = verifyType
                    .split(',')
                    .map(item => {
                        return verifyTypeRegMap[item];
                    })
                    .filter(item => !!item);
                if (value === '' || value === undefined || value === null) {
                    if (rule.required) {
                        callback(new Error('密码不能为空'));
                    } else {
                        callback();
                    }
                } else if (value === this.pwForm.OldPassword) {
                    callback(new Error('新密码不能跟原密码相同'));
                } else if (value.length < pwdMinLength || value.length > 20) {
                    callback(
                        new Error(
                            '密码长度不能小于' +
                                pwdMinLength +
                                '个字符，且不能超过20个字符'
                        )
                    );
                } else if (
                    !verifyRegList.reduce(
                        (p, c) => p && c.pattern.test(value),
                        true
                    )
                ) {
                    callback(
                        new Error(
                            '密码需包含' +
                                verifyRegList.map(item => item.desc).join('、')
                        )
                    );
                } else {
                    callback();
                }
            });
        },
        // 确认密码校验
        confirmPasswordValidator (rule, value, callback) {
            if (value === '' || value === undefined || value === null) {
                if (rule.required) {
                    callback(new Error('确认密码不能为空'));
                } else {
                    callback();
                }
            } else if (value !== this.pwForm.Password) {
                callback(new Error('新密码和确认密码必须相同'));
            } else {
                callback();
            }
        },
        // 确认提交
        passwordMod () {
            this.$refs.pwForm.validate(valid => {
                if (valid) {
                    let time = Date.parse(new Date());
                    let oldPassword = LoginUtils.Encrypt(
                        this.pwForm.OldPassword,
                        `PCI${time}`
                    );
                    let password = LoginUtils.Encrypt(
                        this.pwForm.Password,
                        `PCI${time}`
                    );
                    let userCode = LoginUtils.Encrypt(this.userCode, `PCI${time}`);
                    let params = {
                        UserCode: userCode,
                        OldPassword: oldPassword,
                        Password: password,
                        Time: time
                    };
                    let url = '/api/sysmanager/inner/sysUser/modifyPassword';
                    const pureAxios = axios.create();
                    pureAxios.defaults.baseURL = '';
                    pureAxios
                        .put(url, params)
                        .then(({ data }) => {
                            if (data.OpCode === 0) {
                                window.parent.postMessage(
                                    {
                                        Data: 'success'
                                    },
                                    '*'
                                );
                                this.$emit('onSuccess');
                            } else {
                                this.$message.error(data.OpDesc);
                            }
                        })
                        .catch(err => {
                            console.log(err);
                        });
                }
            });
        }
    }
};
</script>

<style lang="scss">
html,
body {
    width: 100%;
    height: 100%;
}
.login-password-mod {
    position: relative;
    width: 100%;
    height: 100%;
    &-form {
        width: 450px;
        height: 100%;
        min-height: 180px;
        padding: 24px 32px 24px 24px;
        .el-form-item__label {
            width: 120px;
        }
    }
    .login-password-mod-form-item {
        white-space: nowrap;
        .el-form-item__content {
            margin-left: 120px;
        }
    }
    &-footer {
        position: absolute;
        bottom: 0;
        left: 0;
        width: 100%;
        padding: 10px;
        text-align: right;
        border-top: #ddd 1px solid;
    }
}
</style>
