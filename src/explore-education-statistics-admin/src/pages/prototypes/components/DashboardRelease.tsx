import React from 'react';
// import {User} from "@admin/services/PrototypeLoginService";
import Link from '@admin/components/Link';
import { format } from 'date-fns';
import { User } from '@admin/services/PrototypeLoginService';
import Details from '@common/components/Details';

interface Props {
  title?: string;
  tag?: string;
  lead?: string;
  isNew?: boolean;
  isLatest?: boolean;
  isLive?: boolean;
  editing?: boolean;
  review?: boolean;
  years: string;
  lastEdited: Date;
  lastEditor: User;
  published: Date;
  nextRelease?: Date;
  dataType?: string;
}

const DashboardRelease = ({
  title,
  tag,
  lead,
  isNew,
  isLatest,
  isLive,
  editing,
  review,
  years,
  lastEdited,
  lastEditor,
  published,
  nextRelease,
  dataType,
}: Props) => {
  return (
    <Details
      className="govuk-!-margin-bottom-0"
      summary={`${title} ${years} ${isLive ? '(Live)' : ''} ${
        isLatest ? '(Live - Latest release)' : ''
      }`}
      tag={tag}
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
              <dt className="govuk-summary-list__key">Published to live</dt>
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
                {lead}, email:{' '}
                <a href="mailto:email@example.com">email@example.com</a>, tel:
                07654 653762
              </span>
            )}
            {!lead && (
              <span>
                John Smith, email:{' '}
                <a href="mailto:js@example.com">js@example.com</a>, tel: 07654
                653763
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
        <div className="govuk-summary-list__row">
          <dt className="govuk-summary-list__key">Last edited</dt>
          <dd className="govuk-summary-list__value  dfe-details-no-margin">
            {' '}
            {format(lastEdited, 'd MMMM yyyy')}
            {' at '}
            {format(lastEdited, 'HH:mm')} by <a href="#">{lastEditor.name}</a>
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
