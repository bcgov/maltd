/* global cy, before */
// / <reference types="Cypress" />

import LandingPage from "../../support/pageObjects/landing-page";

describe("Validate invalid user inputs", () => {
  before(() => {
    cy.fixture("userData.json").as("users");
  });

  it("Asserts invalid inputs are not accepted", () => {
    cy.visit(Cypress.env("baseUrl"));

    const landingPage = new LandingPage();

    // Find button is diabled
    landingPage.getFindButton().should("be.disabled");

    cy.get("@users").then(users => {
      const invalidUser = users.userName.invalid;

      // Validates invalid inputs and error message
      let i = invalidUser.length;

      while (i > 0) {
        i -= 1;
        landingPage.getInputField().type(invalidUser[i]);

        if (invalidUser[i].length < 3) {
          landingPage.getFindRedButton().should("be.disabled");
          landingPage.getInputField().clear();
        } else {
          landingPage.getFindButton().click();
          landingPage.getLoading().should("not.be.visible");
          landingPage
            .getErrorText()
            .should(
              "have.text",
              "This user does not exist, please try again with a different IDIR username."
            );
          landingPage.getInputField().clear();
        }
      }
    });
  });
});
