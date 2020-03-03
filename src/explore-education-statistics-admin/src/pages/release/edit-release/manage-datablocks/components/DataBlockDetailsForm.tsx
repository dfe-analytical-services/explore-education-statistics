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
} from '@common/modules/table-tool/services/tableBuilderService';
import { TableHeadersConfig } from '@common/modules/table-tool/utils/tableHeaders';
import { DataBlock, GeographicLevel } from '@common/services/dataBlockService';
import { FormikProps } from 'formik';
import React, {
  ReactNode,
  Reducer,
  useEffect,
  useReducer,
  useRef,
  useState,
} from 'react';
import { ObjectSchemaDefinition } from 'yup';

type BlockState = {
  saved?: boolean;
  error?: boolean;
  updated?: boolean;
};

type BlockStateReducer = Reducer<BlockState, BlockState>;

const reducer: BlockStateReducer = (
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
};

interface Props {
  children?: ReactNode;
  initialValues?: DataBlockDetailsFormValues;
  query: TableDataQuery;
  tableHeaders: TableHeadersConfig;
  releaseId: string;
  initialDataBlock?: DataBlock;
  onDataBlockSave: (dataBlock: DataBlock) => Promise<DataBlock>;
}

export interface DataBlockDetailsFormValues {
  title: string;
  source: string;
  customFootnotes: string;
  name: string;
}

const DataBlockDetailsForm = ({
  children,
  initialValues = { title: '', name: '', source: '', customFootnotes: '' },
  query,
  tableHeaders,
  releaseId,
  initialDataBlock,
  onDataBlockSave,
}: Props) => {
  const formikRef = useRef<Formik<DataBlockDetailsFormValues>>(null);

  const [currentDataBlock, setCurrentDataBlock] = useState<
    DataBlock | undefined
  >(initialDataBlock);

  const [blockState, setBlockState] = useReducer<BlockStateReducer>(reducer, {
    saved: false,
    error: false,
  });

  useEffect(() => {
    setBlockState({ saved: false, error: false });
  }, [query, tableHeaders, releaseId]);

  const saveDataBlock = async (values: DataBlockDetailsFormValues) => {
    const dataBlock: DataBlock = {
      id: initialDataBlock ? initialDataBlock.id : undefined,
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
      tables: [],
    };

    try {
      const savedDataBlock = await onDataBlockSave(dataBlock);
      setCurrentDataBlock(savedDataBlock);
      setBlockState({ saved: true, updated: currentDataBlock !== undefined });
    } catch (error) {
      setBlockState({ error: true });
    }
  };

  const baseValidationRules: ObjectSchemaDefinition<DataBlockDetailsFormValues> = {
    title: Yup.string().required('Please enter a title'),
    name: Yup.string().required('Please supply a name'),
    source: Yup.string(),
    customFootnotes: Yup.string(),
  };

  return (
    <Formik<DataBlockDetailsFormValues>
      enableReinitialize
      ref={formikRef}
      initialValues={initialValues}
      validationSchema={Yup.object<DataBlockDetailsFormValues>(
        baseValidationRules,
      )}
      onSubmit={saveDataBlock}
      render={(form: FormikProps<DataBlockDetailsFormValues>) => {
        return (
          <div>
            <Form {...form} id="dataBlockDetails">
              <FormGroup>
                <FormFieldTextInput<DataBlockDetailsFormValues>
                  id="data-block-name"
                  name="name"
                  label="Data block name"
                  hint=" Name and save your data block before viewing it under the
                    'View data blocks' tab at the top of this page."
                  percentageWidth="one-half"
                />

                <hr />
                {children}

                <FormFieldTextArea<DataBlockDetailsFormValues>
                  id="data-block-title"
                  name="title"
                  label="Table title"
                  additionalClass="govuk-!-width-two-thirds"
                  rows={2}
                />

                <FormFieldTextInput<DataBlockDetailsFormValues>
                  id="data-block-source"
                  name="source"
                  label="Source"
                  percentageWidth="two-thirds"
                />

                <FormFieldTextArea<DataBlockDetailsFormValues>
                  id="data-block-footnotes"
                  name="customFootnotes"
                  label="Footnotes"
                  additionalClass="govuk-!-width-two-thirds"
                />

                <Button type="submit" className="govuk-!-margin-top-6">
                  {currentDataBlock ? 'Update data block' : 'Save data block'}
                </Button>
              </FormGroup>

              {blockState.error && (
                <p>
                  An error occurred saving the data block, please try again
                  later.
                </p>
              )}
              {blockState.saved && (
                <p>
                  {blockState.updated
                    ? 'The data block has been updated.'
                    : 'The data block has been saved.'}
                </p>
              )}
            </Form>
          </div>
        );
      }}
    />
  );
};

export default DataBlockDetailsForm;
