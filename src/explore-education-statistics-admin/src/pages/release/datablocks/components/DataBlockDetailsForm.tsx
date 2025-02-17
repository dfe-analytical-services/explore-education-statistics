import Button from '@common/components/Button';
import { FormFieldset, FormGroup } from '@common/components/form';
import FormFieldTextArea from '@common/components/form/FormFieldTextArea';
import FormProvider from '@common/components/form/FormProvider';
import Form from '@common/components/form/Form';
import { OmitStrict } from '@common/types';
import Yup from '@common/validation/yup';
import React, { useMemo } from 'react';
import FormFieldCheckbox from '@common/components/form/FormFieldCheckbox';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import { ObjectSchema } from 'yup';

interface FormValues {
  heading: string;
  highlightName?: string;
  highlightDescription?: string;
  isHighlight?: boolean;
  name: string;
  source?: string;
}

export type DataBlockDetailsFormValues = OmitStrict<FormValues, 'isHighlight'>;

const dataBlockTableTitleMaxLength = 220;
const featuredTableNameMaxLength = 120;
const featuredTableDescriptionMaxLength = 200;

const formId = 'dataBlockDetailsForm';

interface Props {
  hasChart?: boolean;
  initialValues?: DataBlockDetailsFormValues;
  onTitleChange?: (title: string) => void;
  onSubmit: (dataBlock: DataBlockDetailsFormValues) => void;
}

const DataBlockDetailsForm = ({
  hasChart = false,
  initialValues = {
    heading: '',
    name: '',
    highlightName: '',
    highlightDescription: '',
    source: '',
  },
  onTitleChange,
  onSubmit,
}: Props) => {
  const handleSubmit = async ({
    highlightName,
    highlightDescription,
    isHighlight,
    ...values
  }: FormValues) => {
    return onSubmit({
      ...values,
      highlightName: isHighlight ? highlightName : '',
      highlightDescription: isHighlight ? highlightDescription : '',
    });
  };

  const validationSchema = useMemo<ObjectSchema<FormValues>>(() => {
    return Yup.object({
      name: Yup.string().required('Enter a data block name'),
      heading: Yup.string()
        .required('Enter a table title')
        .max(
          dataBlockTableTitleMaxLength,
          `Table title must be ${dataBlockTableTitleMaxLength} characters or less`,
        ),
      source: Yup.string(),
      highlightName: Yup.string().when('isHighlight', {
        is: true,
        then: s =>
          s
            .required('Enter a featured table name')
            .max(
              featuredTableNameMaxLength,
              `Featured table name must be ${featuredTableNameMaxLength} characters or less`,
            ),
      }),
      highlightDescription: Yup.string().when('isHighlight', {
        is: true,
        then: s =>
          s
            .required('Enter a featured table description')
            .max(
              featuredTableDescriptionMaxLength,
              `Featured table description must be ${featuredTableDescriptionMaxLength} characters or less`,
            ),
      }),
      isHighlight: Yup.boolean(),
    });
  }, []);

  return (
    <FormProvider
      fallbackServerValidationError={
        hasChart
          ? 'The form submission is invalid and could not be processed. Please check the Chart configuration for errors.'
          : undefined
      }
      initialValues={{
        ...initialValues,
        isHighlight: !!initialValues?.highlightName,
      }}
      validationSchema={validationSchema}
    >
      {({ formState, getValues }) => {
        return (
          <Form id={formId} submitId={formId} onSubmit={handleSubmit}>
            <h2>Data block details</h2>

            <FormGroup>
              <FormFieldTextInput<FormValues>
                name="name"
                label="Name"
                hint="Name of the data block. This will not be visible to users."
                className="govuk-!-width-one-half"
              />

              <FormFieldTextArea<FormValues>
                name="heading"
                className="govuk-!-width-two-thirds"
                label="Table title"
                hint="Use a concise descriptive title that summarises the main message in the table."
                rows={3}
                onBlur={() => {
                  onTitleChange?.(getValues('heading'));
                }}
                maxLength={dataBlockTableTitleMaxLength}
              />

              <FormFieldTextInput<FormValues>
                name="source"
                label="Source"
                hint="The data source used to create this data."
                className="govuk-!-width-two-thirds"
              />

              <FormFieldset
                id="highlight"
                legend="Would you like to make this a featured table?"
                legendSize="s"
                hint="Checking this option will make this table available as a featured table when the publication is selected via the table builder"
              >
                <FormFieldCheckbox<FormValues>
                  name="isHighlight"
                  label="Set as a featured table for this publication"
                  conditional={
                    <>
                      <FormFieldTextInput<FormValues>
                        name="highlightName"
                        label="Featured table name"
                        hint="We will show this name to table builder users as a featured table"
                        className="govuk-!-width-two-thirds"
                        maxLength={featuredTableNameMaxLength}
                      />
                      <FormFieldTextArea<FormValues>
                        name="highlightDescription"
                        label="Featured table description"
                        hint="Describe the contents of this featured table to table builder users"
                        className="govuk-!-width-two-thirds"
                        maxLength={featuredTableDescriptionMaxLength}
                      />
                    </>
                  }
                />
              </FormFieldset>

              <Button
                type="submit"
                className="govuk-!-margin-top-6"
                disabled={formState.isSubmitting}
              >
                Save data block
              </Button>
            </FormGroup>
          </Form>
        );
      }}
    </FormProvider>
  );
};

export default DataBlockDetailsForm;
