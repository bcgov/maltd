/* eslint-disable react/jsx-curly-newline */
import React, { Component } from "react";
import PropTypes from "prop-types";
import axios from "axios";
import { Header, Footer, Button } from "shared-components";
import "./MainPage.css";
import "../page.css";
import UserSearch from "../../composite/UserSearch/UserSearch";
import BackIcon from "../../base/BackIcon/BackIcon";
import UserAccess from "../../composite/UserAccess/UserAccess";
import checkArrayEquality from "../../../modules/HelperFunctions";

const Unauthorized = 401;

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
      userExists: null,
      items: [],
      selectedDropdownItem: null,
      duplicateErrorMessage: null,
      addProjectButtonShowLoader: false,
      errorMessage:
        "This user does not exist, please try again with a different IDIR username."
    };

    const baseURL = window.REACT_APP_MALTD_API
      ? window.REACT_APP_MALTD_API
      : process.env.REACT_APP_MALTD_API;

    this.apiBasePath = window.REACT_APP_API_BASE_PATH
      ? window.REACT_APP_API_BASE_PATH
      : process.env.REACT_APP_API_BASE_PATH;

    if (!this.apiBasePath) {
      this.apiBasePath = "/api";
    }

    const config = {
      timeout: 120000
    };

    if (baseURL) {
      config.baseURL = baseURL;
    }

    this.axios = axios.create(config);
  }

  static getAccessToken() {
    return localStorage.getItem("jwt");
  }

  static onUnauthorizedResponse() {
    localStorage.removeItem("jwt");
    window.location.reload();
  }

  async onButtonClick() {
    const { value } = this.state;

    try {
      const res = await this.axios.get(`${this.apiBasePath}/projects`, {
        headers: { Authorization: `Bearer ${MainPage.getAccessToken()}` }
      });
      this.setState({
        items: res.data,
        isLoading: true,
        disabledButton: true,
        disabledInput: true
      });
      try {
        await this.axios.get(`${this.apiBasePath}/users?q=${value}`, {
          headers: { Authorization: `Bearer ${MainPage.getAccessToken()}` }
        });
      } catch (e) {
        const { status } = e.response;
        let errMessage = "";
        if (status === 500)
          errMessage =
            "An error occurred while processing your request. Please try again.";
        if (status === 504)
          errMessage = "Your request took too long. Please try again.";
        if (status === 400)
          errMessage =
            "There is an issue with your request. Please check the IDIR and try again.";
        if (errMessage) this.setState({ errorMessage: errMessage });
      }

      const result = await this.axios.get(
        `${this.apiBasePath}/users/${value}`,
        {
          headers: { Authorization: `Bearer ${MainPage.getAccessToken()}` }
        }
      );
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
    } catch (err) {
      if (err && err.response && err.response.status) {
        const { status } = err.response;
        if (status === Unauthorized) {
          MainPage.onUnauthorizedResponse();
        }
      }
      this.clearForm();
    }
  }

  onInputChange(event) {
    this.setState({ userExists: null });
    const val = event.target.value;

    if (val.length === 0) {
      this.setState({
        invalidInput: false,
        validInput: false,
        disabledButton: true
      });
    } else if (val.length < 3) {
      this.setState({
        invalidInput: true
      });
    } else {
      this.setState({
        invalidInput: false,
        validInput: true,
        disabledButton: false
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

  async removeUserFromProject(projectId) {
    const { value, projects } = this.state;

    try {
      const res = await this.axios.delete(
        `${this.apiBasePath}/projects/${projectId}/users/${value}`,
        {
          headers: { Authorization: `Bearer ${MainPage.getAccessToken()}` }
        }
      );
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
        if (!isNonMemberForAllResources && proj.id === projectId) {
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
    } catch (err) {
      if (err && err.response && err.response.status) {
        const { status } = err.response;
        if (status === Unauthorized) {
          MainPage.onUnauthorizedResponse();
        }
      }
    }
  }

  async addUserToProject() {
    this.setState({ addProjectButtonShowLoader: true });
    const { selectedDropdownItem, value, projects } = this.state;

    // get resources currently existing for added project if any
    let existingProjectResources;
    projects.forEach(p => {
      if (p.id === selectedDropdownItem.id) {
        existingProjectResources = p.resources.slice(0);
      }
    });

    try {
      const res = await this.axios.put(
        `${this.apiBasePath}/projects/${selectedDropdownItem.id}/users/${value}`,
        null,
        {
          headers: { Authorization: `Bearer ${MainPage.getAccessToken()}` }
        }
      );
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
          ],
          selectedDropdownItem: null,
          addProjectButtonShowLoader: false
        });
        return true;
      }
      if (!checkArrayEquality(existingProjectResources, userResources)) {
        const updatedProjects = [];
        projects.forEach(proj => {
          if (proj.id === selectedDropdownItem.id) {
            updatedProjects.push({
              ...selectedDropdownItem,
              name: proj.name,
              resources: userResources
            });
          } else {
            updatedProjects.push(proj);
          }
        });
        this.setState({
          projects: updatedProjects,
          selectedDropdownItem: null,
          addProjectButtonShowLoader: false
        });
        return true;
      }
      this.setState({
        duplicateErrorMessage:
          "This project has already been added. Please try again with a different project.",
        addProjectButtonShowLoader: false
      });
      setTimeout(() => {
        this.setState({
          duplicateErrorMessage: null,
          selectedDropdownItem: null
        });
      }, 5000);
      return true;
    } catch (err) {
      if (err && err.response && err.response.status) {
        const { status } = err.response;
        if (status === Unauthorized) {
          MainPage.onUnauthorizedResponse();
        }
      }
      return false;
    }
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
      userExists,
      items,
      selectedDropdownItem,
      duplicateErrorMessage,
      addProjectButtonShowLoader,
      errorMessage
    } = this.state;

    const { onLogoutClick, header } = this.props;

    const inputField = {
      type: "text",
      name: "idir",
      placeholder: "Enter IDIR username to find",
      valid: validInput,
      invalid: invalidInput,
      value,
      disabled: disabledInput
    };

    const userSearch = {
      state: {
        isLoading,
        userExists,
        errorMessage
      }
    };

    const userAccess = {
      projects,
      userName,
      userEmail
    };

    const backIcon = {
      message: "Find another user"
    };

    const dropdown = {
      items,
      selectedDropdownItem
    };

    return (
      <main>
        <Header header={header} />
        <div className="navbar-styling">
          {!isUserSearch && (
            <div className="backicon-spacing">
              <BackIcon
                backIcon={backIcon}
                onClick={() => this.onBackClick()}
              />
            </div>
          )}
          {userSearch && <div />}
          <Button
            onClick={onLogoutClick}
            label="Logout"
            styling="logout-btn btn"
          />
        </div>
        <div className="page">
          <div className="content col-md-12">
            <div className="my-3 p-3 rounded shadow less-spacing-top">
              <h2>Add or Remove User</h2>
              {isUserSearch && (
                <UserSearch
                  userSearch={userSearch}
                  inputField={inputField}
                  isButtonDisabled={disabledButton}
                  onChange={e => this.onInputChange(e)}
                  onClick={() => this.onButtonClick()}
                  onKeyEnter={e => this.onKeyEnter(e)}
                />
              )}
              {!isUserSearch && (
                <UserAccess
                  userAccess={userAccess}
                  isButtonDisabled={addProjectButtonShowLoader}
                  onXClick={id => this.removeUserFromProject(id)}
                  onPlusClick={() => this.addUserToProject()}
                  onDropdownClick={item =>
                    this.updateSelectedDropdownItem(item)
                  }
                  dropdown={dropdown}
                  duplicateErrorMessage={duplicateErrorMessage}
                />
              )}
            </div>
          </div>
        </div>
        <Footer />
      </main>
    );
  }
}

MainPage.propTypes = {
  onLogoutClick: PropTypes.func.isRequired,
  header: PropTypes.shape({
    name: PropTypes.string.isRequired,
    history: PropTypes.object.isRequired
  }).isRequired
};
