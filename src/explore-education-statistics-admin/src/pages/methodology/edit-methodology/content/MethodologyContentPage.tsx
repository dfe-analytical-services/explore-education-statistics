import BrowserWarning from '@admin/components/BrowserWarning';
import EditablePageModeToggle from '@admin/components/editable/EditablePageModeToggle';
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
import {
  MethodologyContextState,
  MethodologyContentProvider,
  useMethodologyContentState,
} from '@admin/pages/methodology/edit-methodology/content/context/MethodologyContentContext';
import React from 'react';
import { RouteComponentProps } from 'react-router';

const MethodologyContentPageInternal = () => {
  const { methodology, canUpdateMethodology } = useMethodologyContentState();

  const canUpdateContent =
    canUpdateMethodology && methodology.status === 'Draft';

  return (
    <EditingContextProvider
      initialEditingMode={canUpdateContent ? 'edit' : 'preview'}
    >
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

              <h2
                aria-hidden
                className="govuk-heading-lg"
                data-testid="page-title"
              >
                {methodology.title}
              </h2>

              <div className="govuk-grid-row">
                <div className="govuk-grid-column-two-thirds">
                  <dl className="dfe-meta-content govuk-!-margin-top-0">
                    <div>
                      <dt className="govuk-caption-m">Published: </dt>
                      <dd data-testid="published-date">
                        <strong>
                          {methodology.published ? (
                            <FormattedDate>
                              {methodology.published}
                            </FormattedDate>
                          ) : (
                            'Not yet published'
                          )}
                        </strong>
                      </dd>
                    </div>
                  </dl>
                  {editingMode !== 'edit' && (
                    <>
                      <PrintThisPage />
                      <PageSearchForm inputLabel="Search in this methodology page." />
                    </>
                  )}
                </div>
              </div>
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

const MethodologyContentPage = ({
  match,
}: RouteComponentProps<MethodologyRouteParams>) => {
  const { methodologyId } = match.params;

  const { value, isLoading } = useAsyncHandledRetry<
    MethodologyContextState
  >(async () => {
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
