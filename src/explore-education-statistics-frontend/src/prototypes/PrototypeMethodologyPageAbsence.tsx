import React from 'react';
import Accordion from '../components/Accordion';
import AccordionSection from '../components/AccordionSection';
import Details from '../components/Details';
import GoToTopLink from '../components/GoToTopLink';
import Link from '../components/Link';
import PrototypeAnnexA from './components/methodology/absence/PrototypeAnnexA';
import PrototypeAnnexB from './components/methodology/absence/PrototypeAnnexB';
import PrototypeAnnexC from './components/methodology/absence/PrototypeAnnexC';
import PrototypeAnnexD from './components/methodology/absence/PrototypeAnnexD';
import PrototypeAnnexE from './components/methodology/absence/PrototypeAnnexE';
import PrototypeSection1 from './components/methodology/absence/PrototypeSection1';
import PrototypeSection2 from './components/methodology/absence/PrototypeSection2';
import PrototypeSection3 from './components/methodology/absence/PrototypeSection3';
import PrototypeSection4 from './components/methodology/absence/PrototypeSection4';
import PrototypeSection5 from './components/methodology/absence/PrototypeSection5';
import PrototypeSection6 from './components/methodology/absence/PrototypeSection6';
import PrototypeSection7 from './components/methodology/absence/PrototypeSection7';
import PrototypeAbsenceData from './components/PrototypeAbsenceData';
import PrototypeDataSample from './components/PrototypeDataSample';
import PrototypeMap from './components/PrototypeMap';
import PrototypePage from './components/PrototypePage';

