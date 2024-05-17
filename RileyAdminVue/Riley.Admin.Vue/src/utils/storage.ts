import Cookies from 'js-cookie'


/**
 * 浏览器永久缓存
 */
export const Local={
    getKey(key:string){
        return `${__NEXT_NAME__}:${key}`
    },
    set(key:string,val:any){
        window.localStorage.setItem(Local.getKey(key),JSON.stringify(val))
    },
    get(key:string){
        const json=<string>window.localStorage.getItem(Local.getKey(key))
        return JSON.parse(json)
    },
    remove(key:string){
        window.localStorage.removeItem(Local.getKey(key))
    },
    clear(){
        window.localStorage.clear()
    }
}


export const Session={
    set(key:string,val:any){
        if(key==='token') return Cookies.set(key,val)
        window.sessionStorage.setItem(Local.getKey(key),JSON.stringify(val))
    },
    get(key:string){
        if(key==='token') return Cookies.get(key)
        const json=<string>window.sessionStorage.getItem(Local.getKey(key))
        return JSON.parse(json)
    },
    remove(key:string){
        if(key==='token') return Cookies.remove(key)
        window.sessionStorage.removeItem(Local.getKey(key))
    },
    clear(){
        Cookies.remove('token')
        window.sessionStorage.clear()
    }

}