import LoadingSpinner from '@common/components/LoadingSpinner';
import WarningMessage from '@common/components/WarningMessage';
import React from 'react';
import { RouteComponentProps } from 'react-router';
import { useQuery } from '@tanstack/react-query';
import { EducationInNumbersRouteParams } from '@admin/routes/educationInNumbersRoutes';
import educationInNumbersQueries from '@admin/queries/educationInNumbersQueries';
import educationInNumbersContentQueries from '@admin/queries/educationInNumbersContentQueries';
import { EditingContextProvider } from '@admin/contexts/EditingContext';
import EditablePageModeToggle from '@admin/components/editable/EditablePageModeToggle';
import { EducationInNumbersPageContentProvider } from '@admin/pages/education-in-numbers/content/context/EducationInNumbersPageContentContext';
import EducationInNumbersContent from '@admin/pages/education-in-numbers/content/components/EducationInNumbersContent';

const EducationInNumbersContentPage = ({
  match,
}: RouteComponentProps<EducationInNumbersRouteParams>) => {
  const { educationInNumbersPageId } = match.params;

  const { data: pageVersion, isLoading: isPageVersionLoading } = useQuery(
    educationInNumbersQueries.getEducationInNumbersPage(
      educationInNumbersPageId,
    ),
  );

  const { data: pageContent, isLoading: isPageContentLoading } = useQuery(
    educationInNumbersContentQueries.get(educationInNumbersPageId),
  );

  const isLoading = isPageVersionLoading || isPageContentLoading;

  const canUpdateContent = !pageVersion?.published;

  return (
    <LoadingSpinner loading={isLoading}>
      {pageContent && pageVersion ? (
        <EducationInNumbersPageContentProvider
          value={{
            pageContent,
            pageVersion,
          }}
        >
          <EditingContextProvider
            editingMode={canUpdateContent ? 'edit' : 'preview'}
          >
            {canUpdateContent && <EditablePageModeToggle />}

            <div className="govuk-width-container">
              <h2
                aria-hidden
                className="govuk-heading-lg"
                data-testid="page-title"
              >
                {pageVersion.title}
              </h2>

              <EducationInNumbersContent />
            </div>
          </EditingContextProvider>
        </EducationInNumbersPageContentProvider>
      ) : (
        <WarningMessage>Could not load page content</WarningMessage>
      )}
    </LoadingSpinner>
  );
};

export default EducationInNumbersContentPage;
