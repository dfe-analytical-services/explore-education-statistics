import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import EducationInNumbersPageForm from '@admin/pages/education-in-numbers/components/EducationInNumbersSummaryForm';
import {
  educationInNumbersListRoute,
  educationInNumbersRoute,
} from '@admin/routes/routes';
import { EducationInNumbersRouteParams } from '@admin/routes/educationInNumbersRoutes';
import educationInNumbersService from '@admin/services/educationInNumbersService';
import appendQuery from '@common/utils/url/appendQuery';
import React from 'react';
import { RouteComponentProps } from 'react-router';

const EducationInNumbersCreatePage = ({
  history,
}: RouteComponentProps<EducationInNumbersRouteParams>) => {
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
            appendQuery<EducationInNumbersRouteParams>(
              educationInNumbersRoute.path,
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
