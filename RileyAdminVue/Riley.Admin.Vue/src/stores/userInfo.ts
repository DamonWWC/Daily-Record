import {defineStore} from 'pinia'
import {Local} from '@/utils/storage'

export const adminTokenKey='admin-token'


export const useUserInfo=defineStore('userInfo',{
    state:():UserInfosState=>({
        userInfos:{
            token:Local.get(adminTokenKey)|| '',
            userName:'',
            photo:'',
            time:0,
            roles:[],
            authBtnList:[]
        },
    }),
    actions:{
        setUserInfos(){

        },
        setUserName(userName:string){
            this.userInfos.userName=userName
        },
        setPhoto(photo:string){
            this.userInfos.photo=photo
        },
        setToken(token:string){
            this.userInfos.token=token
        },
        getToken(){
            const token=Local.get(adminTokenKey)
            this.userInfos.token=token
            return token
        },
        removeToken(){
            this.userInfos.token=''
            Local.remove(adminTokenKey)
        },
        clear(){
            this.userInfos.token=''
            Local.remove(adminTokenKey)
            window.requests=[]
            window.location.reload()
        },
        getApiUserInfo(){
            
        }

    }
})