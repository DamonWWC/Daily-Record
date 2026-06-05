<template>
    <div class="upload-tool">
        <el-upload v-if="isEdit || (!isEdit && uploadType === 'picture-card')" action=""
            :class="['upload-demo', { 'hide-viewer': autoHidden || localFileList.length >= maxLimit || !isEdit }]"
            :list-type="uploadType" :before-upload="handleBeforeUpload" :http-request="uploadSectionFile"
            :show-file-list="uploadType === 'picture-card'" :file-list="localFileList" :limit="maxLimit"
            :on-exceed="handleExceed" ref="upload" name="image" :accept="acceptType" :on-remove="handleRemove"
            :on-preview="handlePreviewImg" :on-change="handleChange">
            <template v-if="isEdit">
                <div v-if="uploadType === 'picture-card'" class="el-upload__content">
                    <i class="iconfont icon-tianjiatupian1"></i>
                    <div>添加文件</div>
                </div>
                <!-- <el-icon v-if="uploadType === 'picture-card'" class="cursor-pointer">
                    <Plus />
                </el-icon> -->
                <el-button v-else type="primary" class="flex items-center">
                    <span v-if="btnText">{{ btnText }}</span>
                    <span v-else>文件上传</span>
                </el-button>
            </template>
            <template #tip v-if="isEdit">
                <div class="el-upload__tip" v-if="isEdit">
                    <slot name="tip">{{ ['picture-card', 'picture'].includes(uploadType) ? '最多上传 4 张照片，单张不超过10M' : ''
                    }}
                    </slot>
                </div>
            </template>
        </el-upload>

        <span v-if="uploadType !== 'picture-card'" class="flex custom-file-list">
            <span v-for="item in fileList" :key="item.name" class="group mr-8 cursor-pointer flex items-center py-1">
                <el-icon class="mr-1 group-hover:inline-flex">
                    <document />
                </el-icon>
                <span class="group-hover:inline-flex">{{ item.name }}</span>
                <span class="hidden group-hover:inline-flex">
                    <el-tooltip placement="top" content="下载文件" v-if="showDownload">
                        <span class="ml-2"><el-icon @click="handleDownload(item)">
                                <download />
                            </el-icon></span>
                    </el-tooltip>
                    <el-tooltip placement="top" content="删除文件" v-if="isEdit && showDelete">
                        <span class="ml-2"><el-icon @click="handleDelete(item.url)">
                                <delete />
                            </el-icon></span>
                    </el-tooltip>
                </span>
            </span>
        </span>

        <el-image-viewer v-if="dialogVisible" :initial-index="imgIndex" :url-list="urlList" :z-index="99999999"
            :on-close="handleImageViewerClose" />
    </div>
</template>

<script>
import { Message } from '@ecp/ecp-ui';
import ElImageViewer from 'element-ui/packages/image/src/image-viewer';

// 预设变量
let modeProd = process.env.NODE_ENV === 'production';

