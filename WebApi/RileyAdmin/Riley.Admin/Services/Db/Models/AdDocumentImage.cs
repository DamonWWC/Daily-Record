using System;
using System.Collections.Generic;

namespace Riley.Admin.Services.Db.Models;

/// <summary>
/// 文档图片
/// </summary>
public partial class AdDocumentImage
{
    /// <summary>
    /// 主键Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 文档Id
    /// </summary>
    public long DocumentId { get; set; }

    /// <summary>
    /// 请求路径
    /// </summary>
    public string? Url { get; set; }

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
}
