import ReleaseSummary from "@admin/pages/admin-dashboard/components/ReleaseSummary";
import {AdminDashboardRelease} from '@admin/services/dashboard/types';
import {Dictionary} from "@common/types";
import React, {useEffect, useState} from 'react';

interface Props {
  getReleasesFn: () => Promise<AdminDashboardRelease[]>;
  noReleasesMessage: string;
}

const ReleasesByStatusTab = ({getReleasesFn, noReleasesMessage}: Props) => {

  const [releasesByPublication, setReleasesByPublication] = useState<
    Dictionary<AdminDashboardRelease[]>
  >();

  useEffect(() => {
    getReleasesFn().then(releases => {
      const byPublication: Dictionary<AdminDashboardRelease[]> = {};
      releases.forEach(release => {
        if (byPublication[release.publicationTitle]) {
          byPublication[release.publicationTitle].push(release);
        } else {
          byPublication[release.publicationTitle] = [release];
        }
      });
      setReleasesByPublication(byPublication);
    });
  }, []);

  return (
    <>
      {releasesByPublication && Object.keys(releasesByPublication).length > 0 && (
        <>
          {Object.keys(releasesByPublication).map(publication => (
            <React.Fragment key={publication}>
              <hr />
              <h3>{publication}</h3>
              {
                releasesByPublication[publication].map(release => (
                  <ReleaseSummary
                    key={release.id}
                    publicationId={release.publicationId}
                    release={release}
                  />
                ))
              }
            </React.Fragment>
          ))}
        </>
      )}
      {releasesByPublication && Object.keys(releasesByPublication).length === 0 && (
        <div>{noReleasesMessage}</div>
      )}
    </>
  );
};

export default ReleasesByStatusTab;
