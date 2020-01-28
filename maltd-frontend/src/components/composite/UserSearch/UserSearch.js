/* eslint-disable react/jsx-filename-extension */
import React from "react";
import PropTypes from "prop-types";
import { Container, Row, Col, Spinner } from "reactstrap";
import InputField from "../../base/InputField/InputField";
import GeneralButton from "../../base/GeneralButton/GeneralButton";
import "./UserSearch.css";

export default function UserSearch({
  userSearch: { state },
  inputField,
  generalButton
}) {
  return (
    <Container>
      <Row>
        <Col
          className="block-example border rounded mb-0 p-3"
          sm="12"
          md={{ size: 6, offset: 3 }}
        >
          <h4>Add or Remove User</h4>
          <p />
          <p>
            Find a user by IDIR username and add them to or remove them from
            projects.
          </p>
          <p />
          {!state.isLoading && (
            <div>
              <InputField inputField={inputField} />
              <p />
              <GeneralButton generalButton={generalButton} />
            </div>
          )}
          {state.isLoading && (
            <div>
              <div>
                <InputField inputField={inputField} />
                <p />
                <div>
                  <div className="align-left">
                    <p className="align-left">Loading...</p>
                    <Spinner className="spinner" />
                  </div>
                  <GeneralButton generalButton={generalButton} />
                </div>
              </div>
            </div>
          )}
        </Col>
      </Row>
    </Container>
  );
}

UserSearch.propTypes = {
  userSearch: PropTypes.shape({
    state: PropTypes.any.isRequired
  }).isRequired,
  inputField: PropTypes.shape({
    type: PropTypes.string.isRequired,
    placeholder: PropTypes.string.isRequired,
    name: PropTypes.string.isRequired,
    valid: PropTypes.bool.isRequired,
    invalid: PropTypes.bool.isRequired,
    value: PropTypes.string.isRequired
  }).isRequired,
  generalButton: PropTypes.shape({
    type: PropTypes.string.isRequired,
    color: PropTypes.string.isRequired,
    disabled: PropTypes.bool.isRequired,
    label: PropTypes.any.isRequired
  }).isRequired
};
