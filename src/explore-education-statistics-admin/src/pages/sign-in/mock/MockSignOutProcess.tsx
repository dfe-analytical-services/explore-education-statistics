import React from 'react';
import {Redirect} from "react-router";

const MockSignOutProcess = () => {
  window.sessionStorage.removeItem('mockLoginUserId');
  return <Redirect to='/signed-out' />;
};

export default MockSignOutProcess;