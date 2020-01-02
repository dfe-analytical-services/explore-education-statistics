import Link from '@admin/components/Link';
import ManageReleaseContext, {
  ManageRelease,
} from '@admin/pages/release/ManageReleaseContext';
import {
  getSelectedTimePeriodCoverageLabel,
  getTimePeriodCoverageDateRangeStringLong,
} from '@admin/pages/release/util/releaseSummaryUtil';
import { summaryEditRoute } from '@admin/routes/edit-release/routes';
import commonService from '@admin/services/common/service';
import {
  IdTitlePair,
  TimePeriodCoverageGroup,
} from '@admin/services/common/types';
import service from '@admin/services/release/edit-release/summary/service';
import { ReleaseSummaryDetails } from '@admin/services/release/types';
import withErrorControl, {
  ErrorControlProps,
} from '@admin/validation/withErrorControl';
import FormattedDate from '@common/components/FormattedDate';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import {
  dayMonthYearIsComplete,
  dayMonthYearToDate,
} from '@common/services/publicationService';
import React, { useContext, useEffect, useState } from 'react';

interface ReleaseSummaryModel {
  releaseSummaryDetails: ReleaseSummaryDetails;
  timePeriodCoverageGroups: TimePeriodCoverageGroup[];
  releaseTypes: IdTitlePair[];
}

const ReleaseSummaryPage = ({ handleApiErrors }: ErrorControlProps) => {
  const [model, setModel] = useState<ReleaseSummaryModel>();

  const { publication, releaseId } = useContext(
    ManageReleaseContext,
  ) as ManageRelease;

  useEffect(() => {
    Promise.all([
      service.getReleaseSummaryDetails(releaseId),
      commonService.getReleaseTypes(),
      commonService.getTimePeriodCoverageGroups(),
    ])
      .then(
        ([
          releaseSummaryResult,
          releaseTypesResult,
          timePeriodGroupsResult,
        ]) => {
          setModel({
            releaseSummaryDetails: releaseSummaryResult,
            timePeriodCoverageGroups: timePeriodGroupsResult,
            releaseTypes: releaseTypesResult,
          });
        },
      )
      .catch(handleApiErrors);
  }, [releaseId, handleApiErrors]);

  return (
    <>
      <h2 className="govuk-heading-m">Release summary</h2>
      <p>These details will be shown to users to help identify this release.</p>
      {model && (
        <>
          <SummaryList>
            <SummaryListItem term="Publication title">
              {publication.title}
            </SummaryListItem>
            <SummaryListItem term="Time period">
              {getSelectedTimePeriodCoverageLabel(
                model.releaseSummaryDetails.timePeriodCoverage.value,
                model.timePeriodCoverageGroups,
              )}
            </SummaryListItem>
            <SummaryListItem term="Release period">
              <time>
                {getTimePeriodCoverageDateRangeStringLong(
                  model.releaseSummaryDetails.releaseName,
                )}
              </time>
            </SummaryListItem>
            <SummaryListItem term="Lead statistician">
              {publication.contact && publication.contact.contactName}
            </SummaryListItem>
            <SummaryListItem term="Scheduled release">
              <FormattedDate>
                {new Date(model.releaseSummaryDetails.publishScheduled)}
              </FormattedDate>
            </SummaryListItem>
            <SummaryListItem term="Next release expected">
              {dayMonthYearIsComplete(
                model.releaseSummaryDetails.nextReleaseDate,
              ) && (
                <FormattedDate>
                  {dayMonthYearToDate(
                    model.releaseSummaryDetails.nextReleaseDate,
                  )}
                </FormattedDate>
              )}
            </SummaryListItem>
            <SummaryListItem term="Release type">
              {model.releaseSummaryDetails.type.title}
            </SummaryListItem>
          </SummaryList>
          <div className="dfe-align--right">
            <Link to={summaryEditRoute.generateLink(publication.id, releaseId)}>
              Edit release summary
            </Link>
          </div>
        </>
      )}
    </>
  );
};

export default withErrorControl(ReleaseSummaryPage);
