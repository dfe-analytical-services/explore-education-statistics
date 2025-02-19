import ButtonLink from '@admin/components/ButtonLink';
import {
  releaseApiDataSetDetailsRoute,
  ReleaseDataSetRouteParams,
} from '@admin/routes/releaseRoutes';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import React from 'react';
import { generatePath } from 'react-router-dom';

interface Props {
  dataSetId: string;
  publicationId: string;
  releaseVersionId: string;
  onClose: () => void;
}
export default function ApiDataSetCreateProcessing({
  dataSetId,
  publicationId,
  releaseVersionId,
  onClose,
}: Props) {
  return (
    <>
      <p>Your new API data set is being processed.</p>
      <ButtonGroup>
        <ButtonLink
          to={generatePath<ReleaseDataSetRouteParams>(
            releaseApiDataSetDetailsRoute.path,
            {
              publicationId,
              releaseVersionId,
              dataSetId,
            },
          )}
        >
          View API data set details
        </ButtonLink>
        <ButtonText onClick={onClose}>Close</ButtonText>
      </ButtonGroup>
    </>
  );
}
