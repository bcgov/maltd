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
    const component = renderer.create(<MainPage />);
    const tree = component.toJSON();
    expect(tree).toMatchSnapshot();
  });

  let wrapper;
  let instance;
  let mock;
  const baseUrl = process.env.REACT_APP_MALTD_API
    ? process.env.REACT_APP_MALTD_API
    : "https://localhost:5001";

  beforeEach(() => {
    wrapper = shallow(<MainPage />);
    instance = wrapper.instance();
    mock = new MockAdapter(axios);
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
    test("Catches error when /api/projects endpoint is not called successfully", () => {
      const clearFormFunc = jest.spyOn(MainPage.prototype, "clearForm");
      const data = { response: true };
      mock.onGet(`${baseUrl}/api/projects`).reply(400, data);

      wrapper
        .find("UserSearch")
        .props()
        .onClick()
        .catch(() => {
          expect(clearFormFunc).toHaveBeenCalled();
        });
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

    test("Function should make network request and should update state on success", async done => {
      const mock = new MockAdapter(axios);

      expect(wrapper.state().projects).toEqual([]);

      wrapper.setState({
        isUserSearch: false,
        selectedDropdownItem: { id: 123 },
        value: "val"
      });
      mock
        .onPut(
          `${baseUrl}/api/projects/${
            wrapper.state().selectedDropdownItem.id
          }/users/${wrapper.state().value}`
        )
        .reply(200);

      wrapper
        .find("UserAccess")
        .props()
        .onPlusClick();

      await waitUntil(() => {
        return wrapper.state().projects;
      });

      expect(wrapper.state().projects).toEqual([{ id: 123 }]);
      done();
    });
  });
});
