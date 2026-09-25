import Link from '@admin/components/Link';
import {
  releaseApiDataSetDetailsRoute,
  ReleaseDataSetRouteParams,
} from '@admin/routes/releaseRoutes';
import ButtonText from '@common/components/ButtonText';
import Modal from '@common/components/Modal';
import VisuallyHidden from '@common/components/VisuallyHidden';
import React from 'react';
import { generatePath } from 'react-router';

interface Props {
  canManagePublicApiDataSets: boolean;
  dataFileTitle: string;
  publicApiDataSetId: string;
  publicationId: string;
  releaseVersionId: string;
}

/**
 * Shown in place of the delete modal when the data file has an API data set
 * linked to it, which has to be removed before the file can be deleted.
 */
export default function DataFileDeleteBlockedModal({
  canManagePublicApiDataSets,
  dataFileTitle,
  publicApiDataSetId,
  publicationId,
  releaseVersionId,
}: Props) {
  return (
    <Modal
      showClose
      title="Cannot delete files"
      triggerButton={
        <ButtonText variant="warning">
          Delete files
          <VisuallyHidden>{` for ${dataFileTitle}`}</VisuallyHidden>
        </ButtonText>
      }
    >
      {canManagePublicApiDataSets ? (
        <p>
          This data file has an API data set linked to it. Please remove the API
          data set before deleting.
        </p>
      ) : (
        <p>
          This data file has an API data set linked to it. It will need removing
          before the data file can be deleted. You do not have the required role
          to resolve the issue, but you can contact the EES team for support at{' '}
          <a href="mailto:explore.statistics@education.gov.uk">
            explore.statistics@education.gov.uk
          </a>
          .
        </p>
      )}
      <p>
        <Link
          to={generatePath<ReleaseDataSetRouteParams>(
            releaseApiDataSetDetailsRoute.path,
            {
              publicationId,
              releaseVersionId,
              dataSetId: publicApiDataSetId,
            },
          )}
        >
          Go to API data set
        </Link>
      </p>
    </Modal>
  );
}
