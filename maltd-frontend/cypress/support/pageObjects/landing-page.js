/* globals cy */
class LandingPage {
  // Get the input field
  static getInputField() {
    return cy.get('input[name="idir"]');
  }

  // Get the Find button
  static getFindButton() {
    return cy.get(".d-flex > .general-button");
  }

  // Get the Red find button
  static getFindRedButton() {
    return cy.get(".general-button.my-2.btn.btn-danger.disabled");
  }

  // Get the error text element
  static getErrorText() {
    return cy.get(".small.error-message");
  }

  // Get the loading text element
  static getLoading() {
    return cy.get(".loading");
  }

  // Get the logout button
  static getLogoutButton() {
    return cy.get(".collapse.>.general-button");
  }
}

export default LandingPage;
