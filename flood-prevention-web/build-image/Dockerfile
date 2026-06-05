# 业务镜像的Dockerfile

# 镜像的构建脚本
# 此脚本放到服务器上执行

# 镜像名和版本号按实际情况调整
# env-centos 基础镜像

FROM harbor.pcitech.com/base/env-centos:latest

# 应用名称及版本
LABEL app="hjmos-flood-prevention-web" version="1.0.0"

# 调整为具体的业务项目部署资源包
ADD hjmos-flood-prevention-web.tar.gz /opt/

# 设置工作目录
WORKDIR /opt/hjmos-flood-prevention-web

# 抛出端口 k8s集群端口限制: 30000~32767 !!!
EXPOSE 30392

# 容器化部署标志 环境变量，供uni-server使用
ENV ENV_NONDAEMON=true


# 镜像启动入口
ENTRYPOINT sudo -E sh bin/run.sh start && tail -f /dev/null
