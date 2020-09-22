/* global cy, Cypress */
// / <reference types="Cypress" />

Cypress.Commands.add("login", (baseUrl, email, password) => {
  cy.visit(baseUrl);
  cy.get('a[id="zocial-idir"]').click();
  cy.get("#user")
    .clear()
    .type(email);
  cy.get("#password")
    .clear()
    .type(password);
  cy.get('input[value="Continue"]')
    .click()
    .should("not.exist");
});

Cypress.Commands.add("findUser", userId => {
  cy.get('input[name="idir"]')
    .clear()
    .type(userId);
  cy.xpath('//*[@id="root"]/div/main/div[2]/div/div/div/div/div/div[2]/button')
    .should("exist")
    .click()
    .should("not.exist");
});

Cypress.Commands.add("verifyEmailFormat", () => {
  cy.get("#email-info")
    .should("exist")
    .contains("@gov.bc.ca");
});

// Cypress.Commands.add('validateNoProjectsIsAdded', () => {
//     cy.get(".cols > :nth-child(2) > p")
//     .should('have.text', 'No projects')
// })

Cypress.Commands.add("addAndValidateProject", (projectName, memberResource) => {
  cy.get("select")
    .select(projectName)
    .should("have.value", projectName);
  cy.wait(3000);
  cy.get(".drop-plus > :nth-child(2) > .bcgov-button")
    .should("exist")
    .click({ force: true })
    .should("not.exist");
  cy.get("#member-resources").should("have.text", memberResource);
});

Cypress.Commands.add(
  "addAdditionalSharePointProject",
  (projectName, memberResource) => {
    const resources = cy.get("#member-resources");
    cy.get("select")
      .select(projectName)
      .should("have.value", projectName);
    cy.wait(3000);
    cy.get(".drop-plus > .bcgov-button")
      .should("exist")
      .click({ force: true })
      .should("not.exist");
    for (let i = 0; i < resources.length; i++) {
      if (resources[i] === memberResource)
        console.info("Sharepoint resource is updated.");
      break;
    }
  }
);

Cypress.Commands.add(
  "validateDuplicateProject",
  (projectName, errorMessage) => {
    cy.get("select")
      .select(projectName)
      .should("have.value", projectName);
    cy.wait(3000);
    cy.get(".drop-plus > .bcgov-button")
      .should("exist")
      .click({ force: true });
    cy.get(".error-message").should("have.text", errorMessage);
  }
);

Cypress.Commands.add("deleteProjects", () => {
  cy.get(":nth-child(2) > .pointer").click({ multiple: true });
  cy.wait(1200);
  cy.get(":nth-child(2) > .pointer")
    .click({ multiple: true })
    .should("not.exist");
});

Cypress.Commands.add("logout", () => {
  cy.xpath('//*[@id="root"]/div/main/div[1]/button')
    .should("exist")
    .click();
});
