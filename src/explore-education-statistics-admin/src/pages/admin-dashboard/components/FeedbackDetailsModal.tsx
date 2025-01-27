import React from 'react';
import ButtonText from '@common/components/ButtonText';
import Modal from '@common/components/Modal';
import { FeedbackViewModel } from '@common/services/types/feedback';
import FormattedDate from '@common/components/FormattedDate';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import { getResponseText } from '@admin/pages/bau/FeedbackPage';

interface Props {
  feedback: FeedbackViewModel;
}

export default function FeedbackDetailsModal({ feedback }: Props) {
  return (
    <Modal
      title="Feedback details"
      showClose
      triggerButton={
        <ButtonText className="dfe-white-space--nowrap">
          View details
        </ButtonText>
      }
    >
      <SummaryList>
        <SummaryListItem term="Date">
          <FormattedDate format="d MMM yyyy, HH:mm">
            {feedback.created}
          </FormattedDate>
        </SummaryListItem>
        <SummaryListItem term="URL">{feedback.url}</SummaryListItem>
        <SummaryListItem term="Response">
          {getResponseText(feedback.response)}
        </SummaryListItem>
        {feedback.response !== 'Useful' && (
          <>
            <SummaryListItem term="What were you doing?">
              {feedback.context ?? '-'}
            </SummaryListItem>
            <SummaryListItem term="What went wrong?">
              {feedback.issue ?? '-'}
            </SummaryListItem>
            <SummaryListItem term="What did you hope to achieve?">
              {feedback.intent ?? '-'}
            </SummaryListItem>
          </>
        )}
        <SummaryListItem term="User agent">
          {feedback.userAgent}
        </SummaryListItem>
      </SummaryList>
    </Modal>
  );
}
