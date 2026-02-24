import DataFileReplacementPlan from '@admin/pages/release/data/components/DataFileReplacementPlan';
import WarningMessage from '@common/components/WarningMessage';
import {
  releaseDataFileReplacementCompleteRoute,
  ReleaseDataFileReplaceRouteParams,
} from '@admin/routes/releaseRoutes';
import releaseDataFileService, {
  DataFile,
} from '@admin/services/releaseDataFileService';
import Button from '@common/components/Button';
import ModalConfirm from '@common/components/ModalConfirm';
import React from 'react';
import { generatePath, RouteComponentProps } from 'react-router';

const PendingDataReplacementSection: React.FC<{
  dataFileId: string;
  replacementDataFileError: unknown;
  publicApiDataSetId: string | undefined;
  replacementDataFile?: DataFile;
  publicationId: string;
  releaseVersionId: string;
  history: RouteComponentProps<ReleaseDataFileReplaceRouteParams>['history'];
  fetchDataFile: () => void;
}> = ({
  dataFileId,
  replacementDataFileError,
  publicApiDataSetId,
  replacementDataFile,
  publicationId,
  releaseVersionId,
  history,
  fetchDataFile,
}) => {
  const getReplacementPlanMessage = () => {
    if (replacementDataFile?.status === 'COMPLETE') {
      return null;
    }

    if (replacementDataFileError) {
      return (
        <>
          <WarningMessage>
            There was a problem loading the data replacement information.
          </WarningMessage>

          {replacementCancelButton}
        </>
      );
    }

    if (replacementDataFile?.status === 'FAILED') {
      return (
        <>
          <WarningMessage>
            Replacement data file import failed. Please cancel and try again.
          </WarningMessage>

          {replacementCancelButton}
        </>
      );
    }

    return (
      <WarningMessage>
        The replacement data file is still being processed. Data replacement
        cannot continue until it has completed.
      </WarningMessage>
    );
  };

  const replacementCancelButton = (
    <ModalConfirm
      title={
        replacementDataFile?.publicApiDataSetId
          ? 'Cancel data replacement and remove draft API'
          : 'Cancel data replacement'
      }
      triggerButton={
        <Button variant="secondary">Cancel data replacement</Button>
      }
      onConfirm={async () => {
        if (replacementDataFile?.id) {
          await releaseDataFileService.deleteDataFiles(
            releaseVersionId,
            replacementDataFile.id,
          );
        }

        fetchDataFile();
      }}
    >
      {getCancelBodyText(publicApiDataSetId !== undefined)}
    </ModalConfirm>
  );

  return (
    <section>
      <h2>Pending data replacement</h2>

      {getReplacementPlanMessage()}

      {replacementDataFile?.status === 'COMPLETE' && (
        <DataFileReplacementPlan
          cancelButton={replacementCancelButton}
          publicationId={publicationId}
          releaseVersionId={releaseVersionId}
          fileId={dataFileId}
          replacementFileId={replacementDataFile.id}
          onReplacement={() => {
            history.push(
              generatePath<ReleaseDataFileReplaceRouteParams>(
                releaseDataFileReplacementCompleteRoute.path,
                {
                  publicationId,
                  releaseVersionId,
                  fileId: replacementDataFile.id,
                },
              ),
            );
          }}
        />
      )}
    </section>
  );
};

function getCancelBodyText(hasApiDataSetLinked: boolean) {
  return (
    <div>
      {hasApiDataSetLinked && (
        <p>
          <strong>
            Are you sure you want to cancel this data replacement and remove the
            attached draft API version?
          </strong>
        </p>
      )}
      <p>
        By cancelling this replacement you will delete the replacement file.
        This action cannot be reversed.
      </p>
      {hasApiDataSetLinked && (
        <p>
          Note that this data replacement has an associated draft API data set
          version update. The API data set update will also be cancelled and
          removed by this action.
        </p>
      )}
    </div>
  );
}

export default PendingDataReplacementSection;
