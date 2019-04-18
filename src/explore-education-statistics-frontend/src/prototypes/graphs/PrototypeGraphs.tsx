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
  testChartsVerticalOffset,
  testChartsVertical,
  testChartsVerticalWithReferenceLineAndAxisTitles,
} from '@frontend/prototypes/publication/data/PrototypeDataFactory';
import { DataBlock } from '@frontend/modules/find-statistics/components/DataBlock';

const GraphsPage = () => {
  return (
    <PrototypePage breadcrumbs={[{ text: 'Example graphs', link: '#' }]}>
      <h1 className="govuk-heading-xl">Example graphs</h1>

      <Accordion id="graphs">
        <AccordionSection heading="Bar Charts">
          <h3>Horizontal Bars</h3>
          <DataBlock
            {...ks4SchoolAverageHeadlineScoresByPupilCharacteristics}
            showTables={false}
          />
          <h3>Ranking</h3>
          <DataBlock {...testChartsVertical} showTables={false} />

          <h3>Part-to-Whole – to show ratio/percentage/proportion</h3>
          <DataBlock
            {...ks4PerformanceInMatsComparedToNationalAverage}
            showTables={false}
          />
          <h3>Deviation where the value of data is most important</h3>
          <DataBlock
            {...testChartsVerticalWithReferenceLineAndAxisTitles}
            showTables={false}
          />
          <h3>Deviation where the amount of change is most important</h3>
          <DataBlock {...testChartsVerticalOffset} showTables={false} />
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
