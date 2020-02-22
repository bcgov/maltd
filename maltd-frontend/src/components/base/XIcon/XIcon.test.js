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

  test("Component onClick and onKeyDown work as expected", () => {
    const testState = { isClicked: false };
    const onClick = () => {
      testState.isClicked = true;
    };

    const wrapper = shallow(
      <XIcon id="123" onKeyDown={onClick} onClick={onClick} />
    );

    wrapper.find("#main").simulate("keydown");
    expect(testState.isClicked).toEqual(true);

    wrapper.find("#main").simulate("click");
    expect(testState.isClicked).toEqual(true);
  });
});
