import React, { useState, useEffect } from "react";
import Keycloak from "keycloak-js";
import MainPage from "../page/MainPage/MainPage";

const KEYCLOAK = {
  url: "https://sso-dev.pathfinder.gov.bc.ca/auth",
  realm: "ezb8kej4",
  clientId: "malt-frontend"
};

/**
 * @constant authenticationGuard - a higher order component that checks for user authorization and returns the wrapped component if the user is authenticated
 */

export default function AuthenticationGuard() {
  const [authedKeycloak, setAuthedKeycloak] = useState(null);

  function onLogoutClick() {
    authedKeycloak.logout({ redirectUri: "http://localhost:3001" });
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
