import ButtonText from '@common/components/ButtonText';
import useMounted from '@common/hooks/useMounted';
import useToggle from '@common/hooks/useToggle';
import classNames from 'classnames';
import DataSetFilePageSection from '@frontend/modules/data-catalogue/components/DataSetFilePageSection';
import { pageSections } from '@frontend/modules/data-catalogue/DataSetFilePage';
import { DataSetVariable } from '@frontend/services/dataSetFileService';
import React from 'react';

const sectionId = 'dataSetVariables';
const tableId = 'variables-table';
const defaultVisible = 5;

interface Props {
  variables: DataSetVariable[];
}

export default function DataSetFileVariables({ variables }: Props) {
  const totalVariables = variables.length;
  const expandable = totalVariables > defaultVisible;
  const [showAll, toggleShowAll] = useToggle(!expandable);
  const { isMounted } = useMounted();

  // Without JS every row is visible, so only describe the table as
  // truncated once JS is running and actually hiding rows.
  const isShowingAll = showAll || !isMounted;

  return (
    <DataSetFilePageSection heading={pageSections[sectionId]} id={sectionId}>
      <table id={tableId} data-testid={tableId}>
        <caption className="govuk-!-margin-bottom-3">
          {isShowingAll
            ? `Table showing all ${totalVariables} variables`
            : `Table showing first ${defaultVisible} of ${totalVariables} variables`}
        </caption>
        <thead>
          <tr>
            <th scope="col">Variable name</th>
            <th scope="col">Variable description</th>
          </tr>
        </thead>
        <tbody>
          {variables.map(({ value, label }, index) => (
            <tr
              key={value}
              className={classNames({
                'dfe-js-hidden': !showAll && index >= defaultVisible,
              })}
            >
              <td className="dfe-word-break--break-word">{value}</td>
              <td>{label}</td>
            </tr>
          ))}
        </tbody>
      </table>
      {expandable && (
        <ButtonText
          ariaControls={tableId}
          ariaExpanded={showAll}
          className="dfe-js-only"
          onClick={toggleShowAll}
        >
          {showAll
            ? `Show only ${defaultVisible} variables`
            : `Show all ${totalVariables} variables`}
        </ButtonText>
      )}
    </DataSetFilePageSection>
  );
}
