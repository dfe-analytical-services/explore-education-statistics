import { Release } from '@common/services/publicationService';
import React, { Component } from 'react';
import { FormGroup, FormRadioGroup } from '@common/components/form';
import PrototypePublicationService from '@admin/pages/prototypes/components/PrototypePublicationService';
import EditablePublicationPage from '@admin/pages/prototypes/components/EditablePublicationPage';
import PrototypePage from './components/PrototypePage';
import Link from '../../components/Link';

interface State {
  data: Release | undefined;
  // publication: string | undefined;
  // editing: boolean;
}

class PublicationPage extends Component<{}, State> {
  public state = {
    data: undefined,
    // publication: undefined,
    // editing: false,
  };

  public async componentDidMount() {
    const publication = 'pupil-absence-in-schools-in-england';

    const request = PrototypePublicationService.getLatestPublicationRelease(
      publication,
    );

    const data = await request;

    this.setState({
      data,
      // publication,
    });
  }

  public render() {
    const { data } = this.state;

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
        {' '}
        <div className="govuk-width-container dfe-align--comments">
          <FormRadioGroup
            legend="Review this release"
            id="review-release"
            name="review-release"
            options={[
              {
                id: 'approve-release',
                label: 'Approved for publication',
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
                label: 'Add a comment or question',
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
        <hr />
        <div className="govuk-width-container dfe-align--comments">
          <EditablePublicationPage
            reviewing
            data={PrototypePublicationService.getNewPublication()}
          />
        </div>
      </PrototypePage>
    );
  }
}

export default PublicationPage;
