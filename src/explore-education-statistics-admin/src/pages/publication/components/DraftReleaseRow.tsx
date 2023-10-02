import Link from '@admin/components/Link';
import { getReleaseApprovalStatusLabel } from '@admin/pages/release/utils/releaseSummaryUtil';
import CancelAmendmentModal from '@admin/pages/admin-dashboard/components/CancelAmendmentModal';
import releaseService, {
  DeleteReleasePlan,
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
import React, { useState } from 'react';
import { generatePath } from 'react-router';

interface Props {
  publicationId: string;
  release: ReleaseSummaryWithPermissions;
  onAmendmentDelete?: () => void;
}

const DraftReleaseRow = ({
  publicationId,
  release,
  onAmendmentDelete,
}: Props) => {
  const { value: checklist, isLoading: isLoadingChecklist } =
    useAsyncHandledRetry(() => releaseService.getReleaseChecklist(release.id));

  const [deleteReleasePlan, setDeleteReleasePlan] = useState<
    DeleteReleasePlan & {
      releaseId: string;
    }
  >();

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
          className="govuk-!-margin-right-4 govuk-!-display-inline-block"
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
            className="govuk-!-margin-right-4 govuk-!-display-inline-block"
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
          <CancelAmendmentModal
            scheduledMethodologies={deleteReleasePlan?.scheduledMethodologies}
            triggerButton={
              <ButtonText
                variant="warning"
                onClick={async () => {
                  setDeleteReleasePlan({
                    ...(await releaseService.getDeleteReleasePlan(release.id)),
                    releaseId: release.id,
                  });
                }}
              >
                Cancel amendment
                <VisuallyHidden> for {release.title}</VisuallyHidden>
              </ButtonText>
            }
            onConfirm={async () => {
              if (deleteReleasePlan) {
                await releaseService.deleteRelease(deleteReleasePlan.releaseId);
                setDeleteReleasePlan(undefined);
                onAmendmentDelete?.();
              }
            }}
            onCancel={() => setDeleteReleasePlan(undefined)}
          />
        )}
      </td>
    </tr>
  );
};

export default DraftReleaseRow;
