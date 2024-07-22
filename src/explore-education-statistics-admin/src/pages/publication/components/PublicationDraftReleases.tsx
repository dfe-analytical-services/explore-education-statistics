import DraftReleaseRow from '@admin/pages/publication/components/DraftReleaseRow';
import {
  DraftStatusGuidanceModal,
  IssuesGuidanceModal,
} from '@admin/pages/publication/components/PublicationGuidance';
import { ReleaseVersionSummaryWithPermissions } from '@admin/services/releaseService';
import InsetText from '@common/components/InsetText';
import React from 'react';

interface Props {
  publicationId: string;
  releases: ReleaseVersionSummaryWithPermissions[];
  onAmendmentDelete?: () => void;
}

const PublicationDraftReleases = ({
  publicationId,
  releases,
  onAmendmentDelete,
}: Props) => {
  if (releases.length === 0) {
    return <InsetText>You have no draft releases.</InsetText>;
  }

  return (
    <table
      className="dfe-hide-empty-cells"
      data-testid="publication-draft-releases"
    >
      <thead>
        <tr>
          <th className="govuk-!-width-one-third">Release period</th>
          <th className="dfe-white-space--nowrap">
            Status <DraftStatusGuidanceModal />
          </th>
          <th className="dfe-white-space--nowrap">
            Errors <IssuesGuidanceModal />
          </th>
          <th className="dfe-white-space--nowrap">
            Warnings <IssuesGuidanceModal />
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
            onAmendmentDelete={onAmendmentDelete}
          />
        ))}
      </tbody>
    </table>
  );
};

export default PublicationDraftReleases;
