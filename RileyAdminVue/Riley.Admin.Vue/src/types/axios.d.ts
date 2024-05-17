// eslint-disable-next-line @typescript-eslint/no-unused-vars
import * as axios from 'axios'

declare module 'axios' {
    export interface AxiosResponse<T = any> {
        code:number
        data:T 
        message:string
        type?:string
        [key: string]:T
    }
}