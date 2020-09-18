import React from "react";
import renderer from "react-test-renderer";
import { MdDeleteForever } from "react-icons/md";
import Adapter from "enzyme-adapter-react-16";
import Enzyme, { shallow } from "enzyme";
import ListElement from "./ListElement";

Enzyme.configure({ adapter: new Adapter() });

describe("List Element", () => {
  test("Component renders as expected", () => {
    const component = renderer.create(
      <ListElement
        listElement={{
          title: "title",
          description: "description"
        }}
      />
    );

    const tree = component.toJSON();
    expect(tree).toMatchSnapshot();
  });

  test("Component does not display Delete icon when it should not", () => {
    const testState = { isClicked: false };
    const onClick = () => {
      testState.isClicked = true;
    };

    const wrapper = shallow(
      <ListElement
        listElement={{
          title: "title",
          description: "description",
          id: "12"
        }}
        onXClick={null}
      />
    );

    expect(wrapper.contains(<MdDeleteForever onClick={onClick} />)).toBe(false);
  });

  test("Component generates the member of resources string appropriately", () => {
    const wrapper = shallow(
      <ListElement
        listElement={{
          title: "title",
          description: "description",
          resources: [{ type: "Dyn" }, { type: "Share" }]
        }}
      />
    );

    const rows = wrapper.find("#member-resources");

    expect(rows.length).toEqual(1);
    expect(rows.text()).toEqual("Member: Dyn Share ");
  });
});
