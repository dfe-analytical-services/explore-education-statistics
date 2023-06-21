import PublicationDraftReleases from '@admin/pages/publication/components/PublicationDraftReleases';
import PublicationScheduledReleases from '@admin/pages/publication/components/PublicationScheduledReleases';
import publicationService from '@admin/services/publicationService';
import { ReleaseSummaryWithPermissions } from '@admin/services/releaseService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import WarningMessage from '@common/components/WarningMessage';
import { useQuery } from '@tanstack/react-query';
import React, { useMemo } from 'react';

interface Props {
  publicationId: string;
  onAmendmentDelete?: () => void;
}

export default function PublicationUnpublishedReleases({
  publicationId,
  onAmendmentDelete,
}: Props) {
  const { data: releases, isLoading, isSuccess, refetch } = useQuery(
    ['publicationUnpublishedReleases', publicationId],
    () =>
      publicationService.listReleases<ReleaseSummaryWithPermissions>(
        publicationId,
        {
          live: false,
          pageSize: 100,
          includePermissions: true,
        },
      ),
  );

  const draftReleases = useMemo(() => {
    return (
      releases?.results.filter(
        release =>
          (release.approvalStatus === 'Draft' ||
            release.approvalStatus === 'HigherLevelReview') &&
          release.permissions?.canViewRelease, // We don't display draft releases they have no permission to view
      ) ?? []
    );
  }, [releases?.results]);

  const scheduledReleases = useMemo(() => {
    return (
      releases?.results.filter(
        release =>
          release.approvalStatus === 'Approved' &&
          release.permissions?.canViewRelease, // We don't display scheduled releases they have no permission to view
      ) ?? []
    );
  }, [releases?.results]);

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
