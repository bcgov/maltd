/* global cy, Cypress */
// / <reference types="Cypress" />

describe("MALT Ui Functional Test Suite", () => {
  it("Valid Login Test", () => {
    cy.clearCookies();
    cy.login(
      Cypress.env("baseUrl"),
      Cypress.env("userName"),
      Cypress.env("password")
    );

    cy.title().should("be.equal", "Account & License Management Tool");

    cy.logout();
  });

  it("Find User Test", () => {
    cy.clearCookies();
    cy.login(
      Cypress.env("baseUrl"),
      Cypress.env("userName"),
      Cypress.env("password")
    );

    cy.findUser(Cypress.env("userName"));
    cy.verifyEmailFormat();

    cy.logout();
  });

  it("Find Invalid User Test", () => {
    cy.clearCookies();
    cy.login(
      Cypress.env("baseUrl"),
      Cypress.env("userName"),
      Cypress.env("password")
    );

    cy.findUser("userName");
    cy.get("[data-cy=error-text]").should(
      "have.text",
      "This user does not exist, please try again with a different IDIR username."
    );

    cy.logout();
  });

  it("Add Project to the User Test", () => {
    cy.clearCookies();
    cy.login(
      Cypress.env("baseUrl"),
      Cypress.env("userName"),
      Cypress.env("password")
    );

    cy.findUser(Cypress.env("userName"));
    cy.get(".cols > :nth-child(2) > p").should("have.text", "No projects");
    cy.addAndValidateProject("Dev Org Sandbox", "Member: Dynamics ");

    cy.logout();
  });

  it("Add Additional Projects to the User Test", () => {
    cy.clearCookies();
    cy.login(
      Cypress.env("baseUrl"),
      Cypress.env("userName"),
      Cypress.env("password")
    );

    cy.findUser(Cypress.env("userName"));
    cy.addAdditionalSharePointProject(
      "Test Org Sandbox",
      "Member: Dynamics SharePoint "
    );

    cy.logout();
  });

  it("Duplicate Projects cannot be added to the User Test", () => {
    cy.clearCookies();
    cy.login(
      Cypress.env("baseUrl"),
      Cypress.env("userName"),
      Cypress.env("password")
    );

    cy.findUser(Cypress.env("userName"));
    cy.validateDuplicateProject(
      "Dev Org Sandbox",
      "This project has already been added. Please try again with a different project."
    );

    cy.logout();
  });

  it("Delete a Project for User Test", () => {
    cy.clearCookies();
    cy.login(
      Cypress.env("baseUrl"),
      Cypress.env("userName"),
      Cypress.env("password")
    );

    cy.findUser(Cypress.env("userName"));
    cy.deleteProjects();
    cy.wait(1200);
    cy.deleteProjects();
    cy.get(".cols > :nth-child(2) > p").should("have.text", "No projects");
    cy.logout();
  });
});
