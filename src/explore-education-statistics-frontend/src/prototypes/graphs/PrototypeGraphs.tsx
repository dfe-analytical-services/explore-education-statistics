/* eslint-disable */
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
  testTimeSeries,
  testTimeSeriesWithLine,
  testDistribution,
  newApiHorizontalData,
} from '@frontend/prototypes/publication/data/PrototypeDataFactory';
import DataBlock from '@frontend/modules/find-statistics/components/DataBlock';
import HorizontalBarBlock from 'explore-education-statistics-common/src/modules/find-statistics/components/charts/HorizontalBarBlock';

const GraphsPage = () => {
  return (
    <PrototypePage breadcrumbs={[{ text: 'Example graphs', link: '#' }]}>
      <h1 className="govuk-heading-xl">Example graphs</h1>

      <Accordion id="graphs">
        <AccordionSection heading="Bar Charts">
          <DataBlock {...ks4SchoolRevisedAttainmentChart} showTables={false} />

          <h3>Horizontal Bars</h3>
          <HorizontalBarBlock {...newApiHorizontalData} stacked />
        </AccordionSection>
      </Accordion>
    </PrototypePage>
  );
};

export default GraphsPage;
