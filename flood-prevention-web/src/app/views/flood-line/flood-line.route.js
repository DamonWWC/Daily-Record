const _deviceControlRoutes = [
    {
        path: 'broadcast-release',
        component: () => import('../flood-station/device-control/broadcast-release'),
        label: '广播发布',
        name: 'LineBroadcastRelease'
    },
    {
        path: 'manual-broadcast-release',
        component: () => import('../flood-station/device-control/manual-broadcast-release'),
        label: '人工广播发布'
    },
    {
        path: 'pis-device-control',
        component: () => import('../flood-station/device-control/pis-device-control'),
        label: 'PIS设备控制'
    },
    {
        path: 'pis-text-delivery',
        component: () => import('../flood-station/device-control/pis-text-delivery'),
        label: 'PIS文本下发'
    },
    {
        path: 'pis-scene-change',
        component: () => import('../flood-station/device-control/pis-scene-change'),
        label: '切换PIS场景'
    }
    // {
    //     path: 'station-access-control',
    //     component: () => import('../flood-station/device-control/station-access-control'),
    //     label: '车站门禁控制'
    // },
    // {
    //     path: 'afc-station-system-control',
    //     component: () => import('../flood-station/device-control/afc-station-system-control'),
    //     label: 'AFC车站系统控制'
    // },
    // {
    //     path: 'afc-device-control',
    //     component: () => import('../flood-station/device-control/afc-device-control'),
    //     label: 'AFC设备控制'
    // },
    // {
    //     path: 'rolling-door-control',
    //     component: () => import('../flood-station/device-control/rolling-door-control'),
    //     label: '卷帘门控制'
    // },
    // {
    //     path: 'escalator-control',
    //     component: () => import('../flood-station/device-control/escalator-control'),
    //     label: '扶梯控制'
    // },
    // {
    //     path: 'elevator-control',
    //     component: () => import('../flood-station/device-control/elevator-control'),
    //     label: '垂梯控制'
    // }
];
export let deviceControlRoutes = _deviceControlRoutes;
export const _emergencySuppliesRoutes = [
    {
        path: 'config',
        component: () => import('../flood-station/emergency-supplies/Config.vue'),
        label: '应急物资配置要求'
    },
    {
        path: 'apply',
        component: () => import('../flood-station/emergency-supplies/Apply.vue'),
        label: '应急物资申请'
    },
    {
        path: 'apply-detail',
        component: () => import('../flood-station/emergency-supplies/components/apply-detail.vue'),
        label: '应急物资申请详情',
        hidden: true
    }
];
export function LineFloodRoutes () {
    return [
        {
            path: '/flood-line/assessment',
            component: () => import('./assessment'),
            label: '线路水淹评估',
            name: 'LineAssessment'
        },
        {
            path: '/flood-line/waterPumpMonitor',
            component: () => import('./assessment/components/water-pump-monitor.vue'),
            label: '线路水泵监视',
            name: 'LinePumpMonitor'
        },
        {
            path: '/plan/flood-line/plan-solve',
            component: () => import('./plan-solve'),
            label: '处置预案'
        },
        {
            path: '/flood-line/device-control',
            component: () => import('../flood-line/device-control'),
            redirect: '/flood-line/device-control/broadcast-release',
            label: '设备控制',
            children: _deviceControlRoutes
        },
        {
            path: '/flood-line/emergency-supplies',
            component: () => import('../flood-station/emergency-supplies'),
            label: '应急物资',
            children: _emergencySuppliesRoutes
        },
        {
            path: '/flood-line/videoLink',
            component: () => import('./assessment/components/video-dialog.vue'),
            label: '视频联动'
        },
        {
            name: 'detectionResult',
            path: '/flood-line/detectionResult',
            component: () => import('./assessment/components/WaterPumpFaultDetectionResult'),
            label: '水泵故障检测结果'
        },
        {
            name: 'createWorkOrder',
            path: '/flood-line/createWorkOrder',
            component: () => import('./assessment/components/CreateWorkOrder'),
            label: '创建工单'
        },
        {
            name: 'disposalReport',
            path: '/flood-line/disposalReport',
            component: () => import('./plan-solve/components/disposal-report'),
            label: '处置报告'
        }
    ];
}
