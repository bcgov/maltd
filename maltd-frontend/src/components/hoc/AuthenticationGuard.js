/* eslint-disable no-alert */
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
  // let redirectUri;

  // if (process.env.REACT_APP_KEYCLOAK_REDIRECT_URI) {
  //   redirectUri = process.env.REACT_APP_KEYCLOAK_REDIRECT_URI;
  // }

  const keycloakConfig = {
    url: process.env.REACT_APP_KEYCLOAK_URL,
    realm: process.env.REACT_APP_KEYCLOAK_REALM,
    clientId: process.env.REACT_APP_KEYCLOAK_CLIENT_ID
  };

  // Initialize client
  // if the keycloakConfig.url is not set, the allow keycloak to load configuration from keycloak.json
  console.log(
    keycloakConfig.url
      ? "Using internal keycloak config"
      : "Using keycloak.json keycloak config"
  );
  const keycloak = keycloakConfig.url ? Keycloak(keycloakConfig) : Keycloak();

  function onLogoutClick() {
    keycloak.logout();
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
