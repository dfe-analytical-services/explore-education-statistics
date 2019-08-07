import React from 'react';
import { LoginContext } from '@admin/components/Login';
import { FormGroup, FormRadioGroup } from '@common/components/form';
import Link from '@admin/components/Link';
import PrototypePublicationService from '@admin/pages/prototypes/components/PrototypePublicationService';
import EditablePublicationPage from '@admin/pages/prototypes/components/EditablePublicationPage';
import PrototypePage from './components/PrototypePage';

interface Props {
  task?: string;
}

const PublicationPage = ({ task }: Props) => {
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
          legend="Update release status"
          id="status"
          name="status"
          value={status}
          onChange={e => {
            setStatus(e.target.value);
          }}
          options={[
            {
              id: 'approve-release-higher-review',
              label: 'Schedule for release',
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
              label: 'In draft',
              value: 'inDraft',
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

            {
              id: 'questionHigherReview',
              label: 'Ready for sign-off',
              value: 'readyHigherReview',
              conditional: (
                <FormGroup>
                  <label htmlFor="questionHigherReview" className="govuk-label">
                    Add your comment or question
                  </label>
                  <textarea
                    name="questionHigherReview"
                    id="questionHigherReview"
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
      {reviewType === 'level2' && task !== 'previewRelease' && (
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
