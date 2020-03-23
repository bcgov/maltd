/* global cy, before, Cypress */
// / <reference types="Cypress" />

import LandingPage from "../../support/pageObjects/landing-page";

describe("Validate invalid user inputs", () => {
  before(() => {
    cy.fixture("userData.json").as("users");
  });

  it("Asserts invalid inputs are not accepted", () => {
    cy.visit(Cypress.env("baseUrl"));
    // Find button is diabled
    LandingPage.getFindButton().should("be.disabled");

    cy.get("@users").then(users => {
      const invalidUser = users.userName.invalid;

      // Validates invalid inputs and error message
      let i = invalidUser.length;

      while (i > 0) {
        i -= 1;
        LandingPage.getInputField().type(invalidUser[i]);

        if (invalidUser[i].length < 3) {
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
