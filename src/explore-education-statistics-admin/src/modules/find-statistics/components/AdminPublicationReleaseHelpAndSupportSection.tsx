import { ManageContentPageViewModel } from '@admin/services/release/edit-release/content/types';
import Accordion from '@admin/components/EditableAccordion';
import AccordionSection from '@admin/components/EditableAccordionSection';
import Link from '@admin/components/Link';
import React from 'react';
import { ReleaseType } from '@common/services/publicationService';
import NationalStatisticsSection from '@common/modules/find-statistics/components/NationalStatisticsSection';
import ContactUsSection from '@common/modules/find-statistics/components/ContactUsSection';

const AdminPublicationReleaseHelpAndSupportSection = ({
  release,
  publication,
  editing,
}: {
  publication: ManageContentPageViewModel['release']['publication'];
  release: ManageContentPageViewModel['release'];
  editing?: boolean;
}) => {
  return (
    <>
      <h2
        className="govuk-heading-m govuk-!-margin-top-9"
        data-testid="extra-information"
      >
        Help and support
      </h2>

      <Accordion id="static-content-section" canReorder={false}>
        <AccordionSection
          heading="Methodology"
          caption="Find out how and why we collect, process and publish these statistics"
          headingTag="h3"
        >
          {publication.methodology || publication.externalMethodology ? (
            <p>
              Read our{' '}
              {publication.methodology &&
                (editing ? (
                  <a>{`${publication.title}: methodology`}</a>
                ) : (
                  <Link to={`/methodologies/${publication.methodology.id}`}>
                    {`${publication.title}: methodology`}
                  </Link>
                ))}
              {!publication.methodology &&
                publication.externalMethodology &&
                (editing ? (
                  <a>{`${publication.title}: methodology`}</a>
                ) : (
                  <Link
                    to=""
                    rel="external"
                    target="_blank"
                    href={publication.externalMethodology.url}
                  >
                    {`${publication.title}: methodology`}
                  </Link>
                ))}{' '}
              guidance.
            </p>
          ) : (
            <p>No methodology added.</p>
          )}
        </AccordionSection>
        {release.type && release.type.title === ReleaseType.NationalStatistics && (
          <AccordionSection heading="National Statistics" headingTag="h3">
            <NationalStatisticsSection />
          </AccordionSection>
        )}
        <AccordionSection heading="Contact us" headingTag="h3">
          <ContactUsSection
            editing={editing}
            publicationContact={publication.contact}
            themeTitle={publication.topic.theme.title}
          />
        </AccordionSection>
      </Accordion>
    </>
  );
};

export default AdminPublicationReleaseHelpAndSupportSection;
