import React from 'react';
import Link from '@admin/components/Link';
import { format } from 'date-fns';
import Details from '@common/components/Details';
import { User } from '@admin/services/PrototypeLoginService';
import { LoginContext } from '@admin/components/Login';
import {
  ApprovalStatus,
  TeamContact,
} from '@admin/services/publicationService';

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
  releaseName: string;
  timePeriodCoverage: string;
  approvalStatus: ApprovalStatus;
  lead: TeamContact;
  isNew: boolean;
  isLatest: boolean;
  isLive: boolean;
  editing: boolean;
  review: boolean;
  lastEdited: Date;
  lastEditor: User;
  published: Date;
  nextRelease: Date;
  dataType: string;
  showComments: boolean;
}

const DashboardRelease = ({
  releaseName,
  timePeriodCoverage,
  approvalStatus,
  lead,
  isNew,
  isLatest,
  isLive,
  editing,
  review,
  lastEdited,
  lastEditor,
  published,
  nextRelease,
  dataType,
  showComments,
}: Props) => {
  return (
    <Details
      className="govuk-!-margin-bottom-0"
      summary={`${timePeriodCoverage}, ${releaseName} ${getLiveLatestLabel(
        isLive,
        isLatest,
      )}`}
      tag={getTag(approvalStatus)}
    >
      <dl className="govuk-summary-list govuk-!-margin-bottom-3">
        <div className="govuk-summary-list__row">
          {isNew && (
            <React.Fragment>
              <dt className="govuk-summary-list__key">
                Scheduled publish date
              </dt>
              <dd className="govuk-summary-list__value">
                {format(published, 'd MMMM yyyy')}
              </dd>
              <dd className="govuk-summary-list__actions">
                <Link to="/prototypes/publication-create-new-absence-status">
                  Set status
                </Link>
              </dd>
            </React.Fragment>
          )}
          {!isNew && (
            <React.Fragment>
              <dt className="govuk-summary-list__key">Publish date</dt>
              <dd className="govuk-summary-list__value">
                {format(published, 'd MMMM yyyy')}
              </dd>
              <dd className="govuk-summary-list__actions" />
            </React.Fragment>
          )}
        </div>
        {nextRelease && (
          <React.Fragment>
            <dt className="govuk-summary-list__key">Next release</dt>
            <dd className="govuk-summary-list__value">
              {format(nextRelease, 'd MMMM yyyy')}
            </dd>
            <dd className="govuk-summary-list__actions" />
          </React.Fragment>
        )}
        <div className="govuk-summary-list__row">
          <dt className="govuk-summary-list__key">Lead statistician</dt>
          <dd className="govuk-summary-list__value">
            {lead && (
              <span>
                {lead.contactName}
                <br />
                <a href="mailto:{lead.teamEmail}">{lead.teamEmail}</a>
                <br />
                {lead.contactTelNo}
              </span>
            )}
          </dd>
          <dd className="govuk-summary-list__actions" />
        </div>
        {dataType && (
          <div className="govuk-summary-list__row">
            <dt className="govuk-summary-list__key">Data type</dt>
            <dd className="govuk-summary-list__value">{dataType}</dd>
            <dd className="govuk-summary-list__actions" />
          </div>
        )}
        {showComments && (
          <div className="govuk-summary-list__row">
            <dt className="govuk-summary-list__key">Comments</dt>
            <dd className="govuk-summary-list__value">
              <Details
                summary="Ann Evans, 17 June 2018, 17:35"
                className="govuk-!-margin-bottom-0"
              >
                <p>
                  Lorem ipsum, dolor sit amet consectetur adipisicing elit.
                  Fugit rem, optio sunt dolorum corrupti harum labore quia
                  repellat! Quae voluptatem illo soluta optio ducimus at
                  possimus quisquam doloremque veritatis provident!
                </p>
              </Details>
              <Details summary="Stephen Doherty, 17 June 2018, 13:15">
                <p>
                  Corrupti harum labore quia repellat! Quae voluptatem illo
                  soluta optio ducimus at possimus quisquam doloremque veritatis
                  provident!
                </p>
              </Details>
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
                  loginContext.user.id === lastEditor.id
                    ? 'me'
                    : lastEditor.name;

                return (
                  <>
                    {' '}
                    {format(lastEdited, 'd MMMM yyyy')}
                    {' at '}
                    {format(lastEdited, 'HH:mm')} by{' '}
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
              <Link to="/prototypes/publication-edit">Edit this release</Link>
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
