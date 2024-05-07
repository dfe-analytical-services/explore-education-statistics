import styles from '@admin/pages/release/datablocks/components/chart/ChartReferenceLinesConfiguration.module.scss';
import { ChartAxisConfigurationFormValues } from '@admin/pages/release/datablocks/components/chart/ChartAxisConfiguration';
import ChartReferenceLineForm from '@admin/pages/release/datablocks/components/chart/ChartReferenceLineConfigurationForm';
import ButtonGroup from '@common/components/ButtonGroup';
import Button from '@common/components/Button';

import ButtonText from '@common/components/ButtonText';
import VisuallyHidden from '@common/components/VisuallyHidden';
import useToggle from '@common/hooks/useToggle';
import {
  AxisType,
  ChartDefinitionAxis,
  ChartType,
  ReferenceLine,
  ReferenceLineStyle,
} from '@common/modules/charts/types/chart';
import React, { useEffect, useMemo, useState } from 'react';
import {
  OtherAxisPositionType,
  otherAxisPositionTypes,
} from '@common/modules/charts/types/referenceLinePosition';
import { SelectOption } from '@common/components/form/FormSelect';
import { LocationFilter } from '@common/modules/table-tool/types/filters';
import { DataSetCategory } from '@common/modules/charts/types/dataSet';
import isEqual from 'lodash/isEqual';
import upperFirst from 'lodash/upperFirst';
import { useFormContext } from 'react-hook-form';

export interface ChartReferenceLinesFormValues {
  label: string;
  labelWidth?: number;
  position: string | number;
  otherAxisEnd?: string;
  otherAxisPosition?: number;
  otherAxisPositionType?: OtherAxisPositionType;
  otherAxisStart?: string;
  style: ReferenceLineStyle;
}

export interface ChartReferenceLinesConfigurationProps {
  axisDefinition?: ChartDefinitionAxis;
  chartType: ChartType;
  dataSetCategories: DataSetCategory[];
  referenceLines: ReferenceLine[];
  onSubmit: (referenceLines: ReferenceLine[]) => void;
}

