import PublicationDraftReleases from '@admin/pages/publication/components/PublicationDraftReleases';
import PublicationScheduledReleases from '@admin/pages/publication/components/PublicationScheduledReleases';
import publicationQueries from '@admin/queries/publicationQueries';
import LoadingSpinner from '@common/components/LoadingSpinner';
import WarningMessage from '@common/components/WarningMessage';
import { useQuery } from '@tanstack/react-query';
import React, { useEffect, useMemo } from 'react';

interface Props {
  publicationId: string;
  onAmendmentDelete?: () => void;
  setVisibleCount?: (count: number) => void; // reports CURRENT visible count for this section
  showBackToTopLink?: boolean;
}

export default function PublicationUnpublishedReleases({
  publicationId,
  onAmendmentDelete,
  setVisibleCount,
  showBackToTopLink,
}: Props) {
  const {
    data: releases,
    isLoading,
    isSuccess,
    refetch,
  } = useQuery(
    publicationQueries.listUnpublishedReleaseVersions(publicationId),
  );

  const draftReleases = useMemo(() => {
    return (
      releases?.results.filter(
        release =>
          (release.approvalStatus === 'Draft' ||
            release.approvalStatus === 'HigherLevelReview') &&
          release.permissions?.canViewReleaseVersion, // We don't display draft releases they have no permission to view
      ) ?? []
    );
  }, [releases?.results]);

  const scheduledReleases = useMemo(() => {
    return (
      releases?.results.filter(
        release =>
          release.approvalStatus === 'Approved' &&
          release.permissions?.canViewReleaseVersion, // We don't display scheduled releases they have no permission to view
      ) ?? []
    );
  }, [releases?.results]);
  const visibleCount = draftReleases.length + scheduledReleases.length;

  useEffect(() => {
    setVisibleCount?.(visibleCount);
  }, [setVisibleCount, visibleCount]);

  return (
    <>
      <h3>Scheduled releases</h3>

      <LoadingSpinner
        loading={isLoading}
        text="Loading scheduled releases"
        hideText
      >
        <section className="govuk-!-margin-bottom-8">
          {isSuccess ? (
            <PublicationScheduledReleases
              publicationId={publicationId}
              releases={scheduledReleases}
              showBackToTopLink={showBackToTopLink}
            />
          ) : (
            <WarningMessage>
              There was a problem loading the scheduled releases.
            </WarningMessage>
          )}
        </section>
      </LoadingSpinner>

      <h3>Draft releases</h3>

      <LoadingSpinner
        loading={isLoading}
        text="Loading draft releases"
        hideText
      >
        <section className="govuk-!-margin-bottom-8">
          {isSuccess ? (
            <PublicationDraftReleases
              publicationId={publicationId}
              releases={draftReleases}
              onAmendmentDelete={async () => {
                await refetch();
                onAmendmentDelete?.();
              }}
              showBackToTopLink={showBackToTopLink}
            />
          ) : (
            <WarningMessage>
              There was a problem loading the draft releases.
            </WarningMessage>
          )}
        </section>
      </LoadingSpinner>
    </>
  );
}
