import PageTitle from '@admin/components/PageTitle';
import PreReleaseTableToolFinalStep from '@admin/pages/release/pre-release/components/PreReleaseTableToolFinalStep';
import { ReleaseRouteParams } from '@admin/routes/releaseRoutes';
import publicationService from '@admin/services/publicationService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import TableToolWizard, {
  InitialTableToolState,
} from '@common/modules/table-tool/components/TableToolWizard';
import WizardStep from '@common/modules/table-tool/components/WizardStep';
import WizardStepHeading from '@common/modules/table-tool/components/WizardStepHeading';
import tableBuilderService from '@common/services/tableBuilderService';
import React from 'react';
import { RouteComponentProps } from 'react-router';

const PreReleaseTableToolPage = ({
  match,
}: RouteComponentProps<ReleaseRouteParams>) => {
  const { publicationId, releaseId } = match.params;

  const { value: publication } = useAsyncHandledRetry(
    () => publicationService.getPublication(publicationId),
    [publicationId],
  );

  const { value: tableToolState, isLoading } = useAsyncHandledRetry<
    InitialTableToolState | undefined
  >(async () => {
    const { subjects } = await tableBuilderService.getReleaseMeta(releaseId);

    return {
      initialStep: 1,
      subjects,
      highlights: [],
      query: {
        publicationId,
        releaseId,
        subjectId: '',
        indicators: [],
        filters: [],
        locations: {},
      },
    };
  }, [releaseId]);

  return (
    <LoadingSpinner loading={isLoading}>
      {tableToolState && (
        <>
          <PageTitle
            title="Create your own tables online"
            caption="Table Tool"
          />

          <p>
            Choose the data and area of interest you want to explore and then
            use filters to create your table.
          </p>

          <p>
            Once you've created your table, you can download the data it
            contains for your own offline analysis.
          </p>

          <TableToolWizard
            themeMeta={[]}
            initialState={tableToolState}
            finalStep={({ response, query }) => (
              <WizardStep>
                {wizardStepProps => (
                  <>
                    <WizardStepHeading {...wizardStepProps}>
                      Explore data
                    </WizardStepHeading>

                    {response && query && (
                      <PreReleaseTableToolFinalStep
                        publication={publication}
                        releaseId={releaseId}
                        table={response.table}
                        tableHeaders={response.tableHeaders}
                      />
                    )}
                  </>
                )}
              </WizardStep>
            )}
          />
        </>
      )}
    </LoadingSpinner>
  );
};

export default PreReleaseTableToolPage;
