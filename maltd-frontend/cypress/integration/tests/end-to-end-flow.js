/* global cy, Cypress */
// / <reference types="Cypress" />

import LandingPage from "../../support/pageObjects/landing-page";
import UserPage from "../../support/pageObjects/user-page";
import IdirLoginPage from "../../support/pageObjects/idir-login-page";

describe("End to end workflow", () => {
  it("Launches the base url", () => {
    cy.visit(Cypress.env("baseUrl"));
  });

  it("Tests end to end workflow", () => {
    cy.visit(Cypress.env("authUrl"));

    IdirLoginPage.getIdirButton()
      .should("be.visible")
      .click();
    IdirLoginPage.getUserNameField().type(Cypress.env("userName"));
    IdirLoginPage.getPasswordField().type(Cypress.env("password"));
    IdirLoginPage.getContinueButton().click();
    cy.wait(2000);
    cy.reload(true);
    cy.wait(1200);
    LandingPage.getInputField().type(Cypress.env("userName"));
    LandingPage.getFindButton().click();
    cy.wait(2500);

    // Asserts the email format
    UserPage.getEmailInfo()
      .should("be.visible")
      .contains("@gov.bc.ca");
    // Adds project to the user
    UserPage.getNoProjects()
      .should("be.visible")
      .contains("No projects");
    cy.wait(2000);
    UserPage.getDropDownTitle()
      .should("be.visible")
      .click();
    UserPage.getDropDownMenu().click();
    UserPage.getPlusIcon().click();

    cy.wait(2000);
    UserPage.getDropDownTitle()
      .should("be.visible")
      .click();
    UserPage.getDropDownMenu().click();
    UserPage.getPlusIcon().click();

    // Assert the error message is present
    UserPage.getDuplilcateError().should(
      "have.text",
      "This project has already been added. Please try again with a different project."
    );

    // Asserts the project and member details
    UserPage.getProjectInfo()
      .should("be.visible")
      .invoke("text")
      .should("include", "Sharepoint");
    UserPage.getMemberResources()
      .should("be.visible")
      .invoke("text")
      .should("include", "Member");

    // Confirms added project is always present
    UserPage.getBackNav().click();
    LandingPage.getInputField().type(Cypress.env("userName"));
    LandingPage.getFindButton().click();
    cy.wait(2000);
    UserPage.getProjectInfo()
      .should("be.visible")
      .invoke("text")
      .should("include", "Sharepoint");
    UserPage.getMemberResources()
      .should("be.visible")
      .invoke("text")
      .should("include", "Member");

    // Removes the project for user
    UserPage.getCloseIcon()
      .should("be.visible")
      .click({ multiple: true });

    UserPage.getBackNav().click();

    // Confirms the project is removed in the next login
    LandingPage.getInputField().type(Cypress.env("userName"));
    LandingPage.getFindButton().click();
    cy.wait(2000);
    UserPage.getNoProjects()
      .should("be.visible")
      .contains("No projects");

    UserPage.getLogOutButton().click();
  });
});
