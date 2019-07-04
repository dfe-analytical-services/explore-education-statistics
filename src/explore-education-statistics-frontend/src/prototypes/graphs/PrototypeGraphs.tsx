/* eslint-disable */
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import PrototypePage from '@frontend/prototypes/components/PrototypePage';
import React from 'react';
import {
  newApiHorizontalData,
  newApiTest,
  newChartsApiDataBlock,
} from '@frontend/prototypes/publication/data/PrototypeDataFactory';
import DataBlock from '@common/modules/find-statistics/components/DataBlock';
import HorizontalBarBlock from 'explore-education-statistics-common/src/modules/find-statistics/components/charts/HorizontalBarBlock';

const GraphsPage = () => {
  return (
    <PrototypePage breadcrumbs={[{ text: 'Example graphs', link: '#' }]}>
      <h1 className="govuk-heading-xl">Example graphs</h1>

      <DataBlock {...newChartsApiDataBlock} />
    </PrototypePage>
  );
};

export default GraphsPage;
