import React, { Component } from 'react';
import EditablePublicationPage from '@admin/pages/prototypes/components/EditablePublicationPage';
import PrototypePublicationService from '@admin/pages/prototypes/components/PrototypePublicationService';
import PrototypePage from './components/PrototypePage';
import PrototypeAdminNavigation from './components/PrototypeAdminNavigation';
import Link from '../../components/Link';

interface State {
  editing: boolean;
}

class PublicationPage extends Component<{}, State> {
  public state = {
    editing: true,
  };

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
        <div className="govuk-form-group govuk-width-container">
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
        <div className="govuk-width-container">
          <EditablePublicationPage
            editing={editing}
            data={PrototypePublicationService.getNewPublication()}
          />
        </div>
        <hr />
        <div className="govuk-grid-row govuk-!-margin-top-9">
          <div className="govuk-grid-column-one-half ">
            <Link to="/prototypes/publication-create-new-absence-view-table">
              <span className="govuk-heading-m govuk-!-margin-bottom-0">
                Previous step
              </span>
              Manage tables and charts
            </Link>
          </div>
          <div className="govuk-grid-column-one-half dfe-align--right">
            <Link to="/prototypes/publication-create-new-absence-status">
              <span className="govuk-heading-m govuk-!-margin-bottom-0">
                Next step
              </span>
              Set publish status
            </Link>
          </div>
        </div>
      </PrototypePage>
    );
  }
}

export default PublicationPage;
