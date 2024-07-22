import Link from '@admin/components/Link';
import ReleasePublishingStages from '@admin/pages/release/components/ReleasePublishingStages';
import ReleasePublishingStatusTag from '@admin/pages/release/components/ReleasePublishingStatusTag';
import useReleasePublishingStatus from '@admin/pages/release/hooks/useReleasePublishingStatus';
import {
  ReleaseRouteParams,
  releaseSummaryRoute,
} from '@admin/routes/releaseRoutes';
import { ReleaseVersionSummary } from '@admin/services/releaseService';
import FormattedDate from '@common/components/FormattedDate';
import VisuallyHidden from '@common/components/VisuallyHidden';
import React from 'react';
import { generatePath } from 'react-router';

interface Props {
  publicationId: string;
  release: ReleaseVersionSummary;
}

const ScheduledReleaseRow = ({ publicationId, release }: Props) => {
  const { currentStatus, currentStatusDetail } = useReleasePublishingStatus({
    releaseId: release.id,
  });

  return (
    <tr>
      <td>{release.title}</td>
      <td>
        <ReleasePublishingStatusTag
          currentStatus={currentStatus}
          color={currentStatusDetail.color}
        />
      </td>
      <td>
        <ReleasePublishingStages
          checklistStyle
          currentStatus={currentStatus}
          includeScheduled
        />
      </td>
      <td>
        {release.publishScheduled && (
          <FormattedDate>{release.publishScheduled}</FormattedDate>
        )}
      </td>
      <td>
        <Link
          to={generatePath<ReleaseRouteParams>(releaseSummaryRoute.path, {
            publicationId,
            releaseVersionId: release.id,
          })}
        >
          {release.permissions?.canUpdateRelease ? 'Edit' : 'View'}
          <VisuallyHidden> {release.title}</VisuallyHidden>
        </Link>
      </td>
    </tr>
  );
};

export default ScheduledReleaseRow;
