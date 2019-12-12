import Button from '@common/components/Button';
import {
  Form,
  FormFieldTextInput,
  FormGroup,
  Formik,
} from '@common/components/form';
import FormFieldTextArea from '@common/components/form/FormFieldTextArea';
import Yup from '@common/lib/validation/yup';
import {
  TableDataQuery,
  TimeIdentifier,
} from '@common/modules/full-table/services/tableBuilderService';
import { TableHeadersFormValues } from '@common/modules/table-tool/components/TableHeadersForm';
import { DataBlock, GeographicLevel } from '@common/services/dataBlockService';
import { FormikProps } from 'formik';
import React, { ReactNode } from 'react';
import { ObjectSchemaDefinition } from 'yup';

interface Props {
  children?: ReactNode;
  initialValues: { title?: string };
  query: TableDataQuery;
  tableHeaders: TableHeadersFormValues;
  releaseId: string;
  initialDataBlock?: DataBlock;
  onDataBlockSave: (dataBlock: DataBlock) => Promise<DataBlock>;
}

interface FormValues {
  title: string;
  source: string;
  customFootnotes: string;
  name: string;
}

const DataBlockDetailsForm = ({
  children,
  initialValues,
  query,
  tableHeaders,
  releaseId,
  initialDataBlock,
  onDataBlockSave,
}: Props) => {
  const formikRef = React.useRef<Formik<FormValues>>(null);

  const [currentDataBlock, setCurrentDataBlock] = React.useState<
    DataBlock | undefined
  >(initialDataBlock);

  const [blockState, setBlockState] = React.useReducer(
    (
      state,
      { saved, error, updated } = {
        saved: false,
        error: false,
        updated: false,
      },
    ) => {
      if (saved || updated) {
        return { saved: true, updated };
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
      id: currentDataBlock && currentDataBlock.id,

      dataBlockRequest: {
        ...query,
        geographicLevel: query.geographicLevel as GeographicLevel,
        timePeriod: query.timePeriod && {
          ...query.timePeriod,
          startCode: query.timePeriod.startCode as TimeIdentifier,
          endCode: query.timePeriod.endCode as TimeIdentifier,
        },
      },

      heading: values.title,
      customFootnotes: values.customFootnotes,
      name: values.name,

      source: values.source,
      tables: [
        {
          indicators: [],
          tableHeaders,
        },
      ],
    };

    try {
      const savedDataBlock = await onDataBlockSave(dataBlock);
      setCurrentDataBlock(savedDataBlock);
      setBlockState({ saved: true, updated: currentDataBlock !== undefined });
    } catch (error) {
      setBlockState({ error: true });
    }
  };

  const baseValidationRules: ObjectSchemaDefinition<FormValues> = {
    title: Yup.string().required('Please enter a title'),
    name: Yup.string().required('Please supply a name'),
    source: Yup.string(),
    customFootnotes: Yup.string(),
  };

  const usedInitialValues = React.useMemo(() => {
    return {
      title:
        (currentDataBlock && currentDataBlock.heading) ||
        initialValues.title ||
        '',
      customFootnotes:
        (currentDataBlock && currentDataBlock.customFootnotes) || '',
      name: (currentDataBlock && currentDataBlock.name) || '',
      source: (currentDataBlock && currentDataBlock.source) || '',
    };
  }, [currentDataBlock, initialValues.title]);

  return (
    <Formik<FormValues>
      enableReinitialize
      ref={formikRef}
      initialValues={usedInitialValues}
      validationSchema={Yup.object<FormValues>(baseValidationRules)}
      onSubmit={saveDataBlock}
      render={(form: FormikProps<FormValues>) => {
        return (
          <div>
            <Form {...form} id="dataBlockDetails">
              <FormGroup>
                <FormFieldTextInput<FormValues>
                  id="data-block-name"
                  name="name"
                  label="Data block name"
                  hint=" Name and save your datablock before viewing it under the
                    'View data blocks' tab at the top of this page."
                  percentageWidth="one-half"
                />

                <hr />
                {children}

                <FormFieldTextArea<FormValues>
                  id="data-block-title"
                  name="title"
                  label="Table title"
                  additionalClass="govuk-!-width-two-thirds"
                  rows={2}
                />

                <FormFieldTextInput<FormValues>
                  id="data-block-source"
                  name="source"
                  label="Source"
                  percentageWidth="two-thirds"
                />

                <FormFieldTextArea<FormValues>
                  id="data-block-footnotes"
                  name="customFootnotes"
                  label="Footnotes"
                  additionalClass="govuk-!-width-two-thirds"
                />

                <Button
                  disabled={!form.isValid}
                  type="submit"
                  className="govuk-!-margin-top-6"
                >
                  {currentDataBlock ? 'Update data block' : 'Save data block'}
                </Button>
              </FormGroup>

              {blockState.error && (
                <div>
                  An error occurred saving the Data Block, please try again
                  later.
                </div>
              )}
              {blockState.saved && (
                <div>
                  {blockState.updated
                    ? 'The Data Block has been updated.'
                    : 'The Data Block has been saved.'}
                </div>
              )}
            </Form>
          </div>
        );
      }}
    />
  );
};

export default DataBlockDetailsForm;
