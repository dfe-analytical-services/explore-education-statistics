import React, { Component } from 'react';
import { Helmet } from 'react-helmet';
import ReactMarkdown from 'react-markdown';
import { match } from 'react-router';
import { CartesianGrid, Line, LineChart, XAxis, YAxis } from 'recharts';
import Date from '../components/Date';
import Details from '../components/Details';
import Link from '../components/Link';
import StepByStepNavigation from '../components/StepByStepNavigation';
import StepByStepNavigationStep from '../components/StepByStepNavigationStep';
import { baseUrl, contentApi } from '../services/api';

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
      updates: [
        {
          on: '',
          reason: '',
        },
      ],
    },
  };

  public componentDidMount() {
    this.fetchData();
  }

  public componentDidUpdate(prevProps: Props) {
    if (this.props.match.params !== prevProps.match.params) {
      this.fetchData();
    }
  }

  public render() {
    const { data } = this.state;
    const { theme } = this.props.match.params;
    const { topic } = this.props.match.params;
    const { release } = this.props.match.params;

    const releaseCount =
      data.publication.releases.slice(1).length +
      data.publication.legacyReleases.length;

    const chartData = [
      { name: '2012/13', unauthorised: 1.1, authorised: 4.2, overall: 5.3 },
      { name: '2013/14', unauthorised: 1.1, authorised: 3.5, overall: 4.5 },
      { name: '2014/15', unauthorised: 1.1, authorised: 3.5, overall: 4.6 },
      { name: '2015/16', unauthorised: 1.1, authorised: 3.4, overall: 4.6 },
      { name: '2016/17', unauthorised: 1.3, authorised: 3.4, overall: 4.7 },
    ];

    return (
      <div>
        <Helmet>
          <title>{data.title} - GOV.UK</title>
        </Helmet>
        <div className="govuk-grid-row">
          <div className="govuk-grid-column-two-thirds">
            {!release && (
              <strong
                className="govuk-tag"
                data-testid="publication-page--latest-data-heading"
              >
                This is the latest data
              </strong>
            )}

            <h2>{data.title}</h2>

            <ReactMarkdown className="govuk-body" source={data.summary} />

            <StepByStepNavigation>
              <StepByStepNavigationStep title="Headline pupil absence facts and figures for 2016/17">
                <LineChart
                  width={600}
                  height={300}
                  data={chartData}
                  margin={{ top: 5, right: 30, left: 20, bottom: 25 }}
                >
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis
                    dataKey="name"
                    label={{
                      offset: 5,
                      position: 'bottom',
                      value: 'School year',
                    }}
                    padding={{ left: 20, right: 20 }}
                    tickMargin={10}
                  />
                  <YAxis
                    label={{
                      angle: -90,
                      offset: 0,
                      position: 'left',
                      value: 'Absence rate',
                    }}
                    scale="auto"
                    unit="%"
                  />
                  <Line
                    type="linear"
                    dataKey="unauthorised"
                    stroke="#28A197"
                    strokeWidth="1"
                    unit="%"
                    activeDot={{ r: 3 }}
                  />
                  <Line
                    type="linear"
                    dataKey="authorised"
                    stroke="#6F72AF"
                    strokeWidth="1"
                    unit="%"
                    activeDot={{ r: 3 }}
                  />
                  <Line
                    type="linear"
                    dataKey="overall"
                    stroke="#DF3034"
                    strokeWidth="1"
                    unit="%"
                    activeDot={{ r: 3 }}
                  />
                </LineChart>
              </StepByStepNavigationStep>
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
                    <Link to="/feedback?type=page">Feedback on this page</Link>
                  </li>
                  <li>
                    <Link to="/feedback?type=suggestion">
                      Make a suggestion
                    </Link>
                  </li>
                  <li>
                    <Link to="/feedback?type=question">Ask a question</Link>
                  </li>
                </ul>
              </StepByStepNavigationStep>
            </StepByStepNavigation>
          </div>
          <div className="govuk-grid-column-one-third">
            <aside className="app-related-items">
              <h3 id="subsection-title">About this data</h3>

              <h4 data-testid="publication-page--release-name">
                <span className="govuk-caption-m">Release name: </span>
                {data.releaseName} {!release && <span>(latest data)</span>}
                <Details summary={`See previous ${releaseCount} releases`}>
                  <ul
                    className="govuk-list"
                    data-testid="publication-page--release-name-list"
                  >
                    {data.publication.releases.slice(1).map((elem, index) => (
                      <li key={elem.id} data-testid="item-internal">
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
                      <li key={elem.id} data-testid="item-external">
                        <a href={elem.url}>{elem.description}</a>
                      </li>
                    ))}
                  </ul>
                </Details>
              </h4>

              <h4>
                <span className="govuk-caption-m">Published: </span>
                <Date value={data.published} />
              </h4>

              <h4 data-testid="publication-page--last-updated">
                <span className="govuk-caption-m">Last updated: </span>
                <Date value={data.updates[0].on} />

                <Details summary={`See all ${data.updates.length} updates`}>
                  {data.updates.map(elem => (
                    <div data-testid="publication-page--update-element">
                      <Date
                        className="govuk-body govuk-!-font-weight-bold"
                        value={elem.on}
                      />
                      <p>{elem.reason}</p>
                    </div>
                  ))}
                </Details>
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

              <hr />

              <h3 id="getting-the-data">Getting the data</h3>

              <ul className="govuk-list">
                <li>
                  <a
                    href={`${baseUrl.data}/downloads/${
                      data.publication.slug
                    }/csv/`}
                    data-testid="publication-page--download-csvs"
                  >
                    Download .csv files
                  </a>
                </li>
                <li>
                  <a href={baseUrl.data}>Access API</a>
                </li>
              </ul>
            </aside>
          </div>
        </div>
      </div>
    );
  }

  private fetchData() {
    const { publication } = this.props.match.params;
    const { release } = this.props.match.params;

    const url = release
      ? `release/${release}`
      : `publication/${publication}/latest`;

    contentApi
      .get(url)
      .then(({ data }) => this.setState({ data }))
      .catch(error => alert(error));
  }
}

export default PublicationPage;
