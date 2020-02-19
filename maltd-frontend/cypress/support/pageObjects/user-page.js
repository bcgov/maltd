/* global cy */
class UserPage {
  static getBackButton() {
    return cy.get(".limit-width > p");
  }
}
export default UserPage;
