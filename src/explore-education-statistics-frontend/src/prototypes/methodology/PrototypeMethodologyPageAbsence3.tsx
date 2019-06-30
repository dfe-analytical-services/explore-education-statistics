import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import PrototypePage from '@frontend/prototypes/components/PrototypePage';
import PrototypeSearchForm from '@frontend/prototypes/components/PrototypeSearchForm';
import PrototypeSection1 from '@frontend/prototypes/methodology/absence3/PrototypeSection1';
import PrototypeSection2 from '@frontend/prototypes/methodology/absence3/PrototypeSection2';
import PrototypeSection3 from '@frontend/prototypes/methodology/absence3/PrototypeSection3';
import PrototypeSection4 from '@frontend/prototypes/methodology/absence3/PrototypeSection4';
import MethodologyContent from '@frontend/prototypes/methodology/components/MethodologyContent';
import MethodologyHeader from '@frontend/prototypes/methodology/components/MethodologyHeader';
import MethodologySection from '@frontend/prototypes/methodology/components/MethodologySection';
import React from 'react';

const PublicationPage = () => {
  return (
    <PrototypePage
      breadcrumbs={[
        {
          link: '/prototypes/methodology-home',
          text: 'Methodology',
        },
        {
          link: '/prototypes/methodology-specific',
          text: 'Specific methodology',
        },
      ]}
    >
      <h1 className="govuk-heading-xl">
        Secondary and primary school applications and offers: methodology
      </h1>
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <dl className="dfe-meta-content govuk-!-margin-0">
            <dt className="govuk-caption-m">Published: </dt>
            <dd>
              <strong>October 2018</strong>
            </dd>
          </dl>
        </div>
        <div className="govuk-grid-column-one-third">
          <PrototypeSearchForm />
        </div>
      </div>

      <hr />
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <p className="govuk-body-l">
            Find out about the methodology behind applications and offers
            statistics and data and how and why they're collected and published.
          </p>
        </div>

        <div className="govuk-grid-column-one-third">
          <aside className="app-related-items">
            <h2 className="govuk-heading-m" id="subsection-title">
              Related content
            </h2>
            <ul className="govuk-list">
              <li>
                <a href="/prototypes/publication">
                  Secondary and primary school applications and offers
                </a>
              </li>
            </ul>
          </aside>
        </div>
      </div>

      <Accordion id="contents-sections">
        <AccordionSection heading="1. Overview of applications and offers statistics">
          <MethodologySection>
            <MethodologyHeader>
              <h3 className="govuk-heading-s">In this section</h3>
              <ul className="govuk-body-s">
                <li>
                  <a href="#section1-1">1.1 Overview</a>
                </li>
              </ul>
            </MethodologyHeader>

            <MethodologyContent>
              <PrototypeSection1 />
            </MethodologyContent>
          </MethodologySection>
        </AccordionSection>

        <AccordionSection heading="2. The admissions process">
          <MethodologySection>
            <MethodologyHeader>
              <h3 className="govuk-heading-s">In this section</h3>
              <ul className="govuk-body-s">
                <li>
                  <a href="#section2-1">2.1 The admissions process</a>
                </li>
              </ul>
            </MethodologyHeader>
            <MethodologyContent>
              <PrototypeSection2 />
            </MethodologyContent>
          </MethodologySection>
        </AccordionSection>

        <AccordionSection heading="3. Methodology">
          <MethodologySection>
            <MethodologyHeader>
              <h3 className="govuk-heading-s">In this section</h3>
              <ul className="govuk-body-s">
                <li>
                  <a href="#section3-1">3.1 Collection and cleaning of data</a>
                </li>
                <li>
                  <a href="#section3-2">3.2 Persistent absence methodology</a>
                </li>
                <li>
                  <a href="#section3-3">3.3 Further information</a>
                </li>
              </ul>
            </MethodologyHeader>
            <MethodologyContent>
              <PrototypeSection3 />
            </MethodologyContent>
          </MethodologySection>
        </AccordionSection>

        <AccordionSection heading="4. Contacts">
          <PrototypeSection4 />
        </AccordionSection>
      </Accordion>

      <div className="govuk-!-margin-top-9">
        <a href="#print" className="govuk-link">
          Print this page
        </a>
      </div>
    </PrototypePage>
  );
};

export default PublicationPage;
