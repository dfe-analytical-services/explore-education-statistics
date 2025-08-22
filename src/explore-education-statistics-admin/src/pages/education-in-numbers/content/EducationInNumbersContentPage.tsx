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
import {
  EducationInNumbersPageContentProvider,
  useEducationInNumbersPageContentState,
} from './context/EducationInNumbersPageContentContext';
import EducationInNumbersContent from './components/EducationInNumbersContent';

export const EducationInNumbersContentPageInternal = () => {
  const { pageContent, pageVersion } = useEducationInNumbersPageContentState();

  const canUpdateContent = !pageVersion.published;

  return (
    <EditingContextProvider editingMode={canUpdateContent ? 'edit' : 'preview'}>
      {canUpdateContent && <EditablePageModeToggle />}

      <div className="govuk-width-container">
        <h2 aria-hidden className="govuk-heading-lg" data-testid="page-title">
          {pageVersion.title}
        </h2>

        <EducationInNumbersContent
          pageContent={pageContent}
          pageVersion={pageVersion}
        />
      </div>
    </EditingContextProvider>
  );
};

const EducationInNumbersContentPage = ({
  match,
}: RouteComponentProps<EducationInNumbersRouteParams>) => {
  const { educationInNumbersPageId } = match.params;

  const { data: pageVersion, isLoading: isPageVersionFetching } = useQuery(
    educationInNumbersQueries.getEducationInNumbersPage(
      educationInNumbersPageId,
    ),
  );

  const { data: pageContent, isLoading: isPageContentFetching } = useQuery(
    educationInNumbersContentQueries.get(educationInNumbersPageId),
  );

  console.log('pageVersion:', pageVersion);
  console.log('pageContent:', pageContent);

  const isLoading = isPageVersionFetching || isPageContentFetching; // @MarkFix rename

  return (
    <LoadingSpinner loading={isLoading}>
      {pageContent && pageVersion ? (
        <EducationInNumbersPageContentProvider
          value={{
            pageContent,
            pageVersion,
          }}
        >
          <EducationInNumbersContentPageInternal />
        </EducationInNumbersPageContentProvider>
      ) : (
        <WarningMessage>Could not load page content</WarningMessage>
      )}
    </LoadingSpinner>
  );
};

export default EducationInNumbersContentPage;
