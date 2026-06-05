import Axios from 'axios';
import { WomServer, Nec, InfoNotification, MediaCenter, HjmosFloodServe, HjmosDevice } from '@api/prefix.config';

// 工作指令服务名
let instructionService = `${WomServer}/instruction`;

// 工作指令管理接口
export const Instruction = {
    // 获取指令列表
    async getInstructionSets(params) {
        const { data } = await Axios.get(`${instructionService}/get-instruction-sets`, { params });
        return data.data;
    },

    // 获取已发布指令列表
    async publishedInstructionsInfo(params) {
        const { data } = await Axios.get(`${instructionService}/published-instructions-info`, { params });
        return data.data;
    },

    // 查看已发布的工作指定详情
    async publishedInstructionsDetail(params) {
        const { data } = await Axios.get(`${instructionService}/published-instruction-detail`, { params });
        return data.data;
    },

    // 下发工作指令
    async issueInstruction(params) {
        const { data } = await Axios.post(`${instructionService}/issue-instruction`, params);
        return data.data;
    },

    // 工作指令审核
    async auditInstruction(params) {
        const { data } = await Axios.get(`${instructionService}/audit-instruction`, { params });
        return data.data;
    },

    // 新增工作指令
    async addInstruction(params) {
        const { data } = await Axios.post(`${instructionService}/add-instruction`, params);
        return data.data;
    },

    // 根据指令ID获取指令详情
    async getInstructionById(params) {
        const { data } = await Axios.get(`${instructionService}/get-instruction-by-id`, { params });
        return data.data;
    }
};

let planHandelService = `${Nec}/plan-handle`;

export const Rescue = {
    // 获取救援列表
    async getRescueList(params) {
        const { data } = await Axios.post(`${planHandelService}/get-rescue`, params);
        return data.data;
    },
    // 救援记录已联络接口
    async setRescueContact(params) {
        const { data } = await Axios.post(`${planHandelService}/rescue-contact`, params);
        return data.data;
    },
    // 救援记录已到达接口
    async setRescueArrive(params) {
        const { data } = await Axios.post(`${planHandelService}/rescue-arrive`, params);
        return data.data;
    }
};

// 预案处置
export const PlanHandle = {
    // 获取人员签到数据
    async getPersonArrive(params) {
        const { data } = await Axios.post(`${planHandelService}/get-person-arrive`, params);
        return data.data;
    },

    // 人员已到岗
    async setPersonArrive(params) {
        const { data } = await Axios.post(`${planHandelService}/person-arrive`, params);
        return data.data;
    },

    // 人员签到岗位调整
    async changePersonArrive(params) {
        const { data } = await Axios.post(`${planHandelService}/person-arrive-change`, params);
        return data.data;
    }
};

let disposalProcessService = `${Nec}/disposal-process`;

// 处置流程
export const DisposalProcess = {
    // 结束处置流程
    async setProcessEnd(params) {
        const { data } = await Axios.post(`${disposalProcessService}/end`, params);
        return data.data;
    },

    // 通过ID获取详情
    async getProcessInfo(params) {
        const { data } = await Axios.get(`${disposalProcessService}/getInfo`, { params });
        return data.data;
    },

    // 当前进行的预案处置-列表查询
    async getProcessList(params) {
        const { data } = await Axios.post(`${disposalProcessService}/processList`, params);
        return data.data;
    },

    // 启动处置流程
    async setProcessStart(params) {
        const { data } = await Axios.post(`${disposalProcessService}/start`, params);
        return data.data;
    },
    // 获取处置报告
    async getDisposalReport(params) {
        const { data } = await Axios.get(`${disposalProcessService}/getDisposalReport`, { params });
        return data.data;
    },
    // 保存处置报告
    async saveDisposalReport(params) {
        const { data } = await Axios.post(`${disposalProcessService}/saveDisposalReport`, params);
        return data.data;
    }
};

let processStageService = `${Nec}/process-stage`;

