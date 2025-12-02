import Link from '@admin/components/Link';
import styles from '@admin/pages/publication/PublicationReleasesPage.module.scss';
import { getReleaseApprovalStatusLabel } from '@admin/pages/release/utils/releaseSummaryUtil';
import CancelAmendmentModal from '@admin/pages/admin-dashboard/components/CancelAmendmentModal';
import releaseVersionService, {
  DeleteReleasePlan,
  ReleaseVersionSummaryWithPermissions,
} from '@admin/services/releaseVersionService';
import {
  releaseChecklistRoute,
  ReleaseRouteParams,
  releaseSummaryRoute,
} from '@admin/routes/releaseRoutes';
import DeleteDraftModal from '@admin/pages/admin-dashboard/components/DeleteDraftModal';
import releaseQueries from '@admin/queries/releaseQueries';
import ButtonText from '@common/components/ButtonText';
import Tag from '@common/components/Tag';
import VisuallyHidden from '@common/components/VisuallyHidden';
import LoadingSpinner from '@common/components/LoadingSpinner';
import React, { useState } from 'react';
import { generatePath } from 'react-router';
import { useQuery } from '@tanstack/react-query';

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
  const { data: checklist, isFetching } = useQuery(
    releaseQueries.getChecklist(release.id),
  );

  const { errors = [], warnings = [] } = checklist ?? {};
  const totalIssues = errors.length + warnings.length;

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
        <LoadingSpinner inline loading={isFetching} size="sm">
          {errors.length}
        </LoadingSpinner>
      </td>
      <td>
        <LoadingSpinner inline loading={isFetching} size="sm">
          {warnings.length}
        </LoadingSpinner>
      </td>
      <td>
        <div className={styles.actionsColumn}>
          <Link
            to={generatePath<ReleaseRouteParams>(releaseSummaryRoute.path, {
              publicationId,
              releaseVersionId: release.id,
            })}
          >
            {release.permissions?.canUpdateReleaseVersion
              ? 'Edit draft'
              : 'View'}
            <VisuallyHidden> {release.title}</VisuallyHidden>
          </Link>

          {release.permissions?.canDeleteReleaseVersion &&
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

          {release.permissions?.canDeleteReleaseVersion &&
            release.amendment && (
              <CancelAmendmentModal
                scheduledMethodologies={
                  deleteReleasePlan?.scheduledMethodologies
                }
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
          {totalIssues > 0 && (
            <Link
              to={generatePath<ReleaseRouteParams>(releaseChecklistRoute.path, {
                publicationId,
                releaseVersionId: release.id,
              })}
            >
              View issues
              <VisuallyHidden> for {release.title}</VisuallyHidden>
            </Link>
          )}
        </div>
      </td>
    </tr>
  );
};

export default DraftReleaseRow;
