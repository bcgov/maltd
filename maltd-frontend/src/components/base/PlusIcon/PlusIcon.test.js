import React from "react";
import renderer from "react-test-renderer";
import Adapter from "enzyme-adapter-react-16";
import Enzyme, { shallow } from "enzyme";
import PlusIcon from "./PlusIcon";

Enzyme.configure({ adapter: new Adapter() });

describe("Plus Icon", () => {
  test("Component renders as expected", () => {
    const component = renderer.create(<PlusIcon onClick={() => jest.fn()} />);

    const tree = component.toJSON();
    expect(tree).toMatchSnapshot();
  });

  test("Component onClick and onKeyDown work as expected", () => {
    const testState = { isClicked: false };
    const onClick = () => {
      testState.isClicked = true;
    };

    const wrapper = shallow(
      <PlusIcon id="123" onKeyDown={onClick} onClick={onClick} />
    );

    wrapper.find("#plus-icon").simulate("keydown");
    expect(testState.isClicked).toEqual(true);

    wrapper.find("#plus-icon").simulate("click");
    expect(testState.isClicked).toEqual(true);
  });
});
