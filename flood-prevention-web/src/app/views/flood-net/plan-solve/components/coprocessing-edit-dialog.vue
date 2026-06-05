<template>
  <el-dialog title="业务协同处置" :visible.sync="dialogVisible" :append-to-body="true">
    <el-form :model="ruleForm" :rules="rules" ref="ruleForm" label-width="7.5rem" class="demo-ruleForm"
      label-position="top">
      <el-form-item label="处置措施名称" prop="instructionId">
        <el-select v-model="ruleForm.instructionId" placeholder="请选择工作指令模版" style="width:100%"
          @change="handleInstructionChange">
          <el-option v-for="item in instructionList" :key="item.id" :label="item.instructionName" :value="item.id">
          </el-option>
        </el-select>
      </el-form-item>
      <el-form-item label="处置措施内容" prop="content">
        <el-input type="textarea" :autosize="{ minRows: 4, maxRows: 6 }" v-model="ruleForm.content"
          placeholder="预案对应的配置建议措施内容" maxlength="1200" show-word-limit></el-input>
      </el-form-item>
      <el-form-item label="接收单位" prop="receivers">
        <el-select v-model="ruleForm.receivers" multiple placeholder="请选择接收单位" style="width:100%;height: 32px;">
          <el-option v-for="item in receiverList" :key="item.receiverCode" :label="item.receiverName"
            :value="item.receiverCode">
          </el-option>
        </el-select>
      </el-form-item>
      <el-form-item label="上传附件">
        <upload-tool :fileList.sync="fileList" :upload-type="'picture-card'" :max-limit="4" :max-memory="10"
          :upload-request="uploadRequest" :show-download="false" :show-delete="false" show-place="bottom">
        </upload-tool>
      </el-form-item>
    </el-form>
    <div slot="footer" class="dialog-footer">
      <el-button type="primary" @click="submitForm('ruleForm')">确认发送</el-button>
      <el-button @click="dialogVisible = false">取 消</el-button>
    </div>
  </el-dialog>
</template>

<script>
import UploadTool from '@/app/components/common/upload-tool.vue';
import { HjmosDfsApi } from '@api/common';
import { PlanSolveApi } from '@api/flood';
export default {
    components: {
        UploadTool
    },
    mounted () {
        this.fetchInstructionSets();
    },
    data () {
        return {
            dialogVisible: false,
            fileList: [], // 图片列表
            instructionList: [], // 处置措施下拉列表
            receiverList: [],
            ruleForm: {
                instructionId: '',
                instructionName: '',
                content: '',
                receivers: []
            },
            rules: {
                receivers: [
                    { required: true, message: '请选择接收单位', trigger: 'change' }
                ],
                instructionId: [
                    { required: true, message: '请选择工作指令模版', trigger: 'change' }
                ],
                content: [
                    { required: true, message: '请填写处置措施内容', trigger: 'blur' }
                ]
            }
        };
    },
    methods: {
        handleInstructionChange (val) {
            const item = this.instructionList.find(item => item.id === val);
            this.ruleForm.content = item?.instructionContent;
            this.ruleForm.instructionName = item?.instructionName;
            this.receiverList = item?.receivers;
        },
        openDialog (instructionItem, processId) {
            this.disposalProcessId = processId;
            this.ruleForm.instructionId = instructionItem?.id || '';
            this.ruleForm.instructionName = instructionItem?.instructionName || '';
            this.ruleForm.content = instructionItem?.instructionContent || '';
            this.ruleForm.receivers = [];
            this.fileList = [];
            this.receiverList = instructionItem?.receivers;
            this.dialogVisible = true;
            this.$nextTick(() => this.$refs.ruleForm.clearValidate());
        },
        closeDialog () {
            this.dialogVisible = false;
        },
        submitForm (formName) {
            this.$refs[formName].validate(async (valid) => {
                if (valid) {
                    let receivers = _.filter(this.receiverList, x => this.ruleForm.receivers?.includes(x.receiverCode));

                    await PlanSolveApi.issueInstruction({
                        content: this.ruleForm.content,
                        instructionId: this.ruleForm.instructionId,
                        instructionName: this.ruleForm.instructionName,
                        instructionTag: 'YACZ',
                        processId: this.disposalProcessId, // 处置ID
                        receivers: receivers,
                        attachments: this.fileList.map(x => ({ name: x.name, url: x.shortUrl })),
                        lineCode: this.$route.query.lineCode,
                        senderCode: this.$route.query.stationCode
                    });

                    this.$message.success({
                        message: '发送成功',
                        showClose: true
                    });

                    this.dialogVisible = false;
                } else {
                    console.log('error submit!!');
                    return false;
                }
            });
        },
        resetForm (formName) {
            this.$refs[formName].resetFields();
        },
        async uploadRequest (params) {
            const res = await HjmosDfsApi.fileUploaderByStream(params);
            const fileItem = this.fileList.find(item => item.uid === params.get('file').uid);
            let host = process.env.NODE_ENV === 'development' ? '10.51.9.130' : window.location.hostname;
            fileItem.shortUrl = res;
            fileItem.url = `http://${host}:30768/dfs${res}`;
        },
        async fetchInstructionSets () {
            try {
                let typeCode = this.$route.query.stationCode ? 1 : 0;
                const res = await PlanSolveApi.getInstructionSets({
                    typeCode,
                    instructionTag: 'YACZ'
                });
                this.instructionList = res;
            } catch (error) {
                console.log('Error:', error);
            }
        }
    }
};
</script>

<style lang="scss" scoped>
::v-deep .el-dialog {
  width: 960px;

  .el-dialog__body {
    height: 600px;
    padding: 24px;
    overflow-y: auto;
  }

  .el-dialog__footer {
    padding: 16px 24px !important;
  }
}

::v-deep {
  .el-upload__tip {
    opacity: 0.6;
    font-size: 14px;
    color: #FFFFFF;
  }
}
</style>
