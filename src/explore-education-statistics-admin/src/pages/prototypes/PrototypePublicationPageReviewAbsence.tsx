import { Release } from '@common/services/publicationService';
import React, { Component } from 'react';
import { LoginContext } from '@admin/components/Login';
import { FormGroup, FormRadioGroup } from '@common/components/form';
import Link from '@admin/components/Link';
import PrototypePublicationService from '@admin/pages/prototypes/components/PrototypePublicationService';
import EditablePublicationPage from '@admin/pages/prototypes/components/EditablePublicationPage';
import PrototypePage from './components/PrototypePage';

const PublicationPage = () => {
  const [status, setStatus] = React.useState('');
  const userContext = React.useContext(LoginContext);

  const reviewType =
    userContext.user &&
    userContext.user.permissions.includes('responsible statistician')
      ? 'level2'
      : 'level1';

  const Level1ReviewForm = () => {
    return (
      <div>
        <FormRadioGroup
          legend="Release status"
          id="review-release"
          name="review-release"
          value={status}
          onChange={e => {
            setStatus(e.target.value);
          }}
          options={[
            {
              id: 'approve-release',
              label: 'Approve for higher review',
              value: 'approve-release',
              conditional: (
                <FormGroup>
                  <label htmlFor="approved-comments" className="govuk-label">
                    Add any extra comments
                  </label>
                  <textarea
                    name="approved-comments"
                    id="approved-comments"
                    className="govuk-textarea"
                  />
                </FormGroup>
              ),
            },

            {
              id: 'question',
              label: 'In review - add comments and questions',
              value: 'question',
              conditional: (
                <FormGroup>
                  <label htmlFor="question" className="govuk-label">
                    Add your comment or question
                  </label>
                  <textarea
                    name="question"
                    id="question"
                    className="govuk-textarea"
                  />
                </FormGroup>
              ),
            },
          ]}
        />

        <Link
          to="/prototypes/admin-dashboard?status=readyApproval"
          className="govuk-button"
        >
          Update
        </Link>
      </div>
    );
  };

  const Level2ReviewForm = () => {
    return (
      <div>
        <FormRadioGroup
          legend="Higher review release status"
          id="higher-review-release"
          name="rhigher-eview-release"
          value={status}
          onChange={e => {
            setStatus(e.target.value);
          }}
          options={[
            {
              id: 'approve-release-higher-review',
              label: 'Approve for publication',
              value: 'approve-release-higher-review',
              conditional: (
                <FormGroup>
                  <label htmlFor="approved-comments" className="govuk-label">
                    Add any extra comments
                  </label>
                  <textarea
                    name="approved-comments"
                    id="approved-comments"
                    className="govuk-textarea"
                  />
                </FormGroup>
              ),
            },

            {
              id: 'question',
              label:
                'In higher review - add comments and questions for author / team lead',
              value: 'question',
              conditional: (
                <FormGroup>
                  <label htmlFor="question" className="govuk-label">
                    Add your comment or question
                  </label>
                  <textarea
                    name="question"
                    id="question"
                    className="govuk-textarea"
                  />
                </FormGroup>
              ),
            },
          ]}
        />

        <Link
          to="/prototypes/admin-dashboard?status=readyApproval&amp;level=2"
          className="govuk-button"
        >
          Update
        </Link>
      </div>
    );
  };

  return (
    <PrototypePage
      wide
      breadcrumbs={[
        {
          link: '/prototypes/admin-dashboard?status=readyApproval',
          text: 'Administrator dashboard',
        },
        { text: 'Review release', link: '#' },
      ]}
    >
      {reviewType === 'level1' && <Level1ReviewForm />}
      {reviewType === 'level2' && <Level2ReviewForm />}

      <hr />
      <div className="govuk-width-container dfe-align--comments">
        <EditablePublicationPage
          reviewing
          data={PrototypePublicationService.getNewPublication()}
        />
      </div>
    </PrototypePage>
  );
};

export default PublicationPage;
