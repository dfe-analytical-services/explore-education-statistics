import adminTableBuilderService, {
  PairedIndicators,
} from '@admin/services/tableBuilderService';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import { Form, FormFieldset } from '@common/components/form';
import FormFieldSelect from '@common/components/form/FormFieldSelect';
import FormProvider from '@common/components/form/FormProvider';
import { SelectOption } from '@common/components/form/FormSelect';
import LoadingSpinner from '@common/components/LoadingSpinner';
import VisuallyHidden from '@common/components/VisuallyHidden';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import tableBuilderService, {
  Subject,
} from '@common/services/tableBuilderService';
import { Dictionary } from '@common/types';
import { mapFieldErrors } from '@common/validation/serverValidations';
import Yup from '@common/validation/yup';
import get from 'lodash/get';
import React, { Fragment, useMemo } from 'react';
import { useFieldArray, useFormContext } from 'react-hook-form';
import { ObjectSchema } from 'yup';

const formId = 'pairedIndicatorsForm';

export const pairedIndicatorErrorMessages = {
  cannotPairWithItself: 'An indicator cannot be paired with itself.',
  childHasMultipleParents:
    'A child indicator can only have one parent indicator.',
  parentHasMultipleChildren:
    'A parent indicator can only have one child indicator.',
  cannotBeChained:
    'An indicator cannot be both a parent and a child indicator.',
} as const;

interface FormValues {
  pairedIndicators: {
    parentIndicatorId: string;
    childIndicatorId: string;
  }[];
}

const errorMappings = [
  mapFieldErrors<FormValues>({
    target: 'pairedIndicators',
    messages: {
      PairedIndicatorsImportNotComplete:
        'Paired indicators cannot be changed until the data set import is complete.',
      PairedIndicatorsIndicatorNotForSubject:
        'All paired indicators must belong to the data set.',
      PairedIndicatorChildHasMultipleParents:
        pairedIndicatorErrorMessages.childHasMultipleParents,
      PairedIndicatorParentHasMultipleChildren:
        pairedIndicatorErrorMessages.parentHasMultipleChildren,
      PairedIndicatorsCannotBeChained:
        pairedIndicatorErrorMessages.cannotBeChained,
    },
  }),
];

const validationSchema: ObjectSchema<FormValues> = Yup.object({
  pairedIndicators: Yup.array()
    .of(
      Yup.object({
        parentIndicatorId: Yup.string().required('Select a parent indicator'),
        childIndicatorId: Yup.string().required('Select a child indicator'),
      }),
    )
    .required()
    .test('pairedIndicatorRules', (pairs, { createError }) => {
      const parentIds = pairs.map(pair => pair.parentIndicatorId);
      const childIds = pairs.map(pair => pair.childIndicatorId);

      for (let index = 0; index < pairs.length; index += 1) {
        const { parentIndicatorId, childIndicatorId } = pairs[index];

        if (!parentIndicatorId || !childIndicatorId) {
          // eslint-disable-next-line no-continue
          continue;
        }

        const rowPath = `pairedIndicators[${index}]`;

        if (parentIndicatorId === childIndicatorId) {
          return createError({
            path: `${rowPath}.childIndicatorId`,
            message: pairedIndicatorErrorMessages.cannotPairWithItself,
          });
        }

        if (childIds.indexOf(childIndicatorId) !== index) {
          return createError({
            path: `${rowPath}.childIndicatorId`,
            message: pairedIndicatorErrorMessages.childHasMultipleParents,
          });
        }

        if (parentIds.indexOf(parentIndicatorId) !== index) {
          return createError({
            path: `${rowPath}.parentIndicatorId`,
            message: pairedIndicatorErrorMessages.parentHasMultipleChildren,
          });
        }

        if (
          parentIds.includes(childIndicatorId) ||
          childIds.includes(parentIndicatorId)
        ) {
          return createError({
            path: `${rowPath}.childIndicatorId`,
            message: pairedIndicatorErrorMessages.cannotBeChained,
          });
        }
      }

      return true;
    }),
});

interface Props {
  releaseVersionId: string;
  subject: Subject;
  onCancel: () => void;
  onSave: (subjectId: string, request: PairedIndicators) => Promise<void>;
}

