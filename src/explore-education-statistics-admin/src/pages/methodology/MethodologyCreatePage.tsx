import Page from '@admin/components/Page';
import MethodologySummaryForm from '@admin/pages/methodology/components/MethodologySummaryForm';
import methodologyService from '@admin/services/methodologyService';
import React from 'react';
import { RouteComponentProps } from 'react-router';

// TODO EES-2153 Remove this page
const MethodologyCreatePage = ({ history }: RouteComponentProps) => {
  return (
    <Page
      wide
      title="Create new methodology"
      breadcrumbs={[
        {
          name: 'Create new methodology',
        },
      ]}
    >
      <MethodologySummaryForm
        id="createMethodologyForm"
        submitText="Create methodology"
        onCancel={history.goBack}
        onSubmit={async values => {
          const createdMethodology = await methodologyService.createMethodology(
            {
              title: values.title,
            },
          );

          history.push(`/methodologies/${createdMethodology.id}/summary`);
        }}
      />
    </Page>
  );
};

export default MethodologyCreatePage;
