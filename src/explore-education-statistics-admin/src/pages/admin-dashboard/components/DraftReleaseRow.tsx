import Link from '@admin/components/Link';
import { getReleaseApprovalStatusLabel } from '@admin/pages/release/utils/releaseSummaryUtil';
import releaseService, { MyRelease } from '@admin/services/releaseService';
import {
  ReleaseRouteParams,
  releaseSummaryRoute,
} from '@admin/routes/releaseRoutes';
import ButtonText from '@common/components/ButtonText';
import Details from '@common/components/Details';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Tag from '@common/components/Tag';
import VisuallyHidden from '@common/components/VisuallyHidden';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';
import { generatePath } from 'react-router';
import TagGroup from '@common/components/TagGroup';

interface Props {
  isBauUser: boolean;
  release: MyRelease;
  onDelete: () => void;
}

const DraftReleaseRow = ({ isBauUser, release, onDelete }: Props) => {
  const {
    value: checklist,
    isLoading: isLoadingChecklist,
  } = useAsyncHandledRetry(() =>
    releaseService.getReleaseChecklist(release.id),
  );

  const totalIssues = checklist
    ? checklist?.errors?.length + checklist?.warnings.length
    : 0;

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
      {!isBauUser && (
        <td>
          <LoadingSpinner inline loading={isLoadingChecklist} size="sm">
            {totalIssues === 0 ? (
              'No issues'
            ) : (
              <Details
                className="govuk-!-margin-bottom-0"
                summary={`View issues (${totalIssues})`}
              >
                <TagGroup>
                  <Tag colour="red">{`${checklist?.errors.length} errors`}</Tag>
                  <Tag colour="yellow">{`${checklist?.warnings.length} warnings`}</Tag>
                </TagGroup>
              </Details>
            )}
          </LoadingSpinner>
        </td>
      )}
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
        {release.permissions.canDeleteRelease && release.amendment && (
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
