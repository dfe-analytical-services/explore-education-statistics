FROM quay.io/keycloak/keycloak:13.0.1
EXPOSE 8080
EXPOSE 8443

ENV KEYCLOAK_USER=admin
ENV KEYCLOAK_PASSWORD=admin
ENV KEYCLOAK_IMPORT=/tmp/keycloak-ees-realm.json

COPY ./keycloak-ees-realm.json /tmp/keycloak-ees-realm.json
