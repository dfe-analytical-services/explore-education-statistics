import Button from '@common/components/Button';
import Details from '@common/components/Details';
import TimePeriod from '@common/services/types/TimePeriod';
import FormFieldSortableList from '@frontend/prototypes/table-tool/components/FormFieldSortableList';
import { SortableOption } from '@frontend/prototypes/table-tool/components/FormSortableList';
import {
  FilterOption,
  IndicatorOption,
} from '@frontend/prototypes/table-tool/components/meta/initialSpec';
import { Form, Formik } from 'formik';
import React from 'react';

interface Props {
  indicators: IndicatorOption[];
  filters: {
    [key: string]: FilterOption[];
  };
  timePeriods: TimePeriod[];
  onSubmit: (values: FormValues) => void;
}

export interface FormValues {
  columnGroups: SortableOption[];
  columns: SortableOption[];
  rowGroups: SortableOption[];
  rows: SortableOption[];
}

const TableHeadersForm = (props: Props) => {
  const { filters, onSubmit } = props;
  const { indicators, timePeriods } = filters;

  return (
    <Details summary="Re-order table headers">
      <p className="govuk-hint">
        Drag and drop the options below to re-order the table headers.
      </p>

      <Formik<FormValues>
        enableReinitialize
        initialValues={{
          columnGroups: filters.schoolTypes,
          columns: timePeriods,
          rowGroups: filters.characteristics,
          rows: indicators,
        }}
        onSubmit={onSubmit}
        render={() => {
          return (
            <Form>
              <div className="govuk-grid-row">
                <div className="govuk-grid-column-one-quarter">
                  <FormFieldSortableList<FormValues>
                    name="rowGroups"
                    id="sort-rowGroups"
                    legend="Row groups"
                  />
                </div>
                <div className="govuk-grid-column-one-quarter">
                  <FormFieldSortableList<FormValues>
                    name="rows"
                    id="sort-rows"
                    legend="Rows"
                  />
                </div>
                <div className="govuk-grid-column-one-quarter">
                  <FormFieldSortableList<FormValues>
                    name="columnGroups"
                    id="sort-columnGroups"
                    legend="Column groups"
                  />
                </div>
                <div className="govuk-grid-column-one-quarter">
                  <FormFieldSortableList<FormValues>
                    name="columns"
                    id="sort-columns"
                    legend="Columns"
                  />
                </div>
              </div>
              <div className="govuk-grid-column-one-quarter">
                <FormFieldSortableList<FormValues>
                  name="rows"
                  id="sort-rows"
                  legend="Rows"
                />
              </div>
              <div className="govuk-grid-column-one-quarter">
                <FormFieldSortableList<FormValues>
                  name="columnGroups"
                  id="sort-columnGroups"
                  legend="Column groups"
                />
              </div>
              <div className="govuk-grid-column-one-quarter">
                <FormFieldSortableList<FormValues>
                  name="columns"
                  id="sort-columns"
                  legend="Columns"
                />
              </div>

              <Button type="submit">Re-order table</Button>
            </Form>
          );
        }}
      />
    </Details>
  );
};

export default TableHeadersForm;
