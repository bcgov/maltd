/* global cy */
class UserPage {
  static getBackButton() {
    return cy.get("div.limit-width > h5");
  }
}
export default UserPage;
