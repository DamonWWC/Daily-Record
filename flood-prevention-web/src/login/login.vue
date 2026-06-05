<template>
    <div id="login" class="login-page">

        <!-- 如使用微动效背景视频, 放开这段注释 -->
        <!-- <video class="login-video-bg" src="videoUrl" autoplay loop muted @error="onVideoError" ref="bgVideo" /> -->

        <div class="login-title">{{`${title}`}}</div>
        <div class="login-box flex-center">

            <div class="login-box_main flex-center">
                <!-- 如应用部署网络环境复杂(例如: 需要支持非内网访问等), ecp-login-component 添加参数 is-complex-network 以处理sso固定跳转内网地址的问题 -->
                <ecp-login-component class="login-box_main_content flex-center" />
            </div>
        </div>
    </div>
</template>

<script>
import { EcpLoginComponent } from 'ecp-login-component';

export default {
    name: 'flood-prevention-web-login-page',
    components: {
        EcpLoginComponent
    },
    data () {
        return {
            // // 微动效背景视频链接
            // videoUrl: ''
        };
    },
    computed: {
        globalConfigs () {
            return this.$store.state.globalConfigs;
        },
        title () {
            return this.globalConfigs?.IMPORT_CONFIGS?.title || '登录页';
        }
    },
    mounted () {
        document.title = this.title;
    },
    methods: {
        onVideoError () {
            this.$refs.bgVideo.remove();
        }
    }
};
</script>

<style lang="scss">
$--login-background: mix($--color-black, $--color-primary, 65%);
$--login-breathing-base: mix($--color-white, $--color-primary, 50%);
$--login-breathing-shadow: mix($--color-white, $--color-primary, 70%);
html,
body {
    width: 100%;
    height: 100%;
    min-width: 960px;
    min-height: 540px;
    background-color: $--login-background;
    display: flex;
}

::-webkit-scrollbar {
    width: 5px;
    height: 7px;
}

::-webkit-scrollbar-thumb {
    background: rgba($--color-white, 0.15);
    cursor: pointer;
    border-radius: 3px;
}

::-webkit-scrollbar-track {
    background: none;
}

.flex-center {
    display: flex;
    justify-content: center;
    align-items: center;
}

.login-page {
    width: 100%;
    height: 100%;
    overflow: auto;
    position: relative;
    background-color: $--login-background;
    background-repeat: no-repeat;
    background-size: cover;
    background-position: center;
    display: flex;
    flex-direction: column;
    justify-content: space-between;
    align-items: flex-end;

    .login-video-bg {
        width: 100%;
        height: 100%;
        position: absolute;
        z-index: 1;
        pointer-events: none;
        user-select: none;
        cursor: default;
        object-fit: cover;
    }

    .login-title {
        position: relative;
        width: 100%;
        padding: 0 107px;
        margin-top: 77px;
        color: $--color-white;
        letter-spacing: 1px;
        font-size: 48px;
        line-height: 64px;
        user-select: none;
        pointer-events: none;
        cursor: default;
        white-space: pre-wrap;
    }

    .login-box {
        width: 26.5%;
        min-width: 500px;
        margin: 0 257px 224px;
        flex: 0 0 auto;
        overflow: hidden;
        flex-direction: column;
        z-index: 2;
        .login-box_main {
            width: 100%;
            padding: 56px 40px;
            flex-direction: column;
            background-position: center;
            position: relative;

            .flood-prevention-web-login-form-before {
                margin-bottom: 60px;
                font-size: 24px;
                line-height: 32px;
                font-family: MicrosoftYaHei;
                text-align: center;
                color: rgba($--color-white, 0.65);
                user-select: none;
                pointer-events: none;
                cursor: default;
            }

            .login-box_main_content {
                width: 100% !important;
                position: relative;
                z-index: 9;
                flex-direction: column;
                background-color: transparent;
                flex: 1 0 auto;

                .el-form-item {
                    margin-bottom: 24px;

                    &__error {
                        padding-top: 4px;
                    }
                }

                .el-input {
                    position: relative;

                    &__prefix {
                        display: none;
                    }

                    input {
                        width: 100%;
                        height: 38px;
                        line-height: 38px;
                        color: rgba($--color-white, 0.85);
                        background-color: transparent;
                        border-color: rgba($--color-white, 0.2);

                        &:focus {
                            border: 1px solid $--color-primary;
                            outline: 0;
                        }
                        &::placeholder {
                            line-height: 24px;
                            font-size: 16px;
                            font-family: MicrosoftYaHei;
                            color: rgba($--color-white, 0.25);
                        }
                    }
                }

                .btn-wrap {
                    margin-top: 48px;

                    .el-button {
                        height: 40px !important;
                        font-size: $--font-size-medium;
                    }
                }
                .el-form-item:nth-last-child(1) {
                    margin-bottom: 0;
                }

                .el-checkbox {
                    .el-checkbox__inner {
                        background-color: transparent;
                        border-color: rgba($--color-white, 0.65);
                    }
                    .el-checkbox__label {
                        color: rgba($--color-white, 0.65);
                    }
                }
            }
        }
    }
}
</style>
