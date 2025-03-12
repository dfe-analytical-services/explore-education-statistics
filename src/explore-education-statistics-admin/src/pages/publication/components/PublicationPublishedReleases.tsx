import PublicationPublishedReleasesTable from '@admin/pages/publication/components/PublicationPublishedReleasesTable';
import publicationQueries from '@admin/queries/publicationQueries';
import {
  ReleaseRouteParams,
  releaseSummaryRoute,
} from '@admin/routes/releaseRoutes';
import releaseVersionService, {
  ReleaseVersionSummaryWithPermissions,
} from '@admin/services/releaseVersionService';
import ButtonText from '@common/components/ButtonText';
import LoadingSpinner from '@common/components/LoadingSpinner';
import WarningMessage from '@common/components/WarningMessage';
import { PaginatedList } from '@common/services/types/pagination';
import { useInfiniteQuery } from '@tanstack/react-query';
import last from 'lodash/last';
import React, { MutableRefObject, useEffect, useMemo, useState } from 'react';
import { generatePath, useHistory } from 'react-router';
import { Publication } from '@admin/services/publicationService';
import { ReleaseLabelFormValues } from './ReleaseLabelEditModal';

interface Props {
  publication: Publication;
  pageSize?: number;
  refetchRef?: MutableRefObject<() => void>;
  onEdit: (
    releaseId: string,
    releaseDetailsFormValues: ReleaseLabelFormValues,
  ) => Promise<void>;
}

export default function PublicationPublishedReleases({
  publication,
  pageSize = 5,
  refetchRef,
  onEdit,
}: Props) {
  const history = useHistory();

  const [focusReleaseId, setFocusReleaseId] = useState<string>();

  const {
    data: releases,
    hasNextPage,
    fetchNextPage,
    isFetchingNextPage,
    isLoading,
    isSuccess,
    refetch,
  } = useInfiniteQuery({
    ...publicationQueries.listPublishedReleaseVersions(
      publication.id,
      pageSize,
    ),
    ...{
      getNextPageParam: (
        lastPage: PaginatedList<ReleaseVersionSummaryWithPermissions>,
      ) =>
        lastPage.paging.totalPages > lastPage.paging.page
          ? lastPage.paging.page + 1
          : undefined,
    },
  });

  useEffect(() => {
    if (refetchRef) {
      // eslint-disable-next-line no-param-reassign
      refetchRef.current = refetch;
    }
  });

  const lastPage = last(releases?.pages);

  const allReleases = useMemo(
    () => releases?.pages.flatMap(page => page.results) ?? [],
    [releases?.pages],
  );

  const showMoreNumber = useMemo(() => {
    if (!lastPage || !allReleases.length) {
      return 0;
    }

    const remainingResults = lastPage.paging.totalResults - allReleases.length;

    return remainingResults < pageSize ? remainingResults : pageSize;
  }, [allReleases.length, lastPage, pageSize]);

  return (
    <>
      <h3 aria-atomic aria-live="polite">
        {`Published releases${
          lastPage
            ? ` (${allReleases.length} of ${lastPage.paging.totalResults})`
            : ''
        }`}
      </h3>

      <LoadingSpinner
        text="Loading published releases"
        hideText
        loading={isLoading}
      >
        {isSuccess ? (
          <>
            <PublicationPublishedReleasesTable
              focusReleaseId={focusReleaseId}
              publication={publication}
              releases={allReleases}
              onAmend={async id => {
                const { id: amendmentId } =
                  await releaseVersionService.createReleaseVersionAmendment(id);
                history.push(
                  generatePath<ReleaseRouteParams>(releaseSummaryRoute.path, {
                    publicationId: publication.id,
                    releaseVersionId: amendmentId,
                  }),
                );
              }}
              onEdit={onEdit}
            />

            {hasNextPage && showMoreNumber > 0 && (
              <div className="dfe-flex govuk-!-text-align-centre">
                <ButtonText
                  onClick={async () => {
                    const { data } = await fetchNextPage();
                    const nextPage = last(data?.pages);

                    if (nextPage) {
                      setFocusReleaseId(nextPage.results[0]?.id);
                    }
                  }}
                >
                  {`Show ${showMoreNumber} more published release${
                    showMoreNumber > 1 ? 's' : ''
                  }`}
                </ButtonText>

                <LoadingSpinner
                  className="govuk-!-margin-left-2"
                  loading={isFetchingNextPage}
                  text={`Loading ${showMoreNumber} more releases`}
                  alert
                  hideText
                  inline
                  size="sm"
                />
              </div>
            )}
          </>
        ) : (
          <WarningMessage>
            There was a problem loading the published releases.
          </WarningMessage>
        )}
      </LoadingSpinner>
    </>
  );
}
