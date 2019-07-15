import React, { Component } from 'react';
import PrototypePublicationService from '@admin/pages/prototypes/components/PrototypePublicationService';
import EditablePublicationPage from '@admin/pages/prototypes/components/EditablePublicationPage';
import PrototypePage from './components/PrototypePage';
import PrototypeAdminNavigation from './components/PrototypeAdminNavigation';

interface State {
  // data: Release | undefined;
  // publication: string | undefined;
  editing: boolean;
}

class PublicationPage extends Component<{}, State> {
  public state = {
    // publication: undefined,
    editing: true,
  };

  public async componentDidMount() {
    // const publication = 'pupil-absence-in-schools-in-england';

    // const request = PrototypePublicationService.getLatestPublicationRelease(
    // publication,
    // );

    // const data = await request;

    this.setState({
      // publication,
    });
  }

  public render() {
    const { editing } = this.state;

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

        <div className="govuk-form-group govuk-width-container  dfe-align--comments">
          <div className="govuk-warning-text">
            <span className="govuk-warning-text__icon" aria-hidden="true">
              !
            </span>
            <strong className="govuk-warning-text__text">
              <span className="govuk-warning-text__assistive">Warning</span>
              There are unresolved comments requiring your attention
            </strong>
          </div>

          <fieldset className="govuk-fieldset dfe-toggle-edit">
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
                  onChange={() => this.setState({ editing: true })}
                />
                <label
                  className="govuk-label govuk-radios__label"
                  htmlFor="edit"
                >
                  Edit content
                </label>
              </div>
              <div className="govuk-radios__item">
                <input
                  className="govuk-radios__input"
                  type="radio"
                  name="status"
                  id="edit"
                  value="edit"
                  onChange={() => this.setState({ editing: false })}
                />
                <label
                  className="govuk-label govuk-radios__label"
                  htmlFor="edit"
                >
                  Preview content
                </label>
              </div>
            </div>
          </fieldset>
        </div>

        <hr />
        <div className="govuk-width-container  dfe-align--comments">
          <EditablePublicationPage
            editing={editing}
            resolveComments
            data={PrototypePublicationService.getNewPublication()}
          />
        </div>
      </PrototypePage>
    );
  }
}

export default PublicationPage;
