import ReleaseSummary from '@admin/pages/admin-dashboard/components/ReleaseSummary';
import { AdminDashboardRelease } from '@admin/services/dashboard/types';
import { Dictionary } from '@common/types';
import React, {ReactNode} from 'react';

interface Props {
  noReleasesMessage: string;
  releases: AdminDashboardRelease[];
  actions: (release: AdminDashboardRelease) => ReactNode;
  afterCommentsSection?: (release: AdminDashboardRelease) => ReactNode;
}

const ReleasesByStatusTab = ({ releases, noReleasesMessage, actions, afterCommentsSection }: Props) => {
  const releasesByPublication: Dictionary<AdminDashboardRelease[]> = {};

  releases.forEach(release => {
    if (releasesByPublication[release.publicationTitle]) {
      releasesByPublication[release.publicationTitle].push(release);
    } else {
      releasesByPublication[release.publicationTitle] = [release];
    }
  });

  return (
    <>
      {releasesByPublication && Object.keys(releasesByPublication).length > 0 && (
        <>
          {Object.keys(releasesByPublication).map(publication => (
            <React.Fragment key={publication}>
              <hr />
              <h3>{publication}</h3>
              {releasesByPublication[publication].map(release => (
                <ReleaseSummary
                  key={release.id}
                  release={release}
                  actions={actions(release)}
                  afterCommentsSection={afterCommentsSection && afterCommentsSection(release)}
                />
              ))}
            </React.Fragment>
          ))}
        </>
      )}
      {releasesByPublication &&
        Object.keys(releasesByPublication).length === 0 && (
          <div>{noReleasesMessage}</div>
        )}
    </>
  );
};

export default ReleasesByStatusTab;
