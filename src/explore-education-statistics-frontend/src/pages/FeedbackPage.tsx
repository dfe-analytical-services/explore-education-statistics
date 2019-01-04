import React, { Component } from 'react';
import { H1 } from '../components/Heading';

class FeedbackPage extends Component {
  public render() {
    return (
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <H1>Feedback</H1>
        </div>
      </div>
    );
  }
}

export default FeedbackPage;
