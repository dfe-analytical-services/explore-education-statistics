import ButtonLink from '@admin/components/ButtonLink';
import { useManageReleaseContext } from '@admin/pages/release/contexts/ManageReleaseContext';
import {
  ReleaseRouteParams,
  releaseSummaryEditRoute,
} from '@admin/routes/releaseRoutes';
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
import { parseISO } from 'date-fns';
import React from 'react';
import { generatePath } from 'react-router';

const ReleaseSummaryPage = () => {
  const { publication, releaseId } = useManageReleaseContext();

  const { value: release, isLoading } = useAsyncRetry(
    () => releaseService.getRelease(releaseId),
    [releaseId],
  );

  return (
    <LoadingSpinner loading={isLoading}>
      <h2 className="govuk-heading-l">Release summary</h2>

      {release ? (
        <>
          <p>
            These details will be shown to users to help identify this release.
          </p>

          <SummaryList>
            <SummaryListItem term="Publication title">
              {publication.title}
            </SummaryListItem>
            <SummaryListItem term="Time period">
              {release.timePeriodCoverage.label}
            </SummaryListItem>
            <SummaryListItem term="Release period">
              <time>{release.title}</time>
            </SummaryListItem>
            <SummaryListItem term="Lead statistician">
              {publication.contact && publication.contact.contactName}
            </SummaryListItem>
            <SummaryListItem term="Scheduled release">
              {release.publishScheduled ? (
                <FormattedDate>
                  {parseISO(release.publishScheduled)}
                </FormattedDate>
              ) : (
                'Not scheduled'
              )}
            </SummaryListItem>
            <SummaryListItem term="Next release expected">
              {isValidPartialDate(release.nextReleaseDate) ? (
                <time>{formatPartialDate(release.nextReleaseDate)}</time>
              ) : (
                'Not set'
              )}
            </SummaryListItem>
            <SummaryListItem term="Release type">
              {release.type.title}
            </SummaryListItem>
          </SummaryList>

          <Gate condition={() => permissionService.canUpdateRelease(releaseId)}>
            <ButtonLink
              to={generatePath<ReleaseRouteParams>(
                releaseSummaryEditRoute.path,
                {
                  publicationId: publication.id,
                  releaseId,
                },
              )}
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
