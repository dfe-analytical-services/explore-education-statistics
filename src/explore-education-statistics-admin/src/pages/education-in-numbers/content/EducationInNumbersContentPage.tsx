import React from 'react';
import { RouteComponentProps } from 'react-router';
import { EducationInNumbersRouteParams } from '@admin/routes/educationInNumbersRoutes';

const EducationInNumbersContentPage = ({
  match,
}: RouteComponentProps<EducationInNumbersRouteParams>) => {
  const { educationInNumbersPageId } = match.params;

  return (
    <>
      <p>Under construction!</p>
      <p>Page content for {educationInNumbersPageId}</p>
    </>
  );
};

export default EducationInNumbersContentPage;
