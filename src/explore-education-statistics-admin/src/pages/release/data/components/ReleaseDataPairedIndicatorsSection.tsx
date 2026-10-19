import PairedIndicatorsForm from '@admin/pages/release/data/components/PairedIndicatorsForm';
import styles from '@admin/pages/release/data/components/ReleaseDataPairedIndicatorsSection.module.scss';
import adminTableBuilderService, {
  PairedIndicators,
} from '@admin/services/tableBuilderService';
import Button from '@common/components/Button';
import InsetText from '@common/components/InsetText';
import LoadingSpinner from '@common/components/LoadingSpinner';
import VisuallyHidden from '@common/components/VisuallyHidden';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import tableBuilderService, {
  Subject,
} from '@common/services/tableBuilderService';
import React, { useState } from 'react';

interface Props {
  releaseVersionId: string;
  canUpdateRelease: boolean;
}

export default function ReleaseDataPairedIndicatorsSection({
  releaseVersionId,
  canUpdateRelease,
}: Props) {
  const { value: subjects, isLoading: isLoadingSubjects } =
    useAsyncHandledRetry(
      () => tableBuilderService.listReleaseSubjects(releaseVersionId),
      [releaseVersionId],
    );
  const [pairingSubject, setPairingSubject] = useState<Subject>();

  const handleSave = async (subjectId: string, request: PairedIndicators) => {
    await adminTableBuilderService.updatePairedIndicators(
      releaseVersionId,
      subjectId,
      request,
    );
    setPairingSubject(undefined);
  };

  return (
    <>
      <h2>Paired indicators</h2>
      <InsetText>
        <h3>Before you start</h3>
        <p>
          Pair a child indicator with a parent indicator so that the child
          indicator is always selected with the parent indicator in the table
          tool. The child indicator cannot be selected on its own.
        </p>
        <p>
          An indicator can only have one child indicator or one parent
          indicator, and cannot be both a parent and a child indicator.
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

              {subjects?.length !== 0 && !pairingSubject && (
                <div className="table-container">
                  <table className={styles.table}>
                    <caption className="govuk-visually-hidden">
                      Table showing data files in this release and buttons for
                      pairing data file indicators.
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
                      {subjects?.map(subject => (
                        <tr key={subject.id}>
                          <td className="govuk-!-width-two-thirds">
                            {subject.name}
                          </td>
                          <td className={styles.tableCellButtons}>
                            <Button
                              className="govuk-!-margin-bottom-0"
                              onClick={() => setPairingSubject(subject)}
                            >
                              Pair indicators
                              <VisuallyHidden>{` for ${subject.name}`}</VisuallyHidden>
                            </Button>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </>
          </LoadingSpinner>
          <div aria-live="polite">
            {pairingSubject && (
              <PairedIndicatorsForm
                releaseVersionId={releaseVersionId}
                subject={pairingSubject}
                onCancel={() => setPairingSubject(undefined)}
                onSave={handleSave}
              />
            )}
          </div>
        </>
      )}
    </>
  );
}
