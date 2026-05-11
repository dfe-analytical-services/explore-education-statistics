import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import Form from '@common/components/form/Form';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import FormProvider from '@common/components/form/FormProvider';
import styles from '@common/modules/find-statistics/components/KeyStat.module.scss';
import { EinFreeTextStatTile } from '@common/services/types/einBlocks';
import Yup from '@common/validation/yup';
import React from 'react';

export type FreeTextStatTileFormValues = {
  title: string;
  statistic: string;
  trend: string;
  linkUrl?: string;
  linkText?: string;
};

interface EditableFreeTextStatTileFormProps {
  freeTextStatTile?: EinFreeTextStatTile;
  onSubmit: (values: FreeTextStatTileFormValues) => Promise<void> | void;
  onCancel: () => void;
  testId: string;
}

export default function EditableFreeTextStatTileForm({
  freeTextStatTile,
  onSubmit,
  onCancel,
  testId,
}: EditableFreeTextStatTileFormProps) {
  return (
    <div data-testid={testId}>
      <FormProvider
        initialValues={{
          title: freeTextStatTile?.title,
          statistic: freeTextStatTile?.statistic,
          trend: freeTextStatTile?.trend,
          linkText: freeTextStatTile?.linkText,
          linkUrl: freeTextStatTile?.linkUrl,
        }}
        validationSchema={Yup.object<FreeTextStatTileFormValues>({
          title: Yup.string().required('Enter a title').max(100),
          statistic: Yup.string().required('Enter a statistic').max(30),
          trend: Yup.string().required('Enter a trend').max(230),
          linkText: Yup.string().when('linkUrl', {
            is: (val: string) => val !== '',
            then: s => s.required('Enter the link text'),
            otherwise: s => s.notRequired(),
          }),
          linkUrl: Yup.string().url(),
        })}
      >
        {({ formState }) => {
          return (
            <Form
              id={
                freeTextStatTile
                  ? `editableFreeTextStatTileForm-${freeTextStatTile.id}`
                  : 'editableFreeTextStatTileForm-create'
              }
              onSubmit={onSubmit}
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
