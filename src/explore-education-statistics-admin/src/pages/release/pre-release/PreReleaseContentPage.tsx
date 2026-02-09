import Link from '@admin/components/Link';
import PageTitle from '@admin/components/PageTitle';
import ReleaseContent from '@admin/pages/release/content/components/ReleaseContent';
import ReleaseContentRedesign from '@admin/pages/release/content/components/ReleaseContentRedesign';
import { ReleaseContentProvider } from '@admin/pages/release/content/contexts/ReleaseContentContext';
import featuredTableQueries from '@admin/queries/featuredTableQueries';
import releaseContentQueries from '@admin/queries/releaseContentQueries';
import {
  preReleaseTableToolRoute,
  PreReleaseTableToolRouteParams,
} from '@admin/routes/preReleaseRoutes';
import { ReleaseRouteParams } from '@admin/routes/releaseRoutes';
import LoadingSpinner from '@common/components/LoadingSpinner';
import { useQuery } from '@tanstack/react-query';
import classNames from 'classnames';
import React from 'react';
import { RouteComponentProps } from 'react-router';
import { generatePath } from 'react-router-dom';

const PreReleaseContentPage = ({
  match,
}: RouteComponentProps<ReleaseRouteParams>) => {
  const { publicationId, releaseVersionId } = match.params;

  const { data: content, isLoading: isLoadingContent } = useQuery(
    releaseContentQueries.get(releaseVersionId),
  );

  const previewRedesign =
    new URLSearchParams(window.location.search).get('redesign') === 'true';

  const { data: featuredTables = [], isLoading: isLoadingFeaturedTables } =
    useQuery(featuredTableQueries.list(releaseVersionId));

  const handleFeaturedTableLinks = (url: string, text: string) => {
    // the url format is `/data-tables/fast-track/<data-block-parent-id>?featuredTables`
    // so split twice to get the dataBlockParentId.
    const dataBlockParentId = url.split('fast-track/')[1].split('?')[0];
    const featuredTable = featuredTables.find(
      table => table.dataBlockParentId === dataBlockParentId,
    );

    return (
      <Link
        to={generatePath<PreReleaseTableToolRouteParams>(
          preReleaseTableToolRoute.path,
          {
            publicationId,
            releaseVersionId,
            dataBlockId: featuredTable?.dataBlockId,
          },
        )}
      >
        {text}
      </Link>
    );
  };

  return (
    <div
      className={classNames('govuk-width-container', {
        'dfe-width-container--wide': previewRedesign,
      })}
    >
      <LoadingSpinner loading={isLoadingContent || isLoadingFeaturedTables}>
        {content && (
          <ReleaseContentProvider
            value={{
              ...content,
              canUpdateRelease: false,
              featuredTables,
            }}
          >
            {previewRedesign ? (
              <ReleaseContentRedesign
                isPra
                transformFeaturedTableLinks={handleFeaturedTableLinks}
              />
            ) : (
              <>
                <PageTitle
                  caption={content.release.title}
                  title={content.release.publication.title}
                />

                <ReleaseContent
                  transformFeaturedTableLinks={handleFeaturedTableLinks}
                />
              </>
            )}
          </ReleaseContentProvider>
        )}
      </LoadingSpinner>
    </div>
  );
};

export default PreReleaseContentPage;
