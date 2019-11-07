import Details from '@common/components/Details';
import FormattedDate from '@common/components/FormattedDate';
import PageSearchForm from '@common/components/PageSearchForm';
import RelatedAside from '@common/components/RelatedAside';
import { baseUrl } from '@common/services/api';
import { AbstractRelease, Publication, Release, ReleaseType } from '@common/services/publicationService';
import { Dictionary } from '@common/types';
import classNames from 'classnames';
import {LocationDescriptor} from 'history';
import React, { AnchorHTMLAttributes, PropsWithChildren, ReactNode } from 'react';

interface RendererProps {
  releaseId: string;
}

export interface TextRendererProps extends RendererProps {
  children: React.ReactNode | React.ReactNode[]
}

export interface MarkdownRendererProps extends RendererProps {
  source: string;
}

export type LinkProps = {
  children: ReactNode;
  as?: string;

  className?: string;
  prefetch?: boolean;
  to: LocationDescriptor;
  unvisited?: boolean;
  analytics?: unknown;
} & AnchorHTMLAttributes<HTMLAnchorElement>;

type PrintThisPageProps = {
  analytics?: unknown;
} & AnchorHTMLAttributes<HTMLAnchorElement>;

  type LinkTypeProps = PropsWithChildren<LinkProps> & { children: ReactNode};
type TextRendererType = React.ComponentType<TextRendererProps>
type MarkdownRendererType = React.ComponentType<MarkdownRendererProps>
type LinkType = React.ComponentType<LinkProps>;
type PrintThisPageType = React.ComponentType<PrintThisPageProps>


interface Props {
  data: Release,
  styles: Dictionary<string>

  TextRenderer: TextRendererType;
  MarkdownRenderer: MarkdownRendererType;
  SearchForm: typeof PageSearchForm;
  PrintThisPage: PrintThisPageType;
  Link: LinkType;
  logEvent?: (...params : string[]) => void
}

const nullLogEvent = () => {};

