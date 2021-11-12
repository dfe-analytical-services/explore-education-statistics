import ReleaseContributorPermissions from '@admin/pages/publication/components/ReleaseContributorPermissions';
import Details from '@common/components/Details';
import WarningMessage from '@common/components/WarningMessage';
import React, { useState } from 'react';
import { ManageAccessPageRelease } from '@admin/services/releasePermissionService';

export interface Props {
  releases: ManageAccessPageRelease[];
  onChange: (release: ManageAccessPageRelease) => void;
}

const PublicationManageTeamAccessTab = ({ releases, onChange }: Props) => {
  const [renderedReleases, setRenderedReleases] = useState<string[]>([]);

  if (!releases || !releases.length) {
    return (
      <WarningMessage>
        Create a release for this publication to manage team access.
      </WarningMessage>
    );
  }

  const latestRelease = releases[releases.length - 1];
  const previousReleases = releases
    .filter(release => release.releaseId !== latestRelease.releaseId)
    .reverse();

  return (
    <>
      <h2>Update access for latest release ({latestRelease?.releaseTitle})</h2>
      <p>
        Allow team members to be able to access and edit this release. You can
        also{' '}
        <a href="#previous-releases">control access to previous releases</a>.
      </p>
      <ReleaseContributorPermissions
        release={latestRelease}
        onChange={onChange}
      />

      <h3 id="previous-releases">Previous releases</h3>

      {previousReleases.length > 0 ? (
        <>
          {previousReleases.map(release => (
            <div key={release.releaseId}>
              <Details
                summary={`${release.releaseTitle}`}
                onToggle={isOpen => {
                  if (isOpen && !renderedReleases.includes(release.releaseId)) {
                    setRenderedReleases([
                      ...renderedReleases,
                      release.releaseId,
                    ]);
                  }
                }}
              >
                {renderedReleases.includes(release.releaseId) && (
                  <ReleaseContributorPermissions release={release} />
                )}
              </Details>
            </div>
          ))}
        </>
      ) : (
        <p>No previous releases.</p>
      )}
    </>
  );
};

export default PublicationManageTeamAccessTab;
