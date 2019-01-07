import React, { Component } from 'react';
import ReactMarkdown from 'react-markdown';
import { match } from 'react-router';
import api, { baseUrl } from '../api';
import Date from '../components/Date';
import Link from '../components/Link';
import StepByStepNavigation from '../components/StepByStepNavigation';
import StepByStepNavigationStep from '../components/StepByStepNavigationStep';

interface Props {
  match: match<{
    publication: string;
    release: string;
    theme: string;
    topic: string;
  }>;
}

interface LegacyRelease {
  id: string;
  description: string;
  url: string;
}

interface Release {
  id: string;
  releaseName: string;
  slug: string;
}

interface Publication {
  dataSource: string;
  nextUpdate: string;
  releases: Release[];
  legacyReleases: LegacyRelease[];
  slug: string;
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
        dataSource: '',
        legacyReleases: [
          {
            description: '',
            id: '',
            url: '',
          },
        ],
        nextUpdate: '',
        releases: [
          {
            id: '',
            releaseName: '',
            slug: '',
          },
        ],
        slug: '',
      },
      published: '',
      releaseName: '',
      summary: '',
      title: '',
    },
  };

  public componentDidMount() {
    const { publication } = this.props.match.params;
    const { release } = this.props.match.params;

    const url = release
      ? `release/${release}`
      : `publication/${publication}/latest`;

    api
      .get(url)
      .then(({ data }) => this.setState({ data }))
      .catch(error => alert(error));
  }

  public render() {
    const { data } = this.state;
    const { theme } = this.props.match.params;
    const { topic } = this.props.match.params;

    const releaseCount =
      data.publication.releases.length + data.publication.legacyReleases.length;

    return (
      <div>
        <div className="govuk-grid-row">
          <div className="govuk-grid-column-two-thirds">
            <strong className="govuk-tag">This is the latest data</strong>

            <h2>{data.title}</h2>

            <ReactMarkdown className="govuk-body" source={data.summary} />

            <StepByStepNavigation>
              <StepByStepNavigationStep
                title="Where does this data come from?"
                caption="How we collect an process the data"
              >
                <ReactMarkdown
                  className="govuk-body"
                  source={data.publication.dataSource}
                />
              </StepByStepNavigationStep>
              <StepByStepNavigationStep title="Feedback and questions">
                <ul className="govuk-list">
                  <li>
                    <a href="/feedback?type=page" className="govuk-link">
                      Feedback on this page
                    </a>
                  </li>
                  <li>
                    <a href="/feedback?type=suggestion" className="govuk-link">
                      Make a suggestion
                    </a>
                  </li>
                  <li>
                    <a href="/feedback?type=question" className="govuk-link">
                      Ask a question
                    </a>
                  </li>
                </ul>
              </StepByStepNavigationStep>
            </StepByStepNavigation>
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
                      See previous {releaseCount} releases
                    </span>
                  </summary>
                  <div className="govuk-details__text">
                    <ul className="govuk-list">
                      {data.publication.releases.map(elem => (
                        <li key={elem.id}>
                          <Link
                            to={`/themes/${theme}/${topic}/${
                              data.publication.slug
                            }/${elem.slug}`}
                          >
                            {elem.releaseName}
                          </Link>
                        </li>
                      ))}
                      {data.publication.legacyReleases.map(elem => (
                        <li key={elem.id}>
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

              <ul className="govuk-list">
                <li>
                  <Link to="#pdf">Download pdf files</Link>
                </li>
                <li>
                  <Link to="#csv">Download .csv files</Link>
                </li>
                <li>
                  <a href={baseUrl.data} className="govuk-link">
                    Access API
                  </a>
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
