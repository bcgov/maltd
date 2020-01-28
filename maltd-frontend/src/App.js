/* eslint-disable react/jsx-filename-extension */
import React from "react";
import { Container, Row, Col } from "reactstrap";
import UserAccess from "./components/composite/UserAccess/UserAccess";

export default function App() {
  const projects = [{ name: "fams" }, { name: "malt" }];
  const userAccess = {
    projects,
    userName: "Username",
    userEmail: "useremail@gov.bc.ca"
  };

  return (
    <div>
      <h1>MALTD Frontend</h1>
      <Container>
        <Row>
          <Col
            className="block-example border rounded mb-0 p-3"
            sm="12"
            md={{ size: 6, offset: 3 }}
          >
            <UserAccess userAccess={userAccess} />
          </Col>
        </Row>
      </Container>
    </div>
  );
}
