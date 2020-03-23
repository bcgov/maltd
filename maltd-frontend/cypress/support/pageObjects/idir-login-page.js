/* globals cy */
class IdirLoginPage {
  // Get the idir button
  getIdirButton() {
    return cy.get("#zocial-idir");
  }

  // Get the username field
  getUserNameField() {
    return cy.get("#user");
  }

  // Get the password field
  getPasswordField() {
    return cy.get("#password");
  }

  // Get the continue button
  getContinueButton() {
    return cy.get('input[value="Continue"]');
  }
}
export default IdirLoginPage;
