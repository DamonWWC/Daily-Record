// 获取点位code对应的值
export function getValueByCode (array, code) {
    let point = array.find(p => p.code === code);
    return point?.value;
}
