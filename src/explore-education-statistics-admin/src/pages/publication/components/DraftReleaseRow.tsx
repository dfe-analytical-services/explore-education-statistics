import Link from '@admin/components/Link';
import { getReleaseApprovalStatusLabel } from '@admin/pages/release/utils/releaseSummaryUtil';
import CancelAmendmentModal from '@admin/pages/admin-dashboard/components/CancelAmendmentModal';
import releaseVersionService, {
  DeleteReleasePlan,
  ReleaseVersionSummaryWithPermissions,
} from '@admin/services/releaseVersionService';
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
import DeleteDraftModal from '@admin/pages/admin-dashboard/components/DeleteDraftModal';

interface Props {
  publicationId: string;
  release: ReleaseVersionSummaryWithPermissions;
  onAmendmentDelete?: () => void;
}

const DraftReleaseRow = ({
  publicationId,
  release,
  onAmendmentDelete,
}: Props) => {
  const { value: checklist, isLoading: isLoadingChecklist } =
    useAsyncHandledRetry(() =>
      releaseVersionService.getReleaseVersionChecklist(release.id),
    );

  const [deleteReleasePlan, setDeleteReleasePlan] = useState<
    DeleteReleasePlan & {
      releaseVersionId: string;
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
            releaseVersionId: release.id,
          })}
        >
          {release.permissions?.canUpdateRelease ? 'Edit Draft' : 'View'}
          <VisuallyHidden> {release.title}</VisuallyHidden>
        </Link>

        {release.permissions?.canDeleteRelease &&
          release.approvalStatus === 'Draft' &&
          !release.amendment && (
            <DeleteDraftModal
              triggerButton={
                <ButtonText variant="warning">
                  Delete
                  <VisuallyHidden> {release.title}</VisuallyHidden>
                </ButtonText>
              }
              onConfirm={async () => {
                await releaseVersionService.deleteReleaseVersion(release.id);
                onAmendmentDelete?.();
              }}
            />
          )}

        {release.permissions?.canDeleteRelease && release.amendment && (
          <CancelAmendmentModal
            scheduledMethodologies={deleteReleasePlan?.scheduledMethodologies}
            triggerButton={
              <ButtonText
                variant="warning"
                onClick={async () => {
                  setDeleteReleasePlan({
                    ...(await releaseVersionService.getDeleteReleaseVersionPlan(
                      release.id,
                    )),
                    releaseVersionId: release.id,
                  });
                }}
              >
                Cancel amendment
                <VisuallyHidden> for {release.title}</VisuallyHidden>
              </ButtonText>
            }
            onConfirm={async () => {
              if (deleteReleasePlan) {
                await releaseVersionService.deleteReleaseVersion(
                  deleteReleasePlan.releaseVersionId,
                );
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
