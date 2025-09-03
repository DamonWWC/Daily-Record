-- 手动SQL脚本：为现有Users表添加认证字段
-- 此脚本处理现有数据，避免唯一约束冲突

-- 1. 添加新列（允许NULL，稍后设置默认值）
ALTER TABLE "Users" ADD COLUMN "Username" character varying(50);
ALTER TABLE "Users" ADD COLUMN "PasswordHash" character varying(255);
ALTER TABLE "Users" ADD COLUMN "Role" character varying(100) DEFAULT 'User';

-- 2. 为现有用户设置默认用户名（基于邮箱前缀或用户ID）
UPDATE "Users" 
SET "Username" = CASE 
    WHEN "Email" IS NOT NULL AND "Email" != '' 
    THEN SUBSTRING("Email" FROM 1 FOR POSITION('@' IN "Email") - 1) || '_' || "Id"::text
    ELSE 'user_' || "Id"::text
END
WHERE "Username" IS NULL;

-- 3. 为现有用户设置默认密码哈希（临时密码：123456）
-- 注意：这是BCrypt哈希后的 "123456"
UPDATE "Users" 
SET "PasswordHash" = '$2a$11$J5KZJq1vSbF8i3dS9WHnoOLVJbHgMB8kClHKVm8wZZ9V9UPhvB2zG'
WHERE "PasswordHash" IS NULL OR "PasswordHash" = '';

-- 4. 设置列为NOT NULL
ALTER TABLE "Users" ALTER COLUMN "Username" SET NOT NULL;
ALTER TABLE "Users" ALTER COLUMN "PasswordHash" SET NOT NULL;

-- 5. 创建唯一索引
CREATE UNIQUE INDEX "IX_Users_Username" ON "Users" ("Username");

-- 6. 检查结果
SELECT "Id", "Username", "Email", "Role", "IsActive" FROM "Users" ORDER BY "Id";
