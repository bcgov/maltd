import React, { useState, useEffect } from "react";
import Keycloak from "keycloak-js";
import MainPage from "../page/MainPage/MainPage";

let url, realm, clientId;
if (process.env.REACT_APP_KEYCLOAK_URL)
  url = process.env.REACT_APP_KEYCLOAK_URL;
if (process.env.REACT_APP_KEYCLOAK_REALM)
  realm = process.env.REACT_APP_KEYCLOAK_REALM;
if (process.env.REACT_APP_KEYCLOAK_CLIENT_ID)
  clientId = process.env.REACT_APP_KEYCLOAK_CLIENT_ID;

const KEYCLOAK = {
  url,
  realm,
  clientId
};

/**
 * @constant authenticationGuard - a higher order component that checks for user authorization and returns the wrapped component if the user is authenticated
 */

export default function AuthenticationGuard() {
  const [authedKeycloak, setAuthedKeycloak] = useState(null);
  let redirectUri;

  if (process.env.REACT_APP_KEYCLOAK_REDIRECT_URI) {
    redirectUri = process.env.REACT_APP_KEYCLOAK_REDIRECT_URI;
  }

  function onLogoutClick() {
    authedKeycloak.logout({ redirectUri });
  }

  async function keycloakInit() {
    // Initialize client
    const keycloak = Keycloak(KEYCLOAK);
    await keycloak
      .init({
        onLoad: "login-required"
      })
      .success(() => {
        keycloak.loadUserInfo().success();
        localStorage.setItem("jwt", keycloak.token);
        setAuthedKeycloak(keycloak);
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
