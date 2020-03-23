/* global cy */

class UserPage {
  getDropDown() {
    return cy.get("#dropdown");
  }

  getDropDownTitle() {
    return cy.get("#title");
  }

  getDropDownMenu() {
    return cy.get("[data-cy=drop-down-menu] > :nth-child(1)");
  }

  getDropDownItem() {
    return cy.get('button[role="menuitem"]');
  }

  getPlusIcon() {
    return cy.get("[data-cy=plus-icon]");
  }

  getProjectInfo() {
    return cy.get(":nth-child(2) > .project-div > #project-info > .large-size");
  }

  getMemberResources() {
    return cy.get("#member-resources");
  }

  getEmailInfo() {
    return cy.get("#email-info");
  }

  getCloseIcon() {
    return cy.get("[data-cy=close-icon]");
  }

  getBackNav() {
    return cy.get("[data-cy=back-nav]");
  }

  getNoProjects() {
    return cy.get("#noProjects");
  }

  getDuplilcateError() {
    return cy.get(".error-message");
  }

  getLogOutButton() {
    return cy.get(".general-button");
  }
}
export default UserPage;
