import { Exception404 } from './index';

export function ExceptionRoutes () {
    return [
        {
            path: '**',
            label: '暂未开放',
            name: 'Exception404',
            component: Exception404
        }
    ];
}
