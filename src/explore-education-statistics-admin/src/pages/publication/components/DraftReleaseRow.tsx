import Link from '@admin/components/Link';
import { getReleaseApprovalStatusLabel } from '@admin/pages/release/utils/releaseSummaryUtil';
import releaseService, { Release } from '@admin/services/releaseService';
import {
  ReleaseRouteParams,
  releaseSummaryRoute,
} from '@admin/routes/releaseRoutes';
import ButtonText from '@common/components/ButtonText';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Tag from '@common/components/Tag';
import VisuallyHidden from '@common/components/VisuallyHidden';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';
import { generatePath } from 'react-router';

interface Props {
  release: Release;
  onDelete: () => void;
}

const DraftReleaseRow = ({ release, onDelete }: Props) => {
  const {
    value: checklist,
    isLoading: isLoadingChecklist,
  } = useAsyncHandledRetry(() =>
    releaseService.getReleaseChecklist(release.id),
  );

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
          to={generatePath<ReleaseRouteParams>(releaseSummaryRoute.path, {
            publicationId: release.publicationId,
            releaseId: release.id,
          })}
        >
          {release.permissions?.canUpdateRelease ? 'Edit' : 'View'}
          <VisuallyHidden> {release.title}</VisuallyHidden>
        </Link>
        {release.permissions?.canDeleteRelease && release.amendment && (
          <ButtonText className="govuk-!-margin-left-4" onClick={onDelete}>
            Cancel amendment
            <VisuallyHidden> for {release.title}</VisuallyHidden>
          </ButtonText>
        )}

        {release.amendment && (
          <Link
            className="govuk-!-margin-left-4"
            to={generatePath<ReleaseRouteParams>(releaseSummaryRoute.path, {
              publicationId: release.publicationId,
              releaseId: release.previousVersionId,
            })}
          >
            View original
            <VisuallyHidden> for {release.title}</VisuallyHidden>
          </Link>
        )}
      </td>
    </tr>
  );
};

export default DraftReleaseRow;
