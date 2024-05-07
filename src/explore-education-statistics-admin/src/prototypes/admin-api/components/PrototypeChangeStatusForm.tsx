import Button from '@common/components/Button';
import FormProvider from '@common/components/form/rhf/FormProvider';
import RHFForm from '@common/components/form/rhf/RHFForm';
import RHFFormFieldDateInput from '@common/components/form/rhf/RHFFormFieldDateInput';
import RHFFormFieldRadioGroup from '@common/components/form/rhf/RHFFormFieldRadioGroup';
import RHFFormFieldTextArea from '@common/components/form/rhf/RHFFormFieldTextArea';
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
        <RHFForm id="form" onSubmit={onSubmit}>
          <>
            <RHFFormFieldRadioGroup<StatusFormValues>
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
                      <RHFFormFieldTextArea<StatusFormValues>
                        hint="These notes will be appended to the published API dataset. They are used to explain to the public users why this data set is being deprecated."
                        label="Public guidance notes"
                        name="notes"
                        rows={3}
                      />

                      <RHFFormFieldDateInput
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
        </RHFForm>
      )}
    </FormProvider>
  );
};

export default PrototypeChangeStatusForm;
