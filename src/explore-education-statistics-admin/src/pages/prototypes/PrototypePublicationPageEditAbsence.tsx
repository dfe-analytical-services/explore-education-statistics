import { Release } from '@common/services/publicationService';
import React, { Component } from 'react';
import PrototypePage from './components/PrototypePage';
import PrototypeAdminNavigation from './components/PrototypeAdminNavigation';
import { PrototypePublicationService } from '@admin/pages/prototypes/components/PrototypePublicationService';
import { EditablePublicationPage } from '@admin/pages/prototypes/components/EditablePublicationPage';

interface State {
  data: Release | undefined;
  publication: string | undefined;
  editing: boolean;
}

class PublicationPage extends Component<{}, State> {
  public state = {
    data: undefined,
    publication: undefined,
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
      publication,
    });
  }

  public render() {
    return (
      <PrototypePage
        wide
        breadcrumbs={[
          {
            link: '/prototypes/admin-dashboard?status=editLiveRelease',
            text: 'Administrator dashboard',
          },
          { text: 'Edit pupil absence statistics', link: '#' },
        ]}
      >
        <PrototypeAdminNavigation sectionId="addContent" task="editRelease" />
        <div className="govuk-form-group">
          <fieldset className="govuk-fieldset">
            <legend className="govuk-fieldset__legend govuk-fieldset__legend--m">
              <h1 className="govuk-fieldset__heading">Set page view</h1>
            </legend>
            <div className="govuk-radios govuk-radios--inline">
              <div className="govuk-radios__item">
                <input
                  className="govuk-radios__input"
                  type="radio"
                  name="status"
                  id="edit"
                  value="edit"
                  defaultChecked
                  onChange={() => this.setState({ editing: false })}
                />
                <label
                  className="govuk-label govuk-radios__label"
                  htmlFor="edit"
                >
                  Preview release
                </label>
              </div>

              <div className="govuk-radios__item">
                <input
                  className="govuk-radios__input"
                  type="radio"
                  name="status"
                  id="edit"
                  value="edit"
                  onChange={() => this.setState({ editing: true })}
                />
                <label
                  className="govuk-label govuk-radios__label"
                  htmlFor="edit"
                >
                  Edit release
                </label>
              </div>
            </div>
          </fieldset>
        </div>

        <hr />

        <EditablePublicationPage
          editing={this.state.editing}
          data={this.state.data}
        />
      </PrototypePage>
    );
  }
}

export default PublicationPage;
