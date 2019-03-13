import React, { Component } from 'react';
import Page from '../components/Page';
import PageTitle from '../components/PageTitle';

class FeedbackPage extends Component {
  public render() {
    return (
      <Page breadcrumbs={[{ name: 'Feedback' }]}>
        <PageTitle title="Feedback" />

        <form>
          <h3>Your details</h3>
          <div className="govuk-form-group">
            <label
              className="govuk-label govuk-!-width-three-quarters"
              htmlFor="full-name"
            >
              Full name
            </label>
            <input
              className="govuk-input govuk-!-width-three-quarters"
              id="full-name"
              name="full-name"
              type="text"
            />
          </div>
          <div className="govuk-form-group">
            <label className="govuk-label" htmlFor="email">
              Email address
            </label>
            <span id="email-hint" className="govuk-hint">
              We’ll only use this to respond to your feedback
            </span>
            <input
              className="govuk-input"
              id="email"
              name="email"
              type="email"
              aria-describedby="email-hint"
            />
          </div>

          <h3>Your feedback</h3>
          <div className="govuk-form-group">
            <label className="govuk-label" htmlFor="title">
              Title
            </label>
            <input
              className="govuk-input"
              id="title"
              name="title"
              type="text"
            />
          </div>
          <div className="govuk-form-group">
            <label className="govuk-label" htmlFor="feedback">
              Can you provide more detail?
            </label>
            <span id="feedback-hint" className="govuk-hint">
              Do not include personal or financial information, like your
              National Insurance number or credit card details.
            </span>
            <textarea
              className="govuk-textarea"
              id="feedback"
              name="feedback"
              rows={5}
              aria-describedby="feedback-hint"
            />
          </div>
          <button type="submit" className="govuk-button">
            Submit feedback
          </button>
        </form>
      </Page>
    );
  }
}

export default FeedbackPage;