export default {
    components: {
        ElImageViewer
    },
    props: {
        btnText: String,
        uploadType: String,
        fileList: Array,
        maxLimit: {
            type: Number,
            default: 9999
        },
        maxMemory: {
            type: Number,
            default: 9999
        },
        isEdit: {
            type: Boolean,
            default: true
        },
        autoHidden: {
            // 达到最大值时自动隐藏添加按钮
            type: Boolean,
            default: false
        },
        delFlag: {
            // 是否物理删除（默认不进行物理删除，应该在dialog/drawer等父类组件进行确认时，修改delFlag值进行物理删除）
            type: Boolean,
            default: false
        },
        // 自定义上传方法
        uploadRequest: Function,
        // 自定义下载方法
        downloadRequest: Function,
        // 自定义物理删除方法（一般不进行物理删除）
        deleteRequest: Function,
        // 上传接口的token，Authorization信息
        header: Object,
        showPlace: {
            // 文件列表显示的位置：'right'|'bottom'--上传按钮的右边|按钮的下面
            type: String,
            default: 'right'
        },
        showDownload: {
            // 显示下载文件图标
            type: Boolean,
            default: true
        },
        showDelete: {
            // 显示删除文件图标
            type: Boolean,
            default: true
        }
    },
    data () {
        return {
            localFileList: [], // 文件列表
            isSelfChange: false, // 用于表示是否是组件内的操作导致props.fileList变化，若是则取消组件fileList变化（可取消图片闪跳bug）
            dialogVisible: false, // 图片预览
            dialogImageUrl: '',
            imgIndex: 1,
            physicalDeleteList: []// 物理删除列表
        };
    },
    computed: {
        // 文件列表与上传按钮的位置
        flexDirection () {
            return this.showPlace === 'bottom' ? 'column' : 'row';
        },
        // 接收的文件类型
        acceptType () {
            return !this.uploadType
                ? '*.*'
                : ['picture-card', 'picture'].includes(this.uploadType)
                    ? '.jpg, .jpeg, .png, .gif, .bmp, .JPG, .JPEG, .PBG, .GIF'
                    : `.${this.uploadType}`;
        },
        urlList () {
            return this.fileList.map(v => v.url);
        }
    },
    methods: {
        // 上传校验
        handleBeforeUpload (file) {
            // 文件后缀
            let suffix = file.name.substring(file.name.lastIndexOf('.') + 1, file.name.length);
            // 校验格式
            if (this.acceptType !== '*.*' && !this.acceptType.match(suffix)) {
                Message.error(`请上传${this.acceptType}格式文件！`);
                return false;
            }
            // 校验大小
            if (file.size > this.maxMemory * 1024 * 1024) {
                Message.error(`请上传小于${this.maxMemory}M的文件！`);
                return false;
            }
            return new Promise((resolve) => {
                this.$nextTick(function () {
                    resolve(true);
                });
            });
        },
        // 上传
        async uploadSectionFile (params) {
            const file = params.file;
            const form = new FormData();
            form.append('file', file);
            let res;
            // let res = await uploadFile1(form);
            let path = res;
            let fileItem = [{
                name: file.name,
                uid: file.uid,
                url: path
            }];
            // 给父类传值
            let list = this.fileList.concat(fileItem);
            this.isSelfChange = true;
            this.$emit('update:fileList', list);
            this.$nextTick(() => {
                console.log('---props.fileList', list, this.fileList);
            });
            await this.uploadRequest(form);
        },
        // 文件状态改变时的钩子，添加文件、上传成功和上传失败时都会被调用
        handleChange (file, fileList) {
            if (file.status === 'ready') {
                const item = [{
                    name: file.name,
                    url: file.url
                }];
                // TODO: 不应该只有status === 'ready'时才更新fileList，删除操作也会有handleChange调用
                this.localFileList = fileList;
            }
        },
        // 下载
        async handleDownload (file) {
            const objName = file.url;
            const objUrl = modeProd === 'production' ? `${objName}` : `/gateway${objName}`;
            this.$confirm('确定下载此文件吗?', '提示', {
                confirmButtonText: '确定',
                cancelButtonText: '取消',
                type: 'warning'
            })
                .then(() => {
                    if (this.downloadRequest) {
                        this.downloadRequest(objUrl, file.name);
                    } else {
                        this.downloadFile(objUrl, file.name);
                    }
                })
                .catch();
        },
        downloadFile (url, name) {
            const authStr = this.header?.systemIdEncode || 'MTA3OjE=';
            var x = new XMLHttpRequest();
            x.open('GET', url, true);
            x.setRequestHeader('Authorization', `Basic ${authStr}`);
            x.responseType = 'blob';
            x.onload = function () {
                var url = window.URL.createObjectURL(x.response);
                var a = document.createElement('a');
                a.href = url;
                a.download = name;
                a.click();
            };
            x.send();
        },
        // 移除
        handleRemove (file, fileList) {
            if (this.uploadType === 'picture-card') {
                this.localFileList = fileList;
                let list = this.fileList.filter((item) => item.url !== file.url);
                this.isSelfChange = true;
                this.$emit('update:fileList', list);
            }
        },
        async handleDelete (fileUrl) {
            let url = '';
            if (this.uploadType === 'picture-card' && process.env.MODE !== 'production') {
                url = fileUrl.split('/gateway')[1];
            } else {
                url = fileUrl;
            }
            // 当文件格式不对或者超过大小限制时，会走到这边，所以要if一下，不然服务器报上下文错误
            if (this.localFileList.findIndex((v) => v.url === fileUrl) !== -1) {
                this.localFileList = this.localFileList.filter((item) => item.url !== fileUrl);
                let list = this.fileList.filter((item) => item.url !== fileUrl);
                this.isSelfChange = true;
                this.$emit('update:fileList', list);
                this.$nextTick(() => {
                    Message.success('删除文件成功');
                });
            }
        },
        // 物理删除
        async physicalDeleteFile (fileUrlList) {
            for (const element of fileUrlList) {
                await this.deleteRequest(element);
            }
            this.physicalDeleteList = [];
            this.$emit('update:delFlag', false);
        },
        // 超出上限
        handleExceed () {
            Message.error(`已上传${this.maxLimit}个文件，上传其它文件前请先删除已有文件`);
        },
        handlePreviewImg (file) {
            this.dialogImageUrl = file.url;
            this.imgIndex = this.fileList.findIndex((v) => v.url === this.dialogImageUrl);
            this.dialogVisible = true;
        },
        handleImageViewerClose () {
            this.dialogVisible = false;
        }
    },
    watch: {
        delFlag (newV) {
            if (newV) {
                this.physicalDeleteFile(this.physicalDeleteList);
            }
        },
        fileList: {
            handler (newV) {
                if (!this.isSelfChange) {
                    this.localFileList = newV || [];
                }
                this.isSelfChange = false;
            },
            immediate: true
        }
    }
};
</script>

<style lang="scss" scoped>
.upload-tool {
    display: flex;
    align-items: center;
    flex-direction: v-bind(flexDirection);
    // margin-left: 24px;
    // margin-right: 24px;

    .upload-demo {
        display: inline-block;
        margin: auto 24px auto 0;
        // margin-right: 24px;
    }

    ::v-deep .hide-viewer {
        .el-upload--picture-card {
            display: none;
        }

        .el-upload-list__item-delete {
            display: none;
        }
    }

    overflow: unset;

    .el-upload__tip {
        opacity: 0.6;
        font-size: 14px;
        color: #fff;
        margin: 0;
    }

    ::v-deep .el-upload-list--picture-card .el-upload-list__item {
        width: 104px;
        height: 104px;
        border-radius: 0;
        margin-bottom: 0;
    }

    ::v-deep .el-upload--picture-card {
        width: 104px;
        height: 104px;
        line-height: unset;
        background-color: #1A4868;
        border: 1px dashed #2873a6;
        border-radius: 0;
        color: #fff;

        &:hover {
            background-color: #1A4868;
            color: #fff;
        }

        .el-upload__content {
            display: flex;
            flex-direction: column;
            justify-content: center;
            align-items: center;
            height: 100%;

            div {
                opacity: 0.8;
                font-size: 12px;
                color: #FFF;
            }
        }
    }

}
</style>
