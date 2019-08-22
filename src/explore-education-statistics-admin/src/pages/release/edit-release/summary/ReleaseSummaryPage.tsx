import Link from '@admin/components/Link';
import DummyReferenceData from '@admin/pages/DummyReferenceData';
import ManageReleaseContext, {
  ManageRelease,
} from '@admin/pages/release/ManageReleaseContext';
import { summaryEditRoute } from '@admin/routes/edit-release/routes';
import commonService from '@admin/services/common/service';
import {
  dayMonthYearIsComplete,
  dayMonthYearToDate,
  IdTitlePair,
} from '@admin/services/common/types';
import service from '@admin/services/release/edit-release/summary/service';
import { ReleaseSummaryDetails } from '@admin/services/release/types';
import FormattedDate from '@common/components/FormattedDate';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import React, { useContext, useEffect, useState } from 'react';

const ReleaseSummaryPage = () => {
  const [releaseSummaryDetails, setReleaseSummaryDetails] = useState<
    ReleaseSummaryDetails
  >();

  const [releaseTypes, setReleaseTypes] = useState<IdTitlePair[]>();

  const { publication, releaseId } = useContext(
    ManageReleaseContext,
  ) as ManageRelease;

  useEffect(() => {
    const releaseSummaryPromise = service.getReleaseSummaryDetails(releaseId);
    const releaseTypesPromise = commonService.getReleaseTypes();
    Promise.all([releaseSummaryPromise, releaseTypesPromise]).then(
      ([releaseSummaryResult, releaseTypesResult]) => {
        setReleaseSummaryDetails(releaseSummaryResult);
        setReleaseTypes(releaseTypesResult);
      },
    );
  }, [releaseId]);

  const getSelectedTimePeriodCoverageLabel = (timePeriodCoverageCode: string) =>
    DummyReferenceData.findTimePeriodCoverageOption(timePeriodCoverageCode)
      .label;

  const getSelectedReleaseType = (
    releaseTypeId: string,
    availableReleaseTypes: IdTitlePair[],
  ) =>
    availableReleaseTypes.find(type => type.id === releaseTypeId) ||
    availableReleaseTypes[0];

  return (
    <>
      <h2 className="govuk-heading-m">Release summary</h2>
      <p>These details will be shown to users to help identify this release.</p>
      {releaseSummaryDetails && releaseTypes && (
        <SummaryList>
          <SummaryListItem term="Publication title">
            {publication.title}
          </SummaryListItem>
          <SummaryListItem term="Time period">
            {getSelectedTimePeriodCoverageLabel(
              releaseSummaryDetails.timePeriodCoverage.value,
            )}
          </SummaryListItem>
          <SummaryListItem term="Release period">
            <time>{releaseSummaryDetails.releaseName}</time> to{' '}
            <time>{parseInt(releaseSummaryDetails.releaseName, 10) + 1}</time>
          </SummaryListItem>
          <SummaryListItem term="Lead statistician">
            {publication.contact && publication.contact.contactName}
          </SummaryListItem>
          <SummaryListItem term="Scheduled release">
            <FormattedDate>
              {new Date(releaseSummaryDetails.publishScheduled)}
            </FormattedDate>
          </SummaryListItem>
          <SummaryListItem term="Next release expected">
            {dayMonthYearIsComplete(releaseSummaryDetails.nextReleaseDate) && (
              <FormattedDate>
                {dayMonthYearToDate(releaseSummaryDetails.nextReleaseDate)}
              </FormattedDate>
            )}
          </SummaryListItem>
          <SummaryListItem term="Release type">
            {
              getSelectedReleaseType(releaseSummaryDetails.typeId, releaseTypes)
                .title
            }
          </SummaryListItem>
          <SummaryListItem
            term=""
            actions={
              <Link
                to={summaryEditRoute.generateLink(publication.id, releaseId)}
              >
                Edit release setup details
              </Link>
            }
          />
        </SummaryList>
      )}
    </>
  );
};

export default ReleaseSummaryPage;
