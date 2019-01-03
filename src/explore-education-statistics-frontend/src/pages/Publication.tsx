import React, { Component } from 'react';
import Breadcrumbs from '../components/Breadcrumbs';
import API from '../api';
import { match } from 'react-router';
import Date from '../components/Date';
import Glink from '../components/Glink';

interface Props {
  match: match<{
    publication: string,
  }>,
}

interface State {
  data: {
    title: string,
    nextUpdate: string,
    published: string,
    releaseName: string,
  }
}

class Publication extends Component<Props, State> {
  state = {
    data: {
      title: '',
      nextUpdate: '',
      published: '',
      releaseName: '',
    },
  };

  componentDidMount() {
    const { publication } = this.props.match.params;

    API.get(`publication/${publication}`)
      .then(({ data }) => this.setState({ data }))
      .catch(error => alert(error));
  }

  render() {
    const { data } = this.state;

    return (
      <div>
        <Breadcrumbs current={data.title} />
        <div className="govuk-grid-row">
          <div className="govuk-grid-column-two-thirds">
            <strong className="govuk-tag">
              This is the latest data
            </strong>

            <h1 className="govuk-heading-l">
              {data.title}
            </h1>
          </div>
          <div className="govuk-grid-column-one-third">
            <aside className="app-related-items">
              <h3 className="govuk-heading-m" id="subsection-title">
                About this data
              </h3>

              <h3 className="govuk-heading-s">
                <span className="govuk-caption-m">Release name: </span>
                {data.releaseName}  (latest data)
                <span className="govuk-caption-m">
                  <Glink>See previous years</Glink>
                </span>
              </h3>

              <h3 className="govuk-heading-s">
                <span className="govuk-caption-m">Published: </span>
                <Date value={data.published} />
              </h3>

              <h2 className="govuk-heading-s">
                <span className="govuk-caption-m">Next update: </span>
                <Date value={data.nextUpdate} />

                <span className="govuk-caption-m">
                  <a href="#notify">Notify me</a>
                </span>
              </h2>

              <hr className="govuk-section-break govuk-section-break--l govuk-section-break--visible" />

              <h2 className="govuk-heading-m" id="getting-the-data">
                Getting the data
              </h2>

              <ul className="govuk-list">
                <li><a href="#download" className="govuk-link">Download pdf files</a></li>
                <li><a href="#download" className="govuk-link">Download .csv files</a></li>
                <li><a href="#api" className="govuk-link">Access API</a></li>
              </ul>
            </aside>
          </div>
        </div>
      </div>
    );
  }
}

export default Publication;
