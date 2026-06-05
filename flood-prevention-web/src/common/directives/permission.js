/**
 * v-permission="String|Array<String>"
 * v-permission:${arg}
 * arg：从第x级父级往下删除，比如使用了ecp-button-group的这里是 2
 */

import Utils from '../utils';
export const permission = {
    inserted: function (el, binding) {
        if (!Utils.permissionCheck(binding.value)) {
            let remainLevel = Number(binding.arg);
            if (
                !remainLevel ||
                Number.isNaN(Number(remainLevel)) ||
                remainLevel < 1
            ) {
                remainLevel = 1;
            }
            let parentNode = el;
            let currentNode = null;
            for (let level = remainLevel; level > 0; level--) {
                currentNode = parentNode;
                parentNode = parentNode.parentNode;
                if (!parentNode) {
                    break;
                }
            }
            if (parentNode && currentNode) {
                parentNode.removeChild(currentNode);
            }
        }
    }
};
