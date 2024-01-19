
Object.prototype[Symbol.iterator]=function* (){
    // return Object.values(this)[Symbol.iterator]();
    yield* Object.values(this);//生成器函数
}

// Object.prototype[Symbol.iterator]=function (){
//     return Object.values(this)[Symbol.iterator]();  
// }
//让一下代码成立
var [a,b]={
    a:3,
    b:4
}
console.log(a,b);


//由于对象本身不可迭代，所以要实现需要给对象变为可迭代对象  
// // {
    // [Symbol.iterator]:function(){
    //     return 迭代器
    // }
// }