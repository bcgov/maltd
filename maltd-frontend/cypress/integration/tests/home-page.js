/* global cy */
describe("The Home Page", () => {
  it("Successfully loads", () => {
    // Launches the url
    cy.visit("/");

    // Gets input and type user name
    cy.get('input[name="idir"]').type("IDIR/testuser");
  });
});
