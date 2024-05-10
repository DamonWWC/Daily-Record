/**
 * pinia 类型定义
 */

// 用户信息
declare interface UserInfos<T = any> {
    token: string
    authBtnList: string[]
    photo: string
    roles: string[]
    time: number
    userName: string
    [key: string]: T
}
declare interface UserInfosState {
    userInfos: UserInfos
}