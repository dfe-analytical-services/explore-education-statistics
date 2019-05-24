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
  editing?: boolean;
  years: string;
  lastEdited: Date;
  lastEditor: User;
  published: Date;
}

const DashboardRelease = ({
  title,
  tag,
  lead,
  isNew,
  isLatest,
  editing,
  years,
  lastEdited,
  lastEditor,
  published,
}: Props) => {
  return (
    <Details
      className="govuk-!-margin-bottom-0"
      summary={`${title} ${years} ${isLatest ? '(Latest release)' : ''}`}
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
        <div className="govuk-summary-list__row">
          <dt className="govuk-summary-list__key">Lead statistician</dt>
          <dd className="govuk-summary-list__value">
            {lead && <span>{lead}</span>}
            {!lead && <span>John Smith</span>}
          </dd>
          <dd className="govuk-summary-list__actions" />
        </div>
        <div className="govuk-summary-list__row">
          <dt className="govuk-summary-list__key">Last edited</dt>
          <dd className="govuk-summary-list__value  dfe-details-no-margin">
            {' '}
            {format(lastEdited, 'd MMMM yyyy')}
            {' at '}
            {format(lastEdited, 'HH:mm')} by <a href="#">{lastEditor.name}</a>
          </dd>
          <dd className="govuk-summary-list__actions">
            {!editing && (
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
