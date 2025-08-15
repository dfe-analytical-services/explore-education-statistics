import { useEditingContext } from '@admin/contexts/EditingContext';
import FormattedDate from '@common/components/FormattedDate';

import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import React, { useEffect } from 'react';

import useDebouncedCallback from '@common/hooks/useDebouncedCallback';
import { EducationInNumbersSummary } from '@admin/services/educationInNumbersService';
import { EducationInNumbersPageContent } from '@admin/services/educationInNumbersContentService';
import EducationInNumbersAccordion from './EducationInNumbersAccordion';

interface Props {
  pageContent: EducationInNumbersPageContent;
  pageVersion: EducationInNumbersSummary;
}

export default function EducationInNumbersContent({
  pageContent,
  pageVersion,
}: Props) {
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
      <div className="govuk-grid-row">
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
