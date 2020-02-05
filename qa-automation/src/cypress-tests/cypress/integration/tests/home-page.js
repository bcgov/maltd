describe("The Home Page", function() {
  it("Successfully loads", function() {
    // Launches the url
    cy.visit("/");

    // Gets input and type user name
    cy.get('input[name="idir"]').type("IDIR/testuser");
  });
});
