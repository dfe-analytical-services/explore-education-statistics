import ButtonText from '@common/components/ButtonText';
import useToggle from '@common/hooks/useToggle';
import DataSetPageSection from '@frontend/modules/data-catalogue/components/DataSetPageSection';
import { pageSections } from '@frontend/modules/data-catalogue/DataSetPage';
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

export default function DataSetVariables() {
  const [showAll, toggleShowAll] = useToggle(false);
  const displayVariables = showAll
    ? variables
    : variables.slice(0, defaultVisible - 1);
  const totalVariables = variables.length;

  return (
    <DataSetPageSection heading={pageSections.variables} id="variables">
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
        className="govuk-!-margin-bottom-6"
        ariaExpanded={!showAll}
        onClick={toggleShowAll}
      >
        {showAll
          ? `Show only ${defaultVisible} variables`
          : `Show all ${totalVariables} variables`}
      </ButtonText>
    </DataSetPageSection>
  );
}
