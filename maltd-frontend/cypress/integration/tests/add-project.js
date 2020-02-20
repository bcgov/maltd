/* global cy, before */
// / <reference types="Cypress" />

import LandingPage from "../../support/pageObjects/landing-page";
// import UserPage from "../../support/pageObjects/user-page";

describe("The landing pages tests", () => {
  // Runs once before all tests in the block
  before(() => {
    cy.fixture("userData.json").as("users");
  });

  it("Finds users", () => {
    // Launches the url
    cy.visit("/");
    cy.get("@users").then(users => {
      const validUser = users.userName.valid;
      LandingPage.getInputField().type(validUser[1]);
      LandingPage.getFindButton().click();
      // UserPage.getBackButton().click();
    });
  });
});
