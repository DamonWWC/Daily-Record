export interface AuthLoginInput{
    userName:string
    password:string
    passwordKey?:string | null
    captchaId?:string | null
    captchaData?:string | null
}