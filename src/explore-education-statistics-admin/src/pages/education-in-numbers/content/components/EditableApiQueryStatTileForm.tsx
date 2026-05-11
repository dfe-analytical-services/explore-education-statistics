import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import Form from '@common/components/form/Form';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import FormProvider from '@common/components/form/FormProvider';
import styles from '@common/modules/find-statistics/components/KeyStat.module.scss';
import { EinApiQueryStatTile } from '@common/services/types/einBlocks';
import Yup from '@common/validation/yup';
import React from 'react';

export type ApiQueryStatTileFormValues = Omit<
  EinApiQueryStatTile,
  'id' | 'type' | 'order'
>;

interface EditableApiQueryStatTileFormProps {
  statTile?: EinApiQueryStatTile;
  onSubmit: (values: ApiQueryStatTileFormValues) => void;
  onCancel: () => void;
  testId: string;
}

export default function EditableApiQueryStatTileForm({
  statTile,
  onSubmit,
  onCancel,
  testId,
}: EditableApiQueryStatTileFormProps) {
  const handleSubmit = async (values: ApiQueryStatTileFormValues) => {
    await onSubmit({
      ...values,
    });
  };

  return (
    <div data-testid={testId}>
      <FormProvider
        initialValues={{
          title: statTile?.title,
          dataSetId: statTile?.dataSetId,
          version: statTile?.version,
          // @MarkFix isLatestVersion?
          query: statTile?.query,
          statistic: statTile?.statistic,
          indicatorUnit: statTile?.indicatorUnit,
          decimalPlaces: statTile?.decimalPlaces,
          // @MarkFix publicationSlug?
          // @MarkFix releaseSlug?
        }}
        validationSchema={Yup.object<ApiQueryStatTileFormValues>({
          title: Yup.string().required('Enter a title').max(100),
          // @MarkFix add validation stuff
        })}
      >
        {({ formState }) => {
          return (
            <Form
              id={
                statTile
                  ? `editableApiQueryStatTileForm-${statTile.id}`
                  : 'editableApiQueryStatTileForm-create'
              }
              onSubmit={handleSubmit}
            >
              <div className={styles.textTile}>
                <FormFieldTextInput<ApiQueryStatTileFormValues>
                  name="title"
                  label={<span>Title</span>}
                />
                <FormFieldTextInput<ApiQueryStatTileFormValues>
                  name="dataSetId"
                  label={<span>Data Set ID</span>}
                />
                <FormFieldTextInput<ApiQueryStatTileFormValues>
                  name="version"
                  label={<span>Data Set Version</span>}
                />

                <FormFieldTextInput<ApiQueryStatTileFormValues>
                  name="query"
                  label="Public API query"
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
