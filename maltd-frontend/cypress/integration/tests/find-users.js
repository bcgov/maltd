/* global cy, before */
// / <reference types="Cypress" />

import LandingPage from "../../support/pageObjects/landing-page";

describe("Invalid user test", () => {
  // Runs once before all tests in the block
  before(() => {
    cy.fixture("userData.json").as("users");
  });

  it("Asserts invalid usernames are not accepted", () => {
    // Launches the url
    cy.visit("/");

    // Validates the Find button is disabled by default
    LandingPage.getFindButton().should("be.disabled");

    cy.get("@users").then(users => {
      const invalidUser = users.userName.invalid;

      // Validates invalid inputs and error message
      let i = invalidUser.length;

      while (i > 0) {
        i -= 1;
        LandingPage.getInputField().type(invalidUser[i]);

        if (invalidUser[i].length < 5) {
          LandingPage.getFindRedButton().should("be.disabled");
          LandingPage.getInputField().clear();
        } else {
          LandingPage.getFindButton().click();
          LandingPage.getLoading().should("not.be.visible");
          LandingPage.getErrorText().should(
            "have.text",
            "This user does not exist, please try again with a different IDIR username."
          );
          LandingPage.getInputField().clear();
        }
      }
    });
  });
});
