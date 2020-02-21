import React from "react";
import renderer from "react-test-renderer";
import Adapter from "enzyme-adapter-react-16";
import Enzyme, { shallow } from "enzyme";
import XIcon from "./XIcon";

Enzyme.configure({ adapter: new Adapter() });

describe("X Icon", () => {
  test("Component renders as expected", () => {
    const component = renderer.create(
      <XIcon id="123" onClick={() => jest.fn()} />
    );

    const tree = component.toJSON();
    expect(tree).toMatchSnapshot();
  });

  test("Component onClick works as expected", () => {
    const testState = { isClicked: false };
    const wrapper = shallow(
      <XIcon
        id="123"
        onClick={() => {
          testState.isClicked = true;
        }}
      />
    );

    wrapper.find("#main").simulate("click");
    expect(testState.isClicked).toEqual(true);
  });
});
