/* global cy, before */
// / <reference types="Cypress" />

import LandingPage from "../../support/pageObjects/landing-page";
import UserPage from "../../support/pageObjects/user-page";
import IdirLoginPage from "../../support/pageObjects/idir-login-page";

describe("End to end workflow", () => {
  it("Launches the base url", () => {
    cy.visit(Cypress.env("baseUrl"));
  });

  const landingPage = new LandingPage();
  const userPage = new UserPage();
  const idirLoginPage = new IdirLoginPage();

  it("Tests end to end workflow", () => {
    cy.visit(Cypress.env("authUrl"));

    idirLoginPage
      .getIdirButton()
      .should("be.visible")
      .click();
    idirLoginPage.getUserNameField().type(Cypress.env("userName"));
    idirLoginPage.getPasswordField().type(Cypress.env("password"));
    idirLoginPage.getContinueButton().click();
    cy.wait(2000);

    cy.reload(true);
    cy.wait(1200);
    landingPage.getInputField().type(Cypress.env("userName"));
    landingPage.getFindButton().click();
    cy.wait(2500);

    // Asserts the email format
    userPage
      .getEmailInfo()
      .should("be.visible")
      .contains("@gov.bc.ca");
    // Adds project to the user
    userPage
      .getNoProjects()
      .should("be.visible")
      .contains("No projects");
    cy.wait(2000);
    userPage
      .getDropDownTitle()
      .should("be.visible")
      .click();
    userPage.getDropDownMenu().click();
    userPage.getPlusIcon().click();

    cy.wait(2000);
    userPage
      .getDropDownTitle()
      .should("be.visible")
      .click();
    userPage.getDropDownMenu().click();
    userPage.getPlusIcon().click();

    // Assert the error message is present
    userPage
      .getDuplilcateError()
      .should(
        "have.text",
        "This project has already been added. Please try again with a different project."
      );

    // Asserts the project and member details
    userPage
      .getProjectInfo()
      .should("be.visible")
      .invoke("text")
      .should("include", "Sharepoint");
    userPage
      .getMemberResources()
      .should("be.visible")
      .invoke("text")
      .should("include", "Member");

    // Confirms added project is always present
    userPage.getBackNav().click();
    landingPage.getInputField().type(Cypress.env("userName"));
    landingPage.getFindButton().click();
    cy.wait(2000);
    userPage
      .getProjectInfo()
      .should("be.visible")
      .invoke("text")
      .should("include", "Sharepoint");
    userPage
      .getMemberResources()
      .should("be.visible")
      .invoke("text")
      .should("include", "Member");

    // Removes the project for user
    userPage
      .getCloseIcon()
      .should("be.visible")
      .click({ multiple: true });

    userPage.getBackNav().click();

    // Confirms the project is removed in the next login
    landingPage.getInputField().type(Cypress.env("userName"));
    landingPage.getFindButton().click();
    cy.wait(2000);
    userPage
      .getNoProjects()
      .should("be.visible")
      .contains("No projects");

    userPage.getLogOutButton().click();
  });
});
