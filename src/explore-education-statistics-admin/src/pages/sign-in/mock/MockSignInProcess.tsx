import dashboardRoutes from "@admin/routes/dashboard/routes";
import PrototypeLoginService from '@admin/services/PrototypeLoginService';
import React from 'react';
import { Redirect } from 'react-router';

const MockSignInProcess = () => {
  window.sessionStorage.setItem(
    'mockLoginUserId',
    PrototypeLoginService.getUserList()[0].id,
  );
  return <Redirect to={dashboardRoutes.adminDashboard} />;
};

export default MockSignInProcess;
