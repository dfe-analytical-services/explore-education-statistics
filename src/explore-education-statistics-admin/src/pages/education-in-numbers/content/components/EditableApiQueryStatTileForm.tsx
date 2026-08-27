import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import Form from '@common/components/form/Form';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import FormProvider from '@common/components/form/FormProvider';
import styles from '@common/modules/find-statistics/components/KeyStat.module.scss';
import { EinApiQueryStatTile } from '@common/services/types/einBlocks';
import Yup from '@common/validation/yup';
import React from 'react';
import { FormFieldTextArea } from '@common/components/form';

export type ApiQueryStatTileFormValues = Pick<
  EinApiQueryStatTile,
  'title' | 'dataSetId' | 'version' | 'query'
>;

interface EditableApiQueryStatTileFormProps {
  apiQueryStatTile?: EinApiQueryStatTile;
  onSubmit: (values: ApiQueryStatTileFormValues) => void;
  onCancel: () => void;
  testId: string;
}

export default function EditableApiQueryStatTileForm({
  apiQueryStatTile,
  onSubmit,
  onCancel,
  testId,
}: EditableApiQueryStatTileFormProps) {
  return (
    <div data-testid={testId}>
      <FormProvider
        initialValues={{
          title: apiQueryStatTile?.title,
          dataSetId: apiQueryStatTile?.dataSetId,
          version: apiQueryStatTile?.version,
          query: apiQueryStatTile?.query,
        }}
        validationSchema={Yup.object<ApiQueryStatTileFormValues>({
          title: Yup.string().required('Enter a title').max(100),
          dataSetId: Yup.string().required('Enter the dataSetId').length(36),
          version: Yup.string().required('Enter data set version').max(32),
          query: Yup.string()
            .required('Enter the API query JSON body')
            .max(10000),
        })}
      >
        {({ formState }) => {
          return (
            <Form
              id={
                apiQueryStatTile
                  ? `editableApiQueryStatTileForm-${apiQueryStatTile.id}`
                  : 'editableApiQueryStatTileForm-create'
              }
              onSubmit={onSubmit}
            >
              <div className={styles.textTile}>
                <FormFieldTextInput<ApiQueryStatTileFormValues>
                  name="title"
                  label={<span>Title</span>}
                />
                <FormFieldTextInput<ApiQueryStatTileFormValues>
                  name="dataSetId"
                  label={<span>DataSetId</span>}
                />
                <FormFieldTextInput<ApiQueryStatTileFormValues>
                  name="version"
                  label={<span>Version</span>}
                  hint="Must be the full version - i.e. 1.0.0 not 1.0 or 1"
                />
                <FormFieldTextArea<ApiQueryStatTileFormValues>
                  name="query"
                  label={<span>Query</span>}
                  hint="Input the JSON query body here"
                  rows={10}
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
