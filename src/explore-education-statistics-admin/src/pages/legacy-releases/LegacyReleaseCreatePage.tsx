import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import usePublicationContext from '@admin/contexts/PublicationContext';
import LegacyReleaseForm from '@admin/pages/legacy-releases/components/LegacyReleaseForm';
import {
  legacyReleasesRoute,
  PublicationRouteParams,
} from '@admin/routes/routes';
import legacyReleaseService from '@admin/services/legacyReleaseService';
import React from 'react';
import { generatePath, RouteComponentProps } from 'react-router';

const LegacyReleaseCreatePage = ({
  history,
  match,
}: RouteComponentProps<PublicationRouteParams>) => {
  const { publicationId } = match.params;

  const {
    value: publication,
    retry: reloadPublication,
  } = usePublicationContext();

  const legacyReleasesPath = generatePath(legacyReleasesRoute.path, {
    publicationId,
  });

  return (
    <Page
      title="Create legacy release"
      caption={publication?.title}
      backLink={legacyReleasesPath}
      breadcrumbs={[
        {
          name: 'Legacy releases',
          link: legacyReleasesPath,
        },
        { name: 'Create legacy release' },
      ]}
    >
      <LegacyReleaseForm
        cancelButton={
          <Link unvisited to={legacyReleasesPath}>
            Cancel
          </Link>
        }
        onSubmit={async values => {
          await legacyReleaseService.createLegacyRelease({
            description: values.description,
            url: values.url,
            publicationId,
          });

          await reloadPublication();

          history.push(
            generatePath<PublicationRouteParams>(legacyReleasesRoute.path, {
              publicationId,
            }),
          );
        }}
      />
    </Page>
  );
};

export default LegacyReleaseCreatePage;
