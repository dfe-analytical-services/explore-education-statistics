import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import PrototypePage from '@frontend/prototypes/components/PrototypePage';
import PrototypeSearchForm from '@frontend/prototypes/components/PrototypeSearchForm';
import PrototypeAnnexA from '@frontend/prototypes/methodology/absence/PrototypeAnnexA';
import PrototypeAnnexB from '@frontend/prototypes/methodology/absence/PrototypeAnnexB';
import PrototypeAnnexC from '@frontend/prototypes/methodology/absence/PrototypeAnnexC';
import PrototypeAnnexD from '@frontend/prototypes/methodology/absence/PrototypeAnnexD';
import PrototypeAnnexE from '@frontend/prototypes/methodology/absence/PrototypeAnnexE';
import PrototypeSection1 from '@frontend/prototypes/methodology/absence/PrototypeSection1';
import PrototypeSection2 from '@frontend/prototypes/methodology/absence/PrototypeSection2';
import PrototypeSection3 from '@frontend/prototypes/methodology/absence/PrototypeSection3';
import PrototypeSection4 from '@frontend/prototypes/methodology/absence/PrototypeSection4';
import PrototypeSection5 from '@frontend/prototypes/methodology/absence/PrototypeSection5';
import PrototypeSection6 from '@frontend/prototypes/methodology/absence/PrototypeSection6';
import PrototypeSection7 from '@frontend/prototypes/methodology/absence/PrototypeSection7';
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
        Pupil absence in schools in England: methodology
      </h1>
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <dl className="dfe-meta-content govuk-!-margin-0">
            <dt className="govuk-caption-m">Published: </dt>
            <dd>
              <strong>March 2019</strong>
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
            Find out about the methodology behind pupil absence statistics and
            data and how and why they're collected and published.
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
                  Pupil absence statistics and data for schools in England
                </a>
              </li>
            </ul>
          </aside>
        </div>
      </div>

      <Accordion id="contents-sections">
        <AccordionSection heading="1. Overview of absence statistics">
          <MethodologySection>
            <MethodologyHeader>
              <h3 className="govuk-heading-s">In this section</h3>
              <ul className="govuk-body-s">
                <li>
                  <a href="#section1-1">
                    1.1 Pupil attendance requirements for schools
                  </a>
                </li>
                <li>
                  <a href="#section1-2">
                    1.2 Uses and users of absence statistics and data
                  </a>
                </li>
                <li>
                  <a href="#section1-3">1.3 Current termly publications</a>
                </li>
                <li>
                  <a href="#section1-4">1.4 Key absence measures</a>
                </li>
                <li>
                  <a href="#section1-5">1.5 Cohort used in absence measures</a>
                </li>
                <li>
                  <a href="#section1-6">
                    1.6 The school year (five half terms vs six half terms)
                  </a>
                </li>
                <li>
                  <a href="#section1-7">
                    1.7 Published geographical and characteristics breakdowns
                  </a>
                </li>
                <li>
                  <a href="#section1-8">
                    1.8 Underlying data provided alongside publications
                  </a>
                </li>
                <li>
                  <a href="#section1-9">1.9 Suppression of absence data</a>
                </li>
                <li>
                  <a href="#section1-10">1.10 Other related publications</a>
                </li>
                <li>
                  <a href="#section1-11">
                    1.11 Devolved administration statistics on absence
                  </a>
                </li>
              </ul>
            </MethodologyHeader>

            <MethodologyContent>
              <PrototypeSection1 />
            </MethodologyContent>
          </MethodologySection>
        </AccordionSection>

        <AccordionSection heading="2. National Statistics badging">
          <MethodologySection>
            <MethodologyHeader>
              <h3 className="govuk-heading-s">In this section</h3>
              <ul className="govuk-body-s">
                <li>
                  <a href="#section2-1">2.1 National Statistics designation</a>
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
                  <a href="#section3-1">3.1 Overall absence methodology</a>
                </li>
                <li>
                  <a href="#section3-2">3.2 Persistent absence methodology</a>
                </li>
              </ul>
            </MethodologyHeader>
            <MethodologyContent>
              <PrototypeSection3 />
            </MethodologyContent>
          </MethodologySection>
        </AccordionSection>

        <AccordionSection heading="4. Data collection">
          <MethodologySection>
            <MethodologyHeader>
              <h3 className="govuk-heading-s">In this section</h3>
              <ul className="govuk-body-s">
                <li>
                  <a href="#section4-1">
                    4.1 The current process 2005/06 to present
                  </a>
                </li>
                <li>
                  <a href="#section4-2">
                    4.2 Background of absence data collection
                  </a>
                </li>
                <li>
                  <a href="#section4-3">4.3 Data coverage</a>
                </li>
                <li>
                  <a href="#section4-4">4.4 What absence data is collected</a>
                </li>
                <li>
                  <a href="#section4-5">
                    4.5 No longer collected but available historically
                  </a>
                </li>
                <li>
                  <a href="#section4-6">
                    4.6 What absence data is not collected
                  </a>
                </li>
              </ul>
            </MethodologyHeader>
            <MethodologyContent>
              <PrototypeSection4 />
            </MethodologyContent>
          </MethodologySection>
        </AccordionSection>

        <AccordionSection heading="5. Data processing">
          <MethodologySection>
            <MethodologyHeader>
              <h3 className="govuk-heading-s">In this section</h3>
              <ul className="govuk-body-s">
                <li>
                  <a href="#section5-1">5.1 Data linking</a>
                </li>
                <li>
                  <a href="#section5-2">5.2 Data removed</a>
                </li>
                <li>
                  <a href="#section5-3">5.3 Variables added</a>
                </li>
                <li>
                  <a href="#section5-4">5.4 Consistency checks</a>
                </li>
              </ul>
            </MethodologyHeader>
            <MethodologyContent>
              <PrototypeSection5 />
            </MethodologyContent>
          </MethodologySection>
        </AccordionSection>

        <AccordionSection heading="6. Data quality">
          <MethodologySection>
            <MethodologyHeader>
              <h3 className="govuk-heading-s">In this section</h3>
              <ul className="govuk-body-s">
                <li>
                  <a href="#section6-1">6.1 Data quality</a>
                </li>
              </ul>
            </MethodologyHeader>
            <MethodologyContent>
              <PrototypeSection6 />
            </MethodologyContent>
          </MethodologySection>
        </AccordionSection>

        <AccordionSection heading="7. Contacts">
          <PrototypeSection7 />
        </AccordionSection>
      </Accordion>

      <h2 className="govuk-heading-l govuk-!-margin-top-9">Annexes</h2>
      <Accordion id="annex-sections">
        <AccordionSection heading="Annex A - Calculations">
          <PrototypeAnnexA />
        </AccordionSection>
        <AccordionSection heading="Annex B - School attendance codes">
          <PrototypeAnnexB />
        </AccordionSection>
        <AccordionSection heading="Annex C - Links to pupil absence national statistics and data">
          <PrototypeAnnexC />
        </AccordionSection>
        <AccordionSection heading="Annex D - Standard breakdowns">
          <PrototypeAnnexD />
        </AccordionSection>
        <AccordionSection heading="Annex E - Timeline">
          <PrototypeAnnexE />
        </AccordionSection>
        <AccordionSection heading="Annex F - Absence rates over time">
          <PrototypeAnnexE />
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
