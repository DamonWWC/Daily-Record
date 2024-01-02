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
console.log([...routes,routes1]);
const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes:[...routes,routes1]
})

export const allroute= [...routes,routes1]

export default router