const PublicationReleaseContent = ({
  data,
  styles,
  TextRenderer,
  MarkdownRenderer,
  SearchForm,
  PrintThisPage,
  Link,
  logEvent = nullLogEvent
}: Props) => {

  const releaseCount = data.publication.releases ?
    data.publication.releases.length + data.publication.legacyReleases.length
    : -1;

  return (
    <>
      <div className={classNames('govuk-grid-row', styles.releaseIntro)}>
        <div className="govuk-grid-column-two-thirds">
          <div className="govuk-grid-row">
            <div className="govuk-grid-column-three-quarters">
              {data.latestRelease ? (
                <strong className="govuk-tag govuk-!-margin-right-6">
                  {' '}
                  This is the latest data{' '}
                </strong>
              ) : (
                <Link
                  className="dfe-print-hidden"
                  unvisited
                  to={`/find-statistics/${data.publication.slug}`}
                >
                  View latest data:{' '}
                  <span className="govuk-!-font-weight-bold">
                    {data.publication.releases.slice(-1)[0].title}
                  </span>
                </Link>
              )}
              <dl className="dfe-meta-content govuk-!-margin-top-3 govuk-!-margin-bottom-1">
                <dt className="govuk-caption-m">Published:</dt>
                <dd data-testid="published-date">
                  <strong>
                    <FormattedDate>{data.published}</FormattedDate>{' '}
                  </strong>
                </dd>
                <div>
                  <dt className="govuk-caption-m">Next update:</dt>
                  <dd data-testid="next-update">
                    <strong>
                      <FormattedDate format="MMMM yyyy">
                        {data.publication.nextUpdate}
                      </FormattedDate>
                    </strong>
                  </dd>
                </div>
              </dl>
              <Link
                className="dfe-print-hidden"
                unvisited
                analytics={{
                  category: 'Subscribe',
                  action: 'Email subscription',
                }}
                to={`/subscriptions?slug=${data.publication.slug}`}
                data-testid={`subscription-${data.publication.slug}`}
              >
                Sign up for email alerts
              </Link>
            </div>
            <div className="govuk-grid-column-one-quarter">
              {data.type &&
              data.type.title === ReleaseType.NationalStatistics && (
                <img
                  src="/static/images/UKSA-quality-mark.jpg"
                  alt="UK statistics authority quality mark"
                  height="120"
                  width="120"
                />
              )}
            </div>
          </div>

          <MarkdownRenderer releaseId={data.id} source={data.summary} />

          {data.downloadFiles && (
            <Details
              summary="Download data files"
              onToggle={(open: boolean) =>
                open &&
                logEvent(
                  'Downloads',
                  'Release page download data files dropdown opened',
                  window.location.pathname,
                )
              }
            >
              <ul className="govuk-list govuk-list--bullet">
                {data.downloadFiles.map(({ extension, name, path, size }) => (
                  <li key={path}>
                    <Link
                      to={`${baseUrl.data}/download/${path}`}
                      className="govuk-link"
                      analytics={{
                        category: 'Downloads',
                        action: `Release page ${name} file downloaded`,
                        label: `File URL: /api/download/${path}`,
                      }}
                    >
                      {name}
                    </Link>
                    {` (${extension}, ${size})`}
                  </li>
                ))}
              </ul>
            </Details>
          )}
          <SearchForm
            inputLabel="Search in this release page."
            className="govuk-!-margin-top-3 govuk-!-margin-bottom-3"
          />
        </div>

        <div className="govuk-grid-column-one-third">
          <PrintThisPage
            analytics={{
              category: 'Page print',
              action: 'Print this page link selected',
            }}
          />
          <RelatedAside>
            <h2 className="govuk-heading-m">About these statistics</h2>

            <dl className="dfe-meta-content">
              <dt className="govuk-caption-m">For {data.coverageTitle}:</dt>
              <dd data-testid="release-name">
                <strong>{data.yearTitle}</strong>
              </dd>
              <dd>
                <Details
                  summary={`See previous ${releaseCount} releases`}
                  onToggle={(open: boolean) =>
                    open &&
                    logEvent(
                      'Previous Releases',
                      'Release page previous releases dropdown opened',
                      window.location.pathname,
                    )
                  }
                >
                  <ul className="govuk-list">
                    {[
                      ...data.publication.releases.map(
                        ({ id, slug, releaseName }) => [
                          releaseName,
                          <li key={id} data-testid="previous-release-item">
                            <Link
                              to={`/find-statistics/${data.publication.slug}/${slug}`}
                            >
                              {releaseName}
                            </Link>
                          </li>,
                        ],
                      ),
                      ...data.publication.legacyReleases.map(
                        ({ id, description, url }) => [
                          description,
                          <li key={id} data-testid="previous-release-item">
                            <a href={url}>{description}</a>
                          </li>,
                        ],
                      ),
                    ]
                      .sort((a, b) =>
                        b[0].toString().localeCompare(a[0].toString()),
                      )
                      .map(items => items[1])}
                  </ul>
                </Details>
              </dd>
            </dl>
            <dl className="dfe-meta-content">
              <dt className="govuk-caption-m">Last updated:</dt>
              <dd data-testid="last-updated">
                <strong>
                  <FormattedDate>{data.updates[0].on}</FormattedDate>
                </strong>
                <Details
                  onToggle={(open: boolean) =>
                    open &&
                    logEvent(
                      'Last Updates',
                      'Release page last updates dropdown opened',
                      window.location.pathname,
                    )
                  }
                  summary={`See all ${data.updates.length} updates`}
                >
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
            <h2
              className="govuk-heading-m govuk-!-margin-top-6"
              id="related-content"
            >
              Related guidance
            </h2>
            <nav role="navigation" aria-labelledby="related-content">
              <ul className="govuk-list">
                <li>
                  <Link to={`/methodology/${data.publication.slug}`}>
                    {`${data.publication.title}: methodology`}
                  </Link>
                </li>
              </ul>
            </nav>
          </RelatedAside>
        </div>
      </div>
    </>
  );

};

export default PublicationReleaseContent;