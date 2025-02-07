import Link from '@admin/components/Link';
import { DataSetFinalisingStatus } from '@admin/pages/release/data/ReleaseApiDataSetDetailsPage';
import {
  releaseApiDataSetChangelogRoute,
  ReleaseDataSetChangelogRouteParams,
} from '@admin/routes/releaseRoutes';
import { DataSetDraftVersionStatus } from '@admin/services/apiDataSetService';
import NotificationBanner from '@common/components/NotificationBanner';
import Button from '@common/components/Button';
import React from 'react';
import { generatePath } from 'react-router-dom';

interface Props {
  dataSetId: string;
  dataSetVersionId: string;
  draftVersionStatus?: DataSetDraftVersionStatus;
  finalisingStatus?: DataSetFinalisingStatus;
  releaseVersionId: string;
  publicationId: string;
  onFinalise: () => void;
}

export default function ApiDataSetFinaliseBanner({
  dataSetId,
  dataSetVersionId,
  draftVersionStatus,
  finalisingStatus,
  releaseVersionId,
  publicationId,
  onFinalise,
}: Props) {
  if (finalisingStatus === 'finalising') {
    return (
      <NotificationBanner
        fullWidthContent
        heading="Finalising draft API data set version"
        title="Finalising"
      >
        <p>This process can take a few minutes.</p>
      </NotificationBanner>
    );
  }

  if (draftVersionStatus === 'Mapping') {
    return (
      <NotificationBanner
        fullWidthContent
        heading="Draft API data set version is ready to be finalised"
        title="Action required"
      >
        <p>
          The mapping changes need to be finalised before the draft API data set
          version can be published. After finalising, you will also be able to
          view the changelog, add guidance notes and preview this API data set
          version.
        </p>
        <p className="govuk-!-margin-bottom-6">
          You will not be able to make further changes after finalising. If you
          need to make any changes to the mappings you will have to remove this
          draft API data set version and create a new version.
        </p>
        <Button onClick={onFinalise}>Finalise this data set version</Button>
      </NotificationBanner>
    );
  }

  if (draftVersionStatus === 'Draft') {
    return (
      <NotificationBanner
        fullWidthContent
        heading="Draft API data set version is ready to be published"
        role={finalisingStatus === 'finalised' ? 'alert' : undefined}
        title="Mappings finalised"
        variant={finalisingStatus === 'finalised' ? 'success' : undefined}
      >
        <p>
          If you need to make any changes to the mappings you will have to
          remove this draft API data set version and create a new version. You
          can remove this draft API data set version up until the release
          publication.
        </p>
        <p>
          These changes will not be made in the public API until the next
          release has been published.
        </p>

        <p>
          <Link
            to={generatePath<ReleaseDataSetChangelogRouteParams>(
              releaseApiDataSetChangelogRoute.path,
              {
                publicationId,
                releaseVersionId,
                dataSetId,
                dataSetVersionId,
              },
            )}
          >
            View changelog and guidance notes
          </Link>
        </p>
      </NotificationBanner>
    );
  }

  if (finalisingStatus === 'finalised' && draftVersionStatus === 'Failed') {
    return (
      <NotificationBanner
        fullWidthContent
        heading="Data set version finalisation failed"
        role="alert"
        title="There is a problem"
        variant="error"
      />
    );
  }
  return null;
}