const PublicationPage = () => {
  return (
    <PrototypePage
      breadcrumbs={[
        {
          link: '/prototypes/browse-releases-find',
          text: 'Methodology',
        },
        { text: 'Specific methodology', link: '#' },
      ]}
    >
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <h1 className="govuk-heading-xl">A guide to absence statistics</h1>
          <h2 className="govuk-heading-s">
            <span className="govuk-caption-m">Published: </span>March 2018
          </h2>
          <p className="govuk-body-l">
            This document provides a comprehensive guide to the pupil absence in
            schools in England statistics published by the Department for
            Education.
          </p>
          <p>The key areas covered in this guide are:</p>
          <ul className="govuk-list govuk-list--bullet">
            <li>background to published statistics and methodology</li>
            <li>data collection and coverage</li>
            <li>data processing</li>
          </ul>
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
      <h2 className="govuk-heading-l govuk-!-margin-top-6">Contents</h2>
      <Accordion id="contents-sections">
        <AccordionSection heading="Introduction">
          <div className="govuk-grid-row">
            <div className="govuk-grid-column-one-quarter">
              <h3 className="govuk-heading-s">In this section</h3>
              <ul className="govuk-body-s">
                <li>
                  <a href="#section1-1">
                    Pupil attendance requirements for schools
                  </a>
                </li>
                <li>
                  <a href="#section1-2">
                    Uses and users of absence data and statistics
                  </a>
                </li>
              </ul>
            </div>
            <div className="govuk-grid-column-three-quarters">
              <PrototypeSection1 />
            </div>
          </div>
        </AccordionSection>

        <AccordionSection heading="Background">
          <div className="govuk-grid-row">
            <div className="govuk-grid-column-one-quarter">
              <h3 className="govuk-heading-s">In this section</h3>
              <ul className="govuk-body-s">
                <li>
                  <a href="#section2-1">Current termly publications</a>
                </li>
                <li>
                  <a href="#section2-2">Key absence measures</a>
                </li>
                <li>
                  <a href="#section2-3">Cohort used in absence measures</a>
                </li>
                <li>
                  <a href="#section2-4">
                    The school year (five half terms vs six half terms)
                  </a>
                </li>
                <li>
                  <a href="#section2-5">
                    Published geographical and characteristics breakdowns
                  </a>
                </li>
                <li>
                  <a href="#section2-6">
                    Underlying data provided alongside publications
                  </a>
                </li>
                <li>
                  <a href="#section2-7">Suppression of absence data</a>
                </li>
                <li>
                  <a href="#section2-8">Other related publications</a>
                </li>
                <li>
                  <a href="#section2-9">
                    Devolved administration statistics on absence
                  </a>
                </li>
              </ul>
            </div>
            <div className="govuk-grid-column-three-quarters">
              <PrototypeSection2 />
            </div>
          </div>
        </AccordionSection>

        <AccordionSection heading="Methodology">
          <div className="govuk-grid-row">
            <div className="govuk-grid-column-one-quarter">
              <h3 className="govuk-heading-s">In this section</h3>
              <ul className="govuk-body-s">
                <li>
                  <a href="#section3-1">Overall absence methodology</a>
                </li>
                <li>
                  <a href="#section3-2">Persistent absence methodology</a>
                </li>
              </ul>
            </div>
            <div className="govuk-grid-column-three-quarters">
              <PrototypeSection3 />
            </div>
          </div>
        </AccordionSection>

        <AccordionSection heading="Data collection">
          <div className="govuk-grid-row">
            <div className="govuk-grid-column-one-quarter">
              <h3 className="govuk-heading-s">In this section</h3>
              <ul className="govuk-body-s">
                <li>
                  <a href="#section4-1">
                    The current process 2005/06 to present
                  </a>
                </li>
                <li>
                  <a href="#section4-2">
                    Background of absence data collection
                  </a>
                </li>
                <li>
                  <a href="#section4-3">Data coverage</a>
                </li>
                <li>
                  <a href="#section4-4">What absence data is collected</a>
                </li>
                <li>
                  <a href="#section4-5">
                    No longer collected but available historically
                  </a>
                </li>
                <li>
                  <a href="#section4-6">What absence data is not collected</a>
                </li>
              </ul>
            </div>
            <div className="govuk-grid-column-three-quarters">
              <PrototypeSection4 />
            </div>
          </div>
        </AccordionSection>

        <AccordionSection heading="Data processing">
          <div className="govuk-grid-row">
            <div className="govuk-grid-column-one-quarter">
              <h3 className="govuk-heading-s">In this section</h3>
              <ul className="govuk-body-s">
                <li>
                  <a href="#section5-1">Data linking</a>
                </li>
                <li>
                  <a href="#section5-2">Data removed</a>
                </li>
                <li>
                  <a href="#section5-3">Variables added</a>
                </li>
                <li>
                  <a href="#section5-4">Consistency checks</a>
                </li>
                <li>
                  <a href="#section5-5">Data quality</a>
                </li>
              </ul>
            </div>
            <div className="govuk-grid-column-three-quarters">
              <PrototypeSection5 />
            </div>
          </div>
        </AccordionSection>

        <AccordionSection heading="Research relating to pupil absence">
          <div className="govuk-grid-row">
            <div className="govuk-grid-column-one-quarter">
              <h3 className="govuk-heading-s">In this section</h3>
              <ul className="govuk-body-s">
                <li>
                  <a href="#section6-1">The impact of absenteeism on pupils</a>
                </li>
                <li>
                  <a href="#section6-2">Factors associated with absenteeism</a>
                </li>
                <li>
                  <a href="#section6-3">Approaches to reduce absenteeism</a>
                </li>
                <li>
                  <a href="#section6-4">References</a>
                </li>
              </ul>
            </div>
            <div className="govuk-grid-column-three-quarters">
              <PrototypeSection6 />
            </div>
          </div>
        </AccordionSection>

        <AccordionSection heading="Contacts">
          <PrototypeSection7 />
        </AccordionSection>
      </Accordion>

      <h2 className="govuk-heading-l govuk-!-margin-top-9">Annex</h2>
      <Accordion id="annex-sections">
        <AccordionSection heading="Annex A, glossary">
          <PrototypeAnnexA />
        </AccordionSection>
        <AccordionSection heading="Annex B, calculations">
          <PrototypeAnnexB />
        </AccordionSection>
        <AccordionSection heading="Annex C, school attendance codes">
          <PrototypeAnnexC />
        </AccordionSection>
        <AccordionSection heading="Annex D, links to pupil absence national statistics publications">
          <PrototypeAnnexD />
        </AccordionSection>
        <AccordionSection heading="Annex E, standard breakdowns that are currently published">
          <PrototypeAnnexE />
        </AccordionSection>
        <AccordionSection heading="Annex F, timeline">
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