export default function ChartReferenceLinesConfiguration({
  axisDefinition,
  chartType,
  dataSetCategories,
  referenceLines: initialReferenceLines = [],
  onSubmit,
}: ChartReferenceLinesConfigurationProps) {
  const [referenceLines, setReferenceLines] = useState<ReferenceLine[]>(
    initialReferenceLines,
  );

  const { setValue, trigger } =
    useFormContext<ChartAxisConfigurationFormValues>();
  const { axis, type: axisType } = axisDefinition || {};

  const [editingLine, setEditingLine] = useState<ReferenceLine>();
  const [showAddNewLineForm, toggleAddNewLineForm] = useToggle(false);

  const majorAxisOptions = useMemo<SelectOption[]>(() => {
    return dataSetCategories.map(({ filter }) => ({
      label: filter.label,
      value:
        filter instanceof LocationFilter
          ? LocationFilter.createId(filter)
          : filter.value,
    }));
  }, [dataSetCategories]);

  const filteredReferenceLines = useMemo(() => {
    if (axisType === 'major') {
      return referenceLines.filter(line => {
        if (line.position === otherAxisPositionTypes.betweenDataPoints) {
          return (
            chartType === 'verticalbar' &&
            majorAxisOptions.some(
              option => option.value === line.otherAxisEnd,
            ) &&
            majorAxisOptions.some(
              option => option.value === line.otherAxisStart,
            )
          );
        }
        return majorAxisOptions.some(option => option.value === line.position);
      });
    }

    return referenceLines;
  }, [chartType, axisType, referenceLines, majorAxisOptions]);

  const filteredOptions = useMemo<SelectOption[]>(() => {
    return majorAxisOptions.filter(
      option => referenceLines?.every(line => line.position !== option.value),
    );
  }, [referenceLines, majorAxisOptions]);

  // Trigger validation when the new line form is closed,
  // otherwise it will try to validate the form when you
  // aren't adding a new line.
  useEffect(() => {
    if (!showAddNewLineForm) {
      trigger();
    }
  }, [showAddNewLineForm, trigger]);

  return (
    <>
      {!filteredReferenceLines.length && !showAddNewLineForm ? (
        <>
          <h4>Reference lines</h4>
          <p>No reference lines have been added.</p>
        </>
      ) : (
        <table className="govuk-table" data-testid="referenceLines">
          <caption className="govuk-heading-s">Reference lines</caption>
          <thead>
            <tr>
              <th>Position</th>
              <th>
                {axis === 'x' ? 'Y axis position' : 'X axis position'}
                {axisType === 'minor' && (
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
            {filteredReferenceLines.map((referenceLine, index) => (
              <tr key={`line-${index.toString()}`}>
                {isEqual(referenceLine, editingLine) ? (
                  <ChartReferenceLineForm
                    axisDefinition={axisDefinition}
                    axisPositionOptions={majorAxisOptions}
                    chartType={chartType}
                    index={index}
                    isEditing
                    majorAxisOptions={majorAxisOptions}
                    onSave={updatedLines => {
                      setEditingLine(undefined);
                      setReferenceLines(updatedLines);
                      onSubmit(updatedLines);
                    }}
                  />
                ) : (
                  <>
                    <td>
                      {getAxisPosition({
                        majorAxisOptions,
                        referenceLine,
                        axisType,
                      })}
                    </td>
                    <td>
                      {getOtherAxisPositionLabel({
                        majorAxisOptions,
                        referenceLine,
                        axisType,
                      })}
                    </td>
                    <td className="govuk-!-width-one-third">
                      {referenceLine?.label}
                    </td>
                    <td>{referenceLine?.labelWidth}</td>
                    <td>{upperFirst(referenceLine?.style)}</td>
                    <td>
                      {!editingLine && !showAddNewLineForm && (
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
                              const updated = referenceLines?.filter(
                                line => !isEqual(line, referenceLine),
                              );
                              setReferenceLines(updated);
                              onSubmit(updated);
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
            ))}
            {showAddNewLineForm && !editingLine && (
              <tr>
                <ChartReferenceLineForm
                  axisDefinition={axisDefinition}
                  axisPositionOptions={filteredOptions}
                  chartType={chartType}
                  index={referenceLines.length}
                  majorAxisOptions={majorAxisOptions}
                  onSave={updatedLines => {
                    toggleAddNewLineForm.off();
                    setReferenceLines(updatedLines);
                    onSubmit(updatedLines);
                  }}
                />
              </tr>
            )}
          </tbody>
        </table>
      )}

      {!showAddNewLineForm && !editingLine && (
        <Button
          onClick={() => {
            setValue('referenceLines' as const, [
              ...(referenceLines ?? []),
              {
                style: axisDefinition?.referenceLineDefaults?.style,
              } as ReferenceLine,
            ]);
            toggleAddNewLineForm.on();
          }}
        >
          Add new line
        </Button>
      )}
    </>
  );
}

function getOtherAxisPositionLabel({
  majorAxisOptions,
  referenceLine,
  axisType,
}: {
  majorAxisOptions: SelectOption[];
  referenceLine?: ReferenceLine;
  axisType?: AxisType;
}) {
  const { otherAxisEnd, otherAxisPosition, otherAxisStart } =
    referenceLine || {};
  if (otherAxisPosition) {
    return `${otherAxisPosition}${axisType === 'minor' ? '%' : ''}`;
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
  axisType,
}: {
  majorAxisOptions: SelectOption[];
  referenceLine?: ReferenceLine;
  axisType?: AxisType;
}) {
  if (
    axisType === 'major' &&
    referenceLine?.position === otherAxisPositionTypes.betweenDataPoints
  ) {
    const { otherAxisEnd, otherAxisStart } = referenceLine || {};
    const otherAxisEndLabel = majorAxisOptions.find(
      option => option.value === otherAxisEnd,
    )?.label;
    const otherAxisStartLabel = majorAxisOptions.find(
      option => option.value === otherAxisStart,
    )?.label;

    if (otherAxisEndLabel && otherAxisStartLabel) {
      return `${otherAxisStartLabel} - ${otherAxisEndLabel}`;
    }
  }
  return axisType === 'major'
    ? majorAxisOptions.find(option => option.value === referenceLine?.position)
        ?.label
    : referenceLine?.position;
}