// 流程编排控制器
export const ProcessStage = {
    // 通过当前ID查询详情信息（步骤一，其他通过trigger接口）
    async getCurrentProcessStageInfo(params) {
        const { data } = await Axios.get(`${processStageService}/getCurrentInfo`, { params });
        return data.data;
    },

    // 通过当前id查询下一阶段详情信息
    async getNextProcessStageInfo(params) {
        const { data } = await Axios.get(`${processStageService}/getNextInfo`, { params });
        return data.data;
    },

    // 触发接口
    async triggerProcessStage(params) {
        const { data } = await Axios.post(`${processStageService}/trigger`, params);
        return data.data;
    }
};

let planService = `${Nec}/plan`;

// 预案库
export const Plan = {
    // 查询处置过程
    async getProcess(params) {
        const { data } = await Axios.post(`${planService}/get/process`, params);
        return data.data;
    },

    // 设置要点执行状态
    async setPointExeStatus(params) {
        const { data } = await Axios.post(`${planService}/set-point-exe-status`, params);
        return data.data;
    }
};

let eventProcessRecordService = `${Nec}/event-process-record`;

// 事件流程记录控制器
export const EventProcessRecord = {
    // 获取事件的流程处置记录
    async getProcessRecord(params) {
        const { data } = await Axios.get(`${eventProcessRecordService}/getListByCondition`, { params });
        return data.data;
    }
};

let executionLogService = `${Nec}/emergency-linkage/execution-log`;

// 应急联动执行日志管理
export const ExecutionLog = {
    // 根据预案ID或者事件IDs-查询联动日志
    async getExecutionLog(params) {
        const { data } = await Axios.get(`${executionLogService}/linkage/select-exe-log`, { params });
        return data.data;
    }
};

let notificationService = `${InfoNotification}/notification-record`;

// 信息通报
export const NotificationRecord = {
    // 信息通报记录分页接口
    async getInfoNotificationRecord(params) {
        const { data } = await Axios.post(`${notificationService}/page`, params);
        return data.data;
    }
};

let routingPathService = `${MediaCenter}/routing/path`;

// 巡检路线前端控制器
export const RoutingPath = {
    // 根据类型查询巡检路线列表
    async getRoutingPathWidthCodes(params) {
        const { data } = await Axios.post(`${routingPathService}/findByTypeWithCodes`, params);
        return data.data;
    },

    // 查询巡检线路下所有摄像头列表
    async getCameraList(params) {
        const { data } = await Axios.get(`${routingPathService}/routingPathCameraList`, { params });
        return data.data;
    }
};

let cameraService = `${MediaCenter}/camera`;

// 摄像头
export const Camera = {
    // 获取实时视频流ws地址,通过mediaAgent获得rtsp地址
    async getCameraWsUrl(params) {
        const { data } = await Axios.post(`${cameraService}/realtime/play-ws`, params);
        return data.data;
    }
};

let videoLinkageService = `${Nec}/videoLinkage`;

// 摄像头
export const VideoLinkage = {
    // 根据预案id获取摄像头列表
    async getCameraByPlanId(params) {
        const { data } = await Axios.get(`${videoLinkageService}/get-camera-by-planId`, { params });
        return data.data;
    }
};

let lineFloodStationService = `${HjmosFloodServe}/line-flood-station`;

// 线路防汛--车站信息
export const LineFloodStation = {
    // 查询车站设备异常项
    // SB水泵 FYM防淹门 SXT摄像头 SWCGQ水位传感器 YLCGQ雨量传感器
    async getDeviceAbnormalItem(deviceType, params) {
        const { data } = await Axios.get(`${lineFloodStationService}/select-station-device-abnormal-item/v2/${deviceType}`, { params });
        return data.data;
    }
};

// 设备服务
export const HjmosDeviceAPI = {
    // 根据分组查询点位信息
    async getExitEntranceClosedStatus(params) {
        const { data } = await Axios.post(`${HjmosDevice}/point-group/select-point-list/Exit_Entrance_Closed_Status`, params);
        return data.data;
    },
    // 查询原始点位订阅
    async queryOriginalPoint(params) {
        const { data } = await Axios.post(`${HjmosDevice}/point/queryOriginalPoint`, params);
        return data.data;
    },
    // 设备控制接口
    async syncControlCommand(params) {
        const { data } = await Axios.post(`${HjmosDevice}/collector-api/v2/control-command-syn`, params);
        return data.data;
    }
};
