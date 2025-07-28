import React from 'react';
import { generatePath, RouteComponentProps } from 'react-router';
import { useEducationInNumbersPageContext } from '@admin/pages/education-in-numbers/contexts/EducationInNumbersContext';
import EducationInNumbersSummaryForm, {
  EducationInNumbersSummaryFormValues,
} from '@admin/pages/education-in-numbers/components/EducationInNumbersSummaryForm';
import {
  EducationInNumbersRouteParams,
  educationInNumbersSummaryRoute,
} from '@admin/routes/educationInNumbersRoutes';
import educationInNumbersService from '@admin/services/educationInNumbersService';
import ButtonText from '@common/components/ButtonText';

const EducationInNumbersSummaryEditPage = ({
  history,
}: RouteComponentProps<EducationInNumbersRouteParams>) => {
  const {
    educationInNumbersPageId,
    educationInNumbersPage,
    onEducationInNumbersPageChange,
  } = useEducationInNumbersPageContext();

  const handleSubmit = async (values: EducationInNumbersSummaryFormValues) => {
    if (!educationInNumbersPage) {
      throw new Error('Could not update missing education in numbers page');
    }

    const nextPage =
      await educationInNumbersService.updateEducationInNumbersPage(
        educationInNumbersPageId,
        values,
      );

    onEducationInNumbersPageChange(nextPage);

    history.push(
      generatePath<EducationInNumbersRouteParams>(
        educationInNumbersSummaryRoute.path,
        {
          educationInNumbersPageId,
        },
      ),
    );
  };

  return (
    <>
      <h2>Edit page summary</h2>
      <EducationInNumbersSummaryForm
        cancelButton={
          <ButtonText onClick={() => history.goBack()}>Cancel</ButtonText>
        }
        initialValues={educationInNumbersPage}
        isEditForm
        onSubmit={handleSubmit}
      />
    </>
  );
};

export default EducationInNumbersSummaryEditPage;
