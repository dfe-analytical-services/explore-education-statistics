import React from 'react';
import Link from '@admin/components/Link';
import { getReleaseApprovalStatusLabel } from '@admin/pages/release/utils/releaseSummaryUtil';
import releaseService, {
  ReleaseSummaryWithPermissions,
} from '@admin/services/releaseService';
import {
  ReleaseRouteParams,
  releaseSummaryRoute,
} from '@admin/routes/releaseRoutes';
import ButtonText from '@common/components/ButtonText';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Tag from '@common/components/Tag';
import VisuallyHidden from '@common/components/VisuallyHidden';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import { generatePath } from 'react-router';

interface Props {
  publicationId: string;
  release: ReleaseSummaryWithPermissions;
  onDelete: () => void;
}

const DraftReleaseRow = ({ publicationId, release, onDelete }: Props) => {
  const { value: checklist, isLoading: isLoadingChecklist } =
    useAsyncHandledRetry(async () => {
      return releaseService.getReleaseChecklist(release.id);
    });

  return (
    <tr>
      <td>{release.title}</td>
      <td>
        <Tag>
          {`${getReleaseApprovalStatusLabel(release.approvalStatus)}${
            release.amendment ? ' Amendment' : ''
          }`}
        </Tag>
      </td>
      <td>
        <LoadingSpinner inline loading={isLoadingChecklist} size="sm">
          {checklist?.errors.length}
        </LoadingSpinner>
      </td>
      <td>
        <LoadingSpinner inline loading={isLoadingChecklist} size="sm">
          {checklist?.warnings.length}
        </LoadingSpinner>
      </td>
      <td>
        <Link
          className="govuk-!-margin-right-4 dfe-inline-block"
          to={generatePath<ReleaseRouteParams>(releaseSummaryRoute.path, {
            publicationId,
            releaseId: release.id,
          })}
        >
          {release.permissions?.canUpdateRelease ? 'Edit' : 'View'}
          <VisuallyHidden> {release.title}</VisuallyHidden>
        </Link>

        {release.amendment && release.previousVersionId && (
          <Link
            className="govuk-!-margin-right-4 dfe-inline-block"
            to={generatePath<ReleaseRouteParams>(releaseSummaryRoute.path, {
              publicationId,
              releaseId: release.previousVersionId,
            })}
          >
            View existing version
            <VisuallyHidden> for {release.title}</VisuallyHidden>
          </Link>
        )}

        {release.permissions?.canDeleteRelease && release.amendment && (
          <ButtonText variant="warning" onClick={onDelete}>
            Cancel amendment
            <VisuallyHidden> for {release.title}</VisuallyHidden>
          </ButtonText>
        )}
      </td>
    </tr>
  );
};

export default DraftReleaseRow;
