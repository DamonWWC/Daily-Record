import { createRouter, createWebHistory } from 'vue-router'
// import HomeView from '../views/HomeView.vue'

const pages =import.meta.glob('../views/**/page.js',{
  eager:true,
  import:'default'
})

const comps=import.meta.glob('../views/**/index.vue');
const routes= Object.entries(pages).map(([path,meta])=>{
  const comPath=path.replace('page.js','index.vue');
  
  path=path.replace('../views','').replace('/page.js','')|| '/';
  console.log(comps);
  const name=path.split('/').filter(Boolean).join('-')||'index'
  return {
    path,
    name,
    component:comps[comPath],
    meta
  }
}).sort((a,b)=>{
  return a.meta.menuOrder-b.meta.menuOrder
});
const routes1={
  path:'/',
  name:'index',
  component:()=>import('../views/home/index.vue'),
  children:routes,
  meta:{
    title:'首页'
  }
};
export const allroute= [...routes]
console.log(allroute);
const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes:allroute
})

export default router
