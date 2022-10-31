import CancelAmendmentModal from '@admin/pages/admin-dashboard/components/CancelAmendmentModal';
import DraftReleaseRow from '@admin/pages/publication/components/DraftReleaseRow';
import {
  DraftStatusGuidanceModal,
  IssuesGuidanceModal,
} from '@admin/pages/publication/components/PublicationGuidance';
import releaseService, {
  DeleteReleasePlan,
  ReleaseSummaryWithPermissions,
} from '@admin/services/releaseService';
import ButtonText from '@common/components/ButtonText';
import InfoIcon from '@common/components/InfoIcon';
import InsetText from '@common/components/InsetText';
import useToggle from '@common/hooks/useToggle';
import React, { useState } from 'react';

interface Props {
  publicationId: string;
  releases: ReleaseSummaryWithPermissions[];
  onAmendmentDelete?: () => void;
}

const PublicationDraftReleases = ({
  publicationId,
  releases,
  onAmendmentDelete,
}: Props) => {
  const [deleteReleasePlan, setDeleteReleasePlan] = useState<
    DeleteReleasePlan & {
      releaseId: string;
    }
  >();

  const [showDraftStatusGuidance, toggleDraftStatusGuidance] = useToggle(false);
  const [showIssuesGuidance, toggleIssuesGuidance] = useToggle(false);

  if (releases.length === 0) {
    return <InsetText>You have no draft releases.</InsetText>;
  }

  return (
    <>
      <table
        className="dfe-hide-empty-cells"
        data-testid="publication-draft-releases"
      >
        <thead>
          <tr>
            <th className="govuk-!-width-one-third">Release period</th>
            <th>
              Status{' '}
              <ButtonText onClick={toggleDraftStatusGuidance.on}>
                <InfoIcon description="Guidance on draft statuses" />
              </ButtonText>
            </th>
            <th>
              Errors{' '}
              <ButtonText onClick={toggleIssuesGuidance.on}>
                <InfoIcon description="Guidance on draft release issues" />
              </ButtonText>
            </th>
            <th>
              Warnings{' '}
              <ButtonText onClick={toggleIssuesGuidance.on}>
                <InfoIcon description="Guidance on draft release issues" />
              </ButtonText>
            </th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {releases.map(release => (
            <DraftReleaseRow
              key={release.id}
              publicationId={publicationId}
              release={release}
              onDelete={async () => {
                setDeleteReleasePlan({
                  ...(await releaseService.getDeleteReleasePlan(release.id)),
                  releaseId: release.id,
                });
              }}
            />
          ))}
        </tbody>
      </table>

      {deleteReleasePlan && (
        <CancelAmendmentModal
          scheduledMethodologies={deleteReleasePlan.scheduledMethodologies}
          onConfirm={async () => {
            await releaseService.deleteRelease(deleteReleasePlan.releaseId);
            setDeleteReleasePlan(undefined);
            onAmendmentDelete?.();
          }}
          onCancel={() => setDeleteReleasePlan(undefined)}
        />
      )}

      <DraftStatusGuidanceModal
        open={showDraftStatusGuidance}
        onClose={toggleDraftStatusGuidance.off}
      />
      <IssuesGuidanceModal
        open={showIssuesGuidance}
        onClose={toggleIssuesGuidance.off}
      />
    </>
  );
};

export default PublicationDraftReleases;
