import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import EducationInNumbersPageForm from '@admin/pages/education-in-numbers/components/EducationInNumbersSummaryForm';
import { educationInNumbersListRoute } from '@admin/routes/routes';
import {
  EducationInNumbersRouteParams,
  educationInNumbersSummaryRoute,
} from '@admin/routes/educationInNumbersRoutes';
import educationInNumbersService from '@admin/services/educationInNumbersService';
import React from 'react';
import { generatePath, useHistory } from 'react-router-dom';

const EducationInNumbersCreatePage = () => {
  const history = useHistory();
  return (
    <Page
      title="Create a new Education in Numbers page"
      breadcrumbs={[
        {
          name: 'Manage Education in Numbers',
          link: educationInNumbersListRoute.path,
        },
        { name: 'Create page' },
      ]}
    >
      <EducationInNumbersPageForm
        cancelButton={
          <Link unvisited to={educationInNumbersListRoute.path}>
            Cancel
          </Link>
        }
        onSubmit={async values => {
          const newPage =
            await educationInNumbersService.createEducationInNumbersPage(
              values,
            );

          history.push(
            generatePath<EducationInNumbersRouteParams>(
              educationInNumbersSummaryRoute.path,
              {
                educationInNumbersPageId: newPage.id,
              },
            ),
          );
        }}
      />
    </Page>
  );
};

export default EducationInNumbersCreatePage;
