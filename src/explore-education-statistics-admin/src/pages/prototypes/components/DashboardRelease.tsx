import React from 'react';
// import {User} from "@admin/services/PrototypeLoginService";
import Link from '@admin/components/Link';
import { format } from 'date-fns';
import { User } from '@admin/services/PrototypeLoginService';
import Details from '@common/components/Details';

interface Props {
  title?: string;
  isNew?: boolean;
  isLatest?: boolean;
  editing?: boolean;
  years: string;
  lastEdited: Date;
  lastEditor: User;
}

const DashboardRelease = ({
  title,
  isNew,
  isLatest,
  editing,
  years,
  lastEdited,
  lastEditor,
}: Props) => {
  return (
    <Details
      summary={`${title} ${years} ${
        isLatest ? '(Latest release)' : '(Archived)'
      }`}
    >
      <dl className="govuk-summary-list">
        <div className="govuk-summary-list__row">
          <dt className="govuk-summary-list__key">Current status</dt>
          {isNew && (
            <React.Fragment>
              <dd className="govuk-summary-list__value">
                <span className="govuk-tag">New release in progress</span>
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
              <dd className="govuk-summary-list__value">
                {editing && (
                  <span className="govuk-tag">Editing in progress</span>
                )}{' '}
                Live {isLatest && <>(latest release)</>}
                {!isLatest && <>(archived release)</>}
              </dd>
              <dd className="govuk-summary-list__actions" />
            </React.Fragment>
          )}
        </div>
        <div className="govuk-summary-list__row">
          <dt className="govuk-summary-list__key">Release for academic year</dt>
          <dd className="govuk-summary-list__value">
            {years}
            {!isNew && editing && (
              <Link to="#" className="govuk-!-margin-left-3">
                Edit a previous release
              </Link>
            )}
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
        <div className="govuk-summary-list__row">
          <dt className="govuk-summary-list__key">Last edited</dt>
          <dd className="govuk-summary-list__value  dfe-details-no-margin">
            {' '}
            {format(lastEdited, 'd MMMM yyyy')}
            {' at '}
            {format(lastEdited, 'HH:mm')} by <a href="#">{lastEditor.name}</a>
          </dd>
          <dd className="govuk-summary-list__actions">
            {' '}
            <Link to="/prototypes/publication-create-new">
              Create new release
            </Link>
          </dd>
        </div>
      </dl>
      {!editing && (
        <Link
          to="/prototypes/publication-edit"
          className="govuk-button govuk-button--secondary"
        >
          Edit this release
        </Link>
      )}
      {editing && (
        <Link
          to="/prototypes/publication-create-new-absence-config"
          className="govuk-button govuk-button--secondary"
        >
          View / edit this draft
        </Link>
      )}
      {!isNew && editing && (
        <Link to="#" className="govuk-button govuk-button--secondary">
          Edit a previous release
        </Link>
      )}
    </Details>
  );
};

export default DashboardRelease;
