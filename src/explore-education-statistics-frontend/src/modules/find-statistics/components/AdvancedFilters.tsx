import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import { ReleaseType } from '@common/services/types/releaseType';
import { FilterChangeHandler } from '@frontend/modules/find-statistics/components/Filters';
import ReleaseTypeFilters from '@frontend/modules/find-statistics/components/ReleaseTypeFilters';
import React from 'react';

interface Props {
  releaseType?: ReleaseType | 'all';
  onChange: FilterChangeHandler;
}

const AdvancedFilters = ({
  releaseType: initialReleaseType = 'all',
  onChange,
}: Props) => {
  return (
    <>
      <h3 className="govuk-!-margin-top-6">Advanced filters</h3>

      <Accordion id="advancedFilters" showOpenAll={false}>
        <AccordionSection
          heading="Release type"
          headingTag="h4"
          goToTop={false}
        >
          <ReleaseTypeFilters
            releaseType={initialReleaseType}
            onChange={onChange}
          />
        </AccordionSection>
      </Accordion>
    </>
  );
};

export default AdvancedFilters;
