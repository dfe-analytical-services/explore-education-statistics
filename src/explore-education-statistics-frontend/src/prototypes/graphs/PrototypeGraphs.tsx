import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import PrototypePage from '@frontend/prototypes/components/PrototypePage';
import React from 'react';
import {
  ks4AverageHeadlineScoresByPupilEthnicity,
  ks4PerformanceInMatsComparedToNationalAverage,
  ks4SchoolAverageHeadlineScoresByPupilCharacteristics,
  kS4SchoolPerformanceDataBlock,
  ks4SchoolRevisedAttainmentChart,
  ks4TrendInDisavdantagePuilsAttainmentGapIndex,
  testChartsVerticalWithReferenceLine,
} from '@frontend/prototypes/publication/data/PrototypeDataFactory';
import { DataBlock } from '@frontend/modules/find-statistics/components/DataBlock';

const GraphsPage = () => {
  return (
    <PrototypePage
      breadcrumbs={[
        {
          link: '/prototypes/browse-releases-find',
          text: 'Find statistics and download data',
        },
        { text: 'GCSE and equivalent results in England', link: '#' },
      ]}
    >
      <h1 className="govuk-heading-xl">
        GCSE and equivalent results in England, 2016 to 2017
      </h1>

      <Accordion id="graphs">
        <AccordionSection heading="Bar Charts">
          <h3>Horizontal Bars</h3>
          <DataBlock
            {...ks4SchoolAverageHeadlineScoresByPupilCharacteristics}
            showTables={false}
          />
          <DataBlock
            {...ks4PerformanceInMatsComparedToNationalAverage}
            showTables={false}
          />
          <DataBlock
            {...testChartsVerticalWithReferenceLine}
            showTables={false}
          />
          <h3>Vertical Bars</h3>
          <DataBlock {...ks4SchoolRevisedAttainmentChart} showTables={false} />
          <DataBlock
            {...ks4AverageHeadlineScoresByPupilEthnicity}
            showTables={false}
          />
        </AccordionSection>

        <AccordionSection heading="Line Charts">
          <h3>Simple</h3>
          <DataBlock
            {...ks4TrendInDisavdantagePuilsAttainmentGapIndex}
            showTables={false}
          />
          <h3>Multi line with gap</h3>
          <DataBlock {...kS4SchoolPerformanceDataBlock} showTables={false} />
        </AccordionSection>
      </Accordion>
    </PrototypePage>
  );
};

export default GraphsPage;
