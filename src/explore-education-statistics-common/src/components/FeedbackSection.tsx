import React from 'react';

const feedbackFormUrl =
  'https://forms.office.com/Pages/ResponsePage.aspx?id=yXfS-grGoU2187O4s0qC-XMiKzsnr8xJoWM_DeGwIu9UNDJHOEJDRklTNVA1SDdLOFJITEwyWU1OQS4u';

export default function FeedbackSection() {
  return (
    <p>
      We are always developing the service and welcome feedback to help steer
      our work. If you have any feedback that you'd like to share, you can use
      our{' '}
      <a
        href={feedbackFormUrl}
        rel="noopener noreferrer nofollow"
        target="_blank"
        data-testid="feedback-link"
      >
        feedback form (opens in new tab)
      </a>{' '}
      to leave us any comments or questions.
    </p>
  );
}
