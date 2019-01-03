import React, { Component } from 'react';
import { match } from 'react-router';
import api from '../api';
import Breadcrumbs from '../components/Breadcrumbs';
import Date from '../components/Date';
import Glink from '../components/Glink';

interface Props {
  match: match<{
    publication: string;
  }>;
}

interface Publication {
  nextUpdate: string;
  legacyReleases: any[];
}

interface State {
  data: {
    title: string;
    published: string;
    releaseName: string;
    publication: Publication;
  };
}

class PublicationPage extends Component<Props, State> {
  public state = {
    data: {
      publication: {
        legacyReleases: [],
        nextUpdate: '',
      },
      published: '',
      releaseName: '',
      title: '',
    },
  };

  public componentDidMount() {
    const { publication } = this.props.match.params;

    api
      .get(`publication/${publication}/latest`)
      .then(({ data }) => this.setState({ data }))
      .catch(error => alert(error));
  }

  public render() {
    const { data } = this.state;

    return (
      <div>
        <Breadcrumbs current={data.title} />
        <div className="govuk-grid-row">
          <div className="govuk-grid-column-two-thirds">
            <strong className="govuk-tag">This is the latest data</strong>

            <h1 className="govuk-heading-l">{data.title}</h1>
          </div>
          <div className="govuk-grid-column-one-third">
            <aside className="app-related-items">
              <h3 className="govuk-heading-m" id="subsection-title">
                About this data
              </h3>

              <h3 className="govuk-heading-s">
                <span className="govuk-caption-m">Release name: </span>
                {data.releaseName} (latest data)
                <span className="govuk-caption-m">
                  <Glink>
                    See previous {data.publication.legacyReleases.length} years
                  </Glink>
                </span>
              </h3>

              <h3 className="govuk-heading-s">
                <span className="govuk-caption-m">Published: </span>
                <Date value={data.published} />
              </h3>

              <h2 className="govuk-heading-s">
                <span className="govuk-caption-m">Last updated: </span>
                <Date value="1970-01-01T00:00:00" />
                <span className="govuk-caption-m">
                  <Glink>See all 999 updates</Glink>
                </span>
              </h2>

              <h2 className="govuk-heading-s">
                <span className="govuk-caption-m">Next update: </span>
                <Date value={data.publication.nextUpdate} />

                <span className="govuk-caption-m">
                  <Glink>Notify me</Glink>
                </span>
              </h2>

              <hr className="govuk-section-break govuk-section-break--l govuk-section-break--visible" />

              <h2 className="govuk-heading-m" id="getting-the-data">
                Getting the data
              </h2>

              <ul className="govuk-list">
                <li>
                  <Glink>Download pdf files</Glink>
                </li>
                <li>
                  <Glink>Download .csv files</Glink>
                </li>
                <li>
                  <Glink>Access API</Glink>
                </li>
              </ul>
            </aside>
          </div>
        </div>
      </div>
    );
  }
}

export default PublicationPage;
