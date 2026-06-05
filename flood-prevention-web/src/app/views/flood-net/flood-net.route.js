const _deviceControlRoutes = [
    {
        path: 'broadcast-release',
        component: () => import('../flood-station/device-control/broadcast-release'),
        label: '广播发布',
        name: 'NetBroadcastRelease'
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
export function NetFloodRoutes () {
    return [
        {
            path: '/flood-net/assessment',
            component: () => import('./assessment'),
            label: '线网水淹评估',
            name: 'NetAssessment'
        },
        {
            path: '/plan/flood-net/plan-solve',
            component: () => import('./plan-solve'),
            label: '处置预案'
        },
        {
            path: '/flood-net/device-control',
            component: () => import('../flood-net/device-control'),
            redirect: '/flood-net/device-control/broadcast-release',
            label: '设备控制',
            children: _deviceControlRoutes
        },
        {
            path: '/flood-net/emergency-supplies',
            component: () => import('../flood-station/emergency-supplies'),
            label: '应急物资',
            children: _emergencySuppliesRoutes
        }
    ];
}
