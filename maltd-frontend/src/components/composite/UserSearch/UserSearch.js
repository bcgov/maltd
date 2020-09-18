import React from "react";
import PropTypes from "prop-types";
import { Button } from "shared-components";
import InputField from "../../base/InputField/InputField";
import "./UserSearch.css";

export default function UserSearch({
  userSearch: { state },
  inputField,
  isButtonDisabled,
  onClick,
  onChange,
  onKeyEnter
}) {
  return (
    <div className="move-down">
      <p>
        Find a user by IDIR username and add them to or remove them from
        projects.
      </p>
      <p />
      {!state.isLoading && state.userExists === null && (
        <div className="inner">
          <InputField
            inputField={inputField}
            onChange={onChange}
            onKeyEnter={onKeyEnter}
          />
          <p />
          <div className="error-btn-div">
            <div />
            <div className="d-flex justify-content-end">
              <Button
                disabled={isButtonDisabled}
                onClick={onClick}
                label="Find"
                styling="normal-blue btn"
              />
            </div>
          </div>
        </div>
      )}
      {state.isLoading && state.userExists === null && (
        <div className="inner">
          <div>
            <InputField inputField={inputField} />
            <p />
            <div>
              <div className="d-flex justify-content-end">
                <Button
                  onClick={onClick}
                  disabled
                  hasLoader
                  label="Find"
                  styling="normal-blue btn"
                />
              </div>
            </div>
          </div>
        </div>
      )}
      {state.userExists === false && (
        <div className="inner">
          <div>
            <InputField inputField={inputField} onChange={onChange} />
            <p />
            <div className="error-btn-div">
              <div className="float-left">
                <small className="error-message" data-cy="error-text">
                  {state.errorMessage}
                </small>
              </div>
              <div className="d-flex justify-content-end">
                <Button
                  onClick={onClick}
                  disabled={isButtonDisabled}
                  label="Find"
                  styling="normal-blue btn"
                />
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
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
  onClick: PropTypes.func,
  onChange: PropTypes.func,
  onKeyEnter: PropTypes.func,
  isButtonDisabled: PropTypes.bool
};

UserSearch.defaultProps = {
  onClick: () => {},
  onChange: () => {},
  onKeyEnter: () => {},
  isButtonDisabled: false
};
