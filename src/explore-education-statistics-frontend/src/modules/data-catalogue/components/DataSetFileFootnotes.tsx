import DataSetFilePageSection from '@frontend/modules/data-catalogue/components/DataSetFilePageSection';
import { pageSections } from '@frontend/modules/data-catalogue/DataSetFilePage';
import React from 'react';

// TODO EES-4856 replace with real data
const footnotes = [
  {
    id: 'footnote-1',
    label:
      'Local authority response rates to the data collection vary by census date. To account for non-response, national and regional figures have been uprated based on local authority pupil populations.',
  },
  {
    id: 'footnote-2',
    label:
      'Local authority response rates to the data collection vary by census date. To account for non-response, national and regional figures have been uprated based on local authority pupil populations.',
  },
];
export default function DataSetFileFootnotes() {
  return (
    <DataSetFilePageSection heading={pageSections.footnotes} id="footnotes">
      <ol className="govuk-!-margin-bottom-6">
        {footnotes.map(footnote => (
          <li key={footnote.id}>{footnote.label}</li>
        ))}
      </ol>
    </DataSetFilePageSection>
  );
}