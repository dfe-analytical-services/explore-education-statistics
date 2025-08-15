import {
  releaseApiDataSetDetailsRoute,
  releaseDataFileReplaceRoute,
  ReleaseDataFileReplaceRouteParams,
  ReleaseDataSetRouteParams,
} from '@admin/routes/releaseRoutes';
import ButtonText from '@common/components/ButtonText';
import Modal from '@common/components/Modal';
import { generatePath, Link } from 'react-router-dom';
import { useAuthContext } from '@admin/contexts/AuthContext';
import React from 'react';

function CannotReplaceDataModal({ children }: { children: React.ReactNode }) {
  return (
    <Modal
      showClose
      title="Cannot replace data"
      triggerButton={<ButtonText>Replace data</ButtonText>}
    >
      {children}
    </Modal>
  );
}

export default function ReplaceDataButtonOrModal({
  allowReplacementOfDataFile,
  releaseIsNotAmendmentAndIsLinkedToApi,
  dataFileId,
  publicApiDataSetId,
  publicationId,
  releaseVersionId,
}: {
  allowReplacementOfDataFile: boolean;
  releaseIsNotAmendmentAndIsLinkedToApi: boolean;
  dataFileId: string;
  publicApiDataSetId: string | undefined;
  publicationId: string;
  releaseVersionId: string;
}) {
  const { user } = useAuthContext();
  const missingPermissionsForPatchReplacement =
    !user?.permissions.isBauUser && !!publicApiDataSetId;
  if (!allowReplacementOfDataFile) {
    return (
      <CannotReplaceDataModal>
        <p>
          This data file has an API data set linked to it. Please remove the API
          data set before replacing the data.
        </p>
        <p>
          <Link
            to={
              publicApiDataSetId
                ? generatePath<ReleaseDataSetRouteParams>(
                    releaseApiDataSetDetailsRoute.path,
                    {
                      publicationId,
                      releaseVersionId,
                      dataSetId: publicApiDataSetId,
                    },
                  )
                : {}
            }
          >
            Go to API data set
          </Link>
        </p>
      </CannotReplaceDataModal>
    );
  }

  if (releaseIsNotAmendmentAndIsLinkedToApi) {
    return (
      <CannotReplaceDataModal>
        <p>
          This data replacement can not be completed as it is targeting an
          existing draft API data set.
        </p>
        <p>
          Please contact the explore statistics team at{' '}
          <a href="mailto:explore.statistics@education.gov.uk">
            explore.statistics@education.gov.uk
          </a>{' '}
          for support on completing this replacement.
        </p>
      </CannotReplaceDataModal>
    );
  }

  if (missingPermissionsForPatchReplacement) {
    return (
      <CannotReplaceDataModal>
        <p>
          You do not have permission to replace this data file. This is because
          it is linked to an API data set version which can only be modified by
          BAU users.
        </p>
        <p>
          Please contact the explore statistics team at{' '}
          <a href="mailto:explore.statistics@education.gov.uk">
            explore.statistics@education.gov.uk
          </a>{' '}
          for support on completing this replacement.
        </p>
      </CannotReplaceDataModal>
    );
  }

  return (
    <Link
      to={generatePath<ReleaseDataFileReplaceRouteParams>(
        releaseDataFileReplaceRoute.path,
        {
          publicationId,
          releaseVersionId,
          fileId: dataFileId,
        },
      )}
    >
      Replace data
    </Link>
  );
}
