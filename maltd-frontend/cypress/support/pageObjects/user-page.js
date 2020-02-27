/* global cy */

class UserPage {
  static getDropDown() {
    return cy.get("[data-cy=drop-down]");
  }

  static getDropDownMenu() {
    return cy.get("[data-cy=drop-down-menu]");
  }

  static getDropDownItem() {
    return cy.get(".dropdown-item");
  }

  static getPlusIcon() {
    return cy.get("[data-cy=plus-icon]");
  }

  static getProjectInfo() {
    return cy.get("[data-cy=project-info]");
  }

  static getCloseIcon() {
    return cy.get("[data-cy=close-icon]");
  }

  static getBackNav() {
    return cy.get("[data-cy=back-nav]");
  }
}
export default UserPage;
