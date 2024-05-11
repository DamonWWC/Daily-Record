// eslint-disable-next-line @typescript-eslint/no-unused-vars
import * as axios from 'axios'

declare module 'axios'{
    export interface AxiosResponse<T = any>{
        success:boolean
        msg:string
    }
}