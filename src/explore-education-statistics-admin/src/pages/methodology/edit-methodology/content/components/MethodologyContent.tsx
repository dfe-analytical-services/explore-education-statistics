import { useEditingContext } from '@admin/contexts/EditingContext';
import PrintThisPage from '@admin/components/PrintThisPage';

import FormattedDate from '@common/components/FormattedDate';

import PageSearchForm from '@common/components/PageSearchForm';

import MethodologyAccordion from '@admin/pages/methodology/edit-methodology/content/components/MethodologyAccordion';
import MethodologyNotesSection from '@admin/pages/methodology/edit-methodology/content/components/MethodologyNotesSection';

import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import React, { useEffect } from 'react';

import MethodologyHelpAndSupportSection from '@common/modules/methodology/components/MethodologyHelpAndSupportSection';
import RelatedInformation from '@common/components/RelatedInformation';

import { MethodologyContent as MethodologyContentData } from '@admin/services/methodologyContentService';
import { MethodologyVersion } from '@admin/services/methodologyService';
import useDebouncedCallback from '@common/hooks/useDebouncedCallback';
import VisuallyHidden from '@common/components/VisuallyHidden';

interface Props {
  methodology: MethodologyContentData;
  methodologyVersion: MethodologyVersion;
}

export default function MethodologyContent({
  methodology,
  methodologyVersion,
}: Props) {
  const { editingMode, setActiveSection } = useEditingContext();

  const [handleScroll] = useDebouncedCallback(() => {
    const sections = document.querySelectorAll('[data-scroll]');

    // Set a section as active when it's in the top third of the page.
    const buffer = window.innerHeight / 3;
    const scrollPosition = window.scrollY + buffer;

    sections.forEach(section => {
      if (section) {
        const { height } = section.getBoundingClientRect();
        const top =
          section.getBoundingClientRect().top +
          document.documentElement.scrollTop;
        const bottom = top + height;
        if (scrollPosition > top && scrollPosition < bottom) {
          setActiveSection(section.id);
        }
      }
    });
  }, 100);

  // has anchor link is the problems in preview - it puts position relative on.

  useEffect(() => {
    window.addEventListener('scroll', handleScroll);

    return () => {
      window.removeEventListener('scroll', handleScroll);
    };
  }, [handleScroll]);

  return (
    <>
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <div data-scroll id="summary">
            <SummaryList>
              <SummaryListItem term="Publish date">
                {methodology.published ? (
                  <FormattedDate>{methodology.published}</FormattedDate>
                ) : (
                  'Not yet published'
                )}
              </SummaryListItem>
              <MethodologyNotesSection methodology={methodology} />
            </SummaryList>
            {editingMode !== 'edit' && (
              <>
                <PageSearchForm inputLabel="Search in this methodology page." />
                <PrintThisPage />
              </>
            )}
          </div>
        </div>
        <div className="govuk-grid-column-one-third">
          <RelatedInformation>
            <h3 className="govuk-heading-s" id="related-pages">
              Help and support
            </h3>
            <ul className="govuk-list">
              <li>
                <a href="#contact-us">
                  Contact us
                  <VisuallyHidden> about this methodology</VisuallyHidden>
                </a>
              </li>
            </ul>
          </RelatedInformation>
        </div>
      </div>

      <MethodologyAccordion
        methodology={methodology}
        sectionKey="content"
        title="Content"
        onSectionOpen={({ id }) => setActiveSection(id)}
      />
      {editingMode !== 'edit' && methodology.annexes.length ? (
        <h2>Annexes</h2>
      ) : null}
      <MethodologyAccordion
        methodology={methodology}
        sectionKey="annexes"
        title="Annexes"
        onSectionOpen={({ id }) => setActiveSection(id)}
      />

      <MethodologyHelpAndSupportSection
        owningPublication={methodologyVersion.owningPublication}
        trackScroll
      />
    </>
  );
}
