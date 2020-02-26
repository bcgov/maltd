import React, { Component } from "react";
import PropTypes from "prop-types";
import Keycloak from "keycloak-js";

const KEYCLOAK = {
  realm: "mds",
  "ssl-required": "external",
  url: "<URL>",
  clientId: "<CLIENT_ID>"
};

/**
 * @constant authenticationGuard - a higher order component that checks for user authorization and returns the wrapped component if the user is authenticated
 */

export const AuthenticationGuard = WrappedComponent => {
  /**
   * Initializes the KeyCloak client and enables
   * redirects directly to IDIR login page.
   */

  class authenticationGuard extends Component {
    componentDidMount() {
      this.keycloakInit();
    }

    async keycloakInit() {
      // Initialize client
      const keycloak = Keycloak(KEYCLOAK);
      await keycloak
        .init({
          onLoad: "login-required",
          idpHint: KEYCLOAK.idpHint
        })
        .success(() => {
          keycloak
            .loadUserInfo()
            .success(userInfo => this.props.authenticateUser(userInfo));
          localStorage.setItem("jwt", keycloak.token);
          this.props.storeUserAccessData(keycloak.realmAccess.roles);
          this.props.storeKeycloakData(keycloak);
        });
    }

    render() {}
  }
};
