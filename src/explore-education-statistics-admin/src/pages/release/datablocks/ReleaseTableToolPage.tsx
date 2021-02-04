import Link from '@admin/components/Link';
import {
  releaseDataBlocksRoute,
  ReleaseRouteParams,
} from '@admin/routes/releaseRoutes';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import TableHeadersForm from '@common/modules/table-tool/components/TableHeadersForm';
import TableToolWizard, {
  InitialTableToolState,
} from '@common/modules/table-tool/components/TableToolWizard';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import WizardStep from '@common/modules/table-tool/components/WizardStep';
import WizardStepHeading from '@common/modules/table-tool/components/WizardStepHeading';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import { TableHeadersConfig } from '@common/modules/table-tool/types/tableHeaders';
import tableBuilderService from '@common/services/tableBuilderService';
import React, { useEffect, useRef, useState } from 'react';
import { generatePath, RouteComponentProps } from 'react-router-dom';

interface ReleaseTableToolFinalStepProps {
  table: FullTable;
  tableHeaders: TableHeadersConfig;
}

const ReleaseTableToolFinalStep = ({
  table,
  tableHeaders,
}: ReleaseTableToolFinalStepProps) => {
  const dataTableRef = useRef<HTMLElement>(null);
  const [currentTableHeaders, setCurrentTableHeaders] = useState<
    TableHeadersConfig
  >();

  useEffect(() => {
    setCurrentTableHeaders(tableHeaders);
  }, [tableHeaders]);

  return (
    <div className="govuk-!-margin-bottom-4">
      <TableHeadersForm
        initialValues={currentTableHeaders}
        onSubmit={tableHeaderConfig => {
          setCurrentTableHeaders(tableHeaderConfig);

          if (dataTableRef.current) {
            dataTableRef.current.scrollIntoView({
              behavior: 'smooth',
              block: 'start',
            });
          }
        }}
      />
      {table && currentTableHeaders && (
        <TimePeriodDataTable
          ref={dataTableRef}
          fullTable={table}
          tableHeadersConfig={currentTableHeaders}
        />
      )}
    </div>
  );
};

const ReleaseTableToolPage = ({
  match,
}: RouteComponentProps<ReleaseRouteParams>) => {
  const { publicationId, releaseId } = match.params;

  const { value: initialState, isLoading } = useAsyncHandledRetry<
    InitialTableToolState | undefined
  >(async () => {
    const { subjects } = await tableBuilderService.getReleaseMeta(releaseId);

    return {
      initialStep: 1,
      subjects,
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
    <>
      <Link
        back
        className="govuk-!-margin-bottom-6"
        to={generatePath<ReleaseRouteParams>(releaseDataBlocksRoute.path, {
          publicationId,
          releaseId,
        })}
      >
        Back
      </Link>

      <LoadingSpinner loading={isLoading}>
        {initialState && (
          <>
            <h2>Table tool</h2>

            <TableToolWizard
              themeMeta={[]}
              initialState={initialState}
              finalStep={({ response, query }) => (
                <WizardStep>
                  {wizardStepProps => (
                    <>
                      <WizardStepHeading {...wizardStepProps}>
                        Explore data
                      </WizardStepHeading>

                      {query && response && (
                        <ReleaseTableToolFinalStep
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
    </>
  );
};

export default ReleaseTableToolPage;
