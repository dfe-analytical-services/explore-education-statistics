import Button from '@common/components/Button';
import Details from '@common/components/Details';
import { Form, Formik } from 'formik';
import React, { Component } from 'react';
import FormFieldSortableList from 'src/prototypes/table-tool/components/FormFieldSortableList';
import { SortableOption } from 'src/prototypes/table-tool/components/FormSortableList';
import {
  FilterOption,
  IndicatorOption,
} from 'src/prototypes/table-tool/components/meta/initialSpec';
import TimePeriod from 'src/services/types/TimePeriod';

interface Props {
  filters: {
    indicators: IndicatorOption[];
    categorical: {
      [key: string]: FilterOption[];
    };
    timePeriods: TimePeriod[];
  };
  onSubmit: (values: FormValues) => void;
}

export interface FormValues {
  columnGroups: SortableOption[];
  columns: SortableOption[];
  rowGroups: SortableOption[];
  rows: SortableOption[];
}

class TableHeadersForm extends Component<Props> {
  public render() {
    const { filters, onSubmit } = this.props;
    const { categorical, indicators, timePeriods } = filters;

    return (
      <Details summary="Re-order table headers">
        <p className="govuk-hint">
          Drag and drop the options below to re-order the table headers.
        </p>

        <Formik<FormValues>
          enableReinitialize={true}
          initialValues={{
            columnGroups: categorical.schoolTypes,
            columns: timePeriods,
            rowGroups: categorical.characteristics,
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

                <Button type="submit">Re-order table</Button>
              </Form>
            );
          }}
        />
      </Details>
    );
  }
}

export default TableHeadersForm;
