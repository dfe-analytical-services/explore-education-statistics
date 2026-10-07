import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import EducationInNumbersPageForm from '@admin/pages/education-in-numbers/components/EducationInNumbersSummaryForm';
import { educationInNumbersListRoute } from '@admin/routes/routes';
import { educationInNumbersSummaryRoute } from '@admin/routes/educationInNumbersRoutes';
import educationInNumbersService from '@admin/services/educationInNumbersService';
import React from 'react';
import { generatePath } from 'react-router-dom';
import { useNavigate } from 'react-router';

const EducationInNumbersCreatePage = () => {
  const navigate = useNavigate();

  return (
    <Page
      title="Create a new Education in Numbers page"
      breadcrumbs={[
        {
          name: 'Manage Education in Numbers',
          link: educationInNumbersListRoute.fullPath,
        },
        { name: 'Create page' },
      ]}
    >
      <EducationInNumbersPageForm
        cancelButton={
          <Link unvisited to={educationInNumbersListRoute.fullPath}>
            Cancel
          </Link>
        }
        onSubmit={async values => {
          const newPage =
            await educationInNumbersService.createEducationInNumbersPage(
              values,
            );

          navigate(
            generatePath(educationInNumbersSummaryRoute.fullPath, {
              educationInNumbersPageId: newPage.id,
            }),
          );
        }}
      />
    </Page>
  );
};

export default EducationInNumbersCreatePage;
