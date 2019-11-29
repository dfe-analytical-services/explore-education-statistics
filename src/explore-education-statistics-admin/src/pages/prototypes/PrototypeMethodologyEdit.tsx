import React, { Component } from 'react';
import Link from '@admin/components/Link';
import PrototypeMethodologyService from '@admin/pages/prototypes/components/PrototypePublicationService';
import EditableMethodologyPage from '@admin/pages/prototypes/components/EditableMethodologyPage';
import PrototypePage from './components/PrototypePage';
import PrototypeMethodologyNavigation from './components/PrototypeMethodologyNavigation';

interface State {
  // data: Release | undefined;
  // publication: string | undefined;
  editing: boolean;
}

class MethodologyPage extends Component<{}, State> {
  public state = {
    // publication: undefined,
    editing: false,
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
    const sectionId = 'addContent';

    return (
      <PrototypePage
        wide
        breadcrumbs={[{ text: 'Edit methodology', link: '#' }]}
      >
        <PrototypeMethodologyNavigation sectionId={sectionId} />
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
                  onClick={() => this.setState({ editing: false })}
                />
                <label
                  className="govuk-label govuk-radios__label"
                  htmlFor="edit"
                >
                  Preview methodology
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
                  Edit methodology
                </label>
              </div>
            </div>
          </fieldset>
        </div>

        <hr />
        <div className="govuk-width-container">
          <EditableMethodologyPage
            editing={editing}
            data={PrototypeMethodologyService.getNewPublication()}
          />
        </div>
        <hr />
        <div className="govuk-grid-row govuk-!-margin-top-9">
          <div className="govuk-grid-column-one-half ">
            <Link to="/prototypes/publication-create-new-methodology-config">
              <span className="govuk-heading-m govuk-!-margin-bottom-0">
                Previous step
              </span>
              Methodology summary
            </Link>
          </div>
          <div className="govuk-grid-column-one-half dfe-align--right">
            <Link to="/prototypes/publication-create-new-methodology-status">
              <span className="govuk-heading-m govuk-!-margin-bottom-0">
                Next step
              </span>
              Update methodology status
            </Link>
          </div>
        </div>
      </PrototypePage>
    );
  }
}

export default MethodologyPage;
