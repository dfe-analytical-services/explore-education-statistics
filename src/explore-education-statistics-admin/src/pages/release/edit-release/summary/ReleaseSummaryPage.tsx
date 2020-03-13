import Link from '@admin/components/Link';
import { useManageReleaseContext } from '@admin/pages/release/ManageReleaseContext';
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
import permissionService from '@admin/services/permissions/service';
import service from '@admin/services/release/edit-release/summary/service';
import { ReleaseSummaryDetails } from '@admin/services/release/types';
import FormattedDate from '@common/components/FormattedDate';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import {
  dayMonthYearIsComplete,
  dayMonthYearToDate,
} from '@common/services/publicationService';
import React, { useEffect, useState } from 'react';

interface ReleaseSummaryModel {
  releaseSummaryDetails: ReleaseSummaryDetails;
  timePeriodCoverageGroups: TimePeriodCoverageGroup[];
  releaseTypes: IdTitlePair[];
  canEditRelease: boolean;
}

const ReleaseSummaryPage = () => {
  const [model, setModel] = useState<ReleaseSummaryModel>();

  const { publication, releaseId } = useManageReleaseContext();

  useEffect(() => {
    Promise.all([
      service.getReleaseSummaryDetails(releaseId),
      commonService.getReleaseTypes(),
      commonService.getTimePeriodCoverageGroups(),
      permissionService.canUpdateRelease(releaseId),
    ]).then(
      ([
        releaseSummaryDetails,
        releaseTypes,
        timePeriodCoverageGroups,
        canEditRelease,
      ]) => {
        setModel({
          releaseSummaryDetails,
          timePeriodCoverageGroups,
          releaseTypes,
          canEditRelease,
        });
      },
    );
  }, [releaseId]);

  return (
    <>
      <h2 className="govuk-heading-l">Release summary</h2>
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
          {model.canEditRelease && (
            <div className="dfe-align--right">
              <Link
                to={summaryEditRoute.generateLink(publication.id, releaseId)}
              >
                Edit release summary
              </Link>
            </div>
          )}
        </>
      )}
    </>
  );
};

export default ReleaseSummaryPage;
