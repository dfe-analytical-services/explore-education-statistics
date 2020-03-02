import Link from '@admin/components/Link';
import { ManageContentPageViewModel } from '@admin/services/release/edit-release/content/types';
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import ContactUsSection from '@common/modules/find-statistics/components/ContactUsSection';
import NationalStatisticsSection from '@common/modules/find-statistics/components/NationalStatisticsSection';
import { ReleaseType } from '@common/services/publicationService';
import React from 'react';

const AdminPublicationReleaseHelpAndSupportSection = ({
  release,
}: {
  release: ManageContentPageViewModel['release'];
}) => {
  return (
    <>
      <h2
        className="govuk-heading-m govuk-!-margin-top-9"
        data-testid="extra-information"
      >
        Help and support
      </h2>

      <Accordion id="static-content-section">
        <AccordionSection
          heading="Methodology"
          caption="Find out how and why we collect, process and publish these statistics"
          headingTag="h3"
        >
          {release.publication.methodology ||
          release.publication.externalMethodology ? (
            <p>
              Read our{' '}
              {release.publication.methodology && (
                <Link
                  to={`/methodologies/${release.publication.methodology.id}`}
                >
                  {`${release.publication.title}: methodology`}
                </Link>
              )}
              {!release.publication.methodology &&
                release.publication.externalMethodology && (
                  <Link
                    to=""
                    rel="external"
                    target="_blank"
                    href={release.publication.externalMethodology.url}
                  >
                    {`${release.publication.title}: methodology`}
                  </Link>
                )}{' '}
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
            publicationContact={release.publication.contact}
            themeTitle={release.publication.topic.theme.title}
          />
        </AccordionSection>
      </Accordion>
    </>
  );
};

export default AdminPublicationReleaseHelpAndSupportSection;
