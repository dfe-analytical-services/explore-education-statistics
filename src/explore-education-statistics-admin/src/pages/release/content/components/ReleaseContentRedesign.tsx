import Link from '@admin/components/Link';
import ReleasePageTabBar from '@admin/pages/release/content/components/ReleasePageTabBar';
import ReleasePageTabExploreData from '@admin/pages/release/content/components/ReleasePageTabExploreData';
import ReleasePageTabHelp from '@admin/pages/release/content/components/ReleasePageTabHelp';
import ReleasePageTabHome from '@admin/pages/release/content/components/ReleasePageTabHome';
import ReleasePageTabMethodology from '@admin/pages/release/content/components/ReleasePageTabMethodology';
import ReleasePageTitle from '@admin/pages/release/content/components/ReleasePageTitle';
import { useReleaseContentState } from '@admin/pages/release/content/contexts/ReleaseContentContext';
import VisuallyHidden from '@common/components/VisuallyHidden';
import ReleaseSummaryBlock from '@common/modules/release/components/ReleaseSummaryBlock';
import React, { Fragment, useEffect, useState } from 'react';

export const releasePageTabSections = {
  home: {
    title: 'Release home',
  },
  explore: {
    title: 'Explore and download data',
  },
  methodology: {
    title: 'Methodology',
  },
  help: {
    title: 'Help and related information',
  },
} as const;

export type ReleasePageTabSectionItems = typeof releasePageTabSections;
export type ReleasePageTabSectionKey = keyof ReleasePageTabSectionItems;

const ReleaseContent = () => {
  const { release } = useReleaseContentState();

  const [activeTabSection, setActiveTabSection] =
    useState<ReleasePageTabSectionKey>('home');

  const [renderedTabs, setRenderedTabs] = useState<ReleasePageTabSectionKey[]>([
    'home',
  ]);

  useEffect(() => {
    if (!renderedTabs.includes(activeTabSection)) {
      setRenderedTabs(prevTabs => [...prevTabs, activeTabSection]);
    }
  }, [activeTabSection, renderedTabs]);

  const { publication } = release;

  return (
    <>
      <ReleasePageTitle
        publicationSummary={publication.summary || ''}
        publicationTitle={publication.title}
        releaseTitle={release.title}
      />

      <ReleaseSummaryBlock
        lastUpdated={release.updates[0]?.on}
        releaseDate={release.published}
        releaseType={release.type}
        renderProducerLink={
          release.publishingOrganisations?.length ? (
            <span>
              {release.publishingOrganisations.map((org, index) => (
                <Fragment key={org.id}>
                  {index > 0 && ' and '}
                  <Link unvisited to={org.url}>
                    {org.title}
                  </Link>
                </Fragment>
              ))}
            </span>
          ) : (
            <Link
              unvisited
              className="govuk-link--no-underline"
              to="https://www.gov.uk/government/organisations/department-for-education"
            >
              Department for Education
            </Link>
          )
        }
        renderUpdatesLink={
          release.updates.length > 1 ? (
            <Link to="#">
              {release.updates.length} updates{' '}
              <VisuallyHidden>for {release.title}</VisuallyHidden>
            </Link>
          ) : undefined
        }
      />

      <ReleasePageTabBar
        activeTab={activeTabSection}
        onChangeTab={setActiveTabSection}
      />

      {renderedTabs.includes('home') && (
        <ReleasePageTabHome hidden={activeTabSection !== 'home'} />
      )}
      {renderedTabs.includes('explore') && (
        <ReleasePageTabExploreData hidden={activeTabSection !== 'explore'} />
      )}
      {renderedTabs.includes('methodology') && (
        <ReleasePageTabMethodology
          hidden={activeTabSection !== 'methodology'}
        />
      )}
      {renderedTabs.includes('help') && (
        <ReleasePageTabHelp hidden={activeTabSection !== 'help'} />
      )}
    </>
  );
};

export default ReleaseContent;
