import CancelAmendmentModal from '@admin/pages/admin-dashboard/components/CancelAmendmentModal';
import DraftReleaseRow from '@admin/pages/publication/components/DraftReleaseRow';
import {
  DraftStatusGuidanceModal,
  IssuesGuidanceModal,
} from '@admin/pages/publication/components/PublicationGuidance';
import releaseService, {
  DeleteReleasePlan,
  MyRelease,
} from '@admin/services/releaseService';
import ButtonText from '@common/components/ButtonText';
import InfoIcon from '@common/components/InfoIcon';
import useToggle from '@common/hooks/useToggle';
import React, { useState } from 'react';

interface Props {
  releases: MyRelease[];
  onChange: () => void;
}

const PublicationDraftReleases = ({ releases, onChange }: Props) => {
  const [deleteReleasePlan, setDeleteReleasePlan] = useState<
    DeleteReleasePlan & {
      releaseId: string;
    }
  >();

  const [showDraftStatusGuidance, toggleDraftStatusGuidance] = useToggle(false);
  const [showIssuesGuidance, toggleIssuesGuidance] = useToggle(false);

  return (
    <>
      <table
        className="dfe-hide-empty-cells govuk-!-margin-bottom-9"
        data-testid="publication-draft-releases"
      >
        <caption className="govuk-table__caption--m">Draft releases</caption>
        <thead>
          <tr>
            <th className="govuk-!-width-one-third">Release period</th>
            <th>
              State{' '}
              <ButtonText onClick={toggleDraftStatusGuidance.on}>
                <InfoIcon description="Guidance on draft states" />
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
            onChange();
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
