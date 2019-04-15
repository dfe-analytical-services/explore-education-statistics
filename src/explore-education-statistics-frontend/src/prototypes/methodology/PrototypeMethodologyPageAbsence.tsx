import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import React from 'react';
import PrototypePage from 'src/prototypes/components/PrototypePage';
import PrototypeSearchForm from 'src/prototypes/components/PrototypeSearchForm';
import PrototypeAnnexA from 'src/prototypes/methodology/absence/PrototypeAnnexA';
import PrototypeAnnexB from 'src/prototypes/methodology/absence/PrototypeAnnexB';
import PrototypeAnnexC from 'src/prototypes/methodology/absence/PrototypeAnnexC';
import PrototypeAnnexD from 'src/prototypes/methodology/absence/PrototypeAnnexD';
import PrototypeAnnexE from 'src/prototypes/methodology/absence/PrototypeAnnexE';
import PrototypeSection1 from 'src/prototypes/methodology/absence/PrototypeSection1';
import PrototypeSection2 from 'src/prototypes/methodology/absence/PrototypeSection2';
import PrototypeSection3 from 'src/prototypes/methodology/absence/PrototypeSection3';
import PrototypeSection4 from 'src/prototypes/methodology/absence/PrototypeSection4';
import PrototypeSection5 from 'src/prototypes/methodology/absence/PrototypeSection5';
import PrototypeSection7 from 'src/prototypes/methodology/absence/PrototypeSection7';
import { MethodologyContent } from 'src/prototypes/methodology/components/MethodologyContent';
import { MethodologyHeader } from 'src/prototypes/methodology/components/MethodologyHeader';
import { MethodologySection } from 'src/prototypes/methodology/components/MethodologySection';

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
        Pupil absence statistics: guidance and methodology
      </h1>
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <dl className="dfe-meta-content govuk-!-margin-0">
            <dt className="govuk-caption-m">Published: </dt>
            <dd>
              <strong>22 March 2018</strong>
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
        <AccordionSection heading="1. Introduction">
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
              </ul>
            </MethodologyHeader>

            <MethodologyContent>
              <PrototypeSection1 />
            </MethodologyContent>
          </MethodologySection>
        </AccordionSection>

        <AccordionSection heading="2. Background">
          <MethodologySection>
            <MethodologyHeader>
              <h3 className="govuk-heading-s">In this section</h3>
              <ul className="govuk-body-s">
                <li>
                  <a href="#section2-1">2.1 Current termly publications</a>
                </li>
                <li>
                  <a href="#section2-2">2.2 Key absence measures</a>
                </li>
                <li>
                  <a href="#section2-3">2.3 Cohort used in absence measures</a>
                </li>
                <li>
                  <a href="#section2-4">
                    2.4 The school year (five half terms vs six half terms)
                  </a>
                </li>
                <li>
                  <a href="#section2-5">
                    2.5 Published geographical and characteristics breakdowns
                  </a>
                </li>
                <li>
                  <a href="#section2-6">
                    2.6 Underlying data provided alongside publications
                  </a>
                </li>
                <li>
                  <a href="#section2-7">2.7 Suppression of absence data</a>
                </li>
                <li>
                  <a href="#section2-8">2.8 Other related publications</a>
                </li>
                <li>
                  <a href="#section2-9">
                    2.9 Devolved administration statistics on absence
                  </a>
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
                <li>
                  <a href="#section5-5">5.5 Data quality</a>
                </li>
              </ul>
            </MethodologyHeader>
            <MethodologyContent>
              <PrototypeSection5 />
            </MethodologyContent>
          </MethodologySection>
        </AccordionSection>

        <AccordionSection heading="6. Contacts">
          <PrototypeSection7 />
        </AccordionSection>
      </Accordion>

      <h2 className="govuk-heading-l govuk-!-margin-top-9">Annex</h2>
      <Accordion id="annex-sections">
        <AccordionSection heading="Annex A - Glossary">
          <PrototypeAnnexA />
        </AccordionSection>
        <AccordionSection heading="Annex B - Calculations">
          <PrototypeAnnexB />
        </AccordionSection>
        <AccordionSection heading="Annex C - School attendance codes">
          <PrototypeAnnexC />
        </AccordionSection>
        <AccordionSection heading="Annex D - Links to pupil absence national statistics publications">
          <PrototypeAnnexD />
        </AccordionSection>
        <AccordionSection heading="Annex E - Standard breakdowns that are currently published">
          <PrototypeAnnexE />
        </AccordionSection>
        <AccordionSection heading="Annex F - Timeline">
          <p>timeline</p>
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
