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
import ButtonGroup from '@common/components/ButtonGroup';
import InsetText from '@common/components/InsetText';
import LoadingSpinner from '@common/components/LoadingSpinner';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import adminTableBuilderService from '@admin/services/tableBuilderService';

import React, { useState } from 'react';

interface Props {
  releaseVersionId: string;
  canUpdateRelease: boolean;
}

const ReleaseDataReorderSection = ({
  releaseVersionId,
  canUpdateRelease,
}: Props) => {
  const { value: subjects, isLoading: isLoadingSubjects } =
    useAsyncHandledRetry(
      () => tableBuilderService.listReleaseSubjects(releaseVersionId),
      [releaseVersionId],
    );
  const [reorderingFilters, setReorderingFilters] = useState<Subject>();
  const [reorderingIndicators, setReorderingIndicators] = useState<Subject>();

  const handleSaveFilters = async (
    subjectId: string,
    updatedFilters: UpdateFiltersRequest,
  ) => {
    await adminTableBuilderService.updateFilters(
      releaseVersionId,
      subjectId,
      updatedFilters,
    );
    setReorderingFilters(undefined);
  };

  const handleSaveIndicators = async (
    subjectId: string,
    updatedIndicators: UpdateIndicatorsRequest,
  ) => {
    await adminTableBuilderService.updateIndicators(
      releaseVersionId,
      subjectId,
      updatedIndicators,
    );
    setReorderingIndicators(undefined);
  };

  return (
    <>
      <h2>Reorder filters and indicators</h2>
      <InsetText>
        <h3>Before you start</h3>
        <p>
          Reorder the groups and options for filters and indicators on this
          page. This order will be reflected in the table tool on the public
          website.
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
                            <td>
                              <ButtonGroup className={styles.tableCellButtons}>
                                <Button
                                  onClick={() => setReorderingFilters(subject)}
                                >
                                  Reorder filters
                                </Button>
                                <Button
                                  onClick={() =>
                                    setReorderingIndicators(subject)
                                  }
                                >
                                  Reorder indicators
                                </Button>
                              </ButtonGroup>
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
                releaseVersionId={releaseVersionId}
                subject={reorderingFilters}
                onCancel={() => setReorderingFilters(undefined)}
                onSave={handleSaveFilters}
              />
            )}
            {reorderingIndicators && (
              <ReorderIndicatorsList
                releaseVersionId={releaseVersionId}
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
