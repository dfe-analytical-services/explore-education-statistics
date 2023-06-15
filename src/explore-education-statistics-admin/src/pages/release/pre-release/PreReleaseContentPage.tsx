import PageTitle from '@admin/components/PageTitle';
import ReleaseContent from '@admin/pages/release/content/components/ReleaseContent';
import { ReleaseContentProvider } from '@admin/pages/release/content/contexts/ReleaseContentContext';
import { ReleaseRouteParams } from '@admin/routes/releaseRoutes';
import releaseContentService from '@admin/services/releaseContentService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import { useParams } from 'react-router';
import React from 'react';

const PreReleaseContentPage = () => {
  const { releaseId } = useParams<ReleaseRouteParams>();

  const { value: content, isLoading } = useAsyncHandledRetry(
    () => releaseContentService.getContent(releaseId),
    [releaseId],
  );

  return (
    <div className="govuk-width-container">
      <LoadingSpinner loading={isLoading}>
        {content && (
          <ReleaseContentProvider
            value={{
              ...content,
              canUpdateRelease: false,
            }}
          >
            <PageTitle
              caption={content.release.title}
              title={content.release.publication.title}
            />

            <ReleaseContent />
          </ReleaseContentProvider>
        )}
      </LoadingSpinner>
    </div>
  );
};

export default PreReleaseContentPage;
