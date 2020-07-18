import ButtonLink from '@admin/components/ButtonLink';
import Page from '@admin/components/Page';
import usePublicationContext from '@admin/contexts/PublicationContext';
import {
  legacyReleaseCreateRoute,
  legacyReleaseEditRoute,
} from '@admin/routes/legacyReleaseRoutes';
import { PublicationRouteParams } from '@admin/routes/routes';
import legacyReleaseService, {
  LegacyRelease,
} from '@admin/services/legacyReleaseService';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import LoadingSpinner from '@common/components/LoadingSpinner';
import ModalConfirm from '@common/components/ModalConfirm';
import React, { useState } from 'react';
import { generatePath } from 'react-router';

const LegacyReleasesPage = () => {
  const {
    value: publication,
    isLoading,
    retry: reloadPublication,
  } = usePublicationContext();

  const [deleteLegacyRelease, setDeleteLegacyRelease] = useState<
    LegacyRelease
  >();

  return (
    <Page
      title="Legacy releases"
      caption={publication?.title}
      breadcrumbs={[{ name: 'Legacy releases' }]}
    >
      <LoadingSpinner loading={isLoading}>
        {publication && (
          <>
            {publication.legacyReleases.length > 0 ? (
              <table>
                <thead>
                  <tr>
                    <th>Description</th>
                    <th>URL</th>
                    <th className="govuk-!-width-one-third">Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {publication.legacyReleases.map(release => (
                    <tr key={release.id}>
                      <td>{release.description}</td>
                      <td>
                        <a
                          className="govuk-link--no-visited-state"
                          href={release.url}
                          target="_blank"
                          rel="noopener noreferrer"
                        >
                          {release.url}
                        </a>
                      </td>
                      <td>
                        <ButtonGroup>
                          <ButtonLink
                            to={generatePath(legacyReleaseEditRoute.path, {
                              publicationId: publication.id,
                              legacyReleaseId: release.id,
                            })}
                          >
                            Edit release
                          </ButtonLink>
                          <Button
                            variant="warning"
                            onClick={() => {
                              setDeleteLegacyRelease(release);
                            }}
                          >
                            Delete release
                          </Button>
                        </ButtonGroup>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            ) : (
              <p>No legacy releases for this publication.</p>
            )}

            <ModalConfirm
              mounted={!!deleteLegacyRelease}
              onConfirm={async () => {
                if (deleteLegacyRelease) {
                  await legacyReleaseService.deleteLegacyRelease(
                    deleteLegacyRelease?.id,
                  );

                  await reloadPublication();
                }

                setDeleteLegacyRelease(undefined);
              }}
              onExit={() => setDeleteLegacyRelease(undefined)}
              title="Delete legacy release"
            >
              <p>Are you sure you want to delete this legacy release?</p>
            </ModalConfirm>

            <ButtonGroup>
              <ButtonLink
                to={generatePath<PublicationRouteParams>(
                  legacyReleaseCreateRoute.path,
                  {
                    publicationId: publication.id,
                  },
                )}
              >
                Create legacy release
              </ButtonLink>

              {publication.legacyReleases.length > 0 && (
                <Button variant="secondary">Reorder legacy releases</Button>
              )}
            </ButtonGroup>
          </>
        )}
      </LoadingSpinner>
    </Page>
  );
};

export default LegacyReleasesPage;
