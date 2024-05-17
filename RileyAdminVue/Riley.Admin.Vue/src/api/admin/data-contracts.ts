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

/** 手机号登录信息 */
export interface AuthMobileLoginInput {
    /**
     * 手机号
     * @minLength 1
     */
    mobile: string
    /**
     * 验证码
     * @minLength 1
     */
    code: string
    /**
     * 验证码Id
     * @minLength 1
     */
    codeId: string
  }