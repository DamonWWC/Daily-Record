// 根椐元素宽度自适应字体大小
export const scaleFontSize = (key) => {
    const autoSetScale = (element) => {
        const maxWidth = element.dataset.maxWidth || 124;
        if (element.clientWidth <= maxWidth) {
            return;
        }
        const scaleNum = maxWidth / element.clientWidth;
        element.style.transform = 'scale(' + scaleNum + ')';
        element.style.transformOrigin = '0% 50%';
    };
    const autoScale = document.querySelectorAll(key);
    for (var i = 0; i < autoScale.length; i++) {
        autoSetScale(autoScale[i]);
    }
};

export const handleDownload = (response, name) => {
    // const headers = res.headers['content-disposition'];
    const filename = `${name}.xls`;// decodeURIComponent(String(headers).split('attachment; filename=')[1])
    var url = window.URL.createObjectURL(response.data);
    var a = document.createElement('a');
    a.href = url;
    a.download = filename;
    a.click();
};

// 单元格合并
export const getSpanArr = (tableData, mergeArr) => {
    let mergeObj = {};
    mergeArr.forEach((key) => {
        let count = 0; // 用来记录需要合并行的起始位置
        mergeObj[key] = []; // 记录每一列的合并信息
        tableData.forEach((item, index) => {
            // index == 0表示数据为第一行，直接 push 一个 1
            if (index === 0) {
                mergeObj[key].push(1);
            } else {
                // 判断当前行是否与上一行其值相等 如果相等 在 count 记录的位置其值 +1 表示当前行需要合并 并push 一个 0 作为占位
                if (item[key] === tableData[index - 1][key]) {
                    mergeObj[key][count] += 1;
                    mergeObj[key].push(0);
                } else {
                    // 如果当前行和上一行其值不相等
                    count = index; // 记录当前位置
                    mergeObj[key].push(1); // 重新push 一个 1
                }
            }
        });
    });
    return mergeObj;
};

// 单元格合并
export const objectSpanMethod = ({ row, column, rowIndex, columnIndex }, mergeArr, mergeObj) => {
    // 判断列的属性
    if (mergeArr.indexOf(column.property) !== -1) {
        // 判断其值是不是为0
        if (mergeObj[column.property][rowIndex]) {
            return [mergeObj[column.property][rowIndex], 1];
        } else {
            // 如果为0则为需要合并的行
            return [0, 0];
        }
    }
};

function getPath (route, name) {
    if (route.name === name) return route.path;
    if (route.children && route.children.length > 0) {
        // console.log('rout', route.children);
        for (var i = 0; i < route.children.length; i++) {
            const result = getPath(route.children[i], name);
            if (result) {
                return `${route.path}/${result}`;
            }
        }
    }
    return null;
}
function getPath1 (route, name) {
    if (route.name === name) return route.path;
    const childResult = route.children && route.children.find(child => getPath1(child, name));
    console.log('childrenresult:', childResult);
    return childResult ? `${route.path}/${childResult}` : null;
}

export const getAddress = (routes, name, currentPath) => {
    if (routes && routes.length > 0) {
        for (var i = 0; i < routes.length; i++) {
            const address = getPath(routes[i], name);
            if (address) {
                // const index1 = window.location.href.indexOf('#');
                // const index2 = window.location.href.indexOf('?') === -1 ? window.location.href.length : window.location.href.indexOf('?');
                // const replacestr = window.location.href.substring(index1 + 1, index2);
                // console.log('re', window.location.href, replacestr);
                return window.location.href.replace(currentPath, address);
            }
        }
    }
    return null;
};
