import DraftReleaseRow from '@admin/pages/publication/components/DraftReleaseRow';
import styles from '@admin/pages/publication/PublicationReleasesPage.module.scss';
import {
  DraftStatusGuidanceModal,
  IssuesGuidanceModal,
} from '@admin/pages/publication/components/PublicationGuidance';
import { ReleaseVersionSummaryWithPermissions } from '@admin/services/releaseVersionService';
import InsetText from '@common/components/InsetText';
import ButtonGroup from '@common/components/ButtonGroup';
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
    <>
      <table
        className="dfe-hide-empty-cells"
        data-testid="publication-draft-releases"
      >
        <caption className="govuk-visually-hidden">
          Table showing the draft releases for this publication.
        </caption>
        <thead>
          <tr>
            <th className="govuk-!-width-one-third">Release period</th>
            <th className={`${styles.statusColumn} dfe-white-space--nowrap`}>
              Status
            </th>
            <th className="dfe-white-space--nowrap">Errors</th>
            <th className="dfe-white-space--nowrap">Warnings</th>
            <th className="govuk-!-width-one-quarter">Actions</th>
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
      <ButtonGroup>
        <IssuesGuidanceModal />
        <DraftStatusGuidanceModal />
      </ButtonGroup>
    </>
  );
};

export default PublicationDraftReleases;
