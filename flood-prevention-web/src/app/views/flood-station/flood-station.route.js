const _deviceControlRoutes = [
    {
        path: 'broadcast-release',
        component: () => import('./device-control/broadcast-release'),
        label: '广播发布',
        name: 'StationBroadcastRelease'
    },
    {
        path: 'manual-broadcast-release',
        component: () => import('./device-control/manual-broadcast-release'),
        label: '人工广播发布'
    },
    {
        path: 'pis-device-control',
        component: () => import('./device-control/pis-device-control'),
        label: 'PIS设备控制'
    },
    {
        path: 'pis-text-delivery',
        component: () => import('./device-control/pis-text-delivery'),
        label: 'PIS文本下发'
    },
    {
        path: 'pis-scene-change',
        component: () => import('./device-control/pis-scene-change'),
        label: '切换PIS场景'
    },
    {
        path: 'station-access-control',
        component: () => import('./device-control/station-access-control'),
        label: '车站门禁控制'
    },
    {
        path: 'afc-station-system-control',
        component: () => import('./device-control/afc-station-system-control'),
        label: 'AFC车站系统控制'
    },
    {
        path: 'afc-device-control',
        component: () => import('./device-control/afc-device-control'),
        label: 'AFC设备控制'
    },
    {
        path: 'rolling-door-control',
        component: () => import('./device-control/rolling-door-control'),
        label: '卷帘门控制'
    },
    {
        path: 'escalator-control',
        component: () => import('./device-control/escalator-control'),
        label: '扶梯控制'
    },
    {
        path: 'elevator-control',
        component: () => import('./device-control/elevator-control'),
        label: '垂梯控制'
    }
];

export const _emergencySuppliesRoutes = [
    {
        path: 'config',
        component: () => import('./emergency-supplies/Config.vue'),
        label: '应急物资配置要求'
    },
    {
        path: 'apply',
        component: () => import('./emergency-supplies/Apply.vue'),
        label: '应急物资申请'
    },
    {
        path: 'apply-detail',
        component: () => import('./emergency-supplies/components/apply-detail.vue'),
        label: '应急物资申请详情',
        hidden: true
    }
];
export function StationFloodRoutes () {
    return [
        {
            path: '/flood-station/assessment',
            component: () => import('./assessment'),
            label: '车站水淹评估'
        },
        {
            path: '/flood-station/flood-assessment',
            component: () => import('./assessment/components/flood-assessment.vue'),
            label: '主体车站水淹评估'
        },
        {
            path: '/flood-station/waterPumpMonitor',
            component: () => import('./assessment/components/water-pump-monitor.vue'),
            label: '车站水泵监视'
        },
        {
            path: '/flood-station/device-control',
            component: () => import('./device-control'),
            redirect: '/flood-station/device-control/broadcast-release',
            label: '设备控制',
            children: _deviceControlRoutes
        },
        {
            path: '/plan/flood-station/plan-solve',
            component: () => import('./plan-solve'),
            label: '处置预案'
        },
        {
            path: '/flood-station/emergency-supplies',
            component: () => import('./emergency-supplies'),
            redirect: '/flood-station/emergency-supplies/config',
            label: '应急物资',
            children: _emergencySuppliesRoutes
        },
        {
            path: '/flood-station/videoLink',
            component: () => import('./assessment/components/videoLink.vue'),
            label: '视频联动'
        },
        {
            path: '/flood-station/more-function',
            component: () => import('./more-function'),
            label: '更多功能'
        }
    ];
}

export let deviceControlRoutes = _deviceControlRoutes;
