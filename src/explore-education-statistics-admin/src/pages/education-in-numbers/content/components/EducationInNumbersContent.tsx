import { useEditingContext } from '@admin/contexts/EditingContext';
import FormattedDate from '@common/components/FormattedDate';

import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import React, { useEffect } from 'react';

import useDebouncedCallback from '@common/hooks/useDebouncedCallback';
import { useEducationInNumbersPageContentState } from '@admin/pages/education-in-numbers/content/context/EducationInNumbersPageContentContext';
import EducationInNumbersAccordion from '@admin/pages/education-in-numbers/content/components/EducationInNumbersAccordion';

export default function EducationInNumbersContent() {
  const { pageContent, pageVersion } = useEducationInNumbersPageContentState();
  const { setActiveSection } = useEditingContext();

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
      <div
        className="govuk-grid-row"
        data-testid="education-in-numbers-content"
      >
        <div className="govuk-grid-column-two-thirds">
          <div data-scroll id="summary">
            <SummaryList>
              <SummaryListItem term="Publish date">
                {pageVersion.published ? (
                  <FormattedDate>{pageVersion.published}</FormattedDate>
                ) : (
                  'Not yet published'
                )}
              </SummaryListItem>
            </SummaryList>
          </div>
        </div>
      </div>

      <EducationInNumbersAccordion
        pageContent={pageContent}
        title="Content"
        onSectionOpen={({ id }) => setActiveSection(id)}
      />
    </>
  );
}
