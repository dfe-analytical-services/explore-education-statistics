import React, { Component } from 'react';
import ReactMarkdown from 'react-markdown';
import { match } from 'react-router';
import api from '../api';
import Date from '../components/Date';
import Link from '../components/Link';

interface Props {
  match: match<{
    publication: string;
  }>;
}

interface LegacyRelease {
  description: string;
  url: string;
}

interface Publication {
  nextUpdate: string;
  legacyReleases: LegacyRelease[];
}

interface State {
  data: {
    title: string;
    published: string;
    summary: string;
    releaseName: string;
    publication: Publication;
  };
}

class PublicationPage extends Component<Props, State> {
  public state = {
    data: {
      publication: {
        legacyReleases: [
          {
            description: '',
            url: '',
          },
        ],
        nextUpdate: '',
      },
      published: '',
      releaseName: '',
      summary: '',
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
        <div className="govuk-grid-row">
          <div className="govuk-grid-column-two-thirds">
            <strong className="govuk-tag">This is the latest data</strong>

            <h2>{data.title}</h2>

            <ReactMarkdown className="govuk-body" source={data.summary} />
          </div>
          <div className="govuk-grid-column-one-third">
            <aside className="app-related-items">
              <h3 id="subsection-title">About this data</h3>

              <h4>
                <span className="govuk-caption-m">Release name: </span>
                {data.releaseName} (latest data)
                <details className="govuk-details">
                  <summary className="govuk-details__summary">
                    <span className="govuk-details__summary-text">
                      See previous {data.publication.legacyReleases.length}{' '}
                      releases
                    </span>
                  </summary>
                  <div className="govuk-details__text">
                    <ul>
                      {data.publication.legacyReleases.map(elem => (
                        <li>
                          <a className="govuk-link" href={elem.url}>
                            {elem.description}
                          </a>
                        </li>
                      ))}
                    </ul>
                  </div>
                </details>
              </h4>

              <h4>
                <span className="govuk-caption-m">Published: </span>
                <Date value={data.published} />
              </h4>

              <h4>
                <span className="govuk-caption-m">Last updated: </span>
                <Date value="1970-01-01T00:00:00" />

                <details className="govuk-details">
                  <summary className="govuk-details__summary">
                    <span className="govuk-details__summary-text">
                      See all 999 updates
                    </span>
                  </summary>
                  <div className="govuk-details__text">
                    <p>19 April 2017</p>
                    <p>
                      Underlying data file updated to include absence data by
                      pupil residency and school location, andupdated metadata
                      document.
                    </p>
                    <strong>23 March 2017</strong>
                    <p>First published.</p>
                  </div>
                </details>
              </h4>

              <h4>
                <span className="govuk-caption-m">Next update: </span>
                <Date value={data.publication.nextUpdate} />

                <span className="govuk-caption-m">
                  <Link to="#" unvisited>
                    Notify me
                  </Link>
                </span>
              </h4>

              <hr className="govuk-section-break govuk-section-break--l govuk-section-break--visible" />

              <h3 id="getting-the-data">Getting the data</h3>

              <ul>
                <li>
                  <Link to="#">Download pdf files</Link>
                </li>
                <li>
                  <Link to="#">Download .csv files</Link>
                </li>
                <li>
                  <Link to="#">Access API</Link>
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
