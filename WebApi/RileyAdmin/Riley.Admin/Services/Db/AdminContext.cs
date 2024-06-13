using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;
using Riley.Admin.Services.Db.Models;

namespace Riley.Admin.Services.Db;

public partial class AdminContext : DbContext
{
    public AdminContext()
    {
    }

    public AdminContext(DbContextOptions<AdminContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AdApi> AdApis { get; set; }

    public virtual DbSet<AdAppLog> AdAppLogs { get; set; }

    public virtual DbSet<AdDict> AdDicts { get; set; }

    public virtual DbSet<AdDictType> AdDictTypes { get; set; }

    public virtual DbSet<AdDocument> AdDocuments { get; set; }

    public virtual DbSet<AdDocumentImage> AdDocumentImages { get; set; }

    public virtual DbSet<AdFile> AdFiles { get; set; }

    public virtual DbSet<AdLoginLog> AdLoginLogs { get; set; }

    public virtual DbSet<AdOprationLog> AdOprationLogs { get; set; }

    public virtual DbSet<AdOrg> AdOrgs { get; set; }

    public virtual DbSet<AdPermission> AdPermissions { get; set; }

    public virtual DbSet<AdPermissionApi> AdPermissionApis { get; set; }

    public virtual DbSet<AdPkg> AdPkgs { get; set; }

    public virtual DbSet<AdPkgPermission> AdPkgPermissions { get; set; }

    public virtual DbSet<AdRole> AdRoles { get; set; }

    public virtual DbSet<AdRoleOrg> AdRoleOrgs { get; set; }

    public virtual DbSet<AdRolePermission> AdRolePermissions { get; set; }

    public virtual DbSet<AdTenant> AdTenants { get; set; }

    public virtual DbSet<AdTenantPermission> AdTenantPermissions { get; set; }

    public virtual DbSet<AdTenantPkg> AdTenantPkgs { get; set; }

    public virtual DbSet<AdUser> AdUsers { get; set; }

    public virtual DbSet<AdUserOrg> AdUserOrgs { get; set; }

    public virtual DbSet<AdUserRole> AdUserRoles { get; set; }

    public virtual DbSet<AdUserStaff> AdUserStaffs { get; set; }

    public virtual DbSet<AdView> AdViews { get; set; }

    public virtual DbSet<AppTask> AppTasks { get; set; }

    public virtual DbSet<AppTaskExt> AppTaskExts { get; set; }

    public virtual DbSet<AppTaskLog> AppTaskLogs { get; set; }

    public virtual DbSet<BaseRegion> BaseRegions { get; set; }

    public virtual DbSet<Demo> Demos { get; set; }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseMySql("server=119.23.227.72;port=3307;database=admin;uid=root;pwd=123456;charset=utf8mb4;sslmode=none;min pool size=1", ServerVersion.Parse("8.0.36-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<AdApi>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("ad_api", tb => tb.HasComment("接口管理"));

            entity.HasIndex(e => new { e.ParentId, e.Path }, "idx_ad_api_01").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasComment("主键Id");
            entity.Property(e => e.CreatedTime)
                .HasComment("创建时间")
                .HasColumnType("datetime(3)");
            entity.Property(e => e.CreatedUserId).HasComment("创建者用户Id");
            entity.Property(e => e.CreatedUserName)
                .HasMaxLength(50)
                .HasComment("创建者用户名");
            entity.Property(e => e.CreatedUserRealName)
                .HasMaxLength(50)
                .HasComment("创建者姓名");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasComment("说明");
            entity.Property(e => e.Enabled)
                .HasComment("启用")
                .HasColumnType("bit(1)");
            entity.Property(e => e.HttpMethods)
                .HasMaxLength(50)
                .HasComment("接口提交方法");
            entity.Property(e => e.IsDeleted)
                .HasComment("是否删除")
                .HasColumnType("bit(1)");
            entity.Property(e => e.Label)
                .HasMaxLength(500)
                .HasComment("接口名称");
            entity.Property(e => e.ModifiedTime)
                .HasComment("修改时间")
                .HasColumnType("datetime(3)");
            entity.Property(e => e.ModifiedUserId).HasComment("修改者用户Id");
            entity.Property(e => e.ModifiedUserName)
                .HasMaxLength(50)
                .HasComment("修改者用户名");
            entity.Property(e => e.ModifiedUserRealName)
                .HasMaxLength(50)
                .HasComment("修改者姓名");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasComment("接口命名");
            entity.Property(e => e.ParentId).HasComment("所属模块");
            entity.Property(e => e.Path)
                .HasMaxLength(500)
                .HasComment("接口地址");
            entity.Property(e => e.Sort).HasComment("排序");
        });

        modelBuilder.Entity<AdAppLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("ad_app_log", tb => tb.HasComment("应用程序日志"));

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasComment("主键Id");
            entity.Property(e => e.Callsite).HasMaxLength(300);
            entity.Property(e => e.Exception).HasColumnType("text");
            entity.Property(e => e.Level).HasMaxLength(5);
            entity.Property(e => e.Logged).HasColumnType("datetime(3)");
            entity.Property(e => e.Logger).HasMaxLength(300);
            entity.Property(e => e.Message).HasColumnType("text");
            entity.Property(e => e.Properties).HasColumnType("text");
        });

        modelBuilder.Entity<AdDict>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("ad_dict", tb => tb.HasComment("数据字典"));

            entity.HasIndex(e => new { e.DictTypeId, e.Name }, "idx_ad_dict_01").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasComment("主键Id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasComment("字典编码");
            entity.Property(e => e.CreatedTime)
                .HasComment("创建时间")
                .HasColumnType("datetime(3)");
            entity.Property(e => e.CreatedUserId).HasComment("创建者用户Id");
            entity.Property(e => e.CreatedUserName)
                .HasMaxLength(50)
                .HasComment("创建者用户名");
            entity.Property(e => e.CreatedUserRealName)
                .HasMaxLength(50)
                .HasComment("创建者姓名");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasComment("描述");
            entity.Property(e => e.DictTypeId).HasComment("字典类型Id");
            entity.Property(e => e.Enabled)
                .HasComment("启用")
                .HasColumnType("bit(1)");
            entity.Property(e => e.IsDeleted)
                .HasComment("是否删除")
                .HasColumnType("bit(1)");
            entity.Property(e => e.ModifiedTime)
                .HasComment("修改时间")
                .HasColumnType("datetime(3)");
            entity.Property(e => e.ModifiedUserId).HasComment("修改者用户Id");
            entity.Property(e => e.ModifiedUserName)
                .HasMaxLength(50)
                .HasComment("修改者用户名");
            entity.Property(e => e.ModifiedUserRealName)
                .HasMaxLength(50)
                .HasComment("修改者姓名");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasComment("字典名称");
            entity.Property(e => e.Sort).HasComment("排序");
            entity.Property(e => e.Value)
                .HasMaxLength(50)
                .HasComment("字典值");
        });

        modelBuilder.Entity<AdDictType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("ad_dict_type", tb => tb.HasComment("数据字典类型"));

            entity.HasIndex(e => e.Name, "idx_ad_dict_type_01").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasComment("主键Id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasComment("编码");
            entity.Property(e => e.CreatedTime)
                .HasComment("创建时间")
                .HasColumnType("datetime(3)");
            entity.Property(e => e.CreatedUserId).HasComment("创建者用户Id");
            entity.Property(e => e.CreatedUserName)
                .HasMaxLength(50)
                .HasComment("创建者用户名");
            entity.Property(e => e.CreatedUserRealName)
                .HasMaxLength(50)
                .HasComment("创建者姓名");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasComment("描述");
            entity.Property(e => e.Enabled)
                .HasComment("启用")
                .HasColumnType("bit(1)");
            entity.Property(e => e.IsDeleted)
                .HasComment("是否删除")
                .HasColumnType("bit(1)");
            entity.Property(e => e.ModifiedTime)
                .HasComment("修改时间")
                .HasColumnType("datetime(3)");
            entity.Property(e => e.ModifiedUserId).HasComment("修改者用户Id");
            entity.Property(e => e.ModifiedUserName)
                .HasMaxLength(50)
                .HasComment("修改者用户名");
            entity.Property(e => e.ModifiedUserRealName)
                .HasMaxLength(50)
                .HasComment("修改者姓名");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasComment("名称");
            entity.Property(e => e.Sort).HasComment("排序");
        });

        modelBuilder.Entity<AdDocument>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("ad_document", tb => tb.HasComment("文档"));

            entity.HasIndex(e => new { e.ParentId, e.Label, e.TenantId }, "idx_ad_document_01").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasComment("主键Id");
            entity.Property(e => e.Content)
                .HasComment("内容")
                .HasColumnType("text");
            entity.Property(e => e.CreatedTime)
                .HasComment("创建时间")
                .HasColumnType("datetime(3)");
            entity.Property(e => e.CreatedUserId).HasComment("创建者用户Id");
            entity.Property(e => e.CreatedUserName)
                .HasMaxLength(50)
                .HasComment("创建者用户名");
            entity.Property(e => e.CreatedUserRealName)
                .HasMaxLength(50)
                .HasComment("创建者姓名");
            entity.Property(e => e.Description)
                .HasMaxLength(100)
                .HasComment("描述");
            entity.Property(e => e.Enabled)
                .HasComment("启用")
                .HasColumnType("bit(1)");
            entity.Property(e => e.Html)
                .HasComment("Html")
                .HasColumnType("text");
            entity.Property(e => e.IsDeleted)
                .HasComment("是否删除")
                .HasColumnType("bit(1)");
            entity.Property(e => e.Label)
                .HasMaxLength(50)
                .HasComment("名称");
            entity.Property(e => e.ModifiedTime)
                .HasComment("修改时间")
                .HasColumnType("datetime(3)");
            entity.Property(e => e.ModifiedUserId).HasComment("修改者用户Id");
            entity.Property(e => e.ModifiedUserName)
                .HasMaxLength(50)
                .HasComment("修改者用户名");
            entity.Property(e => e.ModifiedUserRealName)
                .HasMaxLength(50)
                .HasComment("修改者姓名");
            entity.Property(e => e.Name)
                .HasMaxLength(500)
                .HasComment("命名");
            entity.Property(e => e.Opened)
                .HasComment("打开组")
                .HasColumnType("bit(1)");
            entity.Property(e => e.ParentId).HasComment("父级节点");
            entity.Property(e => e.Sort).HasComment("排序");
            entity.Property(e => e.TenantId).HasComment("租户Id");
            entity.Property(e => e.Type).HasComment("类型");
        });

        modelBuilder.Entity<AdDocumentImage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("ad_document_image", tb => tb.HasComment("文档图片"));

            entity.HasIndex(e => new { e.DocumentId, e.Url }, "idx_ad_document_image_01").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasComment("主键Id");
            entity.Property(e => e.CreatedTime)
                .HasComment("创建时间")
                .HasColumnType("datetime(3)");
            entity.Property(e => e.CreatedUserId).HasComment("创建者用户Id");
            entity.Property(e => e.CreatedUserName)
                .HasMaxLength(50)
                .HasComment("创建者用户名");
            entity.Property(e => e.CreatedUserRealName)
                .HasMaxLength(50)
                .HasComment("创建者姓名");
            entity.Property(e => e.DocumentId).HasComment("文档Id");
            entity.Property(e => e.Url).HasComment("请求路径");
        });

        modelBuilder.Entity<AdFile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("ad_file", tb => tb.HasComment("文件"));

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasComment("主键Id");
            entity.Property(e => e.BucketName)
                .HasMaxLength(200)
                .HasComment("存储桶名称");
            entity.Property(e => e.CreatedTime)
                .HasComment("创建时间")
                .HasColumnType("datetime(3)");
            entity.Property(e => e.CreatedUserId).HasComment("创建者用户Id");
            entity.Property(e => e.CreatedUserName)
                .HasMaxLength(50)
                .HasComment("创建者用户名");
            entity.Property(e => e.CreatedUserRealName)
                .HasMaxLength(50)
                .HasComment("创建者姓名");
            entity.Property(e => e.Extension)
                .HasMaxLength(20)
                .HasComment("文件扩展名");
            entity.Property(e => e.FileDirectory)
                .HasMaxLength(500)
                .HasComment("文件目录");
            entity.Property(e => e.FileGuid).HasComment("文件Guid");
            entity.Property(e => e.FileName)
                .HasMaxLength(200)
                .HasComment("文件名");
            entity.Property(e => e.IsDeleted)
                .HasComment("是否删除")
                .HasColumnType("bit(1)");
            entity.Property(e => e.LinkUrl)
                .HasMaxLength(500)
                .HasComment("链接地址");
            entity.Property(e => e.Md5)
                .HasMaxLength(50)
                .HasComment("md5码，防止上传重复文件");
            entity.Property(e => e.ModifiedTime)
                .HasComment("修改时间")
                .HasColumnType("datetime(3)");
            entity.Property(e => e.ModifiedUserId).HasComment("修改者用户Id");
            entity.Property(e => e.ModifiedUserName)
                .HasMaxLength(50)
                .HasComment("修改者用户名");
            entity.Property(e => e.ModifiedUserRealName)
                .HasMaxLength(50)
                .HasComment("修改者姓名");
            entity.Property(e => e.Provider)
                .HasMaxLength(50)
                .HasComment("OSS供应商");
            entity.Property(e => e.SaveFileName)
                .HasMaxLength(200)
                .HasComment("保存文件名");
            entity.Property(e => e.Size).HasComment("文件字节长度");
            entity.Property(e => e.SizeFormat)
                .HasMaxLength(50)
                .HasComment("文件大小格式化");
        });

        modelBuilder.Entity<AdLoginLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("ad_login_log", tb => tb.HasComment("登录日志"));

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasComment("主键Id");
            entity.Property(e => e.Browser)
                .HasMaxLength(100)
                .HasComment("浏览器");
            entity.Property(e => e.BrowserInfo)
                .HasComment("浏览器信息")
                .HasColumnType("text");
            entity.Property(e => e.CreatedTime)
                .HasComment("创建时间")
                .HasColumnType("datetime(3)");
            entity.Property(e => e.CreatedUserId).HasComment("创建者用户Id");
            entity.Property(e => e.CreatedUserName)
                .HasMaxLength(50)
                .HasComment("创建者用户名");
            entity.Property(e => e.CreatedUserRealName)
                .HasMaxLength(50)
                .HasComment("创建者姓名");
            entity.Property(e => e.Device)
                .HasMaxLength(50)
                .HasComment("设备");
            entity.Property(e => e.ElapsedMilliseconds).HasComment("耗时（毫秒）");
            entity.Property(e => e.Ip)
                .HasMaxLength(100)
                .HasComment("IP")
                .HasColumnName("IP");
            entity.Property(e => e.Msg)
                .HasMaxLength(500)
                .HasComment("操作消息");
            entity.Property(e => e.Name)
                .HasMaxLength(60)
                .HasComment("姓名");
            entity.Property(e => e.Os)
                .HasMaxLength(100)
                .HasComment("操作系统");
            entity.Property(e => e.Result)
                .HasComment("操作结果")
                .HasColumnType("text");
            entity.Property(e => e.Status)
                .HasComment("操作状态")
                .HasColumnType("bit(1)");
            entity.Property(e => e.TenantId).HasComment("租户Id");
        });

        modelBuilder.Entity<AdOprationLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("ad_opration_log", tb => tb.HasComment("操作日志"));

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasComment("主键Id");
            entity.Property(e => e.ApiLabel)
                .HasMaxLength(50)
                .HasComment("接口名称");
            entity.Property(e => e.ApiMethod)
                .HasMaxLength(50)
                .HasComment("接口提交方法");
            entity.Property(e => e.ApiPath)
                .HasMaxLength(500)
                .HasComment("接口地址");
            entity.Property(e => e.Browser)
                .HasMaxLength(100)
                .HasComment("浏览器");
            entity.Property(e => e.BrowserInfo)
                .HasComment("浏览器信息")
                .HasColumnType("text");
            entity.Property(e => e.CreatedTime)
                .HasComment("创建时间")
                .HasColumnType("datetime(3)");
            entity.Property(e => e.CreatedUserId).HasComment("创建者用户Id");
            entity.Property(e => e.CreatedUserName)
                .HasMaxLength(50)
                .HasComment("创建者用户名");
            entity.Property(e => e.CreatedUserRealName)
                .HasMaxLength(50)
                .HasComment("创建者姓名");
            entity.Property(e => e.Device)
                .HasMaxLength(50)
                .HasComment("设备");
            entity.Property(e => e.ElapsedMilliseconds).HasComment("耗时（毫秒）");
            entity.Property(e => e.Ip)
                .HasMaxLength(100)
                .HasComment("IP")
                .HasColumnName("IP");
            entity.Property(e => e.Msg)
                .HasMaxLength(500)
                .HasComment("操作消息");
            entity.Property(e => e.Name)
                .HasMaxLength(60)
                .HasComment("姓名");
            entity.Property(e => e.Os)
                .HasMaxLength(100)
                .HasComment("操作系统");
            entity.Property(e => e.Params)
                .HasComment("操作参数")
                .HasColumnType("text");
            entity.Property(e => e.Result)
                .HasComment("操作结果")
                .HasColumnType("text");
            entity.Property(e => e.Status)
                .HasComment("操作状态")
                .HasColumnType("bit(1)");
            entity.Property(e => e.TenantId).HasComment("租户Id");
        });

        modelBuilder.Entity<AdOrg>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("ad_org", tb => tb.HasComment("组织架构"));

            entity.HasIndex(e => new { e.ParentId, e.Name, e.TenantId }, "idx_ad_org_01").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasComment("主键Id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasComment("编码");
            entity.Property(e => e.CreatedTime)
                .HasComment("创建时间")
                .HasColumnType("datetime(3)");
            entity.Property(e => e.CreatedUserId).HasComment("创建者用户Id");
            entity.Property(e => e.CreatedUserName)
                .HasMaxLength(50)
                .HasComment("创建者用户名");
            entity.Property(e => e.CreatedUserRealName)
                .HasMaxLength(50)
                .HasComment("创建者姓名");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasComment("描述");
            entity.Property(e => e.Enabled)
                .HasComment("启用")
                .HasColumnType("bit(1)");
            entity.Property(e => e.IsDeleted)
                .HasComment("是否删除")
                .HasColumnType("bit(1)");
            entity.Property(e => e.MemberCount).HasComment("成员数");
            entity.Property(e => e.ModifiedTime)
                .HasComment("修改时间")
                .HasColumnType("datetime(3)");
            entity.Property(e => e.ModifiedUserId).HasComment("修改者用户Id");
            entity.Property(e => e.ModifiedUserName)
                .HasMaxLength(50)
                .HasComment("修改者用户名");
            entity.Property(e => e.ModifiedUserRealName)
                .HasMaxLength(50)
                .HasComment("修改者姓名");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasComment("名称");
            entity.Property(e => e.ParentId).HasComment("父级");
            entity.Property(e => e.Sort).HasComment("排序");
            entity.Property(e => e.TenantId).HasComment("租户Id");
            entity.Property(e => e.Value)
                .HasMaxLength(50)
                .HasComment("值");
        });

        modelBuilder.Entity<AdPermission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("ad_permission", tb => tb.HasComment("权限"));

            entity.HasIndex(e => new { e.ParentId, e.Label }, "idx_ad_permission_01").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasComment("主键Id");
            entity.Property(e => e.Code)
                .HasMaxLength(500)
                .HasComment("权限编码");
            entity.Property(e => e.CreatedTime)
                .HasComment("创建时间")
                .HasColumnType("datetime(3)");
            entity.Property(e => e.CreatedUserId).HasComment("创建者用户Id");
            entity.Property(e => e.CreatedUserName)
                .HasMaxLength(50)
                .HasComment("创建者用户名");
            entity.Property(e => e.CreatedUserRealName)
                .HasMaxLength(50)
                .HasComment("创建者姓名");
            entity.Property(e => e.Description)
                .HasMaxLength(200)
                .HasComment("描述");
            entity.Property(e => e.Enabled)
                .HasComment("启用")
                .HasColumnType("bit(1)");
            entity.Property(e => e.External)
                .HasComment("链接外显")
                .HasColumnType("bit(1)");
            entity.Property(e => e.Hidden)
                .HasComment("隐藏")
                .HasColumnType("bit(1)");
            entity.Property(e => e.Icon)
                .HasMaxLength(100)
                .HasComment("图标");
            entity.Property(e => e.IsAffix)
                .HasComment("是否固定")
                .HasColumnType("bit(1)");
            entity.Property(e => e.IsDeleted)
                .HasComment("是否删除")
                .HasColumnType("bit(1)");
            entity.Property(e => e.IsIframe)
                .HasComment("是否内嵌窗口")
                .HasColumnType("bit(1)");
            entity.Property(e => e.IsKeepAlive)
                .HasComment("是否缓存")
                .HasColumnType("bit(1)");
            entity.Property(e => e.Label)
                .HasMaxLength(50)
                .HasComment("权限名称");
            entity.Property(e => e.Link)
                .HasMaxLength(500)
                .HasComment("链接地址");
            entity.Property(e => e.ModifiedTime)
                .HasComment("修改时间")
                .HasColumnType("datetime(3)");
            entity.Property(e => e.ModifiedUserId).HasComment("修改者用户Id");
            entity.Property(e => e.ModifiedUserName)
                .HasMaxLength(50)
                .HasComment("修改者用户名");
            entity.Property(e => e.ModifiedUserRealName)
                .HasMaxLength(50)
                .HasComment("修改者姓名");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasComment("路由命名");
            entity.Property(e => e.NewWindow)
                .HasComment("打开新窗口")
                .HasColumnType("bit(1)");
            entity.Property(e => e.Opened)
                .HasComment("展开分组")
                .HasColumnType("bit(1)");
            entity.Property(e => e.ParentId).HasComment("父级节点");
            entity.Property(e => e.Path)
                .HasMaxLength(500)
                .HasComment("路由地址");
            entity.Property(e => e.Redirect)
                .HasMaxLength(500)
                .HasComment("重定向地址");
            entity.Property(e => e.Sort).HasComment("排序");
            entity.Property(e => e.Type).HasComment("权限类型");
            entity.Property(e => e.ViewId).HasComment("视图Id");
        });

        modelBuilder.Entity<AdPermissionApi>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("ad_permission_api", tb => tb.HasComment("权限接口"));

            entity.HasIndex(e => new { e.PermissionId, e.ApiId }, "idx_ad_permission_api_01").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasComment("主键Id");
            entity.Property(e => e.ApiId).HasComment("接口Id");
            entity.Property(e => e.CreatedTime)
                .HasComment("创建时间")
                .HasColumnType("datetime(3)");
            entity.Property(e => e.CreatedUserId).HasComment("创建者用户Id");
            entity.Property(e => e.CreatedUserName)
                .HasMaxLength(50)
                .HasComment("创建者用户名");
            entity.Property(e => e.CreatedUserRealName)
                .HasMaxLength(50)
                .HasComment("创建者姓名");
            entity.Property(e => e.PermissionId).HasComment("权限Id");
        });

        modelBuilder.Entity<AdPkg>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("ad_pkg", tb => tb.HasComment("套餐"));

            entity.HasIndex(e => new { e.ParentId, e.Name }, "idx_ad_pkg_01").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasComment("主键Id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasComment("编码");
            entity.Property(e => e.CreatedTime)
                .HasComment("创建时间")
                .HasColumnType("datetime(3)");
            entity.Property(e => e.CreatedUserId).HasComment("创建者用户Id");
            entity.Property(e => e.CreatedUserName)
                .HasMaxLength(50)
                .HasComment("创建者用户名");
            entity.Property(e => e.CreatedUserRealName)
                .HasMaxLength(50)
                .HasComment("创建者姓名");
            entity.Property(e => e.Description)
                .HasMaxLength(200)
                .HasComment("说明");
            entity.Property(e => e.Enabled)
                .HasComment("启用")
                .HasColumnType("bit(1)");
            entity.Property(e => e.IsDeleted)
                .HasComment("是否删除")
                .HasColumnType("bit(1)");
            entity.Property(e => e.ModifiedTime)
                .HasComment("修改时间")
                .HasColumnType("datetime(3)");
            entity.Property(e => e.ModifiedUserId).HasComment("修改者用户Id");
            entity.Property(e => e.ModifiedUserName)
                .HasMaxLength(50)
                .HasComment("修改者用户名");
            entity.Property(e => e.ModifiedUserRealName)
                .HasMaxLength(50)
                .HasComment("修改者姓名");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasComment("名称");
            entity.Property(e => e.ParentId).HasComment("父级Id");
            entity.Property(e => e.Sort).HasComment("排序");
        });

        modelBuilder.Entity<AdPkgPermission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("ad_pkg_permission", tb => tb.HasComment("套餐权限"));

            entity.HasIndex(e => new { e.PkgId, e.PermissionId }, "idx_ad_pkg_permission_01").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasComment("主键Id");
            entity.Property(e => e.CreatedTime)
                .HasComment("创建时间")
                .HasColumnType("datetime(3)");
            entity.Property(e => e.CreatedUserId).HasComment("创建者用户Id");
            entity.Property(e => e.CreatedUserName)
                .HasMaxLength(50)
                .HasComment("创建者用户名");
            entity.Property(e => e.CreatedUserRealName)
                .HasMaxLength(50)
                .HasComment("创建者姓名");
            entity.Property(e => e.PermissionId).HasComment("权限Id");
            entity.Property(e => e.PkgId).HasComment("套餐Id");
        });

        modelBuilder.Entity<AdRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("ad_role", tb => tb.HasComment("角色"));

            entity.HasIndex(e => new { e.TenantId, e.ParentId, e.Name }, "idx_ad_role_01").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasComment("主键Id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasComment("编码");
            entity.Property(e => e.CreatedTime)
                .HasComment("创建时间")
                .HasColumnType("datetime(3)");
            entity.Property(e => e.CreatedUserId).HasComment("创建者用户Id");
            entity.Property(e => e.CreatedUserName)
                .HasMaxLength(50)
                .HasComment("创建者用户名");
            entity.Property(e => e.CreatedUserRealName)
                .HasMaxLength(50)
                .HasComment("创建者姓名");
            entity.Property(e => e.DataScope).HasComment("数据范围");
            entity.Property(e => e.Description)
                .HasMaxLength(200)
                .HasComment("说明");
            entity.Property(e => e.Hidden)
                .HasComment("隐藏")
                .HasColumnType("bit(1)");
            entity.Property(e => e.IsDeleted)
                .HasComment("是否删除")
                .HasColumnType("bit(1)");
            entity.Property(e => e.ModifiedTime)
                .HasComment("修改时间")
                .HasColumnType("datetime(3)");
            entity.Property(e => e.ModifiedUserId).HasComment("修改者用户Id");
            entity.Property(e => e.ModifiedUserName)
                .HasMaxLength(50)
                .HasComment("修改者用户名");
            entity.Property(e => e.ModifiedUserRealName)
                .HasMaxLength(50)
                .HasComment("修改者姓名");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasComment("名称");
            entity.Property(e => e.ParentId).HasComment("父级Id");
            entity.Property(e => e.Sort).HasComment("排序");
            entity.Property(e => e.TenantId).HasComment("租户Id");
            entity.Property(e => e.Type).HasComment("角色类型");
        });

        modelBuilder.Entity<AdRoleOrg>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("ad_role_org", tb => tb.HasComment("角色部门"));

            entity.HasIndex(e => new { e.RoleId, e.OrgId }, "idx_ad_role_org_01").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasComment("主键Id");
            entity.Property(e => e.CreatedTime)
                .HasComment("创建时间")
                .HasColumnType("datetime(3)");
            entity.Property(e => e.CreatedUserId).HasComment("创建者用户Id");
            entity.Property(e => e.CreatedUserName)
                .HasMaxLength(50)
                .HasComment("创建者用户名");
            entity.Property(e => e.CreatedUserRealName)
                .HasMaxLength(50)
                .HasComment("创建者姓名");
            entity.Property(e => e.OrgId).HasComment("部门Id");
            entity.Property(e => e.RoleId).HasComment("角色Id");
        });

        modelBuilder.Entity<AdRolePermission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("ad_role_permission", tb => tb.HasComment("角色权限"));

            entity.HasIndex(e => new { e.RoleId, e.PermissionId }, "idx_ad_role_permission_01").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasComment("主键Id");
            entity.Property(e => e.CreatedTime)
                .HasComment("创建时间")
                .HasColumnType("datetime(3)");
            entity.Property(e => e.CreatedUserId).HasComment("创建者用户Id");
            entity.Property(e => e.CreatedUserName)
                .HasMaxLength(50)
                .HasComment("创建者用户名");
            entity.Property(e => e.CreatedUserRealName)
                .HasMaxLength(50)
                .HasComment("创建者姓名");
            entity.Property(e => e.PermissionId).HasComment("权限Id");
            entity.Property(e => e.RoleId).HasComment("角色Id");
        });

        modelBuilder.Entity<AdTenant>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("ad_tenant", tb => tb.HasComment("租户"));

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasComment("主键Id");
            entity.Property(e => e.ConnectionString)
                .HasMaxLength(500)
                .HasComment("连接字符串");
            entity.Property(e => e.CreatedTime)
                .HasComment("创建时间")
                .HasColumnType("datetime(3)");
            entity.Property(e => e.CreatedUserId).HasComment("创建者用户Id");
            entity.Property(e => e.CreatedUserName)
                .HasMaxLength(50)
                .HasComment("创建者用户名");
            entity.Property(e => e.CreatedUserRealName)
                .HasMaxLength(50)
                .HasComment("创建者姓名");
            entity.Property(e => e.DbKey)
                .HasMaxLength(50)
                .HasComment("数据库注册键");
            entity.Property(e => e.DbType).HasComment("数据库");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasComment("说明");
            entity.Property(e => e.Enabled)
                .HasComment("启用")
                .HasColumnType("bit(1)");
            entity.Property(e => e.IsDeleted)
                .HasComment("是否删除")
                .HasColumnType("bit(1)");
            entity.Property(e => e.ModifiedTime)
                .HasComment("修改时间")
                .HasColumnType("datetime(3)");
            entity.Property(e => e.ModifiedUserId).HasComment("修改者用户Id");
            entity.Property(e => e.ModifiedUserName)
                .HasMaxLength(50)
                .HasComment("修改者用户名");
            entity.Property(e => e.ModifiedUserRealName)
                .HasMaxLength(50)
                .HasComment("修改者姓名");
            entity.Property(e => e.OrgId).HasComment("授权部门");
            entity.Property(e => e.TenantType)
                .HasComment("租户类型")
                .HasColumnType("enum('Platform','Tenant')");
            entity.Property(e => e.UserId).HasComment("授权用户");
        });

        modelBuilder.Entity<AdTenantPermission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("ad_tenant_permission", tb => tb.HasComment("租户权限"));

            entity.HasIndex(e => new { e.TenantId, e.PermissionId }, "idx_ad_tenant_permission_01").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasComment("主键Id");
            entity.Property(e => e.CreatedTime)
                .HasComment("创建时间")
                .HasColumnType("datetime(3)");
            entity.Property(e => e.CreatedUserId).HasComment("创建者用户Id");
            entity.Property(e => e.CreatedUserName)
                .HasMaxLength(50)
                .HasComment("创建者用户名");
            entity.Property(e => e.CreatedUserRealName)
                .HasMaxLength(50)
                .HasComment("创建者姓名");
            entity.Property(e => e.PermissionId).HasComment("权限Id");
            entity.Property(e => e.TenantId).HasComment("租户Id");
        });

        modelBuilder.Entity<AdTenantPkg>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("ad_tenant_pkg", tb => tb.HasComment("租户套餐"));

            entity.HasIndex(e => new { e.TenantId, e.PkgId }, "idx_ad_tenant_pkg_01").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasComment("主键Id");
            entity.Property(e => e.CreatedTime)
                .HasComment("创建时间")
                .HasColumnType("datetime(3)");
            entity.Property(e => e.CreatedUserId).HasComment("创建者用户Id");
            entity.Property(e => e.CreatedUserName)
                .HasMaxLength(50)
                .HasComment("创建者用户名");
            entity.Property(e => e.CreatedUserRealName)
                .HasMaxLength(50)
                .HasComment("创建者姓名");
            entity.Property(e => e.PkgId).HasComment("套餐Id");
            entity.Property(e => e.TenantId).HasComment("租户Id");
        });

        modelBuilder.Entity<AdUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("ad_user", tb => tb.HasComment("用户"));

            entity.HasIndex(e => new { e.UserName, e.TenantId }, "idx_ad_user_01").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasComment("主键Id");
            entity.Property(e => e.Avatar)
                .HasMaxLength(500)
                .HasComment("头像");
            entity.Property(e => e.CreatedTime)
                .HasComment("创建时间")
                .HasColumnType("datetime(3)");
            entity.Property(e => e.CreatedUserId).HasComment("创建者用户Id");
            entity.Property(e => e.CreatedUserName)
                .HasMaxLength(50)
                .HasComment("创建者用户名");
            entity.Property(e => e.CreatedUserRealName)
                .HasMaxLength(50)
                .HasComment("创建者姓名");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasComment("邮箱");
            entity.Property(e => e.Enabled)
                .HasComment("启用")
                .HasColumnType("bit(1)");
            entity.Property(e => e.IsDeleted)
                .HasComment("是否删除")
                .HasColumnType("bit(1)");
            entity.Property(e => e.ManagerUserId).HasComment("直属主管Id");
            entity.Property(e => e.Mobile)
                .HasMaxLength(20)
                .HasComment("手机号");
            entity.Property(e => e.ModifiedTime)
                .HasComment("修改时间")
                .HasColumnType("datetime(3)");
            entity.Property(e => e.ModifiedUserId).HasComment("修改者用户Id");
            entity.Property(e => e.ModifiedUserName)
                .HasMaxLength(50)
                .HasComment("修改者用户名");
            entity.Property(e => e.ModifiedUserRealName)
                .HasMaxLength(50)
                .HasComment("修改者姓名");
            entity.Property(e => e.Name)
                .HasMaxLength(60)
                .HasComment("姓名");
            entity.Property(e => e.NickName)
                .HasMaxLength(60)
                .HasComment("昵称");
            entity.Property(e => e.OrgId).HasComment("主属部门Id");
            entity.Property(e => e.Password)
                .HasMaxLength(200)
                .HasComment("密码");
            entity.Property(e => e.PasswordEncryptType).HasComment("密码加密类型");
            entity.Property(e => e.Status).HasComment("用户状态");
            entity.Property(e => e.TenantId).HasComment("租户Id");
            entity.Property(e => e.Type).HasComment("用户类型");
            entity.Property(e => e.UserName)
                .HasMaxLength(60)
                .HasComment("账号");
        });

        modelBuilder.Entity<AdUserOrg>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("ad_user_org", tb => tb.HasComment("用户所属部门"));

            entity.HasIndex(e => new { e.UserId, e.OrgId }, "idx_ad_user_org_01").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasComment("主键Id");
            entity.Property(e => e.CreatedTime)
                .HasComment("创建时间")
                .HasColumnType("datetime(3)");
            entity.Property(e => e.CreatedUserId).HasComment("创建者用户Id");
            entity.Property(e => e.CreatedUserName)
                .HasMaxLength(50)
                .HasComment("创建者用户名");
            entity.Property(e => e.CreatedUserRealName)
                .HasMaxLength(50)
                .HasComment("创建者姓名");
            entity.Property(e => e.IsManager)
                .HasComment("是否主管")
                .HasColumnType("bit(1)");
            entity.Property(e => e.ModifiedTime)
                .HasComment("修改时间")
                .HasColumnType("datetime(3)");
            entity.Property(e => e.ModifiedUserId).HasComment("修改者用户Id");
            entity.Property(e => e.ModifiedUserName)
                .HasMaxLength(50)
                .HasComment("修改者用户名");
            entity.Property(e => e.ModifiedUserRealName)
                .HasMaxLength(50)
                .HasComment("修改者姓名");
            entity.Property(e => e.OrgId).HasComment("部门Id");
            entity.Property(e => e.UserId).HasComment("用户Id");
        });

        modelBuilder.Entity<AdUserRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("ad_user_role", tb => tb.HasComment("用户角色"));

            entity.HasIndex(e => new { e.UserId, e.RoleId }, "idx_ad_user_role_01").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasComment("主键Id");
            entity.Property(e => e.CreatedTime)
                .HasComment("创建时间")
                .HasColumnType("datetime(3)");
            entity.Property(e => e.CreatedUserId).HasComment("创建者用户Id");
            entity.Property(e => e.CreatedUserName)
                .HasMaxLength(50)
                .HasComment("创建者用户名");
            entity.Property(e => e.CreatedUserRealName)
                .HasMaxLength(50)
                .HasComment("创建者姓名");
            entity.Property(e => e.RoleId).HasComment("角色Id");
            entity.Property(e => e.UserId).HasComment("用户Id");
        });

        modelBuilder.Entity<AdUserStaff>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("ad_user_staff", tb => tb.HasComment("用户员工"));

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasComment("主键Id");
            entity.Property(e => e.CreatedTime)
                .HasComment("创建时间")
                .HasColumnType("datetime(3)");
            entity.Property(e => e.CreatedUserId).HasComment("创建者用户Id");
            entity.Property(e => e.CreatedUserName)
                .HasMaxLength(50)
                .HasComment("创建者用户名");
            entity.Property(e => e.CreatedUserRealName)
                .HasMaxLength(50)
                .HasComment("创建者姓名");
            entity.Property(e => e.EntryTime)
                .HasComment("入职时间")
                .HasColumnType("datetime(3)");
            entity.Property(e => e.Introduce)
                .HasMaxLength(500)
                .HasComment("个人简介");
            entity.Property(e => e.IsDeleted)
                .HasComment("是否删除")
                .HasColumnType("bit(1)");
            entity.Property(e => e.JobNumber)
                .HasMaxLength(20)
                .HasComment("工号");
            entity.Property(e => e.ModifiedTime)
                .HasComment("修改时间")
                .HasColumnType("datetime(3)");
            entity.Property(e => e.ModifiedUserId).HasComment("修改者用户Id");
            entity.Property(e => e.ModifiedUserName)
                .HasMaxLength(50)
                .HasComment("修改者用户名");
            entity.Property(e => e.ModifiedUserRealName)
                .HasMaxLength(50)
                .HasComment("修改者姓名");
            entity.Property(e => e.Position)
                .HasMaxLength(255)
                .HasComment("职位");
            entity.Property(e => e.Sex).HasComment("性别");
            entity.Property(e => e.TenantId).HasComment("租户Id");
            entity.Property(e => e.WorkWeChatCard)
                .HasMaxLength(500)
                .HasComment("企业微信名片");
        });

        modelBuilder.Entity<AdView>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("ad_view", tb => tb.HasComment("视图管理"));

            entity.HasIndex(e => new { e.ParentId, e.Label }, "idx_ad_view_01").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasComment("主键Id");
            entity.Property(e => e.Cache)
                .HasComment("缓存")
                .HasColumnType("bit(1)");
            entity.Property(e => e.CreatedTime)
                .HasComment("创建时间")
                .HasColumnType("datetime(3)");
            entity.Property(e => e.CreatedUserId).HasComment("创建者用户Id");
            entity.Property(e => e.CreatedUserName)
                .HasMaxLength(50)
                .HasComment("创建者用户名");
            entity.Property(e => e.CreatedUserRealName)
                .HasMaxLength(50)
                .HasComment("创建者姓名");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasComment("说明");
            entity.Property(e => e.Enabled)
                .HasComment("启用")
                .HasColumnType("bit(1)");
            entity.Property(e => e.IsDeleted)
                .HasComment("是否删除")
                .HasColumnType("bit(1)");
            entity.Property(e => e.Label)
                .HasMaxLength(500)
                .HasComment("视图名称");
            entity.Property(e => e.ModifiedTime)
                .HasComment("修改时间")
                .HasColumnType("datetime(3)");
            entity.Property(e => e.ModifiedUserId).HasComment("修改者用户Id");
            entity.Property(e => e.ModifiedUserName)
                .HasMaxLength(50)
                .HasComment("修改者用户名");
            entity.Property(e => e.ModifiedUserRealName)
                .HasMaxLength(50)
                .HasComment("修改者姓名");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasComment("视图命名");
            entity.Property(e => e.ParentId).HasComment("所属节点");
            entity.Property(e => e.Path)
                .HasMaxLength(500)
                .HasComment("视图路径");
            entity.Property(e => e.Sort).HasComment("排序");
        });

        modelBuilder.Entity<AppTask>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("app_task");

            entity.Property(e => e.Body).HasColumnType("text");
            entity.Property(e => e.CreateTime).HasColumnType("datetime(3)");
            entity.Property(e => e.IntervalArgument).HasMaxLength(1024);
            entity.Property(e => e.LastRunTime).HasColumnType("datetime(3)");
            entity.Property(e => e.Topic).HasMaxLength(255);
        });

        modelBuilder.Entity<AppTaskExt>(entity =>
        {
            entity.HasKey(e => e.TaskId).HasName("PRIMARY");

            entity.ToTable("app_task_ext", tb => tb.HasComment("任务邮件"));

            entity.Property(e => e.TaskId).HasComment("任务Id");
            entity.Property(e => e.AlarmEmail)
                .HasMaxLength(500)
                .HasComment("报警邮件，多个邮件地址则逗号分隔");
            entity.Property(e => e.CreatedTime)
                .HasComment("添加时间")
                .HasColumnType("datetime(3)");
            entity.Property(e => e.CreatedUserId).HasComment("添加用户Id");
            entity.Property(e => e.ModifiedTime)
                .HasComment("修改时间")
                .HasColumnType("datetime(3)");
            entity.Property(e => e.ModifiedUserId).HasComment("修改用户Id");
        });

        modelBuilder.Entity<AppTaskLog>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("app_task_log");

            entity.Property(e => e.CreateTime).HasColumnType("datetime(3)");
            entity.Property(e => e.Exception).HasColumnType("text");
            entity.Property(e => e.Remark).HasColumnType("text");
            entity.Property(e => e.Success).HasColumnType("bit(1)");
            entity.Property(e => e.TaskId).HasMaxLength(255);
        });

        modelBuilder.Entity<BaseRegion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("base_region", tb => tb.HasComment("地区"));

            entity.HasIndex(e => new { e.ParentId, e.Name }, "idx_base_region_01").IsUnique();

            entity.HasIndex(e => new { e.ParentId, e.Code }, "idx_base_region_02").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasComment("主键Id");
            entity.Property(e => e.Code)
                .HasMaxLength(20)
                .HasComment("代码");
            entity.Property(e => e.CreatedTime)
                .HasComment("创建时间")
                .HasColumnType("datetime(3)");
            entity.Property(e => e.CreatedUserId).HasComment("创建者用户Id");
            entity.Property(e => e.CreatedUserName)
                .HasMaxLength(50)
                .HasComment("创建者用户名");
            entity.Property(e => e.CreatedUserRealName)
                .HasMaxLength(50)
                .HasComment("创建者姓名");
            entity.Property(e => e.Enabled)
                .HasComment("启用")
                .HasColumnType("bit(1)");
            entity.Property(e => e.Hot)
                .HasComment("热门")
                .HasColumnType("bit(1)");
            entity.Property(e => e.IsDeleted)
                .HasComment("是否删除")
                .HasColumnType("bit(1)");
            entity.Property(e => e.Level).HasComment("级别");
            entity.Property(e => e.ModifiedTime)
                .HasComment("修改时间")
                .HasColumnType("datetime(3)");
            entity.Property(e => e.ModifiedUserId).HasComment("修改者用户Id");
            entity.Property(e => e.ModifiedUserName)
                .HasMaxLength(50)
                .HasComment("修改者用户名");
            entity.Property(e => e.ModifiedUserRealName)
                .HasMaxLength(50)
                .HasComment("修改者姓名");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasComment("名称");
            entity.Property(e => e.ParentId).HasComment("上级Id");
            entity.Property(e => e.Pinyin)
                .HasMaxLength(200)
                .HasComment("拼音");
            entity.Property(e => e.PinyinFirst)
                .HasMaxLength(20)
                .HasComment("拼音首字母");
            entity.Property(e => e.Sort).HasComment("排序");
            entity.Property(e => e.Url)
                .HasMaxLength(100)
                .HasComment("提取地址");
            entity.Property(e => e.VilageCode)
                .HasMaxLength(10)
                .HasComment("城乡分类代码");
        });

        modelBuilder.Entity<Demo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("demo");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedTime).HasColumnType("datetime");
            entity.Property(e => e.Label).HasMaxLength(255);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
