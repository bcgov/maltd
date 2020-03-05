/* global cy */

class UserPage {
  static getDropDown() {
    return cy.get("#dropdown");
  }

  static getDropDownMenu() {
    return cy.get("[data-cy=drop-down-menu]");
  }

  static getDropDownItem() {
    return cy.get('button[role="menuitem"]');
  }

  static getPlusIcon() {
    return cy.get("[data-cy=plus-icon]");
  }

  static getProjectInfo() {
    return cy.get(
      ":nth-child(2) > .project-div > [data-cy=project-info] > .large-size"
    );
  }

  static getEmailInfo() {
    return cy.get("[data-cy=email-info]");
  }

  static getCloseIcon() {
    return cy.get("[data-cy=close-icon]");
  }

  static getBackNav() {
    return cy.get("[data-cy=back-nav]");
  }
}
export default UserPage;
