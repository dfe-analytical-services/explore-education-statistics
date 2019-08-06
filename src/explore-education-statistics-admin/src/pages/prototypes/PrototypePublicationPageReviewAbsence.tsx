import React from 'react';
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

  const Level2ReviewForm = () => {
    return (
      <form action="/prototypes/admin-dashboard">
        <FormRadioGroup
          legend="Final sign-off release status"
          id="status"
          name="status"
          value={status}
          onChange={e => {
            setStatus(e.target.value);
          }}
          options={[
            {
              id: 'approve-release-higher-review',
              label: 'Approve for publication',
              value: 'approvedPublication',
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
              value: 'readyHigherReview',
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
        <button type="submit" className="govuk-button">
          Update
        </button>
        <div>
          <Link to="#">Print this page</Link>
        </div>
      </form>
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
      {reviewType === 'level1' && (
        <>
          <div className="govuk-width-container">
            <EditablePublicationPage
              higherReview
              data={PrototypePublicationService.getNewPublication()}
            />
          </div>
        </>
      )}
      {reviewType === 'level2' && (
        <>
          <Level2ReviewForm />
          <div className="govuk-width-container dfe-align--comments">
            <EditablePublicationPage
              higherReview
              reviewing
              data={PrototypePublicationService.getNewPublication()}
            />
          </div>
        </>
      )}
    </PrototypePage>
  );
};

export default PublicationPage;
