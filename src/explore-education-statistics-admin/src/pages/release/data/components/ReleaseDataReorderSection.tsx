import PairedIndicatorsForm from '@admin/pages/release/data/components/PairedIndicatorsForm';
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
import adminTableBuilderService, {
  PairedIndicators,
} from '@admin/services/tableBuilderService';

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

  const handleSavePairedIndicators = async (
    subjectId: string,
    pairedIndicators: PairedIndicators,
  ) => {
    await adminTableBuilderService.updatePairedIndicators(
      releaseVersionId,
      subjectId,
      pairedIndicators,
    );
    setReorderingIndicators(undefined);
  };

  return (
    <>
      <h2>Manage filters and indicators</h2>
      <InsetText>
        <h3>Before you start</h3>
        <p>
          Reorder the groups and options for filters and indicators on this page
          by using drag and drop or by pressing the up and down arrows next to
          each item.
        </p>
        <p>
          You can also pair indicators so that a child indicator is always
          selected with its parent indicator.
        </p>
        <p>
          These changes will be reflected in the table tool on the public
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
                  <div className="table-container">
                    <table className={styles.table}>
                      <caption className="govuk-visually-hidden">
                        Table showing data files in this release and buttons for
                        managing data file filters and indicators.
                      </caption>
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
                                <ButtonGroup
                                  className={styles.tableCellButtons}
                                >
                                  <Button
                                    onClick={() =>
                                      setReorderingFilters(subject)
                                    }
                                  >
                                    Manage filters
                                    <VisuallyHidden>
                                      {` for ${subject.name}`}
                                    </VisuallyHidden>
                                  </Button>
                                  <Button
                                    onClick={() =>
                                      setReorderingIndicators(subject)
                                    }
                                  >
                                    Manage indicators
                                    <VisuallyHidden>
                                      {` for ${subject.name}`}
                                    </VisuallyHidden>
                                  </Button>
                                </ButtonGroup>
                              </td>
                            </tr>
                          );
                        })}
                      </tbody>
                    </table>
                  </div>
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
              <>
                <ReorderIndicatorsList
                  releaseVersionId={releaseVersionId}
                  subject={reorderingIndicators}
                  onCancel={() => setReorderingIndicators(undefined)}
                  onSave={handleSaveIndicators}
                />
                <div className="govuk-!-margin-top-9">
                  <PairedIndicatorsForm
                    releaseVersionId={releaseVersionId}
                    subject={reorderingIndicators}
                    onCancel={() => setReorderingIndicators(undefined)}
                    onSave={handleSavePairedIndicators}
                  />
                </div>
              </>
            )}
          </div>
        </>
      )}
    </>
  );
};

export default ReleaseDataReorderSection;
