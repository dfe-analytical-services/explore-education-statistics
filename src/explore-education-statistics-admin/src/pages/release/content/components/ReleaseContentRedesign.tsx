import Link from '@admin/components/Link';
import { useEditingContext } from '@admin/contexts/EditingContext';
import ReleasePageTabBar from '@admin/pages/release/content/components/ReleasePageTabBar';
import ReleasePageTabExploreData from '@admin/pages/release/content/components/ReleasePageTabExploreData';
import ReleasePageTabHelp from '@admin/pages/release/content/components/ReleasePageTabHelp';
import ReleasePageTabHome from '@admin/pages/release/content/components/ReleasePageTabHome';
import ReleasePageTabMethodology from '@admin/pages/release/content/components/ReleasePageTabMethodology';
import ReleasePageTitle from '@admin/pages/release/content/components/ReleasePageTitle';
import { useReleaseContentState } from '@admin/pages/release/content/contexts/ReleaseContentContext';
import { getReleaseApprovalStatusLabel } from '@admin/pages/release/utils/releaseSummaryUtil';
import Tag from '@common/components/Tag';
import VisuallyHidden from '@common/components/VisuallyHidden';
import { useMobileMedia } from '@common/hooks/useMedia';
import ReleaseSummaryBlock from '@common/modules/release/components/ReleaseSummaryBlock';
import {
  formatPartialDate,
  isValidPartialDate,
} from '@common/utils/date/partialDate';
import React, { Fragment, useCallback, useState } from 'react';

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

interface Props {
  isPra?: boolean;
  handleFeaturedTableItemClick?: (id: string) => void;
  transformFeaturedTableLinks?: (url: string, text: string) => void;
}

const ReleaseContent = ({
  isPra = false,
  handleFeaturedTableItemClick,
  transformFeaturedTableLinks,
}: Props) => {
  const { release } = useReleaseContentState();
  const { setActiveSection } = useEditingContext();

  const { isMedia: isMobileMedia } = useMobileMedia();

  const [activeTabSection, setActiveTabSection] =
    useState<ReleasePageTabSectionKey>('home');

  const [renderedTabs, setRenderedTabs] = useState<ReleasePageTabSectionKey[]>([
    'home',
  ]);

  const handleChangeTab = useCallback(
    (sectionKey: ReleasePageTabSectionKey) => {
      if (!renderedTabs.includes(sectionKey)) {
        setRenderedTabs(prevTabs => [...prevTabs, sectionKey]);
      }
      setActiveTabSection(sectionKey);
      setActiveSection('summary-section');
    },
    [renderedTabs, setActiveSection],
  );

  const { nextReleaseDate, publication, publishingOrganisations, updates } =
    release;

  return (
    <>
      <ReleasePageTitle
        publicationSummary={publication.summary || ''}
        publicationTitle={publication.title}
        releaseTitle={release.title}
      />

      <div className="dfe-flex dfe-flex-wrap dfe-align-items--center dfe-gap-4 govuk-!-margin-bottom-6">
        <Tag>{getReleaseApprovalStatusLabel(release.approvalStatus)}</Tag>

        {isValidPartialDate(nextReleaseDate) && (
          <p className="govuk-!-margin-bottom-0">
            Next release{' '}
            <time
              className="govuk-!-font-weight-bold"
              data-testid="Next update"
            >
              {formatPartialDate(nextReleaseDate)}
            </time>
          </p>
        )}

        <span className="govuk-!-display-none-print">
          All releases in this series
        </span>
      </div>

      {!isMobileMedia && (
        <ReleaseSummaryBlock
          lastUpdated={updates[0]?.on}
          publishingOrganisations={release.publishingOrganisations}
          releaseDate={release.published ?? release.publishScheduled}
          releaseType={release.type}
          renderProducerLink={
            publishingOrganisations?.length ? (
              <span>
                {publishingOrganisations.map((org, index) => (
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
            updates.length > 0 ? (
              <span>
                {updates.length} update{updates.length === 1 ? '' : 's'}
                <VisuallyHidden>for {release.title}</VisuallyHidden>
              </span>
            ) : undefined
          }
          trackScroll
        />
      )}

      <ReleasePageTabBar
        activeTab={activeTabSection}
        onChangeTab={handleChangeTab}
      />

      {renderedTabs.includes('home') && (
        <ReleasePageTabHome
          hidden={activeTabSection !== 'home'}
          transformFeaturedTableLinks={transformFeaturedTableLinks}
        />
      )}
      {renderedTabs.includes('explore') && (
        <ReleasePageTabExploreData
          hidden={activeTabSection !== 'explore'}
          isPra={isPra}
          handleFeaturedTableItemClick={handleFeaturedTableItemClick}
        />
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
