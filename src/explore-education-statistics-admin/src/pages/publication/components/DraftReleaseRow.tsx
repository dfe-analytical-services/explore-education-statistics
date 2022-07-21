import Link from '@admin/components/Link';
import {
  getReleaseApprovalStatusLabel,
  getReleaseSummaryLabel,
} from '@admin/pages/release/utils/releaseSummaryUtil';
import releaseService, { MyRelease } from '@admin/services/releaseService';
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

const ErrorsAndWarnings = ({ releaseId }: { releaseId: string }) => {
  const { value: checklist, isLoading } = useAsyncHandledRetry(() =>
    releaseService.getReleaseChecklist(releaseId),
  );
  return (
    <>
      <td>
        <LoadingSpinner inline loading={isLoading} size="sm">
          {checklist?.errors.length}
        </LoadingSpinner>
      </td>
      <td>
        <LoadingSpinner inline loading={isLoading} size="sm">
          {checklist?.warnings.length}
        </LoadingSpinner>
      </td>
    </>
  );
};

interface Props {
  release: MyRelease;
  onDelete: () => void;
}

const DraftReleaseRow = ({ release, onDelete }: Props) => (
  <tr>
    <td>{release.title}</td>
    <td>
      <Tag>
        {getReleaseApprovalStatusLabel(release.approvalStatus)}
        {release.amendment && ' Amendment'}
      </Tag>
    </td>
    <ErrorsAndWarnings releaseId={release.id} />
    <td>
      {release.permissions.canDeleteRelease && release.amendment && (
        <ButtonText onClick={onDelete}>
          Cancel amendment
          <VisuallyHidden> for {release.title}</VisuallyHidden>
        </ButtonText>
      )}
    </td>
    <td>
      {release.amendment && (
        <Link
          to={generatePath<ReleaseRouteParams>(releaseSummaryRoute.path, {
            publicationId: release.publicationId,
            releaseId: release.previousVersionId,
          })}
          data-testid={`View original release link for ${
            release.publicationTitle
          }, ${getReleaseSummaryLabel(release)}`}
        >
          View original
          <VisuallyHidden> for {release.title}</VisuallyHidden>
        </Link>
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

export default DraftReleaseRow;
