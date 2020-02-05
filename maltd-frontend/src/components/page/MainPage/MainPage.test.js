/* eslint-disable react/jsx-filename-extension */
import React from "react";
import renderer from "react-test-renderer";
import Adapter from "enzyme-adapter-react-16";
import Enzyme from "enzyme";
import MainPage from "./MainPage";

Enzyme.configure({ adapter: new Adapter() });

describe("Main page", () => {
  test("Component renders as expected", () => {
    const component = renderer.create(<MainPage />);

    const tree = component.toJSON();
    expect(tree).toMatchSnapshot();
  });
});
