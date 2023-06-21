import Link from '@admin/components/Link';
import LegacyReleaseForm from '@admin/pages/legacy-releases/components/LegacyReleaseForm';
import usePublicationContext from '@admin/pages/publication/contexts/PublicationContext';
import {
  PublicationEditLegacyReleaseRouteParams,
  publicationLegacyReleasesRoute,
} from '@admin/routes/publicationRoutes';
import legacyReleaseService from '@admin/services/legacyReleaseService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import { generatePath, useHistory, useParams } from 'react-router';
import React from 'react';

const PublicationLegacyReleaseEditPage = () => {
  const { legacyReleaseId } =
    useParams<PublicationEditLegacyReleaseRouteParams>();

  const { publicationId } = usePublicationContext();
  const history = useHistory();

  const { value: legacyRelease, isLoading } = useAsyncHandledRetry(() =>
    legacyReleaseService.getLegacyRelease(legacyReleaseId),
  );

  const publicationEditPath = generatePath(
    publicationLegacyReleasesRoute.path,
    {
      publicationId,
    },
  );

  return (
    <LoadingSpinner loading={isLoading}>
      <h2>Edit legacy release</h2>
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

            history.push(publicationEditPath);
          }}
        />
      )}
    </LoadingSpinner>
  );
};

export default PublicationLegacyReleaseEditPage;
