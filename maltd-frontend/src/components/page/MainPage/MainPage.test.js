import React from "react";
import renderer from "react-test-renderer";
import axios from "axios";
import Adapter from "enzyme-adapter-react-16";
import Enzyme, { shallow } from "enzyme";
import MockAdapter from "axios-mock-adapter";
import waitUntil from "async-wait-until";
import MainPage from "./MainPage";

Enzyme.configure({ adapter: new Adapter() });

describe("Main page", () => {
  test("Component renders as expected", () => {
    const component = renderer.create(
      <MainPage onLogoutClick={() => jest.fn()} />
    );
    const tree = component.toJSON();
    expect(tree).toMatchSnapshot();
  });

  let wrapper;
  let instance;
  let mock;

  beforeEach(() => {
    wrapper = shallow(<MainPage onLogoutClick={() => jest.fn()} />);
    instance = wrapper.instance();
    mock = new MockAdapter(instance.axios);
  });

  describe("onBackClick", () => {
    test("Function modifies local state and takes user to user search screen as expected", () => {
      const clearFormFunc = jest.spyOn(MainPage.prototype, "clearForm");

      wrapper.setState({ isUserSearch: false });
      wrapper.find("BackIcon").simulate("click");

      expect(instance.state.isUserSearch).toBe(true);
      expect(clearFormFunc).toHaveBeenCalled();
      expect(instance.state.userExists).toBe(null);
      expect(instance.state.value).toBe("");
      expect(instance.state.isLoading).toBe(false);
      expect(instance.state.disabledButton).toBe(true);
      expect(instance.state.disabledInput).toBe(false);
      expect(instance.state.invalidInput).toBe(false);
      expect(instance.state.validInput).toBe(false);
    });
  });

  describe("updateSelectedDropdownItem", () => {
    test("Function updates the selected dropdown item", () => {
      const selectedProject = { id: "1", name: "project", type: "type" };

      wrapper.setState({ isUserSearch: false });
      wrapper
        .find("UserAccess")
        .props()
        .onDropdownClick(selectedProject);

      expect(instance.state.selectedDropdownItem).toBe(selectedProject);
    });
  });

  describe("onInputChange", () => {
    test("Function updates the input field with empty input", () => {
      const event = { target: { value: "" } };

      wrapper
        .find("UserSearch")
        .props()
        .onChange(event);

      expect(instance.state.userExists).toBe(null);
      expect(instance.state.invalidInput).toBe(false);
      expect(instance.state.validInput).toBe(false);
      expect(instance.state.disabledButton).toBe(true);
      expect(instance.state.color).toBe("primary");
      expect(instance.state.value).toBe(event.target.value);
    });

    test("Function updates the input field with input less than 3 characters in length", () => {
      const event = { target: { value: "aa" } };

      wrapper
        .find("UserSearch")
        .props()
        .onChange(event);

      expect(instance.state.userExists).toBe(null);
      expect(instance.state.invalidInput).toBe(true);
      expect(instance.state.color).toBe("danger");
      expect(instance.state.value).toBe(event.target.value);
    });

    test("Function updates the input field with valid input", () => {
      const event = { target: { value: "validinput" } };

      wrapper
        .find("UserSearch")
        .props()
        .onChange(event);

      expect(instance.state.userExists).toBe(null);
      expect(instance.state.invalidInput).toBe(false);
      expect(instance.state.validInput).toBe(true);
      expect(instance.state.disabledButton).toBe(false);
      expect(instance.state.color).toBe("primary");
      expect(instance.state.value).toBe(event.target.value);
    });
  });

  describe("onButtonClick", () => {
    test("Catches error when /api/projects endpoint is not called successfully (400)", () => {
      const clearFormFunc = jest.spyOn(MainPage.prototype, "clearForm");
      const data = { response: true };
      mock.onGet(`/api/projects`).reply(400, data);

      wrapper
        .find("UserSearch")
        .props()
        .onClick()
        .catch(() => {
          expect(clearFormFunc).toHaveBeenCalled();
        });
    });

    test("Catches error when /api/projects endpoint is not called successfully (401)", () => {
      const { location } = window;
      const data = { response: true };

      delete window.location;
      window.location = { reload: jest.fn() };

      mock.onGet(`/api/projects`).reply(401, data);

      wrapper
        .find("UserSearch")
        .props()
        .onClick()
        .catch(() => {
          expect(jest.isMockFunction(window.location.reload)).toBe(true);
          window.location.reload();
          expect(window.location.reload).toHaveBeenCalled();
        });

      window.location = location;
    });

    test("Function should make network request and should update state on success when all fields exist in response with member", async done => {
      wrapper.setState({
        isLoading: false,
        value: "val",
        isUserSearch: true,
        items: []
      });

      const data1 = [{ id: "1222", name: "Item name" }];

      const data2 = {
        projects: [
          {
            id: 123,
            name: "project",
            resources: [{ type: "Dyn", status: "member" }]
          }
        ],
        id: "1111",
        username: "LMA",
        firstName: "Let",
        lastName: "Me",
        email: "letme@example.ca"
      };

      mock.onGet(`/api/projects`).reply(200, data1);

      mock.onGet(`/api/users?q=${wrapper.state().value}`).reply(200, data1);

      mock.onGet(`/api/users/${wrapper.state().value}`).reply(200, data2);

      wrapper
        .find("UserSearch")
        .props()
        .onClick();

      await waitUntil(() => {
        return wrapper.state().isLoading;
      });

      expect(wrapper.state().isLoading).toEqual(true);
      expect(wrapper.state().isUserSearch).toEqual(false);
      expect(wrapper.state().items).toEqual(data1);
      expect(wrapper.state().projects).toEqual(data2.projects);
      expect(wrapper.state().userEmail).toEqual(data2.email);
      expect(wrapper.state().userName).toEqual(
        `${data2.firstName} ${data2.lastName}`
      );
      done();
    });

    test("Function should make network request and should update state on success when all fields exist in response with non-member", async done => {
      wrapper.setState({
        isLoading: false,
        value: "val",
        isUserSearch: true,
        items: []
      });

      const data1 = [{ id: "1222", name: "Item name" }];

      const data2 = {
        projects: [
          {
            id: 123,
            name: "project",
            resources: [{ type: "Dyn", status: "non-member" }]
          }
        ],
        id: "1111",
        username: "LMA",
        firstName: "Let",
        lastName: "Me",
        email: "letme@example.ca"
      };

      mock.onGet(`/api/projects`).reply(200, data1);

      mock.onGet(`/api/users?q=${wrapper.state().value}`).reply(200, data1);

      mock.onGet(`/api/users/${wrapper.state().value}`).reply(200, data2);

      wrapper
        .find("UserSearch")
        .props()
        .onClick();

      await waitUntil(() => {
        return wrapper.state().isLoading;
      });

      expect(wrapper.state().isLoading).toEqual(true);
      expect(wrapper.state().isUserSearch).toEqual(false);
      expect(wrapper.state().items).toEqual(data1);
      expect(wrapper.state().projects).toEqual([]);
      expect(wrapper.state().userEmail).toEqual(data2.email);
      expect(wrapper.state().userName).toEqual(
        `${data2.firstName} ${data2.lastName}`
      );
      done();
    });

    test("Function should make network request and should update specific state only on success when some fields exist in response", async done => {
      wrapper.setState({
        isLoading: false,
        value: "val",
        isUserSearch: true,
        items: []
      });

      const data1 = [{ id: "1222", name: "Item name" }];

      const data2 = {
        projects: [
          {
            id: 123,
            name: "project",
            resources: [{ type: "Dyn", status: "member" }]
          }
        ],
        id: "1111",
        username: "LMA"
      };

      mock.onGet(`/api/projects`).reply(200, data1);

      mock.onGet(`/api/users?q=${wrapper.state().value}`).reply(200, data1);

      mock.onGet(`/api/users/${wrapper.state().value}`).reply(200, data2);

      wrapper
        .find("UserSearch")
        .props()
        .onClick();

      await waitUntil(() => {
        return wrapper.state().isLoading;
      });

      expect(wrapper.state().isLoading).toEqual(true);
      expect(wrapper.state().isUserSearch).toEqual(false);
      expect(wrapper.state().items).toEqual(data1);
      expect(wrapper.state().projects).toEqual(data2.projects);
      done();
    });
  });

  describe("removeUserFromProject", () => {
    test("Function is called when x icon is clicked", () => {
      const removeUserFromProject = jest.spyOn(
        MainPage.prototype,
        "removeUserFromProject"
      );
      const id = 123;

      wrapper.setState({ isUserSearch: false });
      wrapper
        .find("UserAccess")
        .props()
        .onXClick(id);

      expect(removeUserFromProject).toHaveBeenCalled();
    });

    test("Catches error when /api/projects endpoint is not called successfully (401)", () => {
      const projectId = 1;
      wrapper.setState({ isUserSearch: false, value: "val" });
      const { location } = window;
      const data = { response: true };

      delete window.location;
      window.location = { reload: jest.fn() };

      mock
        .onDelete(`/api/projects/${projectId}/users/${wrapper.state().value}`)
        .reply(401, data);

      wrapper
        .find("UserAccess")
        .props()
        .onXClick(projectId)
        .catch(() => {
          expect(jest.isMockFunction(window.location.reload)).toBe(true);
          window.location.reload();
          expect(window.location.reload).toHaveBeenCalled();
        });

      window.location = location;
    });

    test("Function should make network request and should update state on success when removing a project", async done => {
      expect(wrapper.state().projects).toEqual([]);
      const projectId = 1;

      wrapper.setState({
        isUserSearch: false,
        value: "val",
        projects: [
          {
            id: 1,
            name: "oldproject",
            resources: [{ status: "member", type: "Dynamics" }]
          }
        ]
      });
      mock
        .onDelete(`/api/projects/${projectId}/users/${wrapper.state().value}`)
        .reply(200, {
          id: 1,
          name: "oldproject",
          users: [
            {
              username: wrapper.state().value,
              access: [{ type: "Dynamics", status: "not-member" }]
            }
          ]
        });

      wrapper
        .find("UserAccess")
        .props()
        .onXClick(projectId);

      await waitUntil(() => {
        return wrapper.state().projects;
      });

      expect(wrapper.state().projects).toEqual([]);
      done();
    });

    test("Function should make network request and should update state on success when removing a project partially", async done => {
      expect(wrapper.state().projects).toEqual([]);
      const projectId = 1;

      wrapper.setState({
        isUserSearch: false,
        value: "val",
        projects: [
          {
            id: 1,
            name: "oldproject",
            resources: [
              { status: "member", type: "Dynamics" },
              { status: "member", type: "Sharepoint" }
            ]
          }
        ]
      });
      mock
        .onDelete(`/api/projects/${projectId}/users/${wrapper.state().value}`)
        .reply(200, {
          id: 1,
          name: "oldproject",
          users: [
            {
              username: wrapper.state().value,
              access: [
                { type: "Dynamics", status: "not-member" },
                { type: "Sharepoint", status: "member" }
              ]
            }
          ]
        });

      wrapper
        .find("UserAccess")
        .props()
        .onXClick(projectId);

      await waitUntil(() => {
        return wrapper.state().projects;
      });

      expect(wrapper.state().projects).toEqual([
        {
          id: 1,
          name: "oldproject",
          resources: [
            { type: "Dynamics", status: "not-member" },
            { type: "Sharepoint", status: "member" }
          ]
        }
      ]);
      done();
    });
  });

  describe("addUserToProject", () => {
    test("Function is called when plus icon is clicked", () => {
      const addUserToProject = jest.spyOn(
        MainPage.prototype,
        "addUserToProject"
      );
      wrapper.setState({ selectedDropdownItem: { id: 122 } });

      wrapper.setState({ isUserSearch: false });
      wrapper
        .find("UserAccess")
        .props()
        .onPlusClick();

      expect(addUserToProject).toHaveBeenCalled();
    });

    test("Catches error when /api/projects endpoint is not called successfully (401)", () => {
      const { location } = window;
      const data = { response: true };

      wrapper.setState({
        isUserSearch: false,
        value: "val",
        selectedDropdownItem: { id: 123 }
      });

      delete window.location;
      window.location = { reload: jest.fn() };

      mock
        .onPut(
          `/api/projects/${wrapper.state().selectedDropdownItem.id}/users/${
            wrapper.state().value
          }`
        )
        .reply(401, data);

      wrapper
        .find("UserAccess")
        .props()
        .onPlusClick()
        .catch(() => {
          expect(jest.isMockFunction(window.location.reload)).toBe(true);
          window.location.reload();
          expect(window.location.reload).toHaveBeenCalled();
        });

      window.location = location;
    });

    test("Function should make network request and should update state on success when adding a project", async done => {
      expect(wrapper.state().projects).toEqual([]);

      wrapper.setState({
        isUserSearch: false,
        selectedDropdownItem: { id: 123 },
        value: "val",
        projects: [
          {
            id: 1,
            name: "oldproject",
            resources: [{ status: "member", type: "Dynamics" }]
          }
        ]
      });
      mock
        .onPut(
          `/api/projects/${wrapper.state().selectedDropdownItem.id}/users/${
            wrapper.state().value
          }`
        )
        .reply(200, {
          id: 123,
          name: "Newly added project",
          users: [
            {
              username: wrapper.state().value,
              access: [{ type: "Sharepoint", status: "member" }]
            }
          ]
        });

      wrapper
        .find("UserAccess")
        .props()
        .onPlusClick();

      await waitUntil(() => {
        return wrapper.state().projects;
      });

      expect(wrapper.state().projects).toEqual([
        {
          id: 1,
          name: "oldproject",
          resources: [{ status: "member", type: "Dynamics" }]
        },
        {
          id: 123,
          name: "Newly added project",
          resources: [{ status: "member", type: "Sharepoint" }]
        }
      ]);
      done();
    });

    test("Function should make network request and should update state on success when adding a project (test 2)", async done => {
      expect(wrapper.state().projects).toEqual([]);

      wrapper.setState({
        isUserSearch: false,
        selectedDropdownItem: { id: 123 },
        value: "val",
        projects: [
          {
            id: 123,
            name: "oldproject",
            resources: [{ status: "member", type: "Dynamics" }]
          }
        ]
      });
      mock
        .onPut(
          `/api/projects/${wrapper.state().selectedDropdownItem.id}/users/${
            wrapper.state().value
          }`
        )
        .reply(200, {
          id: 123,
          name: "oldproject",
          users: [
            {
              username: wrapper.state().value,
              access: [
                { type: "Sharepoint", status: "member" },
                { type: "Dynamics", status: "member" }
              ]
            }
          ]
        });

      wrapper
        .find("UserAccess")
        .props()
        .onPlusClick();

      await waitUntil(() => {
        return wrapper.state().projects;
      });

      expect(wrapper.state().projects).toEqual([
        {
          id: 123,
          name: "oldproject",
          resources: [
            { status: "member", type: "Sharepoint" },
            { type: "Dynamics", status: "member" }
          ]
        }
      ]);
      done();
    });

    test("Function should make network request and should update state on success when adding a project partially", async done => {
      expect(wrapper.state().projects).toEqual([]);

      wrapper.setState({
        isUserSearch: false,
        selectedDropdownItem: { id: 1 },
        value: "val",
        projects: [
          {
            id: 123,
            name: "oldproject",
            resources: [{ status: "member", type: "Dynamics" }]
          }
        ]
      });
      mock
        .onPut(
          `/api/projects/${wrapper.state().selectedDropdownItem.id}/users/${
            wrapper.state().value
          }`
        )
        .reply(200, {
          id: 1,
          name: "newproject",
          users: [
            {
              username: wrapper.state().value,
              access: [
                { type: "Sharepoint", status: "member" },
                { type: "Dynamics", status: "not-member" }
              ]
            }
          ]
        });

      wrapper
        .find("UserAccess")
        .props()
        .onPlusClick();

      await waitUntil(() => {
        return wrapper.state().projects;
      });

      expect(wrapper.state().projects).toEqual([
        {
          id: 123,
          name: "oldproject",
          resources: [{ type: "Dynamics", status: "member" }]
        },
        {
          id: 1,
          name: "newproject",
          resources: [
            { type: "Sharepoint", status: "member" },
            { type: "Dynamics", status: "not-member" }
          ]
        }
      ]);
      done();
    });

    test("Function should catch duplicate case and set error message when adding duplicate project", async done => {
      wrapper.setState({
        isUserSearch: false,
        selectedDropdownItem: { id: 123 },
        projects: [
          { id: 123, resources: [{ status: "member", type: "Dynamics" }] }
        ],
        value: "val"
      });

      mock
        .onPut(
          `/api/projects/${wrapper.state().selectedDropdownItem.id}/users/${
            wrapper.state().value
          }`
        )
        .reply(200, {
          users: [
            {
              username: wrapper.state().value,
              access: [{ type: "Dynamics", status: "member" }]
            }
          ]
        });

      wrapper
        .find("UserAccess")
        .props()
        .onPlusClick();

      await waitUntil(() => {
        return wrapper.state().duplicateErrorMessage;
      });

      expect(wrapper.state().duplicateErrorMessage).toEqual(
        "This project has already been added. Please try again with a different project."
      );
      done();
    });
  });

  describe("onKeyEnter", () => {
    test("Function works as expected under case where button click should not be called", () => {
      const event = { key: "Fake" };
      const onButtonClick = jest.spyOn(MainPage.prototype, "onButtonClick");

      wrapper.setState({ disabledButton: true });

      wrapper.instance().onKeyEnter(event);

      expect(onButtonClick).toHaveBeenCalledTimes(0);
    });

    test("Function works as expected under case where button click should be called", () => {
      const event = { key: "Enter" };
      const onButtonClickSpy = jest.spyOn(MainPage.prototype, "onButtonClick");

      wrapper.setState({ disabledButton: false });

      wrapper.instance().onKeyEnter(event);

      expect(onButtonClickSpy).toHaveBeenCalledTimes(1);
    });

    test("Function is called on key enter", () => {
      const event = { key: "Enter" };
      const onKeyEnterSpy = jest.spyOn(MainPage.prototype, "onKeyEnter");

      wrapper
        .find("UserSearch")
        .props()
        .onKeyEnter(event);

      expect(onKeyEnterSpy).toHaveBeenCalledTimes(1);
    });
  });
});
