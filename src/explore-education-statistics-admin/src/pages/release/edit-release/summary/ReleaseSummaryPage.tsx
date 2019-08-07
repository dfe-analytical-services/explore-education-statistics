import Link from '@admin/components/Link';
import DummyReferenceData from '@admin/pages/DummyReferenceData';
import { setupEditRoute } from '@admin/routes/edit-release/routes';
import {
  dayMonthYearIsComplete,
  dayMonthYearToDate,
} from '@admin/services/common/types';
import service from '@admin/services/release/edit-release/summary/service';
import { ReleaseSummaryDetails } from '@admin/services/release/types';
import FormattedDate from '@common/components/FormattedDate';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import React, { useEffect, useState } from 'react';
import { RouteComponentProps } from 'react-router';
import ReleasePageTemplate from '../components/ReleasePageTemplate';

interface MatchProps {
  releaseId: string;
}

const ReleaseSummaryPage = ({ match }: RouteComponentProps<MatchProps>) => {
  const { releaseId } = match.params;

  const [releaseSummaryDetails, setReleaseSummaryDetails] = useState<
    ReleaseSummaryDetails
  >();

  useEffect(() => {
    service.getReleaseSummaryDetails(releaseId).then(setReleaseSummaryDetails);
  }, [releaseId]);

  const getSelectedTimePeriodCoverageLabel = (timePeriodCoverageCode: string) =>
    DummyReferenceData.findTimePeriodCoverageOption(timePeriodCoverageCode)
      .label;

  return (
    <>
      {releaseSummaryDetails && (
        <ReleasePageTemplate
          releaseId={releaseId}
          publicationTitle={releaseSummaryDetails.publicationTitle}
        >
          <SummaryList>
            <SummaryListItem term="Publication title">
              {releaseSummaryDetails.publicationTitle}
            </SummaryListItem>
            <SummaryListItem term="Time period">
              {getSelectedTimePeriodCoverageLabel(
                releaseSummaryDetails.timePeriodCoverageCode,
              )}
            </SummaryListItem>
            <SummaryListItem term="Release period">
              <time>{releaseSummaryDetails.timePeriodCoverageStartYear}</time>{' '}
              to{' '}
              <time>
                {releaseSummaryDetails.timePeriodCoverageStartYear + 1}
              </time>
            </SummaryListItem>
            <SummaryListItem term="Lead statistician">
              {releaseSummaryDetails.leadStatisticianName}
            </SummaryListItem>
            <SummaryListItem term="Scheduled release">
              {dayMonthYearIsComplete(
                releaseSummaryDetails.scheduledPublishDate,
              ) && (
                <FormattedDate>
                  {dayMonthYearToDate(
                    releaseSummaryDetails.scheduledPublishDate,
                  )}
                </FormattedDate>
              )}
            </SummaryListItem>
            <SummaryListItem term="Next release expected">
              {dayMonthYearIsComplete(
                releaseSummaryDetails.nextReleaseExpectedDate,
              ) && (
                <FormattedDate>
                  {dayMonthYearToDate(
                    releaseSummaryDetails.nextReleaseExpectedDate,
                  )}
                </FormattedDate>
              )}
            </SummaryListItem>
            <SummaryListItem term="Release type">
              {releaseSummaryDetails.releaseType.title}
            </SummaryListItem>
            <SummaryListItem
              term=""
              actions={
                <Link to={setupEditRoute.generateLink(releaseId)}>
                  Edit release setup details
                </Link>
              }
            />
          </SummaryList>
        </ReleasePageTemplate>
      )}
    </>
  );
};

export default ReleaseSummaryPage;
