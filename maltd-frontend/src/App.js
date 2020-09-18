import React from "react";
import { Switch, Redirect, Route, useHistory } from "react-router-dom";
import AuthenticationGuard from "./components/hoc/AuthenticationGuard";

export default function App() {
  const header = {
    name: "Account and License Management Tool",
    history: useHistory()
  };

  return (
    <div>
      <Switch>
        <Redirect exact from="/" to="/ecmusermgmt" />
        <Route exact path="/ecmusermgmt">
          <AuthenticationGuard header={header} />
        </Route>
      </Switch>
    </div>
  );
}
