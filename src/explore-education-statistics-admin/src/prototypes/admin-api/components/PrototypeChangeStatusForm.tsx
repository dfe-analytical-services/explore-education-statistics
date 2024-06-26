import Button from '@common/components/Button';
import FormProvider from '@common/components/form/FormProvider';
import Form from '@common/components/form/Form';
import FormFieldDateInput from '@common/components/form/FormFieldDateInput';
import FormFieldRadioGroup from '@common/components/form/FormFieldRadioGroup';
import FormFieldTextArea from '@common/components/form/FormFieldTextArea';
import { PartialDate } from '@common/utils/date/partialDate';
import React from 'react';

export interface StatusFormValues {
  date?: PartialDate;
  notes?: string;
  status: 'live' | 'deprecated';
}

interface Props {
  selectedStatus: StatusFormValues;
  onSubmit: (values: StatusFormValues) => void;
}

const PrototypeChangeStatusForm = ({ selectedStatus, onSubmit }: Props) => {
  const { date, notes, status } = selectedStatus;
  return (
    <FormProvider
      initialValues={{
        date,
        notes,
        status,
      }}
    >
      {() => (
        <Form id="form" onSubmit={onSubmit}>
          <>
            <FormFieldRadioGroup<StatusFormValues>
              legend="Changes on current live version (version 1.0)"
              name="status"
              order={[]}
              options={[
                {
                  label: 'Live',
                  value: 'live',
                },
                {
                  label: 'Deprecated',
                  value: 'deprecated',
                  conditional: (
                    <>
                      <FormFieldTextArea<StatusFormValues>
                        hint="These notes will be appended to the published API dataset. They are used to explain to the public users why this data set is being deprecated."
                        label="Public guidance notes"
                        name="notes"
                        rows={3}
                      />

                      <FormFieldDateInput
                        name="date"
                        legend="Expiry date (expected date of deletion)"
                        legendSize="s"
                        hint="The date helps give users advance warning of when this data set will no longer be available for public usage. If you don't yet know a precise date, just add an estimated month leaving the day blank."
                        type="partialDate"
                      />
                    </>
                  ),
                },
              ]}
            />
            <Button type="submit">Update</Button>
          </>
        </Form>
      )}
    </FormProvider>
  );
};

export default PrototypeChangeStatusForm;
