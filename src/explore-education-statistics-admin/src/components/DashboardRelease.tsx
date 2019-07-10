import FormattedDate from '@common/components/FormattedDate';
import React from 'react';
import Link from '@admin/components/Link';
import { format } from 'date-fns';
import Details from '@common/components/Details';
import { LoginContext } from '@admin/components/Login';
import { ApprovalStatus, Release } from '@admin/services/types/types';
import { setupRoute } from '@admin/routes/releaseRoutes';

const getLiveLatestLabel = (isLive: boolean, isLatest: boolean) => {
  if (isLive && isLatest) {
    return '(Live - Latest release)';
  }
  if (isLive) {
    return '(Live)';
  }
  return undefined;
};

const getTag = (approvalStatus: ApprovalStatus) => {
  if (ApprovalStatus.ReadyToReview === approvalStatus) {
    return 'Ready to review';
  }
  return undefined;
};

interface Props {
  release: Release;
  editing: boolean;
  review: boolean;
  showComments: boolean;
}

const DashboardRelease = ({
  release,
  editing,
  review,
  showComments,
}: Props) => {
  return (
    <Details
      className="govuk-!-margin-bottom-0"
      summary={`${release.timePeriodCoverage.label}, ${
        release.releaseName
      } ${getLiveLatestLabel(release.status.isLive, release.status.isLatest)}`}
      tag={getTag(release.status.approvalStatus)}
    >
      <dl className="govuk-summary-list govuk-!-margin-bottom-3">
        <div className="govuk-summary-list__row">
          {release.status.isNew && (
            <>
              <dt className="govuk-summary-list__key">
                Scheduled publish date
              </dt>
              <dd className="govuk-summary-list__value">
                <FormattedDate>{release.status.published}</FormattedDate>
              </dd>
              <dd className="govuk-summary-list__actions">
                <Link to="/prototypes/publication-create-new-absence-status">
                  Set status
                </Link>
              </dd>
            </>
          )}
          {!release.status.isNew && (
            <>
              <dt className="govuk-summary-list__key">Publish date</dt>
              <dd className="govuk-summary-list__value">
                <FormattedDate>{release.status.published}</FormattedDate>
              </dd>
              <dd className="govuk-summary-list__actions" />
            </>
          )}
        </div>
        {release.status.nextRelease && (
          <>
            <dt className="govuk-summary-list__key">Next release</dt>
            <dd className="govuk-summary-list__value">
              <FormattedDate>{release.status.nextRelease}</FormattedDate>
            </dd>
            <dd className="govuk-summary-list__actions" />
          </>
        )}
        <div className="govuk-summary-list__row">
          <dt className="govuk-summary-list__key">Lead statistician</dt>
          <dd className="govuk-summary-list__value">
            {release.lead && (
              <span>
                {release.lead.name}
                <br />
                <a href="mailto:{lead.teamEmail}">{release.lead.email}</a>
                <br />
                {release.lead.telNo}
              </span>
            )}
          </dd>
          <dd className="govuk-summary-list__actions" />
        </div>
        {release.dataType && (
          <div className="govuk-summary-list__row">
            <dt className="govuk-summary-list__key">Data type</dt>
            <dd className="govuk-summary-list__value">
              {release.dataType.title}
            </dd>
            <dd className="govuk-summary-list__actions" />
          </div>
        )}
        {showComments && release.comments && (
          <div className="govuk-summary-list__row">
            <dt className="govuk-summary-list__key">Comments</dt>
            <dd className="govuk-summary-list__value">
              {release.comments.map(comment => (
                <Details
                  key={`${release.id}-comments-${comment.id}`}
                  summary={`${comment.author.name}, ${format(
                    comment.datetime,
                    'dd MMMM yyyy, HH:mm',
                  )}`}
                  className="govuk-!-margin-bottom-0"
                >
                  <p>{comment.content}</p>
                </Details>
              ))}
            </dd>
            <dd className="govuk-summary-list__actions" />
          </div>
        )}
        <div className="govuk-summary-list__row">
          <dt className="govuk-summary-list__key">Last edited</dt>
          <dd className="govuk-summary-list__value  dfe-details-no-margin">
            <LoginContext.Consumer>
              {loginContext => {
                const editorName =
                  loginContext.user != null &&
                  loginContext.user.id === release.status.lastEditor.id
                    ? 'me'
                    : release.status.lastEditor.name;

                return (
                  <>
                    {' '}
                    <FormattedDate>{release.status.lastEdited}</FormattedDate>
                    {' at '}
                    {format(release.status.lastEdited, 'HH:mm')} by{' '}
                    <a href="#">{editorName}</a>
                  </>
                );
              }}
            </LoginContext.Consumer>
          </dd>
          <dd className="govuk-summary-list__actions">
            {review && (
              <Link to="/prototypes/publication-review">
                Review this release
              </Link>
            )}
            {!editing && !review && (
              <Link to={setupRoute.generateLink(release.id)}>
                Edit this release
              </Link>
            )}
            {editing && (
              <Link to="/prototypes/publication-create-new-absence-config">
                View / edit this draft
              </Link>
            )}
          </dd>
        </div>
      </dl>
    </Details>
  );
};

export default DashboardRelease;