export default function PairedIndicatorsForm({
  releaseVersionId,
  subject,
  onCancel,
  onSave,
}: Props) {
  const { value, isLoading } = useAsyncHandledRetry(async () => {
    const [subjectMeta, { pairedIndicators }] = await Promise.all([
      tableBuilderService.getSubjectMeta(subject.id, releaseVersionId),
      adminTableBuilderService.getPairedIndicators(
        releaseVersionId,
        subject.id,
      ),
    ]);

    return { subjectMeta, pairedIndicators };
  }, [subject.id, releaseVersionId]);

  const indicatorOptGroups = useMemo<Dictionary<SelectOption[]>>(
    () =>
      Object.fromEntries(
        Object.values(value?.subjectMeta.indicators ?? {}).map(group => [
          group.label,
          group.options.map(option => ({
            label: option.label,
            value: option.value,
          })),
        ]),
      ),
    [value?.subjectMeta.indicators],
  );

  const initialValues = useMemo<FormValues>(
    () => ({ pairedIndicators: value?.pairedIndicators ?? [] }),
    [value?.pairedIndicators],
  );

  return (
    <LoadingSpinner loading={isLoading}>
      <h3>{`Pair indicators for ${subject.name}`}</h3>
      <FormProvider
        enableReinitialize
        errorMappings={errorMappings}
        initialValues={initialValues}
        validationSchema={validationSchema}
      >
        {({ formState }) => (
          <Form<FormValues>
            id={formId}
            onSubmit={async values => {
              await onSave(subject.id, values);
            }}
          >
            <PairedIndicatorsFields indicatorOptGroups={indicatorOptGroups} />

            <ButtonGroup>
              <Button type="submit" disabled={formState.isSubmitting}>
                Save paired indicators
              </Button>
              <Button variant="secondary" onClick={onCancel}>
                Cancel
              </Button>
            </ButtonGroup>
          </Form>
        )}
      </FormProvider>
    </LoadingSpinner>
  );
}

function PairedIndicatorsFields({
  indicatorOptGroups,
}: {
  indicatorOptGroups: Dictionary<SelectOption[]>;
}) {
  const {
    formState: { errors },
  } = useFormContext<FormValues>();

  const { fields, append, remove } = useFieldArray<
    FormValues,
    'pairedIndicators'
  >({ name: 'pairedIndicators' });

  const listError =
    get(errors, 'pairedIndicators.message') ??
    get(errors, 'pairedIndicators.root.message');

  return (
    <FormFieldset
      id="pairedIndicators"
      legend="Paired indicators"
      legendSize="m"
      hint="A child indicator is selected automatically with its parent indicator in the table tool, and cannot be selected on its own. An indicator can only have one parent or one child indicator, and cannot be both a parent and a child indicator."
      error={typeof listError === 'string' ? listError : undefined}
    >
      {fields.length === 0 && (
        <p>No indicators have been paired for this data set.</p>
      )}

      {fields.map((field, index) => (
        <Fragment key={field.id}>
          <FormFieldset
            id={`pairedIndicators-${index}`}
            legend={`Pair ${index + 1}`}
            legendSize="s"
            formGroupClass="govuk-!-margin-bottom-0"
          >
            <FormFieldSelect<FormValues>
              name={`pairedIndicators.${index}.parentIndicatorId`}
              label="Parent indicator"
              optGroups={indicatorOptGroups}
              placeholder="Select an indicator"
            />
            <FormFieldSelect<FormValues>
              name={`pairedIndicators.${index}.childIndicatorId`}
              label="Child indicator"
              hint="Selected automatically when the parent indicator is selected"
              optGroups={indicatorOptGroups}
              placeholder="Select an indicator"
            />
            <Button
              className="govuk-!-margin-top-6 govuk-!-margin-bottom-0"
              variant="secondary"
              onClick={() => remove(index)}
            >
              Remove pair
              <VisuallyHidden>{` ${index + 1}`}</VisuallyHidden>
            </Button>
          </FormFieldset>

          <hr className="govuk-section-break govuk-section-break--l govuk-section-break--visible" />
        </Fragment>
      ))}

      <Button
        variant="secondary"
        onClick={() => append({ parentIndicatorId: '', childIndicatorId: '' })}
      >
        {fields.length > 0 ? 'Add another pair' : 'Add pair'}
      </Button>
    </FormFieldset>
  );
}
