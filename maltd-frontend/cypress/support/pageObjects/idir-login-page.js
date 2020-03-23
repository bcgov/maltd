/* globals cy */
class IdirLoginPage {
  // Get the idir button
  static getIdirButton() {
    return cy.get("#zocial-idir");
  }

  // Get the username field
  static getUserNameField() {
    return cy.get("#user");
  }

  // Get the password field
  static getPasswordField() {
    return cy.get("#password");
  }

  // Get the continue button
  static getContinueButton() {
    return cy.get('input[value="Continue"]');
  }
}
export default IdirLoginPage;
