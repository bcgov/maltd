/* global cy, before */
// / <reference types="Cypress" />

import LandingPage from "../../support/pageObjects/landing-page";

describe("The landing pages tests", () => {
  // Runs once before all tests in the block
  before(() => {
    cy.fixture("userData.json").as("users");
  });

  it("Finds users", () => {
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
          cy.get(".error-message")
            .should("be.visible")
            .then(element => {
              const actual = element.text();
              const expected =
                "This user does not exist, please try again with a different IDIR username.";
              expect(actual).to.equal(expected);
            });
          LandingPage.getInputField().clear();
        }
      }
    });
  });
});
