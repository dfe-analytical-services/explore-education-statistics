import { MyRelease } from '@admin/services/releaseService';
import { Dictionary } from '@common/types';
import React, { ReactNode } from 'react';

interface Props {
  noReleasesMessage: string;
  releases: MyRelease[];
  renderReleaseSummary: (release: MyRelease) => ReactNode;
}

const ReleasesByStatusTab = ({
  releases,
  noReleasesMessage,
  renderReleaseSummary,
}: Props) => {
  const releasesByPublication: Dictionary<MyRelease[]> = {};

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
            <div
              key={publication}
              data-testid={`releaseByStatusTab ${publication}`}
            >
              <hr />
              <h3>{publication}</h3>
              {releasesByPublication[publication].map(release =>
                renderReleaseSummary(release),
              )}
            </div>
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
