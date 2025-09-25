import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import {
  Form,
  FormFieldCheckbox,
  FormFieldset,
  FormFieldTextInput,
} from '@common/components/form';
import FormProvider from '@common/components/form/FormProvider';
import LoadingSpinner from '@common/components/LoadingSpinner';
import Yup from '@common/validation/yup';
import React, { useMemo } from 'react';
import { ObjectSchema } from 'yup';
import FormFieldDateInput from '@common/components/form/FormFieldDateInput';
import FormFieldRadioGroup from '@common/components/form/FormFieldRadioGroup';
import { RadioOption } from '@common/components/form/FormRadioGroup';
import { useAuthContext } from '@admin/contexts/AuthContext';

interface FormValues {
  agreeTerms: boolean;
  label: string;
  datePresetSpan?: number | null;
  activates?: Date | string | null;
  expires?: Date | string | null;
  selectionMethod?: 'presetDays' | 'customDates' | null;
}

interface Props {
  onCancel: () => void;
  onSubmit: (
    label: string,
    datePresetSpan: number,
    activates?: Date | null,
    expires?: Date | null,
  ) => void;
}

const presetOptions: RadioOption[] = [
  { label: '1 day', value: '1' },
  { label: '2 days', value: '2' },
  { label: '3 days', value: '3' },
  { label: '4 days', value: '4' },
  { label: '5 days', value: '5' },
  { label: '6 days', value: '6' },
  { label: '7 days', value: '7' },
];
export default function ApiDataSetPreviewTokenCreateForm({
  onCancel,
  onSubmit,
}: Props) {
  const endDateIsLaterThanOrEqualToStartDate = (
    startDate: Date,
    endDate: Date,
  ) => {
    const startMidnight = new Date(
      startDate.getFullYear(),
      startDate.getMonth(),
      startDate.getDate(),
    );
    const endMidnight = new Date(
      endDate.getFullYear(),
      endDate.getMonth(),
      endDate.getDate(),
    );
    return startMidnight <= endMidnight;
  };
  const showInline = true;
  const { user } = useAuthContext();
  const isBau = user?.permissions.isBauUser;
  const validationSchema = useMemo<ObjectSchema<FormValues>>(() => {
    return Yup.object({
      label: Yup.string().required('Enter a token name'),
      agreeTerms: Yup.boolean()
        .required('The terms of usage must be agreed')
        .oneOf([true], 'The terms of usage must be agreed'),
      datePresetSpan: Yup.number()
        .nullable()
        .when('selectionMethod', {
          is: 'presetDays',
          then: s =>
            s
              .required('A duration between 1 and 7 days must be selected')
              .test({
                name: 'in-range',
                message: 'A duration between 1 and 7 days must be selected',
                test(value) {
                  return value >= 1 && value <= 7;
                },
              }),
        }),
      activates: Yup.date().when('selectionMethod', {
        is: 'customDates',
        then: s =>
          s
            .required('Activates date must be a valid date')
            .test({
              name: 'not-in-past',
              message: 'Activates date must not be in the past',
              test(value) {
                if (value == null) return true;
                const now = new Date();
                return endDateIsLaterThanOrEqualToStartDate(now, value);
              },
            })
            .test({
              name: 'within-7-days',
              message: 'Activates date must be within 7 days from today',
              test(value) {
                if (value == null) return false;
                const now = new Date();
                const maxDate = new Date();
                maxDate.setDate(now.getDate() + 7);
                return endDateIsLaterThanOrEqualToStartDate(value, maxDate);
              },
            }),
      }),
      expires: Yup.date().when('selectionMethod', {
        is: 'customDates',
        then: s =>
          s.required('Expires date must be a valid date').test({
            name: 'after-activates',
            message:
              'Expires date must be later than Activates date by at most 7 days',
            test(value, context) {
              const activates = context.parent.activates as Date | null;
              if (value == null || activates == null) return true;
              const maxDate = new Date();
              value.setHours(23, 59, 59, 999);
              maxDate.setDate(activates.getDate() + 7);
              const laterThanActivates = endDateIsLaterThanOrEqualToStartDate(
                activates,
                value,
              );
              const notLaterThanMaxTime = endDateIsLaterThanOrEqualToStartDate(
                value,
                maxDate,
              );
              return laterThanActivates && notLaterThanMaxTime;
            },
          }),
      }),
      selectionMethod: Yup.mixed<'presetDays' | 'customDates'>()
        .oneOf(['presetDays', 'customDates'])
        .notRequired(),
    });
  }, []);
  return (
    <FormProvider validationSchema={validationSchema}>
      {({ formState }) => {
        return (
          <Form
            id="apiDataSetTokenCreateForm"
            onSubmit={values =>
              onSubmit(
                values.label,
                values.selectionMethod === 'presetDays'
                  ? values.datePresetSpan
                  : undefined,
                values.selectionMethod === 'customDates'
                  ? values.activates
                  : undefined,
                values.selectionMethod === 'customDates'
                  ? values.expires
                  : undefined,
              )
            }
          >
            <FormFieldTextInput<FormValues>
              name="label"
              label="Token name"
              hint="Add a name so you can easily reference this token"
            />
            {isBau && (
              <>
                <FormFieldRadioGroup<FormValues>
                  name="selectionMethod"
                  legend="Select a custom duration"
                  legendSize="s"
                  hint="Select a number of days"
                  options={[
                    {
                      label: 'Choose number of days',
                      value: 'presetDays',
                      conditional: (
                        <FormFieldRadioGroup<FormValues>
                          legend="Select a duration"
                          legendSize="s"
                          inline={showInline}
                          name="datePresetSpan"
                          options={presetOptions}
                        />
                      ),
                    },
                    {
                      label: 'Enter specific start and end dates',
                      value: 'customDates',
                      conditional: (
                        <>
                          <FormFieldDateInput<FormValues>
                            hint="The date the preview token activates for use"
                            legend="Activates on"
                            legendSize="s"
                            name="activates"
                            id="activates"
                          />
                          <br />
                          <FormFieldDateInput<FormValues>
                            hint="The date the preview token expires"
                            legend="Expires on"
                            legendSize="s"
                            name="expires"
                            id="expires"
                          />
                        </>
                      ),
                    },
                  ]}
                />
                <br />
              </>
            )}
            <FormFieldset
              error={formState.errors?.agreeTerms?.message}
              id="terms"
              legend="Terms of usage"
              legendSize="s"
            >
              <FormFieldCheckbox
                name="agreeTerms"
                label="I agree to only share this token with individuals that have been granted production access to the unpublished data"
              />
            </FormFieldset>

            <ButtonGroup>
              <Button type="submit" ariaDisabled={formState.isSubmitting}>
                Continue
              </Button>
              <ButtonText
                ariaDisabled={formState.isSubmitting}
                onClick={onCancel}
              >
                Cancel
              </ButtonText>
              <LoadingSpinner
                alert
                hideText
                inline
                loading={formState.isSubmitting}
                size="sm"
                text="Creating new preview token"
              />
            </ButtonGroup>
          </Form>
        );
      }}
    </FormProvider>
  );
}
