# dbmate image
ARG DBMATE_IMAGE

FROM ${DBMATE_IMAGE}

COPY Migrations /Migrations

ENTRYPOINT exec dbmate -d "/Migrations" -e db_conn_str --no-dump-schema up
