import type { AuthLoginInput, AuthMobileLoginInput, ResultOutputObject } from './data-contracts'
import { ContentType, HttpClient, type RequestParams } from './http-client'

export class AuthApi<SecurityDataType = unknown> extends HttpClient<SecurityDataType> {

    login = (data: AuthLoginInput, params: RequestParams = {}) =>
        this.request<ResultOutputObject, any>({
            path: `/api/admin/auth/login`,
            method: 'POST',
            body: data,
            secure: true,
            type: ContentType.Json,
            format: 'json',
            ...params
        })


    mobileLogin=(data:AuthMobileLoginInput,params:RequestParams={}) => 
        this.request<ResultOutputObject,any>({
            path:'/api/admin/auth/mobile-login',
            method:'POST',
            body:data,
            secure:true,
            type:ContentType.Json,
            format:'json',
            ...params,
        })
}