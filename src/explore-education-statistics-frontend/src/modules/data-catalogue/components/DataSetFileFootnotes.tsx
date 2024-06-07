import DataSetFilePageSection from '@frontend/modules/data-catalogue/components/DataSetFilePageSection';
import { pageSections } from '@frontend/modules/data-catalogue/DataSetFilePage';
import { DataSetFootnote } from '@frontend/services/dataSetFileService';
import React from 'react';

const sectionId = 'dataSetFootnotes';

interface Props {
  footnotes: DataSetFootnote[];
}

export default function DataSetFileFootnotes({ footnotes }: Props) {
  return (
    <DataSetFilePageSection heading={pageSections[sectionId]} id={sectionId}>
      <ol>
        {footnotes.map(footnote => (
          <li key={footnote.id}>{footnote.label}</li>
        ))}
      </ol>
    </DataSetFilePageSection>
  );
}
