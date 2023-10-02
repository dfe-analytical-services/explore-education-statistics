import Link from '@admin/components/Link';
import DraftReleaseRowIssues from '@admin/pages/admin-dashboard/components/DraftReleaseRowIssues';
import { getReleaseApprovalStatusLabel } from '@admin/pages/release/utils/releaseSummaryUtil';
import releaseService, {
  ReleaseSummaryWithPermissions,
  DashboardReleaseSummary,
  DeleteReleasePlan,
} from '@admin/services/releaseService';
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
  release: DashboardReleaseSummary & ReleaseSummaryWithPermissions;
  onChangeRelease: () => void;
}

const DraftReleaseRow = ({ isBauUser, release, onChangeRelease }: Props) => {
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
      {!isBauUser && <DraftReleaseRowIssues releaseId={release.id} />}
      <td>
        <Link
          className="govuk-!-margin-right-4 govuk-!-display-inline-block"
          to={generatePath<ReleaseRouteParams>(releaseSummaryRoute.path, {
            publicationId: release.publication.id,
            releaseId: release.id,
          })}
        >
          {release.permissions?.canUpdateRelease ? 'Edit' : 'View'}
          <VisuallyHidden> {release.title}</VisuallyHidden>
        </Link>

        {release.previousVersionId && (
          <Link
            className="govuk-!-margin-right-4 govuk-!-display-inline-block"
            to={generatePath<ReleaseRouteParams>(releaseSummaryRoute.path, {
              publicationId: release.publication.id,
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
