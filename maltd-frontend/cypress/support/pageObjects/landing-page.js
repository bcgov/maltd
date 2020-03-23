/* globals cy */
class LandingPage {
  // Get the input field
  getInputField() {
    return cy.get('input[name="idir"]');
  }

  // Get the Find button
  getFindButton() {
    return cy.get(".d-flex > .general-button");
  }

  // Get the Red find button
  getFindRedButton() {
    return cy.get(".general-button.my-2.btn.btn-danger.disabled");
  }

  // Get the error text element
  getErrorText() {
    return cy.get("[data-cy=error-text]");
  }

  // Get the loading text element
  getLoading() {
    return cy.get(".loading");
  }

  // Get the logout button
  getLogoutButton() {
    return cy.get(".collapse.>.general-button");
  }
}
export default LandingPage;
