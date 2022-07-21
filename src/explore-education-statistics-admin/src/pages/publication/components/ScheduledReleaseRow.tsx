import Link from '@admin/components/Link';
import {
  CurrentStatusBlock,
  ReleaseStages,
} from '@admin/components/ReleaseServiceStatus';
import useReleaseServiceStatus from '@admin/hooks/useReleaseServiceStatus';
import {
  ReleaseRouteParams,
  releaseSummaryRoute,
} from '@admin/routes/releaseRoutes';
import { MyRelease } from '@admin/services/releaseService';
import FormattedDate from '@common/components/FormattedDate';
import VisuallyHidden from '@common/components/VisuallyHidden';
import React from 'react';
import { generatePath } from 'react-router';

interface Props {
  release: MyRelease;
}

const ScheduledReleaseRow = ({ release }: Props) => {
  const { currentStatus, currentStatusDetail } = useReleaseServiceStatus({
    releaseId: release.id,
  });

  return (
    <tr>
      <td>{release.title}</td>
      <td>
        <CurrentStatusBlock
          currentStatus={currentStatus}
          color={currentStatusDetail.color}
        />
      </td>
      <td>
        <ReleaseStages
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
            publicationId: release.publicationId,
            releaseId: release.id,
          })}
        >
          {release.permissions.canUpdateRelease ? 'Edit' : 'View'}
          <VisuallyHidden> {release.title}</VisuallyHidden>
        </Link>
      </td>
    </tr>
  );
};

export default ScheduledReleaseRow;
