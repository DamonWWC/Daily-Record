<template>
    <div>
        <el-drawer :title="!showdetail ? '新建设备故障维修工单' : '设备故障维修工单详情'" :wrapperClosable="false" custom-class="cunstom-drawer"
            :visible.sync="showDrawer" direction="rtl" append-to-body ref="drawer" size="35%" :show-close="false"
            :destroy-on-close="true" :close-on-press-escape="false" @close="$emit('close', false)">
            <template v-if="!showdetail">
                <div class="flex flex-row justify-end drawer__header">
                <el-button class="self-center" @click="beforeCloseHandle()">取 消</el-button>
                <el-button class="self-center" @click="submit()" type="primary">提交</el-button>
            </div>
            <div  class="drawer__content">
                <el-form ref="form" :model="form" label-width="7.5rem" label-position="top" size="small">
                    <el-row>
                        <el-col :span="12">
                            <el-form-item style="margin-right: .3125rem;" label="报告人部门" :rules="[
                                { required: true, message: '报告人部门不能为空' },
                            ]">
                                <el-select style="width:100%" v-model="form.department" placeholder="请选择报告人部门"
                                    autocomplete="off">
                                    <el-option v-for="item in departments" :key="item.value" :label="item.label"
                                        :value="item.value">
                                    </el-option>
                                </el-select>
                            </el-form-item>
                        </el-col>
                        <el-col :span="12">
                            <el-form-item label="报告人" style="margin-left: .3125rem;" :rules="[
                                { required: true, message: '报告人不能为空' },
                            ]">
                                <el-select style="width:100%" v-model="form.reporter" placeholder="请选择报告人"
                                    autocomplete="off">
                                    <el-option v-for="item in reporters" :key="item.value" :label="item.label"
                                        :value="item.value">
                                    </el-option>
                                </el-select>
                            </el-form-item>
                        </el-col>
                    </el-row>
                    <el-form-item label="设备位置" :rules="[
                        { required: true, message: '设备位置不能为空' },
                    ]">
                        <el-select style="width:100%" v-model="form.devicePosition" placeholder="请选择设备位置"
                            autocomplete="off">
                            <el-option v-for="item in devicePositions" :key="item.value" :label="item.label"
                                :value="item.value">
                            </el-option>
                        </el-select>
                    </el-form-item>
                    <el-form-item label="设备类型" :rules="[
                        { required: true, message: '设备类型不能为空' },
                    ]">
                        <el-select style="width:100%" v-model="form.deviceType" placeholder="请选择设备类型" autocomplete="off">
                            <el-option v-for="item in deviceTypes" :key="item.value" :label="item.label"
                                :value="item.value">
                            </el-option>
                        </el-select>
                    </el-form-item>
                    <el-form-item label="设备名称">
                        <el-input style="width:100%" v-model="form.deviceName" placeholder="请输入设备名称"
                            autocomplete="off"></el-input>
                    </el-form-item>
                    <el-form-item label="设备编号">
                        <el-input style="width:100%" v-model="form.deviceCode" placeholder="请输入设备编号"
                            autocomplete="off"></el-input>
                    </el-form-item>
                    <el-row>
                        <el-col :span="12">
                            <el-form-item style="margin-right: .3125rem;" label="紧急程度" :rules="[
                                { required: true, message: '紧急程度不能为空' }
                            ]">
                                <el-select style="width: 100%;" v-model="form.urgency" placeholder="请选择紧急程度">
                                    <el-option label="区域一" value="shanghai"></el-option>
                                    <el-option label="区域二" value="beijing"></el-option>
                                </el-select>
                            </el-form-item>
                        </el-col>
                        <el-col :span="12">
                            <el-form-item style="margin-right: .3125rem;" label="故障时间" :rules="[
                                { required: true, message: '故障时间不能为空' }
                            ]">
                                <el-date-picker type="datetime" v-model="form.downTime" placeholder="选择日期时间"
                                    style="width: 100%;"></el-date-picker>
                            </el-form-item>
                        </el-col>
                    </el-row>
                    <el-form-item label="故障现象" :rules="[
                        { required: true, message: '故障现象不能为空' },
                    ]">
                        <el-input style="width:100%" v-model="form.symptom" placeholder="请输入故障现象"
                            autocomplete="off"></el-input>
                    </el-form-item>
                    <el-form-item label="备注">
                        <el-input type="textarea" v-model="form.note" placeholder="请输入备注"></el-input>
                    </el-form-item>
                    <el-form-item label="上传附件">
                        <el-upload action="https://jsonplaceholder.typicode.com/posts/" list-type="picture-card"
                            :on-preview="handlePictureCardPreview" :on-remove="handleRemove">
                            <div class="el-upload__content">
                                <i class="iconfont icon-tianjiatupian1"></i>
                                <div>添加文件</div>
                            </div>
                            <div slot="tip" class="el-upload__tip">最多上传4张照片，单张不超过10M</div>
                        </el-upload>
                        <!-- <el-dialog :visible.sync="dialogVisible">
                        <img width="100%" :src="dialogImageUrl" alt="">
                    </el-dialog> -->
                    </el-form-item>
                </el-form>
            </div>
            </template>
            <template v-else>
                <div  class="flex flex-row justify-end drawer__header">
                <el-button class="self-center" @click="closeDrawer()">关闭</el-button>
            </div>
            <div class="flex flex-col gap-5 drawer__content">
                <div class="grid grid-cols-7">
                    <div class="self-center justify-self-end">报告人部门：</div>
                    <div class="col-start-2 col-end-6">长沙地铁6号线-综合维修部</div>
                </div>
                <div class="grid grid-cols-7">
                    <div class="self-center justify-self-end">报告人：</div>
                    <div class="col-start-2 col-end-6">张三</div>
                </div>
                <div class="grid grid-cols-7">
                    <div class="self-center justify-self-end">设备位置：</div>
                    <div class="col-start-2 col-end-6">长沙地铁/6号线/车站/34黄花机场T1T2</div>
                </div>
                <div class="grid grid-cols-7">
                    <div class="self-center justify-self-end">设备类型：</div>
                    <div class="col-start-2 col-end-6">环控系统</div>
                </div>
                <div class="grid grid-cols-7">
                    <div class="self-center justify-self-end">设备名称：</div>
                    <div class="col-start-2 col-end-6">站台门</div>
                </div>
                <div class="grid grid-cols-7">
                    <div class="self-center justify-self-end">设备编号：</div>
                    <div class="col-start-2 col-end-6">11111</div>
                </div>
                <div class="grid grid-cols-7">
                    <div class="self-center justify-self-end">紧急程度：</div>
                    <div class="col-start-2 col-end-6">11111</div>
                </div>
                <div class="grid grid-cols-7">
                    <div class="self-center justify-self-end">故障时间：</div>
                    <div class="col-start-2 col-end-6">11111</div>
                </div>
                <div class="grid grid-cols-7">
                    <div class="self-center justify-self-end">故障现象：</div>
                    <div class="col-start-2 col-end-6">11111</div>
                </div>
                <div class="grid grid-cols-7">
                    <div class="self-center justify-self-end">备注：</div>
                    <div class="col-start-2 col-end-6">11111</div>
                </div>
                <div class="grid grid-cols-7">
                    <div class="self-center justify-self-end">上传附件：</div>
                    <div class="flex flex-row col-start-2 col-end-6 gap-2">
                        <el-image style="width: 6.25rem; height: 6.25rem"
                            src="https://fuss10.elemecdn.com/e/5d/4a731a90594a4af544c0c25941171jpeg.jpeg"></el-image>
                        <el-image style="width: 6.25rem; height: 6.25rem"
                            src="https://fuss10.elemecdn.com/e/5d/4a731a90594a4af544c0c25941171jpeg.jpeg"></el-image>
                    </div>
                </div>
            </div>
            <div class="drawer__footer"><i class="iconfont" style="color: yellow;">&#xe7f2;</i>
                工单已上报，但与管理网络连通失败！</div>
            </template>

        </el-drawer>
    </div>
