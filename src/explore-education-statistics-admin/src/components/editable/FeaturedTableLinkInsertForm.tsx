import { FeaturedTable } from '@admin/services/featuredTableService';
import { useConfig } from '@admin/contexts/ConfigContext';
import { useReleaseContentState } from '@admin/pages/release/content/contexts/ReleaseContentContext';
import { FeaturedTableLink } from '@admin/types/ckeditor';
import Button from '@common/components/Button';
import FormComboBox from '@common/components/form/FormComboBox';
import FormProvider from '@common/components/form/FormProvider';
import Form from '@common/components/form/Form';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import createErrorHelper from '@common/components/form/validation/createErrorHelper';
import ButtonGroup from '@common/components/ButtonGroup';
import useDebouncedCallback from '@common/hooks/useDebouncedCallback';
import Yup from '@common/validation/yup';
import React, { useMemo, useState } from 'react';
import { ObjectSchema } from 'yup';

interface FormValues {
  dataBlockParentId: string;
  text: string;
}

interface Props {
  onCancel: () => void;
  onSubmit: (link: FeaturedTableLink) => void;
}

export default function FeaturedTableLinkInsertForm({
  onCancel,
  onSubmit,
}: Props) {
  const config = useConfig();
  const { featuredTables = [] } = useReleaseContentState();
  const [searchResults, setSearchResults] = useState<FeaturedTable[]>([]);
  const [selectedEntry, setSelectedEntry] = useState<FeaturedTable>();

  const [search] = useDebouncedCallback((value: string) => {
    if (value.length < 3) {
      setSearchResults([]);
      return;
    }
    const nextSearchResults = value.length
      ? featuredTables.filter(table => {
          return table.name.toLowerCase().includes(value.toLowerCase());
        })
      : [];
    setSearchResults(nextSearchResults);
  }, 400);

  const handleFormSubmit = (values: FormValues) =>
    onSubmit({
      text: values.text,
      url: `${config.publicAppUrl}/data-tables/fast-track/${values.dataBlockParentId}?featuredTable=true`,
    });

  const validationSchema = useMemo<ObjectSchema<FormValues>>(() => {
    return Yup.object({
      dataBlockParentId: Yup.string().required('Select a featured table'),
      text: Yup.string().required('Enter link text'),
    });
  }, []);

  return (
    <FormProvider validationSchema={validationSchema}>
      {({ formState, getValues, handleSubmit, setValue, trigger }) => {
        const { getError } = createErrorHelper({
          errors: formState.errors,
          touchedFields: formState.touchedFields,
          isSubmitted: true,
        });

        return (
          <Form
            id="featuredTablesForm"
            showErrorSummary={false}
            onSubmit={handleFormSubmit}
          >
            {featuredTables.length > 0 ? (
              <>
                <FormComboBox
                  error={getError('dataBlockParentId')}
                  hideSearchIcon={!!selectedEntry}
                  id="featuredTablesSearch"
                  inputLabel="Featured table"
                  inputValue={selectedEntry?.name}
                  options={searchResults.map(result => result.name)}
                  overflowContainer={false}
                  onInputChange={event => {
                    search(event.target.value);
                  }}
                  onSelect={selectedIndex => {
                    setValue(
                      'text' as const,
                      searchResults[selectedIndex].name,
                      { shouldTouch: true },
                    );
                    setValue(
                      'dataBlockParentId' as const,
                      searchResults[selectedIndex].dataBlockParentId,
                      { shouldTouch: true },
                    );
                    trigger('dataBlockParentId');
                    setSelectedEntry(searchResults[selectedIndex]);
                  }}
                />

                {getValues('dataBlockParentId') && (
                  <FormFieldTextInput<FormValues>
                    formGroupClass="govuk-!-margin-top-5 govuk-!-margin-bottom-2"
                    id="text"
                    label="Link text"
                    name="text"
                  />
                )}

                <ButtonGroup className="govuk-!-margin-top-5">
                  <Button
                    type="submit"
                    onClick={event => {
                      // Have to prevent default here as the form is nested,
                      // despite being in a modal, so otherwise it submits
                      // the parent EditableContentForm.
                      event.preventDefault();
                      handleSubmit(handleFormSubmit)();
                    }}
                  >
                    Insert
                  </Button>
                  <Button variant="secondary" onClick={onCancel}>
                    Cancel
                  </Button>
                </ButtonGroup>
              </>
            ) : (
              <>
                <p>No featured tables available.</p>
                <Button variant="secondary" onClick={onCancel}>
                  Close
                </Button>
              </>
            )}
          </Form>
        );
      }}
    </FormProvider>
  );
}
