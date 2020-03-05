/* global cy, before */
// / <reference types="Cypress" />

import LandingPage from "../../support/pageObjects/landing-page";
import UserPage from "../../support/pageObjects/user-page";

describe("Add projects to user", () => {
  // Runs once before all tests in the block
  before(() => {
    cy.fixture("userData.json").as("users");
  });

  it("Adds project to the user and removes", () => {
    // Launches the url
    cy.visit("/");

    cy.get("@users").then(users => {
      const validUser = users.userName.valid;

      // Validate the email address based on domain name
      LandingPage.getInputField().type(validUser[1]);
      LandingPage.getFindButton().click();
      UserPage.getEmailInfo().should("be.visible");
      UserPage.getEmailInfo().contains("@gov.bc.ca");

      // Add and remove project for the user
      UserPage.getDropDown().click();
      UserPage.getDropDownMenu().should("be.visible");
      UserPage.getDropDownItem()
        .contains("Corrections Dev")
        .click();
      UserPage.getPlusIcon().click();
      UserPage.getProjectInfo().contains("Corrections Dev");
      UserPage.getCloseIcon()
        .should("be.visible")
        .click();
      UserPage.getProjectInfo().should("not.have.value", "Corrections Dev");
      UserPage.getBackNav().click();
    });
  });
});
