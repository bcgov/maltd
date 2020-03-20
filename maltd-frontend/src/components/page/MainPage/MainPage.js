import React, { Component } from "react";
import PropTypes from "prop-types";
import axios from "axios";
import "./MainPage.css";
import UserSearch from "../../composite/UserSearch/UserSearch";
import NavBar from "../../base/NavBar/NavBar";
import BackIcon from "../../base/BackIcon/BackIcon";
import UserAccess from "../../composite/UserAccess/UserAccess";
import { checkArrayEquality } from "../../../modules/HelperFunctions";

const token = localStorage.getItem("jwt");

export default class MainPage extends Component {
  constructor(props) {
    super(props);
    this.state = {
      validInput: false,
      invalidInput: false,
      value: "",
      disabledInput: false,
      disabledButton: true,
      isLoading: false,
      isUserSearch: true,
      projects: [],
      userEmail: null,
      userName: "",
      color: "primary",
      userExists: null,
      items: [],
      selectedDropdownItem: null,
      duplicateErrorMessage: null
    };
  }

  onButtonClick() {
    const { value } = this.state;

    return axios
      .get(`/api/projects`, { headers: { Authorization: `Bearer ${token}` } })
      .then(res => {
        this.setState({
          items: res.data,
          isLoading: true,
          disabledButton: true,
          disabledInput: true
        });
      })
      .then(() => {
        axios.get(`/api/users?q=${value}`, {
          headers: { Authorization: `Bearer ${token}` }
        });
      })
      .then(() => {
        return axios
          .get(`/api/users/${value}`, {
            headers: { Authorization: `Bearer ${token}` }
          })
          .then(result => {
            const { data } = result;
            const finalProjects = [];

            data.projects.forEach(proj => {
              let shouldAddProject = false;
              const resources = [];

              proj.resources.forEach(resource => {
                if (resource.status === "member") {
                  resources.push(resource);
                  shouldAddProject = true;
                }
              });

              if (shouldAddProject)
                finalProjects.push({ name: proj.name, resources, id: proj.id });
            });

            this.setState({
              projects: finalProjects,
              isUserSearch: false
            });

            if (data.email) {
              this.setState({ userEmail: data.email });
            }

            if (data.firstName && data.lastName) {
              this.setState({
                userName: `${data.firstName} ${data.lastName}`
              });
            }
          });
      })
      .catch(() => {
        this.clearForm();
      });
  }

  onInputChange(event) {
    this.setState({ userExists: null });
    const val = event.target.value;

    if (val.length === 0) {
      this.setState({
        invalidInput: false,
        validInput: false,
        disabledButton: true,
        color: "primary"
      });
    } else if (val.length < 3) {
      this.setState({
        invalidInput: true,
        color: "danger"
      });
    } else {
      this.setState({
        invalidInput: false,
        validInput: true,
        disabledButton: false,
        color: "primary"
      });
    }

    this.setState({ value: event.target.value });
  }

  onBackClick() {
    this.setState({ isUserSearch: true });

    this.clearForm();

    this.setState({
      userExists: null,
      value: ""
    });
  }

  onKeyEnter(event) {
    const { disabledButton } = this.state;
    if (event.key === "Enter" && !disabledButton) {
      this.onButtonClick();
    }
  }

  removeUserFromProject(projectId) {
    const { value, projects } = this.state;

    return axios
      .delete(`/api/projects/${projectId}/users/${value}`, {
        headers: { Authorization: `Bearer ${token}` }
      })
      .then(res => {
        // figure out which resources user has access to after remove
        const { users } = res.data;

        let userResources;
        users.forEach(user => {
          if (user.username.toLowerCase() === value) {
            userResources = user.access.slice(0);
          }
        });

        let isNonMemberForAllResources = true;
        userResources.forEach(userResource => {
          if (userResource.status !== "not-member") {
            isNonMemberForAllResources = false;
          }
        });

        const updatedProjects = [];
        projects.forEach(proj => {
          if (!isNonMemberForAllResources) {
            updatedProjects.push({
              id: proj.id,
              name: proj.name,
              resources: userResources
            });
          } else if (proj.id !== projectId) {
            updatedProjects.push(proj);
          }
        });

        this.setState({ projects: updatedProjects });
      })
      .catch(() => {});
  }

