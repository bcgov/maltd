/* global cy, before */
// / <reference types="Cypress" />
import LandingPage from "../../support/pageObjects/landing-page";
import UserPage from "../../support/pageObjects/user-page";

describe("Add projects to user", () => {
  // Runs once before all tests in the block
  before(() => {
    cy.fixture("userData.json").as("users");
  });

  it("Add project", () => {
    // Launches the url
    cy.visit("/");

    cy.get("@users").then(users => {
      const validUser = users.userName.valid;

      LandingPage.getInputField().type(validUser[1]);
      LandingPage.getFindButton().click();
      UserPage.getProjectInfo().contains("@example");

      // cy.selectProject('Dynamics');

      UserPage.getDropDown().click();
      UserPage.getDropDownMenu().should("be.visible");
      UserPage.getDropDownItem()
        .eq(4)
        .contains("Dynamics")
        .click();
      UserPage.getPlusIcon().click();
      UserPage.getProjectInfo().contains("Dynamics");
      UserPage.getCloseIcon()
        .should("be.visible")
        .click();
      UserPage.getProjectInfo().should("not.have.value", "Dynamics");
      UserPage.getBackNav().click();

      LandingPage.getInputField().type(validUser[3]);
      LandingPage.getFindButton().click();
      UserPage.getProjectInfo().contains("@example");

      UserPage.getDropDown().click();
      UserPage.getDropDownMenu().should("be.visible");
      UserPage.getDropDownItem()
        .eq(9)
        .contains("SharePoint")
        .click();
      UserPage.getPlusIcon().click();
      UserPage.getProjectInfo().contains("SharePoint");
      UserPage.getCloseIcon()
        .should("be.visible")
        .click();
      UserPage.getProjectInfo().should("not.have.value", "SharePoint");
      UserPage.getBackNav().click();

      LandingPage.getInputField().type(validUser[2]);
      LandingPage.getFindButton().click();

      UserPage.getProjectInfo().contains("@example");
      UserPage.getDropDown().click();
      UserPage.getDropDownMenu().should("be.visible");
      UserPage.getDropDownItem()
        .eq(12)
        .contains("SharePoint")
        .click();
      UserPage.getPlusIcon().click();
      UserPage.getProjectInfo().contains("SharePoint");
      UserPage.getBackNav().click();

      LandingPage.getInputField().type(validUser[2]);
      LandingPage.getFindButton().click();
      UserPage.getProjectInfo().contains("@example");

      UserPage.getDropDown().click();
      UserPage.getDropDownMenu().should("be.visible");
      UserPage.getDropDownItem()
        .eq(6)
        .contains("Dynamics")
        .click();
      UserPage.getPlusIcon().click();
      UserPage.getProjectInfo().contains("Dynamics");
      UserPage.getBackNav().click();

      LandingPage.getInputField().type(validUser[2]);
      LandingPage.getFindButton().click();
      UserPage.getProjectInfo().contains("@example");

      cy.wait(1000);
      UserPage.getCloseIcon()
        .should("be.visible")
        .click({ multiple: true });
      cy.wait(1000);
      UserPage.getBackNav().click();
    });
  });
});
