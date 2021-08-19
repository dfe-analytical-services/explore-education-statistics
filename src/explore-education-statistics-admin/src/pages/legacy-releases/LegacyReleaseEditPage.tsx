import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import usePublicationContext from '@admin/contexts/PublicationContext';
import LegacyReleaseForm from '@admin/pages/legacy-releases/components/LegacyReleaseForm';
import {
  publicationEditRoute,
  PublicationRouteParams,
} from '@admin/routes/routes';
import legacyReleaseService from '@admin/services/legacyReleaseService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';
import { generatePath, RouteComponentProps, useHistory } from 'react-router';

interface Params extends PublicationRouteParams {
  legacyReleaseId: string;
}

const LegacyReleaseEditPage = ({ match }: RouteComponentProps<Params>) => {
  const { publicationId, legacyReleaseId } = match.params;

  const history = useHistory();

  const {
    value: publication,
    retry: reloadPublication,
  } = usePublicationContext();

  const { value: legacyRelease, isLoading } = useAsyncHandledRetry(() =>
    legacyReleaseService.getLegacyRelease(legacyReleaseId),
  );

  const publicationEditPath = generatePath(publicationEditRoute.path, {
    publicationId,
  });

  return (
    <Page
      title="Edit legacy release"
      caption={publication?.title}
      backLink={publicationEditPath}
      breadcrumbs={[
        {
          name: 'Manage publication',
          link: publicationEditPath,
        },
        { name: 'Edit legacy release' },
      ]}
    >
      <LoadingSpinner loading={isLoading}>
        {legacyRelease && (
          <LegacyReleaseForm
            initialValues={{
              description: legacyRelease.description,
              url: legacyRelease.url,
              order: legacyRelease.order,
            }}
            cancelButton={
              <Link unvisited to={publicationEditPath}>
                Cancel
              </Link>
            }
            onSubmit={async values => {
              await legacyReleaseService.updateLegacyRelease(legacyReleaseId, {
                ...values,
                order: values?.order ?? legacyRelease?.order,
                publicationId,
              });

              await reloadPublication();

              history.push(publicationEditPath);
            }}
          />
        )}
      </LoadingSpinner>
    </Page>
  );
};

export default LegacyReleaseEditPage;
