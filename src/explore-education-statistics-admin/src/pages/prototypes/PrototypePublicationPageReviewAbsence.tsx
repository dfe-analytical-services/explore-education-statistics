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
  editing: boolean;
}

class PublicationPage extends Component<{}, State> {
  public state = {
    data: undefined,
    // publication: undefined,
    editing: false,
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
    const { editing, data } = this.state;

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
        <div className="govuk-width-container">
          <FormGroup>
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
                      <label
                        htmlFor="approved-comments"
                        className="govuk-label"
                      >
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
                  id: 'requires-work',
                  label: 'Requires work',
                  value: 'requires-work',
                  conditional: (
                    <FormGroup>
                      <label
                        htmlFor="requires-work-comments"
                        className="govuk-label"
                      >
                        Please provide feedback
                      </label>
                      <textarea
                        name="requires-work-comments"
                        id="requires-work-comments"
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
                        Add you comment or question
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
          </FormGroup>
          <Link
            to="/prototypes/admin-dashboard?status=readyApproval"
            className="govuk-button"
          >
            Update
          </Link>
        </div>
        <hr />
        <div className="govuk-width-container">
          <EditablePublicationPage data={data} />
        </div>
      </PrototypePage>
    );
  }
}

export default PublicationPage;
