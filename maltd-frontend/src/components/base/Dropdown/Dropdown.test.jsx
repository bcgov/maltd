import React from "react";
import renderer from "react-test-renderer";
import Adapter from "enzyme-adapter-react-16";
import Enzyme, { shallow } from "enzyme";
import Dropdown from "./Dropdown.jsx";

Enzyme.configure({ adapter: new Adapter() });

describe("Dropdown", () => {
  test("Component renders as expected", () => {
    const component = renderer.create(
      <Dropdown
        dropdown={{
          items: [
            {
              id: "123",
              name: "System 1",
              type: "Dynamics"
            }
          ]
        }}
        onDropdownClick={() => jest.fn()}
      />
    );

    const tree = component.toJSON();
    expect(tree).toMatchSnapshot();
  });

  test("Component onDropdownClick works as expected", () => {
    const testState = { isClicked: false, selectedTitle: "first" };
    const onClick = () => {
      testState.isClicked = true;
      testState.selectedTitle = "second";
    };

    const wrapper = shallow(
      <Dropdown
        dropdown={{
          items: [
            {
              id: "123",
              name: "System 1",
              type: "Dynamics"
            }
          ]
        }}
        onDropdownClick={onClick}
      />
    );

    wrapper.find("DropdownItem").simulate("click");
    expect(testState.isClicked).toEqual(true);
    expect(testState.selectedTitle).toEqual("second");
  });
});
