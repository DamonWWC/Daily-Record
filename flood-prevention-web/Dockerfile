FROM 10.51.9.147:8080/library/hjmos-base-static:1.5.8

ADD /dist /dist

RUN \
    mv /dist /static

EXPOSE 8080
ENV JAVA_OPTS="-Duser.timezone=Asia/Shanghai"
ENV APP_OPTS="--spring.application.name=hjmos-flood-prevention-web"
ENTRYPOINT [ "sh", "-c", "java $JAVA_OPTS -jar /hjmos-base-static/hjmos-base-static.jar --server.port=8080 $APP_OPTS"]
