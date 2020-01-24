import React from "react";
import { Container, Row, Col, Spinner } from "reactstrap";
import InputField from "../../base/InputField/InputField";

export default function UserSearch({ userSearch: { state } }) {
  const inputField = {
    type: "text",
    placeholder: "Enter IDIR username to find",
    name: "usernamesearch",
    valid: false,
    invalid: false,
    value: ""
  };

  return (
    <Container>
      <Row>
        <Col
          className="block-example border rounded mb-0"
          sm="12"
          md={{ size: 6, offset: 3 }}
        >
          <strong>Add or Remove User</strong>
          <p>
            Find a user by IDIR username and add them to or remove them from
            projects.
          </p>
          {!state.isLoading && (
            <div>
              <InputField inputField={inputField} />
              <button>Find</button>
            </div>
          )}
          {state.isLoading && (
            <div>
              <Spinner
                color="primary"
                style={{
                  width: "3rem",
                  height: "3rem",
                  margin: "auto",
                  display: "block"
                }}
              />
            </div>
          )}
        </Col>
      </Row>
    </Container>
  );
}
