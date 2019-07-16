import DummyReferenceData from '@admin/pages/DummyReferenceData';
import FormattedDate from '@common/components/FormattedDate';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import React, { useEffect, useState } from 'react';
import { RouteComponentProps } from 'react-router';
import { setupEditRoute } from '@admin/routes/releaseRoutes';
import ReleasePageTemplate from '@admin/pages/edit-release/components/ReleasePageTemplate';
import DummyPublicationsData from '@admin/pages/DummyPublicationsData';
import {
  dayMonthYearIsComplete,
  dayMonthYearToDate,
  ReleaseSetupDetails,
} from '@admin/services/api/common/types/types';
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
    setReleaseSetupDetails(
      DummyPublicationsData.getReleaseSetupDetails(releaseId),
    );
  }, [releaseId]);

  const selectedTimePeriodCoverageGroup =
    releaseSetupDetails && releaseSetupDetails.timePeriodCoverageCode
      ? DummyReferenceData.findTimePeriodCoverageGroup(
          releaseSetupDetails.timePeriodCoverageCode,
        )
      : null;

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
              {selectedTimePeriodCoverageGroup &&
                selectedTimePeriodCoverageGroup.label}
            </SummaryListItem>
            <SummaryListItem term="Release period">
              <FormattedDate format="yyyy">
                {releaseSetupDetails.timePeriodCoverageStartDate}
              </FormattedDate>{' '}
              to{' '}
              <FormattedDate format="yyyy">
                {(
                  releaseSetupDetails.timePeriodCoverageStartDate.getFullYear() +
                  1
                ).toString()}
              </FormattedDate>
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
              {releaseSetupDetails.nextReleaseExpectedDate && (
                <FormattedDate>
                  {releaseSetupDetails.nextReleaseExpectedDate}
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