  addUserToProject() {
    const { selectedDropdownItem, value, projects } = this.state;

    // get resources currently existing for added project if any
    let existingProjectResources;
    projects.forEach(p => {
      if (p.id === selectedDropdownItem.id) {
        existingProjectResources = p.resources.slice(0);
      }
    });

    return axios
      .put(`/api/projects/${selectedDropdownItem.id}/users/${value}`, null, {
        headers: { Authorization: `Bearer ${token}` }
      })
      .then(res => {
        // figure out which resources user has access to after add
        const { users } = res.data;

        let userResources;
        users.forEach(user => {
          if (user.username.toLowerCase() === value) {
            userResources = user.access.slice(0);
          }
        });

        if (!existingProjectResources) {
          this.setState({
            projects: [
              ...projects,
              { id: res.data.id, name: res.data.name, resources: userResources }
            ]
          });
          return true;
        }

        if (!checkArrayEquality(existingProjectResources, userResources)) {
          const updatedProjects = [];

          projects.forEach(proj => {
            if (proj.id === selectedDropdownItem.id) {
              updatedProjects.push({
                ...selectedDropdownItem,
                resources: userResources
              });
            } else {
              updatedProjects.push(proj);
            }
          });

          this.setState({
            projects: updatedProjects,
            selectedDropdownItem: null
          });

          return true;
        }

        this.setState({
          duplicateErrorMessage:
            "This project has already been added. Please try again with a different project."
        });

        setTimeout(() => {
          this.setState({
            duplicateErrorMessage: null,
            selectedDropdownItem: null
          });
        }, 5000);

        return true;
      })
      .catch(err => {
        return false;
      });
  }

  updateSelectedDropdownItem(selectedProject) {
    this.setState({ selectedDropdownItem: selectedProject });
  }

  clearForm() {
    this.setState({
      userExists: false,
      isLoading: false,
      disabledButton: true,
      disabledInput: false,
      invalidInput: false,
      validInput: false
    });
  }

  render() {
    const {
      validInput,
      invalidInput,
      value,
      disabledInput,
      disabledButton,
      isLoading,
      isUserSearch,
      projects,
      userEmail,
      userName,
      color,
      userExists,
      items,
      selectedDropdownItem,
      duplicateErrorMessage
    } = this.state;

    const { onLogoutClick } = this.props;

    const inputField = {
      type: "text",
      name: "idir",
      placeholder: "Enter IDIR username to find",
      valid: validInput,
      invalid: invalidInput,
      value,
      disabled: disabledInput
    };

    const generalButton = {
      type: "submit",
      color,
      disabled: disabledButton,
      label: "Find"
    };

    const userSearch = {
      state: {
        isLoading,
        userExists
      }
    };

    const userAccess = {
      projects,
      userName,
      userEmail
    };

    const backIcon = {
      message: "Find a user"
    };

    const dropdown = {
      items,
      selectedDropdownItem
    };

    return (
      <React.Fragment>
        <NavBar onClick={() => onLogoutClick()} />
        <div className="top-spacing" id="wrapper">
          {!isUserSearch && (
            <div className="backicon-spacing">
              <BackIcon
                backIcon={backIcon}
                onClick={() => this.onBackClick()}
              />
            </div>
          )}
          <div className="my-3 p-3 rounded shadow less-spacing-top">
            <h4 className="add-remove-text">Add or Remove User</h4>
            {isUserSearch && (
              <UserSearch
                userSearch={userSearch}
                inputField={inputField}
                onChange={e => this.onInputChange(e)}
                generalButton={generalButton}
                onClick={() => this.onButtonClick()}
                onKeyEnter={e => this.onKeyEnter(e)}
              />
            )}

            {!isUserSearch && (
              <UserAccess
                userAccess={userAccess}
                onXClick={id => this.removeUserFromProject(id)}
                onPlusClick={() => this.addUserToProject()}
                onDropdownClick={item => this.updateSelectedDropdownItem(item)}
                dropdown={dropdown}
                duplicateErrorMessage={duplicateErrorMessage}
              />
            )}
          </div>
        </div>
      </React.Fragment>
    );
  }
}

MainPage.propTypes = {
  onLogoutClick: PropTypes.func.isRequired
};
