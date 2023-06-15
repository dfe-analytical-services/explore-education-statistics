import BrowserWarning from '@admin/components/BrowserWarning';
import EditablePageModeToggle from '@admin/components/editable/EditablePageModeToggle';
import PageTitle from '@admin/components/PageTitle';
import { EditingContextProvider } from '@admin/contexts/EditingContext';
import PrintThisPage from '@admin/components/PrintThisPage';
import { MethodologyRouteParams } from '@admin/routes/methodologyRoutes';
import methodologyContentService from '@admin/services/methodologyContentService';
import permissionService from '@admin/services/permissionService';
import FormattedDate from '@common/components/FormattedDate';
import LoadingSpinner from '@common/components/LoadingSpinner';
import PageSearchForm from '@common/components/PageSearchForm';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import MethodologyAccordion from '@admin/pages/methodology/edit-methodology/content/components/MethodologyAccordion';
import MethodologyNotesSection from '@admin/pages/methodology/edit-methodology/content/components/MethodologyNotesSection';
import {
  MethodologyContextState,
  MethodologyContentProvider,
  useMethodologyContentState,
} from '@admin/pages/methodology/edit-methodology/content/context/MethodologyContentContext';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import { useParams } from 'react-router';
import React from 'react';

export const MethodologyContentPageInternal = () => {
  const { methodology, canUpdateMethodology, isPreRelease } =
    useMethodologyContentState();

  const canUpdateContent =
    !isPreRelease && canUpdateMethodology && methodology.status === 'Draft';

  return (
    <EditingContextProvider editingMode={canUpdateContent ? 'edit' : 'preview'}>
      {({ editingMode }) => (
        <>
          {canUpdateContent && <EditablePageModeToggle />}

          <div className="govuk-width-container">
            <section
              className={
                editingMode === 'edit' ? 'dfe-page-editing' : 'dfe-page-preview'
              }
            >
              {editingMode === 'edit' && (
                <BrowserWarning>
                  <ul>
                    <li>Editing text blocks</li>
                  </ul>
                </BrowserWarning>
              )}

              {isPreRelease ? (
                <PageTitle caption="Methodology" title={methodology.title} />
              ) : (
                <h2
                  aria-hidden
                  className="govuk-heading-lg"
                  data-testid="page-title"
                >
                  {methodology.title}
                </h2>
              )}

              <SummaryList>
                <SummaryListItem term="Publish date">
                  {methodology.published ? (
                    <FormattedDate>{methodology.published}</FormattedDate>
                  ) : (
                    'Not yet published'
                  )}
                </SummaryListItem>
                <MethodologyNotesSection methodology={methodology} />
              </SummaryList>
              {editingMode !== 'edit' && (
                <>
                  <PageSearchForm inputLabel="Search in this methodology page." />
                  <PrintThisPage />
                </>
              )}

              <MethodologyAccordion
                methodology={methodology}
                sectionKey="content"
                title="Content"
              />
              {editingMode !== 'edit' && methodology.annexes.length ? (
                <h2>Annexes</h2>
              ) : null}
              <MethodologyAccordion
                methodology={methodology}
                sectionKey="annexes"
                title="Annexes"
              />
            </section>
          </div>
        </>
      )}
    </EditingContextProvider>
  );
};

const MethodologyContentPage = () => {
  const { methodologyId } = useParams<MethodologyRouteParams>();

  const { value, isLoading } =
    useAsyncHandledRetry<MethodologyContextState>(async () => {
      const methodology = await methodologyContentService.getMethodologyContent(
        methodologyId,
      );
      const canUpdateMethodology = await permissionService.canUpdateMethodology(
        methodologyId,
      );

      return {
        methodology,
        canUpdateMethodology,
      };
    }, [methodologyId]);

  return (
    <LoadingSpinner loading={isLoading}>
      {value ? (
        <MethodologyContentProvider value={value}>
          <MethodologyContentPageInternal />
        </MethodologyContentProvider>
      ) : (
        <WarningMessage>Could not load methodology</WarningMessage>
      )}
    </LoadingSpinner>
  );
};

export default MethodologyContentPage;
