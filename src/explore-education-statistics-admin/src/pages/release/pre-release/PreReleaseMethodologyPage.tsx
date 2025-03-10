import Link from '@admin/components/Link';
import { MethodologyContentPageInternal } from '@admin/pages/methodology/edit-methodology/content/MethodologyContentPage';
import { MethodologyContentProvider } from '@admin/pages/methodology/edit-methodology/content/context/MethodologyContentContext';
import {
  PreReleaseMethodologyRouteParams,
  preReleaseMethodologiesRoute,
} from '@admin/routes/preReleaseRoutes';
import { ReleaseRouteParams } from '@admin/routes/releaseRoutes';
import LoadingSpinner from '@common/components/LoadingSpinner';
import WarningMessage from '@common/components/WarningMessage';
import React from 'react';
import { generatePath, RouteComponentProps } from 'react-router';
import { useQuery } from '@tanstack/react-query';
import methodologyQueries from '@admin/queries/methodologyQueries';
import methodologyContentQueries from '@admin/queries/methodologyContentQueries';

const PreReleaseMethodologyPage = ({
  match,
}: RouteComponentProps<PreReleaseMethodologyRouteParams>) => {
  const { methodologyId, publicationId, releaseVersionId } = match.params;

  const { data: methodologyVersion, isLoading: isMethodologyVersionLoading } =
    useQuery(methodologyQueries.get(methodologyId));

  const { data: methodologyContent, isLoading: isMethodologyContentLoading } =
    useQuery(methodologyContentQueries.get(methodologyId));

  const isLoading = isMethodologyVersionLoading || isMethodologyContentLoading;

  return (
    <div className="govuk-width-container">
      <LoadingSpinner loading={isLoading}>
        <Link
          back
          className="govuk-!-margin-bottom-6"
          to={generatePath<ReleaseRouteParams>(
            preReleaseMethodologiesRoute.path,
            {
              publicationId,
              releaseVersionId,
            },
          )}
        >
          Back
        </Link>
        {methodologyContent && methodologyVersion ? (
          <MethodologyContentProvider
            value={{
              methodology: methodologyContent,
              methodologyVersion,
              canUpdateMethodology: false,
              isPreRelease: true,
            }}
          >
            <MethodologyContentPageInternal />
          </MethodologyContentProvider>
        ) : (
          <WarningMessage>Could not load methodology</WarningMessage>
        )}
      </LoadingSpinner>
    </div>
  );
};

export default PreReleaseMethodologyPage;
