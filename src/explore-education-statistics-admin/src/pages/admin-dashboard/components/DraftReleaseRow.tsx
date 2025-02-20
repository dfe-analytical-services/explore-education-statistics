import Link from '@admin/components/Link';
import DraftReleaseRowIssues from '@admin/pages/admin-dashboard/components/DraftReleaseRowIssues';
import { getReleaseApprovalStatusLabel } from '@admin/pages/release/utils/releaseSummaryUtil';
import releaseVersionService, {
  ReleaseVersionSummaryWithPermissions,
  DashboardReleaseVersionSummary,
  DeleteReleasePlan,
} from '@admin/services/releaseVersionService';
import {
  ReleaseRouteParams,
  releaseSummaryRoute,
} from '@admin/routes/releaseRoutes';
import ButtonText from '@common/components/ButtonText';
import Tag from '@common/components/Tag';
import VisuallyHidden from '@common/components/VisuallyHidden';
import React, { useState } from 'react';
import { generatePath } from 'react-router';
import CancelAmendmentModal from './CancelAmendmentModal';

interface Props {
  isBauUser: boolean;
  release: DashboardReleaseVersionSummary &
    ReleaseVersionSummaryWithPermissions;
  onChangeRelease: () => void;
}

const DraftReleaseRow = ({ isBauUser, release, onChangeRelease }: Props) => {
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
      {!isBauUser && <DraftReleaseRowIssues releaseVersionId={release.id} />}
      <td>
        <Link
          className="govuk-!-margin-right-4 govuk-!-display-inline-block"
          to={generatePath<ReleaseRouteParams>(releaseSummaryRoute.path, {
            publicationId: release.publication.id,
            releaseVersionId: release.id,
          })}
        >
          {release.permissions?.canUpdateReleaseVersion ? 'Edit' : 'View'}
          <VisuallyHidden> {release.title}</VisuallyHidden>
        </Link>

        {release.previousVersionId && (
          <Link
            className="govuk-!-margin-right-4 govuk-!-display-inline-block"
            to={generatePath<ReleaseRouteParams>(releaseSummaryRoute.path, {
              publicationId: release.publication.id,
              releaseVersionId: release.previousVersionId,
            })}
          >
            View existing version
            <VisuallyHidden> for {release.title}</VisuallyHidden>
          </Link>
        )}

        {release.permissions?.canDeleteReleaseVersion && release.amendment && (
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
                onChangeRelease();
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
