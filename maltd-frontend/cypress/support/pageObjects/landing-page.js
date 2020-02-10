class LandingPage {
  // Get the input field
  getInputField() {
    return cy.get('input[name="idir"]');
  }

  // Get the Find button
  getFindButton() {
    return cy.get(".d-flex > .general-button");
  }

  // Get the error text element
  getErrorText() {
    return cy.get(".error-message");
  }

  // Get the loading text element
  getLoading() {
    return cy.get("#loading");
  }

  // Get the logout button
  getLogoutButton() {
    return cy.get(".collapse > .general-button");
  }
}

export default LandingPage;
