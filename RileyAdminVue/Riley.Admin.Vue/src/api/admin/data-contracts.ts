export interface AuthLoginInput {
    userName:string
    password:string
    passwordKey?:string | null
    captchaId?:string | null
    captchaData?:string | null
}


export interface ResultOutputObject{
    success?:boolean
    code?:string | null
    msg?:string | null
    data?:any
}