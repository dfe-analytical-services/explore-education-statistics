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
}: {
  publication: ManageContentPageViewModel['release']['publication'];
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

      <Accordion
        // publicationTitle={publication.title}
        id="static-content-section"
        canReorder={false}
      >
        <AccordionSection
          heading={`${publication.title}: methodology`}
          caption="Find out how and why we collect, process and publish these statistics"
          headingTag="h3"
        >
          <p>
            Read our{' '}
            <Link to={`/methodology/${release.publication.methodology.id}`}>
              {`${publication.title}: methodology`}
            </Link>{' '}
            guidance.
          </p>
        </AccordionSection>
        {release.type && release.type.title === ReleaseType.NationalStatistics && (
          <AccordionSection heading="National Statistics" headingTag="h3">
            <NationalStatisticsSection />
          </AccordionSection>
        )}
        <AccordionSection heading="Contact us" headingTag="h3">
          <ContactUsSection
            publicationContact={publication.contact}
            themeTitle={publication.topic.theme.title}
          />
        </AccordionSection>
      </Accordion>
    </>
  );
};

export default AdminPublicationReleaseHelpAndSupportSection;
