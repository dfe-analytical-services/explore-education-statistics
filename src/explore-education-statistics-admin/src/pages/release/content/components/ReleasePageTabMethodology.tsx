import Link from '@admin/components/Link';
import ReleasePageTabPanel from '@admin/pages/release/content/components/ReleasePageTabPanel';
import { useReleaseContentState } from '@admin/pages/release/content/contexts/ReleaseContentContext';
import {
  preReleaseMethodologyRoute,
  PreReleaseMethodologyRouteParams,
} from '@admin/routes/preReleaseRoutes';
import ContactUsSection, {
  contactUsNavItem,
} from '@common/modules/find-statistics/components/ContactUsSectionRedesign';
import ReleasePageContentSection from '@common/modules/find-statistics/components/ReleasePageContentSection';
import ReleasePageLayout from '@common/modules/release/components/ReleasePageLayout';
import React, { useMemo } from 'react';
import { generatePath } from 'react-router';

interface MethodologyLink {
  key: string;
  title: string;
  url: string;
  external?: boolean;
}

interface Props {
  isPra?: boolean;
}

const ReleasePageTabMethodology = ({ isPra = false }: Props) => {
  const {
    release: { id: releaseVersionId, publication, publishingOrganisations },
  } = useReleaseContentState();

  const { methodologies, externalMethodology } = publication;

  const allMethodologies = useMemo<MethodologyLink[]>(() => {
    const mappedMethodologies = methodologies.map(methodology => ({
      key: methodology.id,
      title: methodology.title,
      url: isPra
        ? generatePath<PreReleaseMethodologyRouteParams>(
            preReleaseMethodologyRoute.path,
            {
              publicationId: publication.id,
              releaseVersionId,
              methodologyId: methodology.id,
            },
          )
        : `/methodology/${methodology.id}/summary`,
      external: false,
    }));

    if (externalMethodology) {
      mappedMethodologies.push({
        key: externalMethodology.url,
        title: externalMethodology.title,
        url: externalMethodology.url,
        external: true,
      });
    }

    return mappedMethodologies;
  }, [
    externalMethodology,
    isPra,
    methodologies,
    publication.id,
    releaseVersionId,
  ]);

  const navItems = useMemo(
    () =>
      [
        allMethodologies.length > 0 && {
          id: 'methodology-section',
          text: 'Methodology',
        },
        contactUsNavItem,
      ].filter(item => !!item),
    [allMethodologies.length],
  );

  return (
    <ReleasePageTabPanel tabKey="methodology">
      <ReleasePageLayout navItems={navItems}>
        {allMethodologies.length > 0 && (
          <ReleasePageContentSection
            heading="Methodology"
            id="methodology-section"
          >
            <p>
              Find out how and why we collect, process and publish these
              statistics.
            </p>
            <ul
              className="govuk-list govuk-list--spaced"
              data-testid="methodologies-list"
            >
              {allMethodologies.map(methodology => (
                <li key={methodology.key}>
                  <Link to={methodology.url}>
                    {methodology.title}
                    {methodology.external && ' (opens in new tab)'}
                  </Link>
                </li>
              ))}
            </ul>
          </ReleasePageContentSection>
        )}
        <ContactUsSection
          publicationContact={publication.contact}
          publicationTitle={publication.title}
          publishingOrganisations={publishingOrganisations}
        />
      </ReleasePageLayout>
    </ReleasePageTabPanel>
  );
};

export default ReleasePageTabMethodology;
