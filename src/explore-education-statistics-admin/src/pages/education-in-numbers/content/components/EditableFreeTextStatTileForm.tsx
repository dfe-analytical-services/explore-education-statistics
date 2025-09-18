import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import Form from '@common/components/form/Form';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import FormProvider from '@common/components/form/FormProvider';
import styles from '@common/modules/find-statistics/components/KeyStat.module.scss';
import { EinFreeTextStatTile } from '@common/services/types/einBlocks';
import Yup from '@common/validation/yup';
import React from 'react';

export type FreeTextStatTileFormValues = Omit<
  EinFreeTextStatTile,
  'id' | 'type' | 'order'
>;

interface EditableFreeTextStatTileFormProps {
  statTile?: EinFreeTextStatTile;
  onSubmit: (values: FreeTextStatTileFormValues) => void;
  onCancel: () => void;
  testId: string;
}

export default function EditableFreeTextStatTileForm({
  statTile,
  onSubmit,
  onCancel,
  testId,
}: EditableFreeTextStatTileFormProps) {
  const handleSubmit = async (values: FreeTextStatTileFormValues) => {
    await onSubmit({
      ...values,
      linkText: values.linkText,
      linkUrl: values.linkUrl,
    });
  };

  return (
    <div data-testid={testId}>
      <FormProvider
        initialValues={{
          title: statTile?.title,
          statistic: statTile?.statistic,
          trend: statTile?.trend,
          linkText: statTile?.linkText,
          linkUrl: statTile?.linkUrl,
        }}
        validationSchema={Yup.object<FreeTextStatTileFormValues>({
          title: Yup.string().required('Enter a title').max(60),
          statistic: Yup.string().required('Enter a statistic').max(12),
          trend: Yup.string().required('Enter a trend').max(230),
          linkText: Yup.string().when('linkUrl', {
            is: (val: string) => val !== '',
            then: s => s.required('Enter the link text'),
          }),
          linkUrl: Yup.string().url(),
        })}
      >
        {({ formState }) => {
          return (
            <Form
              id={
                statTile
                  ? `editableFreeTextStatTileForm-${statTile.id}`
                  : 'editableFreeTextStatTileForm-create'
              }
              onSubmit={handleSubmit}
            >
              <div className={styles.textTile}>
                <FormFieldTextInput<FreeTextStatTileFormValues>
                  name="title"
                  label={<span>Title</span>}
                />
                <FormFieldTextInput<FreeTextStatTileFormValues>
                  name="statistic"
                  label={<span>Statistic</span>}
                />
                <FormFieldTextInput<FreeTextStatTileFormValues>
                  name="trend"
                  label={<span>Trend</span>}
                />

                <FormFieldTextInput<FreeTextStatTileFormValues>
                  label="Link URL"
                  name="linkUrl"
                />

                <FormFieldTextInput<FreeTextStatTileFormValues>
                  label="Link text"
                  name="linkText"
                />
              </div>

              <ButtonGroup className="govuk-!-margin-top-2">
                <Button disabled={formState.isSubmitting} type="submit">
                  Save
                </Button>
                <Button variant="secondary" onClick={onCancel}>
                  Cancel
                </Button>
              </ButtonGroup>
            </Form>
          );
        }}
      </FormProvider>
    </div>
  );
}
