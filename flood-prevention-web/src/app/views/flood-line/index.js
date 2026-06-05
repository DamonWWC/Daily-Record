// // 不推荐注掉的这种写法
// export { default as Menu1 } from './menu1/menu1.vue';
// export { default as Menu2 } from './menu2/menu2.vue';
// export { default as Menu3 } from './menu3/menu3.vue';

// // 请使用异步 ()=>import(xxxxx) 这种引入方式，以实现懒加载！！！
export const LineAssessment = () => import('./assessment');
