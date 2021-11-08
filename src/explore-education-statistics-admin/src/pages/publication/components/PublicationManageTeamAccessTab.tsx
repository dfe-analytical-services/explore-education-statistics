import { BasicPublicationDetails } from '@admin/services/publicationService';
import ReleaseContributorPermissions from '@admin/pages/publication/components/ReleaseContributorPermissions';
import Details from '@common/components/Details';
import WarningMessage from '@common/components/WarningMessage';
import React, { useState } from 'react';

export interface Props {
  publication: BasicPublicationDetails;
}

const PublicationManageTeamAccessTab = ({ publication }: Props) => {
  const [renderedReleases, setRenderedReleases] = useState<string[]>([]);

  if (!publication.releases || !publication.releases.length) {
    return (
      <WarningMessage>
        Create a release for this publication to manage team access.
      </WarningMessage>
    );
  }

  const latestRelease = publication.releases[publication.releases.length - 1];
  const previousReleases = publication.releases
    .filter(release => release.id !== latestRelease.id)
    .reverse();

  return (
    <>
      <h2>Update access for latest release ({latestRelease?.title})</h2>
      <p>
        Allow team members to be able to access and edit this release. You can
        also{' '}
        <a href="#previous-releases">control access to previous releases</a>.
      </p>
      <ReleaseContributorPermissions release={latestRelease} />

      <h3 id="previous-releases">Previous releases</h3>

      {previousReleases.length > 0 ? (
        <>
          {previousReleases.map(release => (
            <div key={release.id}>
              <Details
                summary={`${release.title}`}
                onToggle={isOpen => {
                  if (isOpen && !renderedReleases.includes(release.id)) {
                    setRenderedReleases([...renderedReleases, release.id]);
                  }
                }}
              >
                {renderedReleases.includes(release.id) && (
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
