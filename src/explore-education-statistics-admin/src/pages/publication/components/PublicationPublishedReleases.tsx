import { PublishedStatusGuidanceModal } from '@admin/pages/publication/components/PublicationGuidance';
import PublicationPublishedReleasesTable from '@admin/pages/publication/components/PublicationPublishedReleasesTable';
import {
  ReleaseRouteParams,
  releaseSummaryRoute,
} from '@admin/routes/releaseRoutes';
import publicationService from '@admin/services/publicationService';
import releaseService, {
  ReleaseSummaryWithPermissions,
} from '@admin/services/releaseService';
import ButtonText from '@common/components/ButtonText';
import LoadingSpinner from '@common/components/LoadingSpinner';
import ModalConfirm from '@common/components/ModalConfirm';
import WarningMessage from '@common/components/WarningMessage';
import useToggle from '@common/hooks/useToggle';
import { useInfiniteQuery } from '@tanstack/react-query';
import last from 'lodash/last';
import React, { MutableRefObject, useEffect, useMemo, useState } from 'react';
import { generatePath, useHistory } from 'react-router';

interface Props {
  publicationId: string;
  pageSize?: number;
  refetchRef?: MutableRefObject<() => void>;
}

export default function PublicationPublishedReleases({
  publicationId,
  pageSize = 5,
  refetchRef,
}: Props) {
  const history = useHistory();

  const [amendReleaseId, setAmendReleaseId] = useState<string>();
  const [focusReleaseId, setFocusReleaseId] = useState<string>();

  const [
    showPublishedStatusGuidance,
    togglePublishedStatusGuidance,
  ] = useToggle(false);

  const {
    data: releases,
    hasNextPage,
    fetchNextPage,
    isFetchingNextPage,
    isLoading,
    isSuccess,
    refetch,
  } = useInfiniteQuery(
    ['publicationPublishedReleases', publicationId],
    ({ pageParam = 1 }) => {
      return publicationService.listReleases<ReleaseSummaryWithPermissions>(
        publicationId,
        {
          live: true,
          page: pageParam,
          pageSize,
          includePermissions: true,
        },
      );
    },
    {
      getNextPageParam: lastPage =>
        lastPage.paging.totalPages > lastPage.paging.page
          ? lastPage.paging.page + 1
          : undefined,
    },
  );

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
              publicationId={publicationId}
              releases={allReleases}
              onAmend={setAmendReleaseId}
              onGuidanceClick={togglePublishedStatusGuidance.on}
            />

            {hasNextPage && showMoreNumber > 0 && (
              <div className="dfe-flex dfe-align--centre">
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

      <PublishedStatusGuidanceModal
        open={showPublishedStatusGuidance}
        onClose={togglePublishedStatusGuidance.off}
      />

      {amendReleaseId && (
        <ModalConfirm
          open={!!amendReleaseId}
          title="Confirm you want to amend this published release"
          onCancel={() => setAmendReleaseId(undefined)}
          onConfirm={async () => {
            const amendment = await releaseService.createReleaseAmendment(
              amendReleaseId,
            );

            history.push(
              generatePath<ReleaseRouteParams>(releaseSummaryRoute.path, {
                publicationId,
                releaseId: amendment.id,
              }),
            );
          }}
          onExit={() => setAmendReleaseId(undefined)}
        >
          <p>
            Please note, any changes made to this published release must be
            approved before updates can be published.
          </p>
        </ModalConfirm>
      )}
    </>
  );
}
