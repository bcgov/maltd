// import UserPage from "../../support/pageObjects/user-page";

// ***********************************************
// This example commands.js shows you how to
// create various custom commands and overwrite
// existing commands.
//
// For more comprehensive examples of custom
// commands please read more here:
// https://on.cypress.io/custom-commands
// ***********************************************
//
// -- This is a parent command --
// Cypress.Commands.add("login", (email, password) => { ... })
Cypress.Commands.add("selectProject", project => {
  cy.get("[data-cy=drop-down-menu]").each(($el, index, $list) => {
    if ($el.text().includes(project)) {
      cy.get("[data-cy=drop-down]").click();
      cy.get("[data-cy=drop-down-menu]").should("be.visible");
      cy.get(".dropdown-item")
        .eq(index)
        .contains(project);
      cy.get("[data-cy=plus-icon]").click();
      cy.get("[data-cy=close-icon]")
        .should("be.visible")
        .click();
      cy.get("[data-cy=project-info]").should("not.have.value", project);
      cy.get("[data-cy=back-nav]").click();
    }
  });
});
//
//
// -- This is a child command --
// Cypress.Commands.add("drag", { prevSubject: 'element'}, (subject, options) => { ... })
//
//
// -- This is a dual command --
// Cypress.Commands.add("dismiss", { prevSubject: 'optional'}, (subject, options) => { ... })
//
//
// -- This will overwrite an existing command --
// Cypress.Commands.overwrite("visit", (originalFn, url, options) => { ... })
