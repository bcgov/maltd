/* global cy */

class UserPage {
  static getDropDown() {
    return cy.get("#dropdown");
  }

  static getDropDownTitle() {
    return cy.get("#title");
  }

  static getDropDownMenu() {
    return cy.get("[data-cy=drop-down-menu] > :nth-child(1)");
  }

  static getDropDownItem() {
    return cy.get('button[role="menuitem"]');
  }

  static getPlusIcon() {
    return cy.get("[data-cy=plus-icon]");
  }

  static getProjectInfo() {
    return cy.get(":nth-child(2) > .project-div > #project-info > .large-size");
  }

  static getMemberResources() {
    return cy.get("#member-resources");
  }

  static getEmailInfo() {
    return cy.get("#email-info");
  }

  static getCloseIcon() {
    return cy.get("[data-cy=close-icon]");
  }

  static getBackNav() {
    return cy.get("[data-cy=back-nav]");
  }

  static getNoProjects() {
    return cy.get("#noProjects");
  }

  static getDuplilcateError() {
    return cy.get(".error-message");
  }

  static getLogOutButton() {
    return cy.get(".general-button");
  }
}
export default UserPage;
