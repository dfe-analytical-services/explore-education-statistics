import styles from '@admin/pages/release/datablocks/components/chart/ChartReferenceLinesConfiguration.module.scss';
import ChartReferenceLineConfigurationForm from '@admin/pages/release/datablocks/components/chart/ChartReferenceLineConfigurationForm';
import { SelectOption } from '@common/components/form/FormSelect';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import VisuallyHidden from '@common/components/VisuallyHidden';
import {
  AxisType,
  ChartDefinitionAxis,
  ReferenceLine,
} from '@common/modules/charts/types/chart';
import { DataSetCategory } from '@common/modules/charts/types/dataSet';
import { MinorAxisDomainValues } from '@common/modules/charts/util/domainTicks';
import { LocationFilter } from '@common/modules/table-tool/types/filters';
import React, { useMemo, useState } from 'react';
import isEqual from 'lodash/isEqual';
import upperFirst from 'lodash/upperFirst';

export interface ChartReferenceLinesConfigurationProps {
  axisDefinition?: ChartDefinitionAxis;
  dataSetCategories: DataSetCategory[];
  lines: ReferenceLine[];
  minorAxisDomain?: MinorAxisDomainValues;
  onChange: (referenceLines: ReferenceLine[]) => void;
}

export default function ChartReferenceLinesConfiguration({
  axisDefinition,
  dataSetCategories,
  lines,
  minorAxisDomain,
  onChange,
}: ChartReferenceLinesConfigurationProps) {
  const { axis, type } = axisDefinition || {};
  const [editingLine, setEditingLine] = useState<ReferenceLine>();

  const majorAxisOptions = useMemo<SelectOption[]>(() => {
    return dataSetCategories.map(({ filter }) => ({
      label: filter.label,
      value:
        filter instanceof LocationFilter
          ? LocationFilter.createId(filter)
          : filter.value,
    }));
  }, [dataSetCategories]);

  const filteredOptions = useMemo<SelectOption[]>(() => {
    return majorAxisOptions.filter(
      option => lines?.every(line => line.position !== option.value),
    );
  }, [lines, majorAxisOptions]);

  const referenceLines = useMemo(() => {
    return type === 'major'
      ? lines?.filter(line =>
          majorAxisOptions.some(option => option.value === line.position),
        ) ?? []
      : lines ?? [];
  }, [type, lines, majorAxisOptions]);

  return (
    <div className="dfe-overflow-x--auto">
      <table className="govuk-table" data-testid="referenceLines">
        <caption className="govuk-heading-s">Reference lines</caption>
        <thead>
          <tr>
            <th>Position</th>
            <th>
              {axis === 'x' ? 'Y axis position' : 'X axis position'}
              {type === 'minor' && (
                <>
                  <br />
                  <div className="govuk-hint govuk-!-font-size-16 govuk-!-margin-bottom-0">
                    0% = start of axis
                  </div>
                </>
              )}
            </th>
            <th className="govuk-!-width-one-third">Label</th>
            <th>Label width</th>
            <th>Style</th>
            <th className={`govuk-!-text-align-right ${styles.actions}`}>
              Actions
            </th>
          </tr>
        </thead>
        <tbody>
          {referenceLines.map((referenceLine, index) => {
            return (
              <tr key={`${referenceLine.label}_${index.toString()}`}>
                {isEqual(editingLine, referenceLine) ? (
                  <ChartReferenceLineConfigurationForm
                    allReferenceLines={lines}
                    axisDefinition={axisDefinition}
                    axisPositionOptions={majorAxisOptions.filter(
                      option =>
                        referenceLine?.position === option.value ||
                        lines?.every(line => line.position !== option.value),
                    )}
                    majorAxisOptions={majorAxisOptions}
                    minorAxisDomain={minorAxisDomain}
                    referenceLine={referenceLine}
                    onSubmit={values => {
                      setEditingLine(undefined);
                      onChange(values);
                    }}
                  />
                ) : (
                  <>
                    <td>
                      {getAxisPosition({
                        majorAxisOptions,
                        referenceLine,
                        type,
                      })}
                    </td>
                    <td>
                      {getOtherAxisPositionLabel({
                        majorAxisOptions,
                        referenceLine,
                        type,
                      })}
                    </td>
                    <td className="govuk-!-width-one-third">
                      {referenceLine?.label}
                    </td>
                    <td>{referenceLine?.labelWidth}</td>
                    <td>{upperFirst(referenceLine?.style)}</td>
                    <td>
                      {!editingLine && (
                        <ButtonGroup className="govuk-!-margin-bottom-0 dfe-justify-content--flex-end">
                          <ButtonText
                            onClick={() => {
                              setEditingLine(referenceLine);
                            }}
                          >
                            Edit <VisuallyHidden>line</VisuallyHidden>
                          </ButtonText>
                          <ButtonText
                            variant="warning"
                            onClick={() => {
                              onChange(
                                referenceLines?.filter(
                                  line => !isEqual(line, referenceLine),
                                ),
                              );
                            }}
                          >
                            Remove <VisuallyHidden>line</VisuallyHidden>
                          </ButtonText>
                        </ButtonGroup>
                      )}
                    </td>
                  </>
                )}
              </tr>
            );
          })}
          {(filteredOptions.length > 0 || type === 'minor') && !editingLine && (
            <tr>
              <ChartReferenceLineConfigurationForm
                allReferenceLines={lines}
                axisDefinition={axisDefinition}
                axisPositionOptions={filteredOptions}
                majorAxisOptions={majorAxisOptions}
                minorAxisDomain={minorAxisDomain}
                onSubmit={onChange}
              />
            </tr>
          )}
        </tbody>
      </table>
    </div>
  );
}

function getOtherAxisPositionLabel({
  majorAxisOptions,
  referenceLine,
  type,
}: {
  majorAxisOptions: SelectOption[];
  referenceLine?: ReferenceLine;
  type?: AxisType;
}) {
  const { otherAxisEnd, otherAxisPosition, otherAxisStart } =
    referenceLine || {};
  if (otherAxisPosition) {
    return `${otherAxisPosition}${type === 'minor' ? '%' : ''}`;
  }

  if (otherAxisEnd && otherAxisStart) {
    return `${majorAxisOptions.find(option => option.value === otherAxisStart)
      ?.label} - ${majorAxisOptions.find(
      option => option.value === otherAxisEnd,
    )?.label}`;
  }

  return null;
}

function getAxisPosition({
  majorAxisOptions,
  referenceLine,
  type,
}: {
  majorAxisOptions: SelectOption[];
  referenceLine?: ReferenceLine;
  type?: AxisType;
}) {
  return type === 'major'
    ? majorAxisOptions.find(option => option.value === referenceLine?.position)
        ?.label
    : referenceLine?.position;
}
