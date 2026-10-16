FROM quay.io/keycloak/keycloak:26.4
EXPOSE 8080
EXPOSE 8443

ENV KC_BOOTSTRAP_ADMIN_USERNAME=admin
ENV KC_BOOTSTRAP_ADMIN_PASSWORD=admin

# Serve under /auth to match the URLs used prior to Keycloak 17 (Quarkus), so that
# the admin's default Keycloak configuration (appsettings.Keycloak.json) works unchanged.
ENV KC_HTTP_RELATIVE_PATH=/auth

# Keycloak no longer auto-generates a certificate for HTTPS, so generate a self-signed
# one for ees.local at build time. Keycloak enables HTTPS automatically when a keystore
# exists at conf/server.keystore (with the default store password "password").
# The certificate can be exported from a running container and trusted in your OS
# certificate store to avoid browser certificate warnings (see README).
RUN keytool -genkeypair -alias ees-idp -keyalg RSA -keysize 2048 -validity 3650 \
    -dname "CN=ees.local" -ext "SAN=DNS:ees.local,DNS:localhost,IP:127.0.0.1" \
    -keystore /opt/keycloak/conf/server.keystore -storepass password -keypass password

# The import file must be named "<realm name>-realm.json"
COPY ./keycloak-ees-realm.json /opt/keycloak/data/import/ees-realm-realm.json

CMD ["start-dev", "--import-realm"]