</template>
<script>
export default {
    props: {
        show: {
            type: Boolean,
            required: true,
            default: false
        }
    },
    data () {
        return {

            uploadImages: [],
            urlDomain: 'http://172.25.23.68:9531',
            action: 'http://api.pcitech.online/constructionsite-service/S3File/upload',
            showDrawer: this.show,
            showdetail: false,
            form: {
                department: '',
                reporter: '',
                devicePosition: '',
                deviceType: '',
                deviceName: '',
                deviceCode: '',
                urgency: '',
                downTime: '',
                symptom: '',
                note: '',
                fileList: []
            },
            departments: [],
            reporters: [],
            deviceName: [],
            deviceTypes: [],
            urgencys: []
        };
    },
    watch: {
        show (newvalue, oldvalue) {
            this.showDrawer = newvalue;
        }
    },
    methods: {
        handleSuccess () {
            this.$alert(JSON.stringify(this.uploadImages, null, 2), '此时绑定的值为：').catch(() => void 0);
        },
        submit () {
            console.log(this.form);
            this.showdetail = true;
        },
        closeDrawer () {
            this.$emit('close', false);
            this.showdetail = false;
        },
        beforeCloseHandle () {
            this.$refs.drawer.closeDrawer();
            this.form = {
                department: '',
                reporter: '',
                devicePosition: '',
                deviceType: '',
                deviceName: '',
                deviceCode: '',
                urgency: '',
                downTime: '',
                symptom: '',
                note: '',
                fileList: []
            };
            this.$emit('close', false);
        }
    },
    mounted () {
        console.log('11');
        console.log(this.form.symptom);
    }
};
</script>
<style scoped>
.el-row {
    margin-bottom: 10px;

    &:last-child {
        margin-bottom: 0;
    }
}

.el-form-item {
    margin-bottom: 10px;

    &:last-child {
        margin-bottom: 0;
    }
}

.drawer__content {
    padding: 0 20px;
    box-sizing: border-box;
    position: absolute;
    bottom: 60px;
    left: 0px;
    right: 0px;
    top: 72px;
    overflow-y: auto;
    font-size:14px;

    :deep(.el-form--label-top .el-form-item__label) {
        padding: 0 !important;
    }
}

.drawer__content::-webkit-scrollbar {
    width: 6px;
    height: 1px;
}

.drawer__content::-webkit-scrollbar-thumb {
    /*滚动条里面小方块*/
    border-radius: 2px;
    -webkit-box-shadow: inset 0 0 5px rgba(0, 0, 0, 0.2);
    background: var(--scrollbar-bg-color);
}

.drawer__content::-webkit-scrollbar-track {
    /*滚动条里面轨道*/
    -webkit-box-shadow: inset 0 0 5px transparent;
    border-radius: 2px;
    background-image: var(--scrollbar-bg-image);
}

.drawer__header {
    padding: 0px 10px;
    width: 100%;
    position: absolute;
    top: 0px;
    right: 0px;
    box-sizing: border-box;
    height: 60px;
}

.drawer__footer {
    padding: 0px 20px;
    width: 100%;
    position: absolute;
    bottom: 10px;
    left: 0px;
    right: 0px;
    box-sizing: border-box;

}

.cunstom-drawer {
    height: calc(100% - 48px) !important;
    top: 48px !important;
    bottom: 0 !important;
    background-image: linear-gradient(180deg, #0B2F4C 0%, rgba(9, 57, 81, 0.90) 100%);
}</style>
