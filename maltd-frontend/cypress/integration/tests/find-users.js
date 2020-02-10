/* global cy, before */
// / <reference types="Cypress" />

import LandingPage from "../../support/pageObjects/landing-page";

describe("The landing Page", () => {
  before(() => {
    // Runs once before all tests in the block
    cy.fixture("userData").then(data => {
      this.data = data;
    });
  });

  it("Find users", () => {
    const landingPage = new LandingPage();
    // Launches the url
    cy.visit("/");

    // Validates the Find button is disabled by default
    landingPage.getFindButton().should("be.disabled");

    // Validating invalid inputs and error message
    let i = this.data.userName.invalid.length;

    while (i > 0) {
      i -= 1;
      landingPage.getInputField().type(`IDIR/${this.data.userName.invalid[i]}`);
      landingPage.getFindButton().click();
      landingPage
        .getErrorText()
        .should(
          "have.text",
          "This user does not exist, please try again with a different IDIR username."
        );
      landingPage.getErrorText().should("not.be.visible");
    }
    landingPage.getLogoutButton().click();
  });
});
