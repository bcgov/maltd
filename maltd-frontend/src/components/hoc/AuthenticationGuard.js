/* eslint-disable no-alert,no-console */
import React, { useState, useEffect } from "react";
import Keycloak from "keycloak-js";
import MainPage from "../page/MainPage/MainPage";

let accessRole;

if (process.env.REACT_APP_KEYCLOAK_ACCESS_ROLE)
  accessRole = process.env.REACT_APP_KEYCLOAK_ACCESS_ROLE;

/**
 * @constant authenticationGuard - a higher order component that checks for user authorization and returns the wrapped component if the user is authenticated
 */

export default function AuthenticationGuard() {
  const [authedKeycloak, setAuthedKeycloak] = useState(null);

  function geKeycloakConfig() {
    const url = window.REACT_APP_KEYCLOAK_URL
      ? window.REACT_APP_KEYCLOAK_URL
      : process.env.REACT_APP_KEYCLOAK_URL;

    const realm = window.REACT_APP_KEYCLOAK_REALM
      ? window.REACT_APP_KEYCLOAK_REALM
      : process.env.REACT_APP_KEYCLOAK_REALM;

    const clientId = window.REACT_APP_KEYCLOAK_CLIENT_ID
      ? window.REACT_APP_KEYCLOAK_CLIENT_ID
      : process.env.REACT_APP_KEYCLOAK_CLIENT_ID;

    if (!url) {
      return null;
    }

    return { url, realm, clientId };
  }

  const redirectUri = window.REACT_APP_KEYCLOAK_REDIRECT_URI
    ? window.REACT_APP_KEYCLOAK_REDIRECT_URI
    : process.env.REACT_APP_KEYCLOAK_REDIRECT_URI;

  const keycloakConfig = geKeycloakConfig();

  // Initialize client
  // if the keycloakConfig.url is not set, the allow keycloak to load configuration from keycloak.json
  console.log(
    keycloakConfig
      ? "Using internal keycloak config"
      : "Using keycloak.json keycloak config"
  );
  const keycloak = keycloakConfig ? Keycloak(keycloakConfig) : Keycloak();

  function onLogoutClick() {
    authedKeycloak.logout({ redirectUri });
  }

  async function keycloakInit() {
    await keycloak
      .init({
        onLoad: "login-required"
      })
      .success(() => {
        keycloak.loadUserInfo().success();
        if (
          accessRole &&
          keycloak.tokenParsed.realm_access.roles.indexOf(accessRole) !== -1
        ) {
          localStorage.setItem("jwt", keycloak.token);
          setAuthedKeycloak(keycloak);
        } else {
          keycloak.clearToken();
          localStorage.clear();
          alert(
            "Authenticated but not Authorized, request access from your portal administrator"
          );
          window.location.assign(keycloak.createLogoutUrl());
        }
      });
  }

  useEffect(() => {
    keycloakInit();
  }, []);

  return (
    <React.Fragment>
      {authedKeycloak && <MainPage onLogoutClick={onLogoutClick} />}
      {!authedKeycloak && null}
    </React.Fragment>
  );
}
