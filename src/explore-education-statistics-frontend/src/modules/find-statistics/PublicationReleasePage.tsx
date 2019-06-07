import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import Details from '@common/components/Details';
import FormattedDate from '@common/components/FormattedDate';
import PrintThisPage from '@common/components/PrintThisPage';
import RelatedAside from '@common/components/RelatedAside';
import SearchForm from '@common/components/SearchForm';
import DataBlock from '@common/modules/find-statistics/components/DataBlock';
import { baseUrl } from '@common/services/api';
import publicationService, {
  Release,
} from '@common/services/publicationService';
import Link from '@frontend/components/Link';
import Page from '@frontend/components/Page';
import PageTitle from '@frontend/components/PageTitle';
import classNames from 'classnames';
import { NextContext } from 'next';
import React, { Component } from 'react';
import ReactMarkdown from 'react-markdown';
import ContentBlock from './components/ContentBlock';
import styles from './PublicationReleasePage.module.scss';

interface Props {
  publication: string;
  release: string;
  data: Release;
}

class PublicationReleasePage extends Component<Props> {
  public static async getInitialProps({
    query,
  }: NextContext<{
    publication: string;
    release: string;
  }>) {
    const { publication, release } = query;

    const request = release
      ? publicationService.getPublicationRelease(release)
      : publicationService.getLatestPublicationRelease(publication);

    const data = await request;

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
        <div className={styles.releaseHeader}>
          <PageTitle title={data.title} />
        </div>
        <div className={classNames('govuk-grid-row', styles.releaseIntro)}>
          <div className="govuk-grid-column-two-thirds">
            <div className="govuk-grid-row">
              <div className="govuk-grid-column-three-quarters">
                {!release && (
                  <strong className="govuk-tag govuk-!-margin-right-6">
                    {' '}
                    This is the latest data{' '}
                  </strong>
                )}
                <dl className="dfe-meta-content govuk-!-margin-top-3 govuk-!-margin-bottom-1">
                  <dt className="govuk-caption-m">Published: </dt>
                  <dd>
                    <strong>
                      <FormattedDate>{data.published}</FormattedDate>{' '}
                    </strong>
                  </dd>
                  <div>
                    <dt className="govuk-caption-m">Next update: </dt>
                    <dd>
                      <strong>
                        <FormattedDate>
                          {data.publication.nextUpdate}
                        </FormattedDate>
                      </strong>
                    </dd>
                  </div>
                </dl>
                <Link
                  unvisited
                  to={`/subscriptions?slug=${data.publication.slug}`}
                  data-testid={`subsciption-${data.publication.slug}`}
                >
                  Subscribe to email updates
                </Link>
              </div>
              <div className="govuk-grid-column-one-quarter">
                <img
                  src="/static/images/UKSA-quality-mark.jpg"
                  alt="UK statistics authority quality mark"
                  height="120"
                  width="120"
                />
              </div>
            </div>

            <ReactMarkdown className="govuk-body" source={data.summary} />

            <Details summary="Download data files">
              <ul className="govuk-list govuk-list--bullet">
                <li>
                  <Link
                    to={`${
                      baseUrl.data
                    }/api/download/pupil-absence-in-schools-in-england/2016-17/absence_by_characteristic.csv`}
                    className="govuk-link"
                  >
                    Absence by characteristic
                  </Link>
                  {` `}(csv, 100mb)
                </li>
                <li>
                  <Link
                    to={`${
                      baseUrl.data
                    }/api/download/pupil-absence-in-schools-in-england/2016-17/absence_by_geographic_level.csv`}
                    className="govuk-link"
                  >
                    Absence by geographic level
                  </Link>
                  {` `}(csv, 100mb)
                </li>
                <li>
                  <Link
                    to={`${
                      baseUrl.data
                    }/api/download/pupil-absence-in-schools-in-england/2016-17/absence_by_term.csv`}
                    className="govuk-link"
                  >
                    Absence by term
                  </Link>
                  {` `}(csv, 100mb)
                </li>
                <li>
                  <Link
                    to={`${
                      baseUrl.data
                    }/api/download/pupil-absence-in-schools-in-england/2016-17/absence_for_four_year_olds.csv`}
                    className="govuk-link"
                  >
                    Absence for four year olds
                  </Link>
                  {` `}(csv, 100mb)
                </li>
                <li>
                  <Link
                    to={`${
                      baseUrl.data
                    }/api/download/pupil-absence-in-schools-in-england/2016-17/absence_in_prus.csv`}
                    className="govuk-link"
                  >
                    Absence in prus
                  </Link>
                  {` `}(csv, 100mb)
                </li>
                <li>
                  <Link
                    to={`${
                      baseUrl.data
                    }/api/download/pupil-absence-in-schools-in-england/2016-17/absence_number_missing_at_least_one_session_by_reason.csv`}
                    className="govuk-link"
                  >
                    Absence number missing at least one session by reason
                  </Link>
                  {` `}(csv, 100mb)
                </li>
                <li>
                  <Link
                    to={`${
                      baseUrl.data
                    }/api/download/pupil-absence-in-schools-in-england/2016-17/absence_rate_percent_bands.csv`}
                    className="govuk-link"
                  >
                    Absence rate percent bands
                  </Link>
                  {` `}(csv, 100mb)
                </li>
              </ul>
              <h2 className="govuk-heading-m govuk-!-margin-top-9">
                Explore and edit this data online
              </h2>

              <p>Use our table tool to add and remove data for this table.</p>

              <Link to="/table-tool/" className="govuk-button">
                Explore data
              </Link>
            </Details>

            <div className="govuk-!-margin-top-3 govuk-!-margin-bottom-3">
              <SearchForm />
            </div>
          </div>

          <div className="govuk-grid-column-one-third">
            <RelatedAside>
              <h3>About these statistics</h3>

              <dl className="dfe-meta-content" data-testid="release-period">
                <dt className="govuk-caption-m">For school year: </dt>
                <dd>
                  <strong>{data.releaseName}</strong>
                </dd>
                <dd>
                  <Details summary={`See previous ${releaseCount} releases`}>
                    <ul
                      className="govuk-list"
                      data-testid="previous-releases-list"
                    >
                      {data.publication.releases
                        .slice(1)
                        .map(({ id, slug, releaseName }) => (
                          <li key={id} data-testid="item-internal">
                            <Link
                              to={`/statistics/${
                                data.publication.slug
                              }/${slug}`}
                            >
                              {releaseName}
                            </Link>
                          </li>
                        ))}
                      {data.publication.legacyReleases.map(
                        ({ id, description, url }) => (
                          <li key={id} data-testid="item-external">
                            <a href={url}>{description}</a>
                          </li>
                        ),
                      )}
                    </ul>
                  </Details>
                </dd>
              </dl>
              <dl className="dfe-meta-content" data-testid="last-updated">
                <dt className="govuk-caption-m">Last updated: </dt>
                <dd>
                  <strong>
                    <FormattedDate>{data.updates[0].on}</FormattedDate>
                  </strong>
                  <Details summary={`See all ${data.updates.length} updates`}>
                    {data.updates.map(elem => (
                      <div data-testid="last-updated-element" key={elem.on}>
                        <FormattedDate className="govuk-body govuk-!-font-weight-bold">
                          {elem.on}
                        </FormattedDate>
                        <p>{elem.reason}</p>
                      </div>
                    ))}
                  </Details>
                </dd>
              </dl>
              {/* <h2
                className="govuk-heading-m govuk-!-margin-top-6"
                id="related-content"
              >
                Related guidance
              </h2>
              <nav role="navigation" aria-labelledby="related-content">
                <ul className="govuk-list">
                  <li>
                    [Link to relevant methodology section here]
                  </li>
                </ul>
              </nav> */}
            </RelatedAside>
          </div>
        </div>
        <hr />
        <h2>Headline facts and figures - {data.releaseName}</h2>

        {data.keyStatistics && (
          <DataBlock {...data.keyStatistics} id="keystats" />
        )}

        {data.content.length > 0 && (
          <Accordion id="contents-sections">
            {data.content.map(({ heading, caption, order, content }) => (
              <AccordionSection heading={heading} caption={caption} key={order}>
                <ContentBlock content={content} id={`content_${order}`} />
              </AccordionSection>
            ))}
          </Accordion>
        )}
        <h2
          className="govuk-heading-m govuk-!-margin-top-9"
          data-testid="extra-information"
        >
          Supporting information
        </h2>
        <Accordion id="extra-information-sections">
          <AccordionSection
            heading="Where does this data come from"
            caption="How we collect and process the data"
            headingTag="h3"
          >
            <ul className="govuk-list">
              <li>
                <a href="#">How do we collect it?</a>
              </li>
              <li>
                <a href="#">What do we do with it?</a>
              </li>
              <li>
                <a href="#">Related policies</a>
              </li>
            </ul>
          </AccordionSection>
          <AccordionSection heading="National statistics" headingTag="h3">
            <p className="govuk-body">
              The United Kingdom Statistics Authority designated these
              statistics as National Statistics in <a href="#">Month Year</a> in
              accordance with the Statistics and Registration Service Act 2007
              and signifying compliance with the Code of Practice for
              Statistics.
            </p>
            <p className="govuk-body">
              Designation can be broadly interpreted to mean that the
              statistics:
            </p>
            <ul className="govuk-list govuk-list--bullet">
              <li>meet identified user needs;</li>
              <li>are well explained and readily accessible;</li>
              <li>are produced according to sound methods, and</li>
              <li>
                are managed impartially and objectively in the public interest
              </li>
            </ul>
            <p className="govuk-body">
              Once statistics have been designated as National Statistics it is
              a statutory requirement that the Code of Practice shall continue
              to be observed. Information on improvements made to these
              statistics to continue their compliance with the Code of Practice
              are provided in this <a href="#">accompanying document</a>
            </p>
          </AccordionSection>
          <AccordionSection heading="Feedback and questions" headingTag="h3">
            <ul className="govuk-list">
              <li>
                <a href="#">Feedback on this page</a>
              </li>
              <li>
                <a href="#">Make a suggestion</a>
              </li>
              <li>
                <a href="#">Ask a question</a>
              </li>
            </ul>
          </AccordionSection>
          <AccordionSection heading="Contact us" headingTag="h3">
            <p>
              If you have a specific enquiry about [[ THEME ]] statistics and
              data:
            </p>
            <h4 className="govuk-heading-s govuk-!-margin-bottom-0">
              [[ TEAM NAME ]]
            </h4>
            <p className="govuk-!-margin-top-0">
              Email <br />
              <a href="mailto:schools.statistics@education.gov.uk">
                [[ TEAM EMAIL ADDRESS ]]
              </a>
            </p>
            <p>
              Telephone: [[ LEAD STATISTICIAN NAME ]] <br /> [[ LEAD
              STATISTICIAN TEL. NO.]]
            </p>
            <h4 className="govuk-heading-s govuk-!-margin-bottom-0">
              Press office
            </h4>
            <p className="govuk-!-margin-top-0">If you have a media enquiry:</p>
            <p>
              Telephone <br />
              020 7925 6789
            </p>
            <h4 className="govuk-heading-s govuk-!-margin-bottom-0">
              Public enquiries
            </h4>
            <p className="govuk-!-margin-top-0">
              If you have a general enquiry about the Department for Education
              (DfE) or education:
            </p>
            <p>
              Telephone <br />
              037 0000 2288
            </p>
          </AccordionSection>
        </Accordion>
        <h2 className="govuk-heading-m govuk-!-margin-top-9">
          Create your own tables online
        </h2>
        <p>
          Use our tool to build tables using our range of national and regional
          data.
        </p>
        <Link
          to={`/table-tool/${data.publication.slug}`}
          className="govuk-button"
        >
          Create tables
        </Link>

        <PrintThisPage />
      </Page>
    );
  }
}

export default PublicationReleasePage;
