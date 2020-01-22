import LoginContext from '@admin/components/Login';
import {
  getReleaseStatusLabel,
  getReleaseSummaryLabel,
} from '@admin/pages/release/util/releaseSummaryUtil';
import {
  Comment,
  AdminDashboardRelease,
} from '@admin/services/dashboard/types';
import Details from '@common/components/Details';
import FormattedDate from '@common/components/FormattedDate';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import {
  dayMonthYearIsComplete,
  dayMonthYearToDate,
} from '@common/services/publicationService';
import { format } from 'date-fns';
import React, { ReactNode, useContext } from 'react';
import ReleaseServiceStatus from '@admin/components/ReleaseServiceStatus';

interface Props {
  release: AdminDashboardRelease;
  actions: ReactNode;
  children?: ReactNode;
}

const ReleaseSummary = ({ release, actions, children }: Props) => {
  const authentication = useContext(LoginContext);

  const editorName =
    authentication.user && authentication.user.id === release.lastEditedUser.id
      ? 'me'
      : release.lastEditedUser.name;

  return (
    <Details
      className="govuk-!-margin-bottom-0"
      summary={getReleaseSummaryLabel(release)}
      tag={[
        getReleaseStatusLabel(release.status),
        // eslint-disable-next-line react/jsx-key
        <ReleaseServiceStatus exclude="details" releaseId={release.id} />,
      ]}
    >
      <SummaryList additionalClassName="govuk-!-margin-bottom-3">
        <SummaryListItem term="Publish date">
          <FormattedDate>
            {release.published || release.publishScheduled}
          </FormattedDate>
        </SummaryListItem>
        <SummaryListItem term="Next release date">
          {dayMonthYearIsComplete(release.nextReleaseDate) && (
            <FormattedDate>
              {dayMonthYearToDate(release.nextReleaseDate)}
            </FormattedDate>
          )}
        </SummaryListItem>
        <SummaryListItem term="Release process status">
          <ReleaseServiceStatus releaseId={release.id} />
        </SummaryListItem>
        <SummaryListItem term="Lead statistician">
          {release.contact && (
            <span>
              {release.contact.contactName}
              <br />
              <a href="mailto:{lead.teamEmail}">{release.contact.teamEmail}</a>
              <br />
              {release.contact.contactTelNo}
            </span>
          )}
        </SummaryListItem>
        {(release.draftComments || release.higherReviewComments) && (
          <SummaryListItem term="Comments">
            <Comments
              heading="Draft comments"
              comments={release.draftComments}
            />
            <Comments
              heading="Responsible statistician comments"
              comments={release.higherReviewComments}
            />
          </SummaryListItem>
        )}

        <SummaryListItem term="Last edited" detailsNoMargin>
          <FormattedDate>{release.lastEditedDateTime}</FormattedDate>
          {' at '}
          {format(new Date(release.lastEditedDateTime), 'HH:mm')} by{' '}
          <a href="#">{editorName}</a>
        </SummaryListItem>
        {release.internalReleaseNote && (
          <SummaryListItem term="Internal release note">
            <span className="dfe-multiline-content">
              {release.internalReleaseNote}
            </span>
          </SummaryListItem>
        )}
      </SummaryList>
      {children}

      <div className="govuk-grid-row">
        <div className="govuk-grid-column-one-half">{actions}</div>
        <div className="govuk-grid-column-one-half dfe-align--right" />
      </div>
    </Details>
  );
};

interface CommentsProps {
  heading: string;
  comments: Comment[];
}

const Comments = ({ heading, comments }: CommentsProps) => {
  return (
    comments && (
      <>
        <h3 className="govuk-heading-s govuk-!-margin-bottom-0">{heading}</h3>
        {comments.map((comment, index) => (
          /* eslint-disable react/no-array-index-key */
          <Details
            key={index}
            summary={`${comment.authorName}, ${format(
              new Date(comment.createdDate),
              'dd MMMM yyyy, HH:mm',
            )}`}
            className={
              index < comments.length - 1
                ? 'govuk-!-margin-bottom-0'
                : undefined
            }
          >
            <span className="dfe-multiline-content">{comment.message}</span>
          </Details>
        ))}
      </>
    )
  );
};

export default ReleaseSummary;
