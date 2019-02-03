import React, { Component } from 'react';
import { Helmet } from 'react-helmet';
import ReactMarkdown from 'react-markdown';
import { match } from 'react-router';
import { CartesianGrid, Line, LineChart, XAxis, YAxis } from 'recharts';
import Accordion from '../components/Accordion';
import AccordionSection from '../components/AccordionSection';
import Date from '../components/Date';
import Details from '../components/Details';
import GoToTopLink from '../components/GoToTopLink';
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
      <>
        <Helmet>
          <title>{data.title} - GOV.UK</title>
        </Helmet>
        <div className="govuk-grid-row">
          <div className="govuk-grid-column-two-thirds">
            {!release && (
              <strong className="govuk-tag govuk-!-margin-bottom-2">
                {' '}
                This is the latest data{' '}
              </strong>
            )}

            <h1 className="govuk-heading-xl">{data.title}</h1>

            <ReactMarkdown className="govuk-body-l" source={data.summary} />

            <Details summary="Read more about our methodology">
              <p>
                To help you analyse and understand the statistics the following
                sections include:
              </p>

              <div className="govuk-inset-text">
                <Link to="#">
                  Find out more about our pupil absence data and statistics
                  methodology and terminology
                </Link>
              </div>
            </Details>
            <Details summary="Download underlying data files">
              <ul className="govuk-list">
                <li>
                  <a
                    href={`${baseUrl.data}/downloads/${
                      data.publication.slug
                    }/csv/`}
                    className="govuk-link"
                  >
                    Download .csv files
                  </a>
                </li>
                <li>
                  <a href={baseUrl.data} className="govuk-link">
                    Access API
                  </a>
                </li>
              </ul>
            </Details>
          </div>
          <div className="govuk-grid-column-one-third">
            <aside className="app-related-items">
              <h3 id="subsection-title">About these statistics</h3>

              <h4 data-testid="publication-page--release-name">
                <span className="govuk-caption-m">For school year: </span>
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
            </aside>
          </div>
        </div>

        <hr />

        <h2 className="govuk-heading-l">
          {!release ? <>Latest headline </> : <>Headline </>}
          facts and figures - {data.releaseName}
        </h2>

        <h2 className="govuk-heading-l">Contents</h2>
        <Accordion id="contents-sections">
          <AccordionSection heading="About this release">
            <p className="govuk-body">
              TODO: Implement about this release content
            </p>
          </AccordionSection>
        </Accordion>

        <h2 className="govuk-heading-m govuk-!-margin-top-9">
          Extra information
        </h2>
        <Accordion id="extra-information-sections">
          <AccordionSection
            heading="Where does this data come from"
            caption="How we collect and process the data"
            headingTag="h3"
          >
            <ul className="govuk-list">
              <li>
                <a href="#" className="govuk-link">
                  How do we collect it?
                </a>
              </li>
              <li>
                <a href="#" className="govuk-link">
                  What do we do with it?
                </a>
              </li>
              <li>
                <a href="#" className="govuk-link">
                  Related policies
                </a>
              </li>
            </ul>
          </AccordionSection>
          <AccordionSection heading="Feedback and questions" headingTag="h3">
            <ul className="govuk-list">
              <li>
                <a href="#" className="govuk-link">
                  Feedback on this page
                </a>
              </li>
              <li>
                <a href="#" className="govuk-link">
                  Make a suggestion
                </a>
              </li>
              <li>
                <a href="#" className="govuk-link">
                  Ask a question
                </a>
              </li>
            </ul>
          </AccordionSection>
          <AccordionSection heading="Contact us" headingTag="h3">
            <h4 className="govuk-heading-">Media enquiries</h4>
            <address className="govuk-body dfe-font-style-normal">
              Press Office News Desk
              <br />
              Department for Education <br />
              Sanctuary Buildings <br />
              Great Smith Street <br />
              London
              <br />
              SW1P 3BT <br />
              Telephone: 020 7783 8300
            </address>

            <h4 className="govuk-heading-">Other enquiries</h4>
            <address className="govuk-body dfe-font-style-normal">
              Data Insight and Statistics Division
              <br />
              Level 1<br />
              Department for Education
              <br />
              Sanctuary Buildings <br />
              Great Smith Street
              <br />
              London
              <br />
              SW1P 3BT <br />
              Telephone: 020 7783 8300
              <br />
              Email: <a href="#">Schools.statistics@education.gov.uk</a>
            </address>
          </AccordionSection>
        </Accordion>

        <h2 className="govuk-heading-m govuk-!-margin-top-9">
          Exploring the data
        </h2>
        <p>
          The statistics can be viewed as reports, or you can customise and
          download as excel or .csv files . The data can also be accessed via an
          API. <a href="#">What is an API?</a>
        </p>
        <Link to="/prototypes/data-table-v3" className="govuk-button">
          Explore pupil absence statistics
        </Link>

        <GoToTopLink />
      </>
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
