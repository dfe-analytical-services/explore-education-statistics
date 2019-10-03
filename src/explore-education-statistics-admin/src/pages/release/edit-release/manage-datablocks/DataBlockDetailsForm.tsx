import { DataBlock } from '@admin/services/release/edit-release/datablocks/types';
import Button from '@common/components/Button';
import { Form, FormFieldset, FormFieldTextInput, FormGroup, Formik } from '@common/components/form';
import FormFieldTextArea from '@common/components/form/FormFieldTextArea';
import Yup from '@common/lib/validation/yup';
import { TableDataQuery } from '@common/modules/full-table/services/tableBuilderService';
import { TableHeadersFormValues } from '@common/modules/table-tool/components/TableHeadersForm';
import { GeographicLevel, TimeIdentifier } from '@common/services/dataBlockService';
import { FormikProps } from 'formik';
import React from 'react';
import { ObjectSchemaDefinition } from 'yup';

interface Props {
  query: TableDataQuery;
  tableHeaders: TableHeadersFormValues;
  releaseId: string;
  onDataBlockSave: (dataBlock: DataBlock) => Promise<DataBlock>;
}

interface FormValues {
  title: string;
  source: string;
  footnotes: string;
  name: string;
}

const DataBlockDetailsForm = ({ query, tableHeaders, releaseId, onDataBlockSave }: Props) => {

  const [blockState, setBlockState] = React.useReducer(
    (state, { saved, error } = { saved: false, error: false }) => {
      if (saved) {
        return { saved: true };
      }
      if (error) {
        return { error: true };
      }
      return { saved: false, error: false };
    },
    { saved: false, error: false },
  );

  React.useEffect(() => {
    setBlockState({ saved: false, error: false });
  }, [query, tableHeaders, releaseId]);

  const saveDataBlock = async (values: FormValues) => {

    const dataBlock: DataBlock = {
      dataBlockRequest :  {
        ...query,
        geographicLevel: query.geographicLevel as GeographicLevel,
        timePeriod: query.timePeriod && {
          ...query.timePeriod,
          startCode: query.timePeriod.startCode as TimeIdentifier,
          endCode: query.timePeriod.endCode as TimeIdentifier,
        },
      },

      heading: values.title,
      tables: [
        {
          indicators: [],
          tableHeaders,
        },
      ],
    };


    try {
      await onDataBlockSave(dataBlock);
      setBlockState({ saved: true });
    } catch (error) {
      setBlockState({ error: true });
    }
  };

  const baseValidationRules: ObjectSchemaDefinition<FormValues> = {
    title: Yup.string().required('Please enter a title'),
    name: Yup.string().required('Please supply a name'),
    source: Yup.string(),
    footnotes: Yup.string(),
  };

  return (
    <Formik<FormValues>
      initialValues={{
        title: '',
        footnotes: '',
        name: '',
        source: '',
      }}
      validationSchema={Yup.object<FormValues>(baseValidationRules)}
      onSubmit={saveDataBlock}
      render={(form: FormikProps<FormValues>) => {
        return (
          <div>
            <Form {...form} id="dataBlockDetails">
              <FormGroup>
                <FormFieldset id="details" legend="Data block details">
                  <FormFieldTextInput<FormValues>
                    id="data-block-title"
                    name="title"
                    label="Data block title"
                  />

                  <FormFieldTextInput<FormValues>
                    id="data-block-source"
                    name="source"
                    label="Source"
                  />

                  <FormFieldTextArea<FormValues>
                    id="data-block-footnotes"
                    name="Release footnotes"
                    label="Footnotes"
                  />

                  <p>
                    Name and save your datablock before viewing it under the
                    'View data blocks' tab at the top of this page.
                  </p>

                  <FormFieldTextInput<FormValues>
                    id="data-block-name"
                    name="name"
                    label="Data block name"
                  />

                  <Button
                    disabled={!form.isValid}
                    type="submit"
                    className="govuk-!-margin-top-6"
                  >
                    Save data block
                  </Button>
                </FormFieldset>
              </FormGroup>

              {blockState.error && (
                <div>
                  An error occurred saving the Data Block, please try again
                  later.
                </div>
              )}
              {blockState.saved && <div>The Data Block has been saved.</div>}
            </Form>
          </div>
        );
      }}
    />
  );
};

export default DataBlockDetailsForm;
