import React from "react";
import renderer from "react-test-renderer";
import Adapter from "enzyme-adapter-react-16";
import Enzyme, { shallow } from "enzyme";
import MainPage from "./MainPage";

Enzyme.configure({ adapter: new Adapter() });

describe("Main page", () => {
  test("Component renders as expected", () => {
    const component = renderer.create(<MainPage />);
    const tree = component.toJSON();
    expect(tree).toMatchSnapshot();
  });

  describe("onBackClick", () => {
    test("Function modifies local state and takes user to user search screen as expected", () => {
      const wrapper = shallow(<MainPage />);
      const instance = wrapper.instance();

      wrapper.setState({ isUserSearch: false });
      wrapper.find("BackIcon").simulate("click");

      expect(instance.state.isUserSearch).toBe(true);
    });
  });
});
