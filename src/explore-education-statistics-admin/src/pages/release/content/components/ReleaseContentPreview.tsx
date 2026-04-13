import Link from '@admin/components/Link';
import ReleasePageTabBar from '@admin/pages/release/content/components/ReleasePageTabBar';
import ReleasePageTabExploreData from '@admin/pages/release/content/components/ReleasePageTabExploreData';
import ReleasePageTabHelp from '@admin/pages/release/content/components/ReleasePageTabHelp';
import ReleasePageTabHome from '@admin/pages/release/content/components/ReleasePageTabHome';
import ReleasePageTabMethodology from '@admin/pages/release/content/components/ReleasePageTabMethodology';
import PublishingOrganisations from '@common/modules/find-statistics/components/PublishingOrganisations';
import ReleasePageTitle from '@admin/pages/release/content/components/ReleasePageTitle';
import { useReleaseContentState } from '@admin/pages/release/content/contexts/ReleaseContentContext';
import { getReleaseApprovalStatusLabel } from '@admin/pages/release/utils/releaseSummaryUtil';
import releaseFileService from '@admin/services/releaseFileService';
import ButtonText from '@common/components/ButtonText';
import InsetText from '@common/components/InsetText';
import Tag from '@common/components/Tag';
import VisuallyHidden from '@common/components/VisuallyHidden';
import { useMobileMedia } from '@common/hooks/useMedia';
import ReleaseSummaryBlock from '@common/modules/release/components/ReleaseSummaryBlock';
import {
  formatPartialDate,
  isValidPartialDate,
} from '@common/utils/date/partialDate';
import getListStringSeparator from '@common/utils/string/getListStringSeparator';
import React, { Fragment, useState } from 'react';

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
  const { isMedia: isMobileMedia } = useMobileMedia();

  const [activeTab, setActiveTab] = useState<ReleasePageTabSectionKey>('home');

  const { nextReleaseDate, publication, publishingOrganisations, updates } =
    release;

  return (
    <>
      <InsetText>
        <p>
          You are viewing the new design of the Release page - if you would like
          to provide feedback, please complete{' '}
          <Link
            to="https://forms.office.com/e/sBRKZgs6zB"
            target="_blank"
            rel="noopener noreferrer nofollow"
          >
            our feedback form (opens in new window)
          </Link>
        </p>
      </InsetText>

      <PublishingOrganisations
        publishingOrganisations={publishingOrganisations}
      />

      <ReleasePageTitle
        publicationSummary={publication.summary || ''}
        publicationTitle={publication.title}
        releaseTitle={release.title}
      />

      <div className="dfe-flex dfe-flex-wrap dfe-align-items--center dfe-gap-4 govuk-!-margin-bottom-6">
        <div className="dfe-flex dfe-flex-wrap dfe-align-items--center dfe-gap-4 dfe-flex-grow--1">
          <Tag>{getReleaseApprovalStatusLabel(release.approvalStatus)}</Tag>

          {isValidPartialDate(nextReleaseDate) && (
            <p className="govuk-!-margin-bottom-0">
              Next release{' '}
              <time
                className="govuk-!-font-weight-bold"
                data-testid="Next release"
              >
                {formatPartialDate(nextReleaseDate)}
              </time>
            </p>
          )}

          <span className="govuk-!-display-none-print">
            All releases in this series
          </span>
        </div>

        <div className="dfe-flex dfe-flex-wrap dfe-align-items--center dfe-gap-4">
          <ButtonText
            preventDoubleClick
            onClick={() => releaseFileService.downloadFilesAsZip(release.id)}
          >
            Download all data (ZIP)
          </ButtonText>
          <span>Create your own tables</span>
        </div>
      </div>

      {!isMobileMedia && (
        <ReleaseSummaryBlock
          lastUpdated={release.lastUpdated}
          publishingOrganisations={release.publishingOrganisations}
          releaseDate={release.publishedDisplayDate}
          releaseType={release.type}
          renderProducerLink={
            publishingOrganisations?.length ? (
              <span>
                {publishingOrganisations.map((org, index) => (
                  <Fragment key={org.id}>
                    {getListStringSeparator(
                      release.publishingOrganisations ?? [],
                      index,
                    )}
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

      <ReleasePageTabBar activeTab={activeTab} onChangeTab={setActiveTab} />

      {activeTab === 'home' && (
        <ReleasePageTabHome
          transformFeaturedTableLinks={transformFeaturedTableLinks}
        />
      )}
      {activeTab === 'explore' && (
        <ReleasePageTabExploreData
          isPra={isPra}
          handleFeaturedTableItemClick={handleFeaturedTableItemClick}
        />
      )}
      {activeTab === 'methodology' && (
        <ReleasePageTabMethodology isPra={isPra} />
      )}
      {activeTab === 'help' && <ReleasePageTabHelp />}
    </>
  );
};

export default ReleaseContent;
