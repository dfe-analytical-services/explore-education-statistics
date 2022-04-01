import Link from '@admin/components/Link';
import { MethodologyContentPageInternal } from '@admin/pages/methodology/edit-methodology/content/MethodologyContentPage';
import {
  MethodologyContextState,
  MethodologyContentProvider,
} from '@admin/pages/methodology/edit-methodology/content/context/MethodologyContentContext';
import {
  PreReleaseMethodologyRouteParams,
  preReleaseMethodologiesRoute,
} from '@admin/routes/preReleaseRoutes';
import methodologyContentService from '@admin/services/methodologyContentService';
import { ReleaseRouteParams } from '@admin/routes/releaseRoutes';
import LoadingSpinner from '@common/components/LoadingSpinner';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React from 'react';
import { generatePath, RouteComponentProps } from 'react-router';

const PreReleaseMethodologyPage = ({
  match,
}: RouteComponentProps<PreReleaseMethodologyRouteParams>) => {
  const { methodologyId, publicationId, releaseId } = match.params;

  const { value, isLoading } = useAsyncHandledRetry<
    MethodologyContextState
  >(async () => {
    const methodology = await methodologyContentService.getMethodologyContent(
      methodologyId,
    );

    return {
      methodology,
      canUpdateMethodology: false,
      isPreRelease: true,
    };
  }, [methodologyId]);

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
              releaseId,
            },
          )}
        >
          Back
        </Link>
        {value ? (
          <MethodologyContentProvider value={value}>
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
