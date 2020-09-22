## MALTD QA Automation Tests

### Install Cypress

```
npm install
```

Run tests from maltd-frontend directory.

### Run E2E test on electron headless mode

```
npm run cy:run -- --spec "cypress/integration/tests/end-to-end-functional-tets.js"
```

### Run tests on headed mode

```
 npx cypress run --headed chrome --no-exit
```

## Environment Variables For Testing

In order to ensure the tests runs successfully, you will be required to set some environment variables as specified in the `cypress.env.example.json` file. Please setup a `cypress.env.json` file for local testing and populate the fields shown with the appropriate values.
