using System;
using System.Collections.Generic;

namespace AspNetCoreDemo.Models;

/// <summary>
/// 文件
/// </summary>
public partial class AdFile
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// OSS供应商
    /// </summary>
    public string? Provider { get; set; }

    /// <summary>
    /// 存储桶名称
    /// </summary>
    public string? BucketName { get; set; }

    /// <summary>
    /// 文件目录
    /// </summary>
    public string? FileDirectory { get; set; }

    /// <summary>
    /// 文件Guid
    /// </summary>
    public Guid FileGuid { get; set; }

    /// <summary>
    /// 保存文件名
    /// </summary>
    public string? SaveFileName { get; set; }

    /// <summary>
    /// 文件名
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// 文件扩展名
    /// </summary>
    public string? Extension { get; set; }

    /// <summary>
    /// 文件字节长度
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// 文件大小格式化
    /// </summary>
    public string? SizeFormat { get; set; }

    /// <summary>
    /// 链接地址
    /// </summary>
    public string? LinkUrl { get; set; }

    /// <summary>
    /// md5码，防止上传重复文件
    /// </summary>
    public string? Md5 { get; set; }

    /// <summary>
    /// 创建者用户Id
    /// </summary>
    public long? CreatedUserId { get; set; }

    /// <summary>
    /// 创建者用户名
    /// </summary>
    public string? CreatedUserName { get; set; }

    /// <summary>
    /// 创建者姓名
    /// </summary>
    public string? CreatedUserRealName { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime? CreatedTime { get; set; }

    /// <summary>
    /// 修改者用户Id
    /// </summary>
    public long? ModifiedUserId { get; set; }

    /// <summary>
    /// 修改者用户名
    /// </summary>
    public string? ModifiedUserName { get; set; }

    /// <summary>
    /// 修改者姓名
    /// </summary>
    public string? ModifiedUserRealName { get; set; }

    /// <summary>
    /// 修改时间
    /// </summary>
    public DateTime? ModifiedTime { get; set; }

    /// <summary>
    /// 是否删除
    /// </summary>
    public ulong IsDeleted { get; set; }
}
