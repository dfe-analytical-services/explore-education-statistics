import { User } from '@admin/services/sign-in/types';
import React from 'react';
import Link from '@admin/components/Link';
import { format } from 'date-fns';
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
  showComments?: boolean;
  task?: string;
}

const PrototypeDashboardRelease = ({
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
  showComments,
  task,
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
            <>
              <dt className="govuk-summary-list__key">
                Scheduled publish date
              </dt>
              <dd className="govuk-summary-list__value">
                {format(published, 'd MMMM yyyy')}
              </dd>
              <dd className="govuk-summary-list__actions" />
            </>
          )}
          {!isNew && (
            <>
              <dt className="govuk-summary-list__key">Publish date</dt>
              <dd className="govuk-summary-list__value">
                {format(published, 'd MMMM yyyy')}
              </dd>
              <dd className="govuk-summary-list__actions" />
            </>
          )}
        </div>
        {nextRelease && (
          <>
            <dt className="govuk-summary-list__key">Expected next release</dt>
            <dd className="govuk-summary-list__value">
              {format(nextRelease, 'd MMMM yyyy')}
            </dd>
            <dd className="govuk-summary-list__actions" />
          </>
        )}
        <div className="govuk-summary-list__row">
          <dt className="govuk-summary-list__key">
            Publication and release contact
          </dt>
          <dd className="govuk-summary-list__value">
            {lead && (
              <span>
                {lead}
                <br />
                <a href="mailto:email@example.com">email@example.com</a>
                <br />
                07654 653762
              </span>
            )}
            {!lead && (
              <span>
                John Smith
                <br />
                <a href="mailto:js@example.com">js@example.com</a>
                <br />
                07654 653763
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
              <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
                In review
              </h3>
              <Details
                summary="Ann Evans, 17 June 2018, 17:35"
                className="govuk-!-margin-bottom-0"
              >
                <Link to="/prototypes/publication-review">
                  General comment or question
                </Link>
                <p>
                  Lorem ipsum, dolor sit amet consectetur adipisicing elit.
                  Fugit rem, optio sunt dolorum corrupti harum labore quia
                  repellat! Quae voluptatem illo soluta optio ducimus at
                  possimus quisquam doloremque veritatis provident!
                </p>
              </Details>
              <Details summary="John Smith, 17 June 2018, 13:15">
                <Link to="/prototypes/publication-review">
                  Section comment: About this release
                </Link>
                <p>
                  Corrupti harum labore quia repellat! Quae voluptatem illo
                  soluta optio ducimus at possimus quisquam doloremque veritatis
                  provident!
                </p>
              </Details>
              <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
                Final sign-off
              </h3>
              <Details
                summary="Stephen Doherty, 17 June 2018, 17:35"
                className="govuk-!-margin-bottom-0"
              >
                <Link to="/prototypes/publication-review">
                  General comment or question
                </Link>
                <p>
                  Lorem ipsum, dolor sit amet consectetur adipisicing elit.
                  Fugit rem, optio sunt dolorum corrupti harum labore quia
                  repellat! Quae voluptatem illo soluta optio ducimus at
                  possimus quisquam doloremque veritatis provident!
                </p>
              </Details>
            </dd>
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
          <dd className="govuk-summary-list__actions" />
        </div>
      </dl>
      {!editing && !review && (
        <Link to="/prototypes/publication-edit" className="govuk-button">
          Edit this release
        </Link>
      )}
      {editing && (
        <Link
          to="/prototypes/publication-create-new-absence-config"
          className="govuk-button"
        >
          View / edit this draft
        </Link>
      )}
      {task === 'resolveComments' && (
        <Link
          to="/prototypes/publication-unresolved-comments"
          className="govuk-button"
        >
          View release and resolve comments
        </Link>
      )}
      {task === 'readyReview' && (
        <Link to="/prototypes/publication-review" className="govuk-button">
          View and review release
        </Link>
      )}
    </Details>
  );
};

export default PrototypeDashboardRelease;
