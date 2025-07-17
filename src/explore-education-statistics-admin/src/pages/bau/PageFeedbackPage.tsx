import Page from '@admin/components/Page';
import LoadingSpinner from '@common/components/LoadingSpinner';
import React from 'react';
import FormattedDate from '@common/components/FormattedDate';
import { useQuery } from '@tanstack/react-query';
import pageFeedbackQueries from '@admin/queries/pageFeedbackQueries';
import pageFeedbackService from '@admin/services/pageFeedbackService';
import ButtonText from '@common/components/ButtonText';
import InsetText from '@common/components/InsetText';
import ButtonLink from '@admin/components/ButtonLink';
import useQueryParams from '@admin/hooks/useQueryParams';
import { PageFeedbackViewModel } from '@common/services/types/pageFeedback';
import truncateAround from '@common/utils/string/truncateAround';
import PageFeedbackDetailsModal from '../admin-dashboard/components/PageFeedbackDetailsModal';

type Params = {
  showRead?: string;
};

export const getResponseText = (
  response: PageFeedbackViewModel['response'],
) => {
  switch (response) {
    case 'Useful':
      return 'Useful';
    case 'NotUseful':
      return 'Not useful';
    case 'ProblemEncountered':
      return 'Problem encountered';
    default:
      return '';
  }
};

const PageFeedbackPage = () => {
  const { showRead } = useQueryParams<Params>();

  const {
    data: feedbackItems = [],
    isLoading: isLoadingFeedback,
    refetch: reloadFeedbackItems,
  } = useQuery(pageFeedbackQueries.getFeedback(showRead ?? ''));

  const toggleReadStatus = async (id: string) => {
    await pageFeedbackService.toggleReadStatus(id);
    reloadFeedbackItems();
  };

  const truncateTextOrBlank = (text: string) => {
    return text
      ? truncateAround(text, 0, {
          startTruncateLength: 200,
          endTruncateLength: 200,
        })
      : '-';
  };

  return (
    <Page
      title="Feedback"
      caption="View feedback"
      wide
      breadcrumbs={[
        { name: 'Platform administration', link: '/administration' },
        { name: 'Feedback' },
      ]}
    >
      <LoadingSpinner loading={isLoadingFeedback} text="Loading feedback">
        <ButtonLink
          to={{ search: showRead ? '' : 'showRead=true' }}
          onClick={() => reloadFeedbackItems()}
        >
          {showRead ? 'Hide read' : 'Show all'}
        </ButtonLink>

        {feedbackItems.length === 0 ? (
          <InsetText>No feedback found</InsetText>
        ) : (
          <table>
            <thead>
              <tr>
                <th scope="col">Date</th>
                <th scope="col">URL</th>
                <th scope="col">Response</th>
                <th scope="col">What were you doing?</th>
                <th scope="col">What went wrong?</th>
                <th scope="col">What did you hope to achieve?</th>
                <th scope="col">Actions</th>
              </tr>
            </thead>
            <tbody className="govuk-!-font-size-16">
              {feedbackItems
                .filter(feedback => (showRead ? true : !feedback.read))
                .map(feedback => {
                  return (
                    <tr key={feedback.id}>
                      <td>
                        <FormattedDate format="d MMM yyyy, HH:mm">
                          {feedback.created}
                        </FormattedDate>
                      </td>
                      <td>{feedback.url}</td>
                      <td className="dfe-white-space--nowrap">
                        {getResponseText(feedback.response)}
                      </td>
                      <td className="dfe-white-space--pre-wrap">
                        {truncateTextOrBlank(feedback.context)}
                      </td>
                      <td className="dfe-white-space--pre-wrap">
                        {truncateTextOrBlank(feedback.issue)}
                      </td>
                      <td className="dfe-white-space--pre-wrap">
                        {truncateTextOrBlank(feedback.intent)}
                      </td>
                      <td>
                        <ButtonText
                          className="dfe-white-space--nowrap govuk-!-margin-bottom-2"
                          onClick={() => toggleReadStatus(feedback.id)}
                        >
                          Mark as {feedback.read ? 'unread' : 'read'}
                        </ButtonText>
                        <br />
                        <PageFeedbackDetailsModal feedback={feedback} />
                      </td>
                    </tr>
                  );
                })}
            </tbody>
          </table>
        )}
      </LoadingSpinner>
    </Page>
  );
};

export default PageFeedbackPage;
