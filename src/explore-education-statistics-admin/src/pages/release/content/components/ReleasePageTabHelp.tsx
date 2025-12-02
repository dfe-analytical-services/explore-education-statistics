import Link from '@admin/components/Link';
import ReleasePageTabPanel from '@admin/pages/release/content/components/ReleasePageTabPanel';
import { useReleaseContentState } from '@admin/pages/release/content/contexts/ReleaseContentContext';
import releaseQueries from '@admin/queries/releaseQueries';
import ContentHtml from '@common/components/ContentHtml';
import LoadingSpinner from '@common/components/LoadingSpinner';
import ContactUsSection, {
  contactUsNavItem,
} from '@common/modules/find-statistics/components/ContactUsSectionRedesign';
import ReleasePageContentSection from '@common/modules/find-statistics/components/ReleasePageContentSection';
import ReleasePageLayout from '@common/modules/release/components/ReleasePageLayout';
import ReleaseTypeSection from '@common/modules/release/components/ReleaseTypeSection';
import { releaseTypes } from '@common/services/types/releaseType';
import { useQuery } from '@tanstack/react-query';
import React, { useMemo } from 'react';

interface Props {
  hidden: boolean;
}

const ReleasePageTabHelp = ({ hidden }: Props) => {
  const { release } = useReleaseContentState();

  const {
    publication,
    publishingOrganisations,
    relatedInformation,
    hasPreReleaseAccessList,
    type,
  } = release;

  const {
    data: releaseVersion,
    isLoading,
    isError,
  } = useQuery(releaseQueries.get(release.id));

  const hasRelatedInformation = relatedInformation.length > 0;

  const navItems = useMemo(
    () =>
      [
        { ...contactUsNavItem, text: 'Get help by contacting us' },
        {
          id: 'release-type-section',
          text: releaseTypes[type],
        },
        hasRelatedInformation && {
          id: 'related-information-section',
          text: 'Related information',
        },
        hasPreReleaseAccessList && {
          id: 'pre-release-access-list-section',
          text: 'Pre-release access list',
        },
      ].filter(item => !!item),
    [hasPreReleaseAccessList, hasRelatedInformation, type],
  );

  return (
    <ReleasePageTabPanel tabKey="help" hidden={hidden}>
      <ReleasePageLayout navItems={navItems}>
        <ContactUsSection
          publicationContact={publication.contact}
          publicationTitle={publication.title}
          publishingOrganisations={publishingOrganisations}
          sectionTitle="Get help by contacting us"
          includeSectionBreak
        />

        <ReleasePageContentSection
          heading={releaseTypes[type]}
          id="release-type-section"
          includeSectionBreak={hasPreReleaseAccessList || hasRelatedInformation}
        >
          <ReleaseTypeSection
            publishingOrganisations={publishingOrganisations}
            type={type}
            showHeading={false}
          />
        </ReleasePageContentSection>

        {hasRelatedInformation && (
          <ReleasePageContentSection
            heading="Related information"
            id="related-information-section"
            includeSectionBreak={hasPreReleaseAccessList}
          >
            <ul
              className="govuk-list govuk-list--spaced"
              data-testid="related-information-list"
            >
              {relatedInformation.map(({ id, description, url }) => (
                <li key={id}>
                  <Link to={url}>{description}</Link>
                </li>
              ))}
            </ul>
          </ReleasePageContentSection>
        )}

        {hasPreReleaseAccessList && (
          <ReleasePageContentSection
            heading="Pre-release access list"
            id="pre-release-access-list-section"
            includeSectionBreak={false}
          >
            <LoadingSpinner loading={isLoading}>
              {isError || !releaseVersion?.preReleaseAccessList ? (
                <p>Failed to fetch the pre-release access list</p>
              ) : (
                <ContentHtml html={releaseVersion.preReleaseAccessList} />
              )}
            </LoadingSpinner>
          </ReleasePageContentSection>
        )}
      </ReleasePageLayout>
    </ReleasePageTabPanel>
  );
};

export default ReleasePageTabHelp;
