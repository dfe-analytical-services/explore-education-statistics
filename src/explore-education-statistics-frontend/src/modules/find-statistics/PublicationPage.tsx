import { NextContext } from 'next';
import React, { Component } from 'react';
import ReactMarkdown from 'react-markdown';
import PageTitle from 'src/components/PageTitle';
import Accordion from '../../components/Accordion';
import AccordionSection from '../../components/AccordionSection';
import Details from '../../components/Details';
import FormattedDate from '../../components/FormattedDate';
import GoToTopLink from '../../components/GoToTopLink';
import Link from '../../components/Link';
import Page from '../../components/Page';
import Tabs from '../../components/Tabs';
import TabsSection from '../../components/TabsSection';
import { baseUrl, contentApi } from '../../services/api';
import ContentBlock from './components/ContentBlock';

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

interface ContentSection {
  caption: string;
  content: any[];
  heading: string;
  order: number;
}

interface KeyStatistic {
  title: string;
  description: string;
}

interface Publication {
  dataSource: string;
  nextUpdate: string;
  releases: Release[];
  legacyReleases: LegacyRelease[];
  slug: string;
}

interface Props {
  publication: string;
  release: string;
  data: {
    title: string;
    published: string;
    summary: string;
    releaseName: string;
    publication: Publication;
    content: ContentSection[];
    keyStatistics: KeyStatistic[];
    updates: {
      on: string;
      reason: string;
    }[];
  };
}

class PublicationPage extends Component<Props> {
  public static async getInitialProps({
    query,
  }: NextContext<{
    publication: string;
    release: string;
  }>) {
    const { publication, release } = query;

    const url = release
      ? `release/${release}`
      : `publication/${publication}/latest`;

    const { data } = await contentApi.get(url);

    return {
      data,
      publication,
      release,
    };
  }

  public render() {
    const { data, release } = this.props;

    const releaseCount =
      data.publication.releases.slice(1).length +
      data.publication.legacyReleases.length;

    return (
      <Page
        breadcrumbs={[
          { name: 'Find statistics and data', link: '/statistics' },
          { name: data.title },
        ]}
      >
        <div className="govuk-grid-row">
          <div className="govuk-grid-column-two-thirds">
            {!release && (
              <strong className="govuk-tag govuk-!-margin-bottom-2">
                {' '}
                This is the latest data{' '}
              </strong>
            )}

            <PageTitle title={data.title} />

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
                    {data.publication.releases.slice(1).map(elem => (
                      <li key={elem.id} data-testid="item-internal">
                        <Link
                          to={`/statistics/${data.publication.slug}/${
                            elem.slug
                          }`}
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
                <FormattedDate>{data.published}</FormattedDate>
              </h4>

              <h4 data-testid="publication-page--last-updated">
                <span className="govuk-caption-m">Last updated: </span>
                <FormattedDate>{data.updates[0].on}</FormattedDate>

                <Details summary={`See all ${data.updates.length} updates`}>
                  {data.updates.map(elem => (
                    <div
                      data-testid="publication-page--update-element"
                      key={elem.on}
                    >
                      <FormattedDate className="govuk-body govuk-!-font-weight-bold">
                        {elem.on}
                      </FormattedDate>
                      <p>{elem.reason}</p>
                    </div>
                  ))}
                </Details>
              </h4>

              <h4>
                <span className="govuk-caption-m">Next update: </span>
                <FormattedDate>{data.publication.nextUpdate}</FormattedDate>

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

        {data.keyStatistics.length > 0 && (
          <>
            <h2 className="govuk-heading-l">
              {!release ? <>Latest headline </> : <>Headline </>}
              facts and figures - {data.releaseName}
            </h2>
            <Tabs>
              <TabsSection id="summary" title="Summary">
                <div className="dfe-dash-tiles dfe-dash-tiles--3-in-row">
                  {data.keyStatistics.map(({ title, description }) => (
                    <div className="dfe-dash-tiles__tile" key={title}>
                      <h3 className="govuk-heading-m dfe-dash-tiles__heading">
                        {title}
                      </h3>
                      <p className="govuk-heading-xl govuk-!-margin-bottom-2">
                        --
                      </p>
                      <Details summary={`What is ${title}?`}>
                        {description}
                      </Details>
                    </div>
                  ))}
                </div>
              </TabsSection>
            </Tabs>
          </>
        )}

        {data.content.length > 0 && (
          <>
            <h2 className="govuk-heading-l">Contents</h2>

            <Accordion id="contents-sections">
              {data.content.map(({ heading, caption, order, content }) => (
                <AccordionSection
                  heading={heading}
                  caption={caption}
                  key={order}
                >
                  <ContentBlock content={content} />
                </AccordionSection>
              ))}
            </Accordion>
          </>
        )}

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
      </Page>
    );
  }
}

export default PublicationPage;
