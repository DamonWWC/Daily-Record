<template>
    <div class="w-full">
        <el-input text :maxlength="props.maxlength" placeholder="请输入验证码" autocomplete="off" v-bind="$attrs">
            <template #prefix>
                <el-icon class="el-input__icon"><ele-Message /></el-icon>
            </template>
            <template #suffix>
                <el-button v-show="state.status !== 'countdown'" :loading="state.loading.getCode" type="primary" link
                    :disabled="state.status === 'countdown'" @click.prevent.stop="onGetCode">
                    {{ text }} </el-button>
                <el-countdown v-show="state.status === 'countdown'" :format="state.changeText" :value="state.countdown"
                    value-style="font-size: var(--el-font-size-base);color:var(--el-color-primary)" @change="onChange">
                </el-countdown>
            </template>
        </el-input>
    </div>
</template>

<script lang="ts" setup name="my-input-code">
import { computed, reactive, ref } from "vue"
import { isMobile } from '@/utils/test'
import { ElMessage } from 'element-plus'

const emits = defineEmits(['send'])

const props = defineProps({
    maxlength: {
        type: Number,
        default: 6
    },
    seconds: {
        type: Number,
        default: 60,
    },
    startText: {
        type: String,
        default: '获取验证码',
    },
    changeText: {
        type: String,
        default: 's秒后重发',
    },
    endText: {
        type: String,
        default: '重新发送验证码',
    },
    mobile: {
        type: String,
        default: '',
    },
    validate: {
        type: Function,
        default: null,
    },
})

const countdown = Date.now()

const state = reactive({
    status: 'ready',
    startText: props.startText,
    changeText: props.changeText,
    endText: props.endText,
    countdown: countdown,

    codeId: '',
    loading: {
        getCode: false,
    }
})

const text = computed(() => {
    return state.status === 'ready' ? state.startText : state.endText
})

const startCountdown = () => {
    state.status = 'countdown'
    state.countdown = Date.now() + (props.seconds + 1) * 1000
}

const onGetCode = () => {
    if (state.status !== 'countdown') {
        if (props.validate) {
            props.validate(getCode)
        } else {
            getCode()
        }
    }
}

const onChange = (value: number) => {
    if (state.countdown != countdown && value < 1000) state.status = 'finish'
}


const getCode = () => {
    if (!isMobile(props.mobile)) {
        ElMessage.warning({ message: '请输入正确的手机号', grouping: true })
        return
    }


    state.loading.getCode = true
    
    
}

</script>

<style></style>