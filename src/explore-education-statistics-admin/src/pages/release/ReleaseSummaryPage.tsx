import ButtonLink from '@admin/components/ButtonLink';
import { useManageReleaseContext } from '@admin/pages/release/contexts/ManageReleaseContext';
import { summaryEditRoute } from '@admin/routes/releaseRoutes';
import permissionService from '@admin/services/permissionService';
import releaseService from '@admin/services/releaseService';
import FormattedDate from '@common/components/FormattedDate';
import Gate from '@common/components/Gate';
import LoadingSpinner from '@common/components/LoadingSpinner';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncRetry from '@common/hooks/useAsyncRetry';
import {
  formatPartialDate,
  isValidPartialDate,
} from '@common/utils/date/partialDate';
import React from 'react';

const ReleaseSummaryPage = () => {
  const { publication, releaseId } = useManageReleaseContext();

  const { value: summary, isLoading } = useAsyncRetry(
    () => releaseService.getReleaseSummary(releaseId),
    [releaseId],
  );

  return (
    <LoadingSpinner loading={isLoading}>
      <h2 className="govuk-heading-l">Release summary</h2>

      {summary ? (
        <>
          <p>
            These details will be shown to users to help identify this release.
          </p>

          <SummaryList>
            <SummaryListItem term="Publication title">
              {publication.title}
            </SummaryListItem>
            <SummaryListItem term="Time period">
              {summary.timePeriodCoverage.label}
            </SummaryListItem>
            <SummaryListItem term="Release period">
              <time>{summary.yearTitle}</time>
            </SummaryListItem>
            <SummaryListItem term="Lead statistician">
              {publication.contact && publication.contact.contactName}
            </SummaryListItem>
            <SummaryListItem term="Scheduled release">
              {summary.publishScheduled ? (
                <FormattedDate>{summary.publishScheduled}</FormattedDate>
              ) : (
                'Not scheduled'
              )}
            </SummaryListItem>
            <SummaryListItem term="Next release expected">
              {isValidPartialDate(summary.nextReleaseDate) ? (
                <time>{formatPartialDate(summary.nextReleaseDate)}</time>
              ) : (
                'Not set'
              )}
            </SummaryListItem>
            <SummaryListItem term="Release type">
              {summary.type.title}
            </SummaryListItem>
          </SummaryList>

          <Gate condition={() => permissionService.canUpdateRelease(releaseId)}>
            <ButtonLink
              to={summaryEditRoute.generateLink({
                publicationId: publication.id,
                releaseId,
              })}
            >
              Edit release summary
            </ButtonLink>
          </Gate>
        </>
      ) : (
        <WarningMessage>Could not load release summary</WarningMessage>
      )}
    </LoadingSpinner>
  );
};

export default ReleaseSummaryPage;
