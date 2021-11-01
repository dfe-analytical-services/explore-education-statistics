import { BasicPublicationDetails } from '@admin/services/publicationService';
import React from 'react';
import Details from '@common/components/Details';
import orderBy from 'lodash/orderBy';

export interface Props {
  publication: BasicPublicationDetails;
}

const PublicationManageTeamAccessTab = ({ publication }: Props) => {
  if (
    publication.releases === undefined ||
    (publication.releases && publication.releases.length === 0)
  ) {
    return <p>No releases!</p>;
  }

  const sortedReleases = orderBy(publication.releases, r =>
    parseInt(r.releaseName, 10),
  );
  const newestRelease = sortedReleases.shift();

  if (newestRelease === undefined) {
    return <p>No releases!</p>;
  }

  return (
    <>
      <h2>Update access for latest release ({newestRelease?.title})</h2>
      <p>
        Allow team members to be able to access and edit this release. You can
        also{' '}
        <a href="#previous-releases">control access to previous releases</a>.
      </p>

      {sortedReleases.length >= 0 && (
        <>
          <h3 id="previous-releases">Previous releases</h3>
          {sortedReleases.map(release => (
            <div key={release.id}>
              <Details summary={`${release.title}`}>
                <p>Permissions for {release.title}</p>
              </Details>
            </div>
          ))}
        </>
      )}
    </>
  );
};

export default PublicationManageTeamAccessTab;
