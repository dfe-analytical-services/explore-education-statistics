import ReorderFiltersList, {
  UpdateFiltersRequest,
} from '@admin/pages/release/data/components/ReorderFiltersList';
import ReorderIndicatorsList, {
  UpdateIndicatorsRequest,
} from '@admin/pages/release/data/components/ReorderIndicatorsList';
import styles from '@admin/pages/release/data/components/ReleaseDataReorderSection.module.scss';
import VisuallyHidden from '@common/components/VisuallyHidden';
import tableBuilderService, {
  Subject,
} from '@common/services/tableBuilderService';
import Button from '@common/components/Button';
import InsetText from '@common/components/InsetText';
import LoadingSpinner from '@common/components/LoadingSpinner';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';

import React, { useState } from 'react';

interface Props {
  releaseId: string;
  canUpdateRelease: boolean;
}

const ReleaseDataReorderSection = ({ releaseId, canUpdateRelease }: Props) => {
  const {
    value: subjects,
    isLoading: isLoadingSubjects,
  } = useAsyncHandledRetry(
    () => tableBuilderService.listReleaseSubjects(releaseId),
    [releaseId],
  );
  const [reorderingFilters, setReorderingFilters] = useState<
    Subject | undefined
  >();
  const [reorderingIndicators, setReorderingIndicators] = useState<
    Subject | undefined
  >();

  const handleSaveFilters = (updatedFilters: UpdateFiltersRequest) => {
    setReorderingFilters(undefined);
    // TO DO: send the update request.
  };
  const handleSaveIndicators = (updatedIndicators: UpdateIndicatorsRequest) => {
    setReorderingIndicators(undefined);
    // TO DO: send the update request.
  };

  return (
    <>
      <h2>Reorder filters and indicators</h2>
      <InsetText>
        <h3>Before you start</h3>
        <p>
          Reorder the groups and options for filters and indicators here. This
          order will be reflected in the table tool on the public website.
        </p>
      </InsetText>

      {!canUpdateRelease ? (
        <WarningMessage>
          This release has been approved, and can no longer be updated.
        </WarningMessage>
      ) : (
        <>
          <LoadingSpinner loading={isLoadingSubjects}>
            <>
              {subjects?.length === 0 && <p>No data files uploaded.</p>}

              {subjects?.length !== 0 &&
                !reorderingFilters &&
                !reorderingIndicators && (
                  <table className={styles.table}>
                    <thead>
                      <tr>
                        <th>Data file</th>
                        <th>
                          <VisuallyHidden>Actions</VisuallyHidden>
                        </th>
                      </tr>
                    </thead>
                    <tbody>
                      {subjects?.map(subject => {
                        return (
                          <tr key={subject.id}>
                            <td className="govuk-!-width-two-thirds">
                              {subject.name}
                            </td>
                            <td className="dfe-align--right">
                              <Button
                                className="govuk-!-margin-bottom-0 govuk-!-margin-right-2"
                                onClick={() => setReorderingFilters(subject)}
                              >
                                Reorder filters
                              </Button>
                              <Button
                                className="govuk-!-margin-bottom-0"
                                onClick={() => setReorderingIndicators(subject)}
                              >
                                Reorder indicators
                              </Button>
                            </td>
                          </tr>
                        );
                      })}
                    </tbody>
                  </table>
                )}
            </>
          </LoadingSpinner>
          <div aria-live="polite">
            {reorderingFilters && (
              <ReorderFiltersList
                subject={reorderingFilters}
                onCancel={() => setReorderingFilters(undefined)}
                onSave={handleSaveFilters}
              />
            )}
            {reorderingIndicators && (
              <ReorderIndicatorsList
                subject={reorderingIndicators}
                onCancel={() => setReorderingIndicators(undefined)}
                onSave={handleSaveIndicators}
              />
            )}
          </div>
        </>
      )}
    </>
  );
};

export default ReleaseDataReorderSection;
