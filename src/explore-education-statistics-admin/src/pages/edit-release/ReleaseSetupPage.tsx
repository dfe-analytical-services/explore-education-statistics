import DummyReferenceData from '@admin/pages/DummyReferenceData';
import {ReleaseSetupDetails} from "@admin/services/edit-release/setup/types";
import FormattedDate from '@common/components/FormattedDate';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import React, { useEffect, useState } from 'react';
import { RouteComponentProps } from 'react-router';
import { setupEditRoute } from '@admin/routes/releaseRoutes';
import ReleasePageTemplate from '@admin/pages/edit-release/components/ReleasePageTemplate';
import service from '@admin/services/edit-release/setup/service';
import {
  dayMonthYearIsComplete,
  dayMonthYearToDate,
} from '@admin/services/common/types/types';
import Link from '@admin/components/Link';

interface MatchProps {
  releaseId: string;
}

const ReleaseSetupPage = ({ match }: RouteComponentProps<MatchProps>) => {
  const { releaseId } = match.params;

  const [releaseSetupDetails, setReleaseSetupDetails] = useState<
    ReleaseSetupDetails
  >();

  useEffect(() => {
    service.getReleaseSetupDetails(releaseId).then(setReleaseSetupDetails);
  }, [releaseId]);

  const getSelectedTimePeriodCoverageLabel = (timePeriodCoverageCode: string) =>
      DummyReferenceData.findTimePeriodCoverageOption(timePeriodCoverageCode).label;

  return (
    <>
      <ReleasePageTemplate
        releaseId={releaseId}
        publicationTitle={
          releaseSetupDetails ? releaseSetupDetails.publicationTitle : ''
        }
      >
        {releaseSetupDetails && (
          <SummaryList>
            <SummaryListItem term="Publication title">
              {releaseSetupDetails.publicationTitle}
            </SummaryListItem>
            <SummaryListItem term="Time period">
              {getSelectedTimePeriodCoverageLabel(releaseSetupDetails.timePeriodCoverageCode)}
            </SummaryListItem>
            <SummaryListItem term="Release period">
              {releaseSetupDetails.timePeriodCoverageStartDate.year && (
                <>
                  <time>{releaseSetupDetails.timePeriodCoverageStartDate.year}</time>
                  {' '}to{' '}
                  {releaseSetupDetails.timePeriodCoverageStartDate.year
                    ? (releaseSetupDetails.timePeriodCoverageStartDate.year + 1).toString()
                    : ''
                  }
                </>
              )}
            </SummaryListItem>
            <SummaryListItem term="Lead statistician">
              {releaseSetupDetails.leadStatisticianName}
            </SummaryListItem>
            <SummaryListItem term="Scheduled release">
              {dayMonthYearIsComplete(
                releaseSetupDetails.scheduledPublishDate,
              ) && (
                <FormattedDate>
                  {dayMonthYearToDate(releaseSetupDetails.scheduledPublishDate)}
                </FormattedDate>
              )}
            </SummaryListItem>
            <SummaryListItem term="Next release expected">
              {dayMonthYearIsComplete(
                releaseSetupDetails.nextReleaseExpectedDate,
              ) && (
                <FormattedDate>
                  {dayMonthYearToDate(releaseSetupDetails.nextReleaseExpectedDate)}
                </FormattedDate>
              )}
            </SummaryListItem>
            <SummaryListItem term="Release type">
              {releaseSetupDetails.releaseType.label}
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
        )}
      </ReleasePageTemplate>
    </>
  );
};

export default ReleaseSetupPage;
