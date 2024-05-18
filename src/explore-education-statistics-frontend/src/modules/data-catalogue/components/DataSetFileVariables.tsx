import ButtonText from '@common/components/ButtonText';
import useToggle from '@common/hooks/useToggle';
import DataSetFilePageSection from '@frontend/modules/data-catalogue/components/DataSetFilePageSection';
import { pageHiddenSections } from '@frontend/modules/data-catalogue/DataSetFilePage';
import React from 'react';

// TODO EES-4856 replace with real data
const variables = [
  {
    value: 'enrolments_pa_10_exact_percent',
    label: '	Percentage of persistent absentees',
  },
  { value: 'num_schools', label: 'Number of schools' },
  {
    value: 'enrolments_pa_10_exact_percent',
    label: '	Percentage of persistent absentees',
  },
  { value: 'num_schools', label: 'Number of schools' },
  {
    value: 'enrolments_pa_10_exact_percent',
    label: '	Percentage of persistent absentees',
  },
  { value: 'num_schools', label: 'Number of schools' },
  {
    value: 'enrolments_pa_10_exact_percent',
    label: '	Percentage of persistent absentees',
  },
  { value: 'num_schools', label: 'Number of schools' },
  {
    value: 'enrolments_pa_10_exact_percent',
    label: '	Percentage of persistent absentees',
  },
  { value: 'num_schools', label: 'Number of schools' },
  {
    value: 'enrolments_pa_10_exact_percent',
    label: '	Percentage of persistent absentees',
  },
  { value: 'num_schools', label: 'Number of schools' },
];

const tableId = 'variables-table';
const defaultVisible = 5;

export default function DataSetFileVariables() {
  const [showAll, toggleShowAll] = useToggle(false);
  const displayVariables = showAll
    ? variables
    : variables.slice(0, defaultVisible - 1);
  const totalVariables = variables.length;

  return (
    <DataSetFilePageSection
      heading={pageHiddenSections.dataSetVariables}
      id="dataSetVariables"
    >
      <table id={tableId}>
        <caption className="govuk-!-margin-bottom-3">
          {showAll
            ? `Table showing all ${totalVariables} variables`
            : `Table showing first 
          ${defaultVisible} of ${totalVariables} variables`}
        </caption>
        <thead>
          <tr>
            <th scope="col">Variable name</th>
            <th scope="col">Variable description</th>
          </tr>
        </thead>
        <tbody>
          {displayVariables.map(({ value, label }) => (
            <tr key={value}>
              <td className="dfe-word-break--break-word">{value}</td>
              <td>{label}</td>
            </tr>
          ))}
        </tbody>
      </table>

      <ButtonText
        ariaControls={tableId}
        ariaExpanded={!showAll}
        onClick={toggleShowAll}
      >
        {showAll
          ? `Show only ${defaultVisible} variables`
          : `Show all ${totalVariables} variables`}
      </ButtonText>
    </DataSetFilePageSection>
  );
}
